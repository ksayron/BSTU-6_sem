using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
namespace AssymeticEncryption;
internal enum EncodingMode
{
    Ascii,
    Base64
}

internal sealed class PrivateKey
{
    public required List<BigInteger> SuperIncreasingSequence { get; init; }
    public required BigInteger A { get; init; }
    public required BigInteger N { get; init; }
    public required BigInteger AInverse { get; init; }
}

internal sealed class PublicKey
{
    public required List<BigInteger> Sequence { get; init; }
}

internal sealed class KeyPair
{
    public required PrivateKey PrivateKey { get; init; }
    public required PublicKey PublicKey { get; init; }
}

internal sealed class EncryptionResult
{
    public required List<BigInteger> CipherBlocks { get; init; }
    public required string PreparedText { get; init; }
    public required List<string> BinaryBlocks { get; init; }
}

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        string fio = "Kucheruk Nikolay Petrovich";

        Console.WriteLine(new string('-', 70));
        Console.WriteLine($"Сообщение: {fio}");
        Console.WriteLine();

        RunSingleDemo(fio, EncodingMode.Ascii, 8);
        RunSingleDemo(fio, EncodingMode.Base64, 6);

        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("Анализ времени при увеличении числа членов ключевой последовательности");
        Console.WriteLine(new string('=', 70));

        RunPerformanceStudy(fio, EncodingMode.Ascii, new[] { 8, 16, 24, 32 }, iterations: 100);
        Console.WriteLine();
        RunPerformanceStudy(fio, EncodingMode.Base64, new[] { 6, 12, 18, 24 }, iterations: 100);

        Console.WriteLine();
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    private static void RunSingleDemo(string message, EncodingMode mode, int z)
    {
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"РЕЖИМ: {mode}, z = {z}");
        Console.WriteLine(new string('=', 70));

        ValidateBlockSize(mode, z);

        KeyPair keys = GenerateKeyPair(z, highestElementBitLength: 100);

        Console.WriteLine("Закрытый ключ (сверхвозрастающая последовательность):");
        PrintSequence(keys.PrivateKey.SuperIncreasingSequence);

        Console.WriteLine($"n = {keys.PrivateKey.N}");
        Console.WriteLine($"a = {keys.PrivateKey.A}");
        Console.WriteLine($"a^(-1) mod n = {keys.PrivateKey.AInverse}");
        Console.WriteLine();

        Console.WriteLine("Открытый ключ:");
        PrintSequence(keys.PublicKey.Sequence);

        Stopwatch encryptSw = Stopwatch.StartNew();
        EncryptionResult encrypted = Encrypt(message, keys.PublicKey, mode, z);
        encryptSw.Stop();

        Stopwatch decryptSw = Stopwatch.StartNew();
        string decrypted = Decrypt(encrypted.CipherBlocks, keys.PrivateKey, mode, z);
        decryptSw.Stop();

        Console.WriteLine();
        Console.WriteLine($"Подготовленный текст для шифрования: {encrypted.PreparedText}");
        Console.WriteLine("Бинарные блоки:");
        foreach (string block in encrypted.BinaryBlocks)
        {
            Console.WriteLine(block);
        }

        Console.WriteLine();
        Console.WriteLine("Шифртекст:");
        for (int i = 0; i < encrypted.CipherBlocks.Count; i++)
        {
            Console.WriteLine($"C[{i}] = {encrypted.CipherBlocks[i]}");
        }

        Console.WriteLine();
        Console.WriteLine($"Расшифрованное сообщение: {decrypted}");
        Console.WriteLine($"Время шифрования: {encryptSw.Elapsed.TotalMilliseconds:F4} мс");
        Console.WriteLine($"Время расшифрования: {decryptSw.Elapsed.TotalMilliseconds:F4} мс");
        Console.WriteLine();
    }

    private static void RunPerformanceStudy(string message, EncodingMode mode, int[] zValues, int iterations)
    {
        Console.WriteLine($"Сравнение для режима {mode}");
        Console.WriteLine(new string('-', 70));
        Console.WriteLine($"{"z",-8} {"Блоков",-10} {"Шифрование, мс",-20} {"Расшифрование, мс",-20}");

        foreach (int z in zValues)
        {
            ValidateBlockSize(mode, z);

            KeyPair keys = GenerateKeyPair(z, highestElementBitLength: 100);

            double encryptTotal = 0.0;
            double decryptTotal = 0.0;
            int blockCount = 0;

            for (int i = 0; i < iterations; i++)
            {
                Stopwatch encSw = Stopwatch.StartNew();
                EncryptionResult encrypted = Encrypt(message, keys.PublicKey, mode, z);
                encSw.Stop();

                Stopwatch decSw = Stopwatch.StartNew();
                string decrypted = Decrypt(encrypted.CipherBlocks, keys.PrivateKey, mode, z);
                decSw.Stop();

                if (decrypted.TrimEnd('\0') != message)
                {
                    throw new InvalidOperationException("Ошибка проверки: расшифрованный текст не совпал с исходным.");
                }

                encryptTotal += encSw.Elapsed.TotalMilliseconds;
                decryptTotal += decSw.Elapsed.TotalMilliseconds;
                blockCount = encrypted.CipherBlocks.Count;
            }

            Console.WriteLine(
                $"{z,-8} {blockCount,-10} {(encryptTotal / iterations),-20:F6} {(decryptTotal / iterations),-20:F6}");
        }
    }

    private static void ValidateBlockSize(EncodingMode mode, int z)
    {
        int unitSize = mode == EncodingMode.Ascii ? 8 : 6;

        if (z <= 0 || z % unitSize != 0)
        {
            throw new ArgumentException(
                $"Для режима {mode} значение z должно быть кратно {unitSize}. Получено: {z}");
        }
    }

    private static KeyPair GenerateKeyPair(int z, int highestElementBitLength)
    {
        List<BigInteger> d = GenerateSuperIncreasingSequence(z, highestElementBitLength);

        BigInteger sum = d.Aggregate(BigInteger.Zero, (acc, x) => acc + x);
        BigInteger n = GenerateRandomGreaterThan(sum);
        BigInteger a = GenerateCoprime(n);
        BigInteger aInverse = ModInverse(a, n);

        List<BigInteger> e = d.Select(di => Mod(di * a, n)).ToList();

        return new KeyPair
        {
            PrivateKey = new PrivateKey
            {
                SuperIncreasingSequence = d,
                A = a,
                N = n,
                AInverse = aInverse
            },
            PublicKey = new PublicKey
            {
                Sequence = e
            }
        };
    }

    private static List<BigInteger> GenerateSuperIncreasingSequence(int z, int highestElementBitLength)
    {
        if (z < 2)
        {
            throw new ArgumentException("z должно быть не меньше 2.");
        }

        List<BigInteger> sequence = new();
        BigInteger sum = BigInteger.Zero;

        for (int i = 0; i < z - 1; i++)
        {
            BigInteger next;

            if (i == 0)
            {
                next = RandomBigIntegerWithBitLength(16);
            }
            else
            {
                BigInteger min = sum + 1;
                BigInteger extra = RandomBigIntegerWithBitLength(Math.Min(24 + i * 2, 40));
                next = min + extra;
            }

            sequence.Add(next);
            sum += next;
        }

        BigInteger lastMin = sum + 1;
        BigInteger last = RandomBigIntegerWithBitLength(highestElementBitLength);

        if (last <= lastMin)
        {
            last = lastMin + RandomBigIntegerWithBitLength(32);
        }

        sequence.Add(last);

        if (!IsSuperIncreasing(sequence))
        {
            throw new InvalidOperationException("Не удалось построить сверхвозрастающую последовательность.");
        }

        return sequence;
    }

    private static bool IsSuperIncreasing(List<BigInteger> sequence)
    {
        BigInteger sum = BigInteger.Zero;

        foreach (BigInteger value in sequence)
        {
            if (value <= sum)
            {
                return false;
            }

            sum += value;
        }

        return true;
    }

    private static EncryptionResult Encrypt(string message, PublicKey publicKey, EncodingMode mode, int z)
    {
        string prepared = PrepareMessageForMode(message, mode);
        List<string> blocks = ConvertToBinaryBlocks(prepared, mode, z);

        List<BigInteger> cipher = new();

        foreach (string block in blocks)
        {
            BigInteger sum = BigInteger.Zero;
            for (int i = 0; i < z; i++)
            {
                if (block[i] == '1')
                {
                    sum += publicKey.Sequence[i];
                }
            }

            cipher.Add(sum);
        }

        return new EncryptionResult
        {
            CipherBlocks = cipher,
            PreparedText = prepared,
            BinaryBlocks = blocks
        };
    }

    private static string Decrypt(List<BigInteger> cipherBlocks, PrivateKey privateKey, EncodingMode mode, int z)
    {
        List<string> binaryBlocks = new();

        foreach (BigInteger c in cipherBlocks)
        {
            BigInteger s = Mod(c * privateKey.AInverse, privateKey.N);
            string block = SolveSuperIncreasingKnapsack(s, privateKey.SuperIncreasingSequence);
            binaryBlocks.Add(block);
        }

        string binary = string.Concat(binaryBlocks);
        return DecodeBinary(binary, mode);
    }

    private static string SolveSuperIncreasingKnapsack(BigInteger s, List<BigInteger> sequence)
    {
        char[] bits = new char[sequence.Count];

        for (int i = sequence.Count - 1; i >= 0; i--)
        {
            if (sequence[i] <= s)
            {
                bits[i] = '1';
                s -= sequence[i];
            }
            else
            {
                bits[i] = '0';
            }
        }

        if (s != 0)
        {
            throw new InvalidOperationException("Не удалось решить сверхвозрастающую задачу ранца.");
        }

        return new string(bits);
    }

    private static string PrepareMessageForMode(string message, EncodingMode mode)
    {
        switch (mode)
        {
            case EncodingMode.Ascii:
                ValidateAscii(message);
                return message;

            case EncodingMode.Base64:
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(message));

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private static void ValidateAscii(string text)
    {
        if (text.Any(c => c > 127))
        {
            throw new ArgumentException("Для режима ASCII строка должна содержать только ASCII-символы.");
        }
    }

    private static List<string> ConvertToBinaryBlocks(string text, EncodingMode mode, int z)
    {
        int unitSize = mode == EncodingMode.Ascii ? 8 : 6;

        StringBuilder binary = new();

        if (mode == EncodingMode.Ascii)
        {
            foreach (char ch in text)
            {
                binary.Append(Convert.ToString(ch, 2).PadLeft(8, '0'));
            }
        }
        else
        {
            foreach (char ch in text)
            {
                if (ch == '=')
                {
                    continue;
                }

                int value = Base64CharToValue(ch);
                binary.Append(Convert.ToString(value, 2).PadLeft(6, '0'));
            }
        }

        while (binary.Length % z != 0)
        {
            binary.Append('0');
        }

        List<string> blocks = new();
        for (int i = 0; i < binary.Length; i += z)
        {
            blocks.Add(binary.ToString(i, z));
        }

        return blocks;
    }

    private static string DecodeBinary(string binary, EncodingMode mode)
    {
        int unitSize = mode == EncodingMode.Ascii ? 8 : 6;
        List<string> chunks = new();

        for (int i = 0; i + unitSize <= binary.Length; i += unitSize)
        {
            chunks.Add(binary.Substring(i, unitSize));
        }

        if (mode == EncodingMode.Ascii)
        {
            StringBuilder result = new();
            foreach (string chunk in chunks)
            {
                int value = Convert.ToInt32(chunk, 2);
                result.Append((char)value);
            }

            return result.ToString().TrimEnd('\0');
        }
        else
        {
            StringBuilder base64Text = new();
            foreach (string chunk in chunks)
            {
                int value = Convert.ToInt32(chunk, 2);
                base64Text.Append(ValueToBase64Char(value));
            }

            string prepared = base64Text.ToString();

            while (prepared.Length % 4 != 0)
            {
                prepared += "=";
            }

            byte[] bytes = Convert.FromBase64String(prepared);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    private static int Base64CharToValue(char c)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        int index = alphabet.IndexOf(c);
        if (index < 0)
        {
            throw new ArgumentException($"Символ '{c}' не входит в алфавит Base64.");
        }

        return index;
    }

    private static char ValueToBase64Char(int value)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        if (value < 0 || value >= 64)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Значение Base64 должно быть от 0 до 63.");
        }

        return alphabet[value];
    }

    private static BigInteger GenerateRandomGreaterThan(BigInteger minExclusive)
    {
        int bitLength = GetBitLength(minExclusive) + 1;

        while (true)
        {
            BigInteger candidate = RandomBigIntegerWithBitLength(bitLength);
            if (candidate > minExclusive)
            {
                return candidate;
            }
        }
    }

    private static BigInteger GenerateCoprime(BigInteger n)
    {
        while (true)
        {
            BigInteger a = RandomInRange(2, n - 1);
            if (BigInteger.GreatestCommonDivisor(a, n) == 1)
            {
                return a;
            }
        }
    }

    private static BigInteger ModInverse(BigInteger a, BigInteger mod)
    {
        BigInteger t = 0;
        BigInteger newT = 1;
        BigInteger r = mod;
        BigInteger newR = a;

        while (newR != 0)
        {
            BigInteger quotient = r / newR;

            (t, newT) = (newT, t - quotient * newT);
            (r, newR) = (newR, r - quotient * newR);
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

    private static BigInteger Mod(BigInteger value, BigInteger mod)
    {
        BigInteger result = value % mod;
        return result < 0 ? result + mod : result;
    }

    private static BigInteger RandomBigIntegerWithBitLength(int bitLength)
    {
        if (bitLength < 2)
        {
            throw new ArgumentException("Длина в битах должна быть не меньше 2.");
        }

        int byteCount = (bitLength + 7) / 8;
        byte[] bytes = new byte[byteCount + 1];

        RandomNumberGenerator.Fill(bytes.AsSpan(0, byteCount));

        int highestBitIndex = (bitLength - 1) % 8;
        bytes[byteCount - 1] |= (byte)(1 << highestBitIndex);

        for (int i = bitLength; i < byteCount * 8; i++)
        {
            int byteIndex = i / 8;
            int bitIndex = i % 8;
            bytes[byteIndex] &= (byte)~(1 << bitIndex);
        }

        bytes[^1] = 0;
        return new BigInteger(bytes);
    }

    private static BigInteger RandomInRange(BigInteger minInclusive, BigInteger maxInclusive)
    {
        if (minInclusive > maxInclusive)
        {
            throw new ArgumentException("Неверный диапазон генерации.");
        }

        BigInteger range = maxInclusive - minInclusive + 1;
        int bitLength = Math.Max(GetBitLength(range), 2);
        BigInteger candidate;

        do
        {
            candidate = RandomBigIntegerWithBitLength(bitLength);
        }
        while (candidate >= range);

        return minInclusive + candidate;
    }

    private static int GetBitLength(BigInteger value)
    {
        if (value <= 0)
        {
            return 0;
        }

        byte[] bytes = value.ToByteArray();
        byte mostSignificant = bytes[^1];
        int bits = (bytes.Length - 1) * 8;
        int msbBits = 0;

        while (mostSignificant != 0)
        {
            mostSignificant >>= 1;
            msbBits++;
        }

        return bits + msbBits;
    }

    private static void PrintSequence(List<BigInteger> sequence)
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] = {sequence[i]}");
        }

        Console.WriteLine();
    }
}