using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace ElGamalTextCryptoDemo;

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

internal sealed class ElGamalKeyPair
{
    public required BigInteger P { get; init; }
    public required BigInteger G { get; init; }
    public required BigInteger X { get; init; }
    public required BigInteger Y { get; init; }
}

internal sealed class ElGamalCipherBlock
{
    public required BigInteger A { get; init; }
    public required BigInteger B { get; init; }
}

internal static class Program
{
    private const int KeySizeBits = 256;
    private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        string fio = "Kucheruk Nikolay Petrovich";

        Console.WriteLine("Эль-Гамаль: шифрование/расшифрование текста");
        Console.WriteLine(new string('=', 90));
        Console.WriteLine($"Исходный текст: {fio}");
        Console.WriteLine($"Размер ключа p: {KeySizeBits} бит");
        Console.WriteLine();

        Console.WriteLine("Генерация параметров Эль-Гамаля...");
        ElGamalKeyPair keys = GenerateElGamalKeyPair(KeySizeBits);

        Console.WriteLine("Параметры сгенерированы.");
        Console.WriteLine($"p = {keys.P}");
        Console.WriteLine($"g = {keys.G}");
        Console.WriteLine($"y = {keys.Y}");
        Console.WriteLine($"x = {keys.X}");
        Console.WriteLine();

        RunScenario(fio, TextEncodingMode.Ascii, keys);
        RunScenario(fio, TextEncodingMode.Base64, keys);

        Console.WriteLine();
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    private static void RunScenario(string text, TextEncodingMode mode, ElGamalKeyPair keys)
    {
        Console.WriteLine(new string('-', 90));
        Console.WriteLine($"Режим: {mode}");
        Console.WriteLine(new string('-', 90));

        PreparedText prepared = PrepareText(text, mode);

        Console.WriteLine($"Подготовленный текст: {prepared.EncodedText}");
        Console.WriteLine($"Числовых блоков: {prepared.Units.Count}");

        Stopwatch encSw = Stopwatch.StartNew();
        List<ElGamalCipherBlock> cipherBlocks = Encrypt(prepared.Units, keys);
        encSw.Stop();

        Stopwatch decSw = Stopwatch.StartNew();
        List<int> restoredUnits = Decrypt(cipherBlocks, keys);
        string restoredText = RestoreText(restoredUnits, mode, prepared.PaddingCount);
        decSw.Stop();

        int pBytes = GetUnsignedByteCount(keys.P);
        long cipherBytes = (long)cipherBlocks.Count * 2 * pBytes;

        Console.WriteLine($"Расшифрованный текст: {restoredText}");
        Console.WriteLine($"Время шифрования: {encSw.Elapsed.TotalMilliseconds:F6} мс");
        Console.WriteLine($"Время расшифрования: {decSw.Elapsed.TotalMilliseconds:F6} мс");
        Console.WriteLine($"Размер исходного текста: {prepared.OriginalByteCount} байт");
        Console.WriteLine($"Размер представления ({mode}): {prepared.EncodedByteCount} байт");
        Console.WriteLine($"Оценочный размер шифртекста: {cipherBytes} байт");
        Console.WriteLine($"Рост относительно исходного текста: {(double)cipherBytes / prepared.OriginalByteCount:F2}x");
        Console.WriteLine($"Рост относительно представления {mode}: {(double)cipherBytes / prepared.EncodedByteCount:F2}x");
        Console.WriteLine();

        Console.WriteLine("Первые шифрблоки (a, b):");
        foreach (ElGamalCipherBlock block in cipherBlocks.Take(5))
        {
            Console.WriteLine($"({block.A}, {block.B})");
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

    private static List<ElGamalCipherBlock> Encrypt(List<int> units, ElGamalKeyPair keys)
    {
        List<ElGamalCipherBlock> cipher = new();

        foreach (int unit in units)
        {
            BigInteger m = unit;
            BigInteger k = RandomInRange(2, keys.P - 2);

            BigInteger a = BigInteger.ModPow(keys.G, k, keys.P);
            BigInteger b = (BigInteger.ModPow(keys.Y, k, keys.P) * m) % keys.P;

            cipher.Add(new ElGamalCipherBlock
            {
                A = a,
                B = b
            });
        }

        return cipher;
    }

    private static List<int> Decrypt(List<ElGamalCipherBlock> cipherBlocks, ElGamalKeyPair keys)
    {
        List<int> units = new();

        foreach (ElGamalCipherBlock block in cipherBlocks)
        {
            BigInteger s = BigInteger.ModPow(block.A, keys.X, keys.P);
            BigInteger sInverse = BigInteger.ModPow(s, keys.P - 2, keys.P);
            BigInteger m = (block.B * sInverse) % keys.P;
            units.Add((int)m);
        }

        return units;
    }

    private static ElGamalKeyPair GenerateElGamalKeyPair(int bitLength)
    {
        while (true)
        {
            BigInteger q = GenerateProbablePrime(bitLength - 1);
            BigInteger p = 2 * q + 1;

            if (!IsProbablePrime(p, 20))
            {
                continue;
            }

            BigInteger g = FindPrimitiveRootForSafePrime(p, q);
            BigInteger x = RandomInRange(2, p - 2);
            BigInteger y = BigInteger.ModPow(g, x, p);

            return new ElGamalKeyPair
            {
                P = p,
                G = g,
                X = x,
                Y = y
            };
        }
    }

    private static BigInteger FindPrimitiveRootForSafePrime(BigInteger p, BigInteger q)
    {
        while (true)
        {
            BigInteger g = RandomInRange(2, p - 2);

            if (BigInteger.ModPow(g, 2, p) == 1)
            {
                continue;
            }

            if (BigInteger.ModPow(g, q, p) == 1)
            {
                continue;
            }

            return g;
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