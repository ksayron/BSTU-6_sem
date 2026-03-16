using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Lab_Asymptotic_ModPow;

internal sealed class MeasurementResult
{
    public int A { get; init; }
    public BigInteger X { get; init; }
    public int NBitLength { get; init; }
    public BigInteger N { get; init; }
    public BigInteger Y { get; init; }
    public double ElapsedMilliseconds { get; init; }
}

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        int[] aValues = { 5, 17, 35 };
        int[] nBitLengths = { 1024, 2048 };

        List<BigInteger> xValues = GenerateExponentValues(count: 8);

        Console.WriteLine("Исследование зависимости времени вычисления y = a^x mod n");
        Console.WriteLine(new string('=', 90));
        Console.WriteLine($"Значения a: {string.Join(", ", aValues)}");
        Console.WriteLine($"Количество значений x: {xValues.Count}");
        Console.WriteLine($"Размеры n (в битах): {string.Join(", ", nBitLengths)}");
        Console.WriteLine();

        Console.WriteLine("Используемые значения x:");
        foreach (BigInteger x in xValues)
        {
            Console.WriteLine(x);
        }

        Console.WriteLine();
        Console.WriteLine("Генерация модулей n...");
        Dictionary<int, BigInteger> moduli = new();

        foreach (int bits in nBitLengths)
        {
            BigInteger n = GenerateRandomOddBigInteger(bits);
            moduli[bits] = n;
            Console.WriteLine($"n ({bits} бит) сгенерировано.");
        }

        Console.WriteLine();
        Console.WriteLine("Выполняются измерения...");
        List<MeasurementResult> results = new();

        foreach (int bitLength in nBitLengths)
        {
            BigInteger n = moduli[bitLength];

            foreach (int a in aValues)
            {
                foreach (BigInteger x in xValues)
                {
                    MeasurementResult result = MeasureModPow(a, x, n, bitLength);
                    results.Add(result);

                    Console.WriteLine(
                        $"a={a,-3} | bits(n)={bitLength,-4} | x≈10^{GetApproxPowerOfTen(x),-3} | " +
                        $"time={result.ElapsedMilliseconds:F6} ms");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine(new string('=', 90));
        PrintResultsTable(results);

        string csvPath = Path.Combine(AppContext.BaseDirectory, "modpow_results.csv");
        SaveResultsToCsv(results, csvPath);

        Console.WriteLine();
        Console.WriteLine($"CSV-файл сохранен: {csvPath}");
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    private static MeasurementResult MeasureModPow(int a, BigInteger x, BigInteger n, int nBitLength)
    {
        BigInteger y;
        Stopwatch sw = Stopwatch.StartNew();
        y = BigInteger.ModPow(new BigInteger(a), x, n);
        sw.Stop();

        return new MeasurementResult
        {
            A = a,
            X = x,
            NBitLength = nBitLength,
            N = n,
            Y = y,
            ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds
        };
    }

    private static List<BigInteger> GenerateExponentValues(int count)
    {
        if (count < 2)
        {
            throw new ArgumentException("Количество значений должно быть не меньше 2.");
        }

        const int minPower = 3;
        const int maxPower = 100;

        List<BigInteger> values = new();

        for (int i = 0; i < count; i++)
        {
            double t = (double)i / (count - 1);
            double power = minPower + t * (maxPower - minPower);
            int roundedPower = (int)Math.Round(power);

            BigInteger x = Pow10(roundedPower);

            if (!IsProbablyPrimeSmallRounded(roundedPower))
            {
                values.Add(x + 39);
            }
            else
            {
                values.Add(x);
            }
        }

        return values
            .Distinct()
            .OrderBy(v => v)
            .ToList();
    }

    private static bool IsProbablyPrimeSmallRounded(int n)
    {
        if (n < 2) return false;
        if (n == 2 || n == 3) return true;
        if (n % 2 == 0) return false;

        int limit = (int)Math.Sqrt(n);
        for (int i = 3; i <= limit; i += 2)
        {
            if (n % i == 0) return false;
        }

        return true;
    }

    private static BigInteger Pow10(int exponent)
    {
        BigInteger result = BigInteger.One;
        BigInteger ten = new BigInteger(10);

        for (int i = 0; i < exponent; i++)
        {
            result *= ten;
        }

        return result;
    }

    private static BigInteger GenerateRandomOddBigInteger(int bitLength)
    {
        if (bitLength < 2)
        {
            throw new ArgumentException("Длина числа в битах должна быть не меньше 2.");
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

        bytes[0] |= 0b00000001;
        bytes[^1] = 0;

        return new BigInteger(bytes);
    }

    private static int GetApproxPowerOfTen(BigInteger value)
    {
        string s = value.ToString();
        return s.Length - 1;
    }

    private static void PrintResultsTable(List<MeasurementResult> results)
    {
        Console.WriteLine("Таблица результатов");
        Console.WriteLine(new string('-', 90));
        Console.WriteLine(
            $"{"a",-5}{"x",-22}{"bits(n)",-10}{"time, ms",-15}{"y mod n (первые 25 цифр)",-30}");
        Console.WriteLine(new string('-', 90));

        foreach (MeasurementResult result in results
                     .OrderBy(r => r.NBitLength)
                     .ThenBy(r => r.A)
                     .ThenBy(r => r.X))
        {
            string xShort = ShortenNumber(result.X, 20);
            string yShort = ShortenNumber(result.Y, 25);

            Console.WriteLine(
                $"{result.A,-5}{xShort,-22}{result.NBitLength,-10}{result.ElapsedMilliseconds,-15:F6}{yShort,-30}");
        }
    }

    private static string ShortenNumber(BigInteger value, int maxLength)
    {
        string s = value.ToString();
        if (s.Length <= maxLength)
        {
            return s;
        }

        return s.Substring(0, maxLength - 3) + "...";
    }

    private static void SaveResultsToCsv(List<MeasurementResult> results, string path)
    {
        using StreamWriter writer = new(path, false, System.Text.Encoding.UTF8);

        writer.WriteLine("a;x;n_bit_length;time_ms;y");

        foreach (MeasurementResult result in results
                     .OrderBy(r => r.NBitLength)
                     .ThenBy(r => r.A)
                     .ThenBy(r => r.X))
        {
            writer.WriteLine(
                $"{result.A};{result.X};{result.NBitLength};" +
                $"{result.ElapsedMilliseconds.ToString("F6", CultureInfo.InvariantCulture)};{result.Y}");
        }
    }
}