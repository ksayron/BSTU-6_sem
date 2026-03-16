using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RsaTextCryptoDemo;

internal enum TextEncodingMode
{
    Ascii,
    Base64
}

internal sealed class PreparedText
{
    public required string OriginalText { get; init; }
    public required string EncodedText { get; init; }
    public required List<int> Units { get; init; }
    public required int PaddingCount { get; init; }
    public required int OriginalByteCount { get; init; }
    public required int EncodedByteCount { get; init; }
}

internal sealed class RsaKeyPair
{
    public required BigInteger P { get; init; }
    public required BigInteger Q { get; init; }
    public required BigInteger N { get; init; }
    public required BigInteger Phi { get; init; }
    public required BigInteger E { get; init; }
    public required BigInteger D { get; init; }
}

internal static class Program
{
    private const int KeySizeBits = 256;
    private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        string fio = "Kucheruk Nikolay Petrovich";

        Console.WriteLine("RSA: шифрование/расшифрование текста");
        Console.WriteLine(new string('=', 90));
        Console.WriteLine($"Исходный текст: {fio}");
        Console.WriteLine($"Размер ключа: {KeySizeBits} бит");
        Console.WriteLine();

        Console.WriteLine("Генерация ключей RSA...");
        RsaKeyPair keys = GenerateRsaKeyPair(KeySizeBits);

        Console.WriteLine("Ключи сгенерированы.");
        Console.WriteLine($"n = {keys.N}");
        Console.WriteLine($"e = {keys.E}");
        Console.WriteLine($"d = {keys.D}");
        Console.WriteLine();

        RunScenario(fio, TextEncodingMode.Ascii, keys);
        RunScenario(fio, TextEncodingMode.Base64, keys);

        Console.WriteLine();
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    private static void RunScenario(string text, TextEncodingMode mode, RsaKeyPair keys)
    {
        Console.WriteLine(new string('-', 90));
        Console.WriteLine($"Режим: {mode}");
        Console.WriteLine(new string('-', 90));

        PreparedText prepared = PrepareText(text, mode);

        Console.WriteLine($"Подготовленный текст: {prepared.EncodedText}");
        Console.WriteLine($"Числовых блоков: {prepared.Units.Count}");

        Stopwatch encSw = Stopwatch.StartNew();
        List<BigInteger> cipherBlocks = Encrypt(prepared.Units, keys);
        encSw.Stop();

        Stopwatch decSw = Stopwatch.StartNew();
        List<int> restoredUnits = Decrypt(cipherBlocks, keys);
        string restoredText = RestoreText(restoredUnits, mode, prepared.PaddingCount);
        decSw.Stop();

        int modulusBytes = GetUnsignedByteCount(keys.N);
        long cipherBytes = (long)cipherBlocks.Count * modulusBytes;

        Console.WriteLine($"Расшифрованный текст: {restoredText}");
        Console.WriteLine($"Время шифрования: {encSw.Elapsed.TotalMilliseconds:F6} мс");
        Console.WriteLine($"Время расшифрования: {decSw.Elapsed.TotalMilliseconds:F6} мс");
        Console.WriteLine($"Размер исходного текста: {prepared.OriginalByteCount} байт");
        Console.WriteLine($"Размер представления ({mode}): {prepared.EncodedByteCount} байт");
        Console.WriteLine($"Оценочный размер шифртекста: {cipherBytes} байт");
        Console.WriteLine($"Рост относительно исходного текста: {(double)cipherBytes / prepared.OriginalByteCount:F2}x");
        Console.WriteLine($"Рост относительно представления {mode}: {(double)cipherBytes / prepared.EncodedByteCount:F2}x");
        Console.WriteLine();

        Console.WriteLine("Первые шифрблоки:");
        foreach (BigInteger block in cipherBlocks.Take(5))
        {
            Console.WriteLine(block);
        }

        Console.WriteLine();
    }

    private static PreparedText PrepareText(string text, TextEncodingMode mode)
    {
        return mode switch
        {
            TextEncodingMode.Ascii => PrepareAscii(text),
            TextEncodingMode.Base64 => PrepareBase64(text),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    private static PreparedText PrepareAscii(string text)
    {
        if (text.Any(ch => ch > 127))
        {
            throw new ArgumentException("Для режима ASCII текст должен содержать только ASCII-символы.");
        }

        List<int> units = text.Select(ch => (int)ch).ToList();

        return new PreparedText
        {
            OriginalText = text,
            EncodedText = text,
            Units = units,
            PaddingCount = 0,
            OriginalByteCount = Encoding.ASCII.GetByteCount(text),
            EncodedByteCount = Encoding.ASCII.GetByteCount(text)
        };
    }

    private static PreparedText PrepareBase64(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        string base64 = Convert.ToBase64String(bytes);
        int paddingCount = base64.Count(ch => ch == '=');

        List<int> units = base64
            .Where(ch => ch != '=')
            .Select(ch => Base64Alphabet.IndexOf(ch))
            .ToList();

        return new PreparedText
        {
            OriginalText = text,
            EncodedText = base64,
            Units = units,
            PaddingCount = paddingCount,
            OriginalByteCount = Encoding.UTF8.GetByteCount(text),
            EncodedByteCount = Encoding.ASCII.GetByteCount(base64)
        };
    }

    private static string RestoreText(List<int> units, TextEncodingMode mode, int paddingCount)
    {
        return mode switch
        {
            TextEncodingMode.Ascii => new string(units.Select(v => (char)v).ToArray()),
            TextEncodingMode.Base64 => RestoreBase64(units, paddingCount),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    private static string RestoreBase64(List<int> units, int paddingCount)
    {
        string base64WithoutPadding = new string(units.Select(v => Base64Alphabet[v]).ToArray());
        string base64 = base64WithoutPadding + new string('=', paddingCount);
        byte[] bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    private static List<BigInteger> Encrypt(List<int> units, RsaKeyPair keys)
    {
        List<BigInteger> cipher = new();

        foreach (int unit in units)
        {
            BigInteger m = unit;
            BigInteger c = BigInteger.ModPow(m, keys.E, keys.N);
            cipher.Add(c);
        }

        return cipher;
    }

    private static List<int> Decrypt(List<BigInteger> cipherBlocks, RsaKeyPair keys)
    {
        List<int> units = new();

        foreach (BigInteger c in cipherBlocks)
        {
            BigInteger m = BigInteger.ModPow(c, keys.D, keys.N);
            units.Add((int)m);
        }

        return units;
    }

    private static RsaKeyPair GenerateRsaKeyPair(int keySizeBits)
    {
        BigInteger e = 65537;

        while (true)
        {
            BigInteger p = GenerateProbablePrime(keySizeBits / 2);
            BigInteger q = GenerateProbablePrime(keySizeBits / 2);

            if (p == q)
            {
                continue;
            }

            BigInteger n = p * q;
            BigInteger phi = (p - 1) * (q - 1);

            if (BigInteger.GreatestCommonDivisor(e, phi) != 1)
            {
                continue;
            }

            BigInteger d = ModInverse(e, phi);

            return new RsaKeyPair
            {
                P = p,
                Q = q,
                N = n,
                Phi = phi,
                E = e,
                D = d
            };
        }
    }

    private static BigInteger GenerateProbablePrime(int bitLength)
    {
        while (true)
        {
            BigInteger candidate = GenerateRandomOddBigInteger(bitLength);

            if (IsProbablePrime(candidate, 20))
            {
                return candidate;
            }
        }
    }

    private static bool IsProbablePrime(BigInteger n, int rounds)
    {
        if (n < 2) return false;
        if (n == 2 || n == 3) return true;
        if (n % 2 == 0) return false;

        BigInteger d = n - 1;
        int s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s++;
        }

        for (int i = 0; i < rounds; i++)
        {
            BigInteger a = RandomInRange(2, n - 2);
            BigInteger x = BigInteger.ModPow(a, d, n);

            if (x == 1 || x == n - 1)
            {
                continue;
            }

            bool witnessFound = true;

            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, n);

                if (x == n - 1)
                {
                    witnessFound = false;
                    break;
                }
            }

            if (witnessFound)
            {
                return false;
            }
        }

        return true;
    }

    private static BigInteger ModInverse(BigInteger a, BigInteger mod)
    {
        BigInteger t = 0;
        BigInteger newT = 1;
        BigInteger r = mod;
        BigInteger newR = a;

        while (newR != 0)
        {
            BigInteger q = r / newR;
            (t, newT) = (newT, t - q * newT);
            (r, newR) = (newR, r - q * newR);
        }

        if (r > 1)
        {
            throw new InvalidOperationException("Обратный элемент не существует.");
        }

        if (t < 0)
        {
            t += mod;
        }

        return t;
    }

    private static BigInteger GenerateRandomOddBigInteger(int bitLength)
    {
        int byteCount = (bitLength + 7) / 8;
        byte[] bytes = new byte[byteCount];

        RandomNumberGenerator.Fill(bytes);

        int excessBits = byteCount * 8 - bitLength;
        bytes[0] &= (byte)(0xFF >> excessBits);
        bytes[0] |= (byte)(1 << (7 - excessBits));
        bytes[^1] |= 1;

        return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
    }

    private static BigInteger RandomInRange(BigInteger minInclusive, BigInteger maxInclusive)
    {
        if (minInclusive > maxInclusive)
        {
            throw new ArgumentException("Некорректный диапазон.");
        }

        BigInteger range = maxInclusive - minInclusive + 1;
        int byteCount = range.ToByteArray(isUnsigned: true, isBigEndian: true).Length;
        byte[] buffer = new byte[byteCount];

        while (true)
        {
            RandomNumberGenerator.Fill(buffer);
            BigInteger candidate = new BigInteger(buffer, isUnsigned: true, isBigEndian: true);

            if (candidate < range)
            {
                return minInclusive + candidate;
            }
        }
    }

    private static int GetUnsignedByteCount(BigInteger value)
    {
        return value.ToByteArray(isUnsigned: true, isBigEndian: true).Length;
    }
}