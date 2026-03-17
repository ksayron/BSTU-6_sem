using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace HashFunctionResearch
{
    internal class Program
    {
        private const int IterationsCount = 1_000_000;

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Исследование криптографической хеш-функции SHA-256");
            Console.WriteLine("--------------------------------------------------");

            Console.Write("Введите сообщение для хеширования: ");
            string message = Console.ReadLine() ?? string.Empty;

            Console.WriteLine();

            string hash = ComputeSha256Hash(message);

            Console.WriteLine("Исходное сообщение:");
            Console.WriteLine(message);

            Console.WriteLine();

            Console.WriteLine("Хеш SHA-256:");
            Console.WriteLine(hash);

            Console.WriteLine();

            Console.WriteLine($"Длина хеша: {hash.Length * 4} бит");

            Console.WriteLine();

            MeasureHashingSpeed(message);

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для завершения...");
            Console.ReadKey();
        }

        private static string ComputeSha256Hash(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using SHA256 sha256 = SHA256.Create();

            byte[] hashBytes = sha256.ComputeHash(messageBytes);

            return ConvertToHex(hashBytes);
        }

        private static string ConvertToHex(byte[] bytes)
        {
            StringBuilder result = new StringBuilder();

            foreach (byte b in bytes)
            {
                result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }

        private static void MeasureHashingSpeed(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using SHA256 sha256 = SHA256.Create();

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < IterationsCount; i++)
            {
                sha256.ComputeHash(messageBytes);
            }

            stopwatch.Stop();

            double totalSeconds = stopwatch.Elapsed.TotalSeconds;
            double hashesPerSecond = IterationsCount / totalSeconds;
            double averageTimeMicroseconds = stopwatch.Elapsed.TotalMilliseconds * 1000 / IterationsCount;

            Console.WriteLine("Оценка быстродействия алгоритма SHA-256");
            Console.WriteLine($"Количество вычислений хеша: {IterationsCount:N0}");
            Console.WriteLine($"Общее время выполнения: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"Скорость вычисления: {hashesPerSecond:N0} хешей/сек");
            Console.WriteLine($"Среднее время одного вычисления: {averageTimeMicroseconds:F4} мкс");
        }
    }
}