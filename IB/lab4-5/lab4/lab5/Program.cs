using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace PermutationCryptoBelarus
{
   

    class Program
    {
        // Параметры таблицы для маршрутной перестановки
        private const int RouteRows = 500;   // можно поменять по указанию преподавателя
        private const int RouteCols = 500;   // RouteRows * RouteCols >= длина текста

        static void Main()
        {
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

            string inputPath = "input_perm.txt";
            string nameKey = "Nikolay";          // Ключ 1
            string surnameKey = "Pupkin";      // Ключ 2

            // Если файла нет, генерируем длинный тестовый текст для матрицы 500x500
            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Создание тестового файла...");
                string dummyText = string.Concat(Enumerable.Repeat("Hello World! This is a test for zigzag and multiple transposition ciphers. ", 4000));
                File.WriteAllText(inputPath, dummyText, Encoding.UTF8);
            }

            string text = File.ReadAllText(inputPath, Encoding.UTF8);

            // -------- Маршрутная перестановка (Зигзаг) --------
            var (routeEnc, tRouteEnc) = Measure(s => RouteTransposition.Encrypt(s, RouteRows, RouteCols), text);
            var (routeDec, tRouteDec) = Measure(s => RouteTransposition.Decrypt(s, RouteRows, RouteCols), routeEnc);

            File.WriteAllText("route_enc.txt", routeEnc, Encoding.UTF8);
            File.WriteAllText("route_dec.txt", routeDec, Encoding.UTF8);

            // -------- Множественная перестановка --------
            int[] key1 = MultipleTransposition.BuildKeyFromWord(nameKey);
            int[] key2 = MultipleTransposition.BuildKeyFromWord(surnameKey);

            var (multiEnc, tMultiEnc) = Measure(s => MultipleTransposition.Encrypt(s, key1, key2), text);
            var (multiDec, tMultiDec) = Measure(s => MultipleTransposition.Decrypt(s, key1, key2), multiEnc);

            File.WriteAllText("multi_enc.txt", multiEnc, Encoding.UTF8);
            File.WriteAllText("multi_dec.txt", multiDec, Encoding.UTF8);

            // -------- Частоты --------
            var freqOriginal = FrequencyAnalyzer.CountFrequencies(text);
            var freqRoute = FrequencyAnalyzer.CountFrequencies(routeEnc);
            var freqMulti = FrequencyAnalyzer.CountFrequencies(multiEnc);

            CreateExcelHistogram("perm_histograms.xlsx", freqOriginal, freqRoute, freqMulti);

            Console.WriteLine("Готово.");
            Console.WriteLine($"Route (ZigZag): encrypt {tRouteEnc} ms, decrypt {tRouteDec} ms");
            Console.WriteLine($"Multiple: encrypt {tMultiEnc} ms, decrypt {tMultiDec} ms");
            Console.ReadKey();
        }

        // -------------------- Timing --------------------
        public static (string result, long ms) Measure(Func<string, string> func, string input)
        {
            var sw = Stopwatch.StartNew();
            string res = func(input);
            sw.Stop();
            return (res, sw.ElapsedMilliseconds);
        }

        // -------------------- Excel Histogram --------------------
        public static void CreateExcelHistogram(
            string fileName,
            Dictionary<char, int> orig,
            Dictionary<char, int> route,
            Dictionary<char, int> multi)
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Frequencies");

            ws.Cells[1, 1].Value = "Letter";
            ws.Cells[1, 2].Value = "Original";
            ws.Cells[1, 3].Value = "Route (ZigZag) Encrypted";
            ws.Cells[1, 4].Value = "Multiple Encrypted";
            ws.Cells[1, 5].Value = "Route (ZigZag) Decrypted";
            ws.Cells[1, 6].Value = "Multiple Decrypted";

            int row = 2;
            foreach (var c in EnglishAlphabet.Letters)
            {
                ws.Cells[row, 1].Value = c.ToString();
                ws.Cells[row, 2].Value = orig[c];
                ws.Cells[row, 3].Value = route[c];
                ws.Cells[row, 4].Value = multi[c];
                ws.Cells[row, 5].Value = orig[c];
                ws.Cells[row, 6].Value = orig[c];
                row++;
            }

            var chart = ws.Drawings.AddChart("hist", eChartType.ColumnClustered) as ExcelBarChart;
            chart.Title.Text = "Частоты символов (перестановочные шифры)";
            chart.SetPosition(0, 0, 10, 0);
            chart.SetSize(900, 600);

            var rangeLetters = ws.Cells[2, 1, row - 1, 1];
            chart.Series.Add(ws.Cells[2, 2, row - 1, 2], rangeLetters).Header = "Original";
            chart.Series.Add(ws.Cells[2, 3, row - 1, 3], rangeLetters).Header = "Route (ZigZag) Encrypted";
            chart.Series.Add(ws.Cells[2, 4, row - 1, 4], rangeLetters).Header = "Multiple Encrypted";
            chart.Series.Add(ws.Cells[2, 5, row - 1, 5], rangeLetters).Header = "Route (ZigZag) Decrypted";
            chart.Series.Add(ws.Cells[2, 6, row - 1, 6], rangeLetters).Header = "Multiple Decrypted";

            package.SaveAs(new FileInfo(fileName));
        }
    }

    // -------------------- English Alphabet --------------------
    public static class EnglishAlphabet
    {
        public static readonly char[] Letters =
        {
        'a','b','c','d','e','f','g','h','i','j','k','l','m',
        'n','o','p','q','r','s','t','u','v','w','x','y','z'
    };

        public static int IndexOf(char c)
        {
            c = char.ToLower(c);
            for (int i = 0; i < Letters.Length; i++)
                if (Letters[i] == c) return i;
            return -1;
        }
    }

    // -------------------- Frequency Analyzer --------------------
    public static class FrequencyAnalyzer
    {
        public static Dictionary<char, int> CountFrequencies(string text)
        {
            var dict = EnglishAlphabet.Letters.ToDictionary(c => c, c => 0);

            foreach (char c in text)
            {
                int idx = EnglishAlphabet.IndexOf(c);
                if (idx != -1)
                {
                    char key = EnglishAlphabet.Letters[idx];
                    dict[key]++;
                }
            }

            return dict;
        }
    }

    // -------------------- Route Transposition (ZIG-ZAG) --------------------
    public static class RouteTransposition
    {
        // Запись построчно (слева направо), чтение по столбцам ЗИГЗАГОМ (вниз, вверх, вниз...)
        public static string Encrypt(string input, int rows, int cols)
        {
            int size = rows * cols;
            if (input.Length < size)
                input = input.PadRight(size, ' ');
            else if (input.Length > size)
                input = input.Substring(0, size);

            char[,] table = new char[rows, cols];
            int idx = 0;

            // 1. Запись по строкам
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    table[r, c] = input[idx++];

            // 2. Чтение ЗИГЗАГОМ по столбцам
            var sb = new StringBuilder(size);
            for (int c = 0; c < cols; c++)
            {
                if (c % 2 == 0)
                {
                    // Четный столбец: читаем сверху вниз
                    for (int r = 0; r < rows; r++) sb.Append(table[r, c]);
                }
                else
                {
                    // Нечетный столбец: читаем снизу вверх
                    for (int r = rows - 1; r >= 0; r--) sb.Append(table[r, c]);
                }
            }

            return sb.ToString();
        }

        public static string Decrypt(string input, int rows, int cols)
        {
            int size = rows * cols;
            if (input.Length < size)
                input = input.PadRight(size, ' ');
            else if (input.Length > size)
                input = input.Substring(0, size);

            char[,] table = new char[rows, cols];
            int idx = 0;

            // 1. Запись ЗИГЗАГОМ по столбцам (восстанавливаем таблицу)
            for (int c = 0; c < cols; c++)
            {
                if (c % 2 == 0)
                {
                    // Четный столбец: пишем сверху вниз
                    for (int r = 0; r < rows; r++) table[r, c] = input[idx++];
                }
                else
                {
                    // Нечетный столбец: пишем снизу вверх
                    for (int r = rows - 1; r >= 0; r--) table[r, c] = input[idx++];
                }
            }

            // 2. Чтение по строкам (слева направо)
            var sb = new StringBuilder(size);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    sb.Append(table[r, c]);

            return sb.ToString();
        }
    }

    // -------------------- Multiple Transposition --------------------
    public static class MultipleTransposition
    {
        // Вспомогательные методы для поиска Наименьшего общего кратного (НОК)
        private static int Gcd(int a, int b)
        {
            while (b != 0) { int t = b; b = a % b; a = t; }
            return a;
        }

        private static int Lcm(int a, int b)
        {
            return (a / Gcd(a, b)) * b;
        }

        // Построение числового ключа из слова
        public static int[] BuildKeyFromWord(string word)
        {
            word = word.ToLower().Replace(" ", "");
            int n = word.Length;
            var chars = word.ToCharArray();

            var indexed = chars
                .Select((ch, i) => new { Char = ch, Index = i })
                .OrderBy(x => x.Char)
                .ThenBy(x => x.Index)
                .ToList();

            int[] key = new int[n];
            int current = 1;
            foreach (var item in indexed)
            {
                key[item.Index] = current;
                current++;
            }

            return key;
        }

        public static string SingleEncrypt(string input, int[] key)
        {
            int kLen = key.Length;
            int remainder = input.Length % kLen;

            if (remainder != 0)
            {
                int pad = kLen - remainder;
                input = input.PadRight(input.Length + pad, ' ');
            }

            var result = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i += kLen)
            {
                char[] transposition = new char[kLen];
                for (int j = 0; j < kLen; j++)
                    transposition[key[j] - 1] = input[i + j];

                for (int j = 0; j < kLen; j++)
                    result.Append(transposition[j]);
            }

            return result.ToString();
        }

        public static string SingleDecrypt(string input, int[] key)
        {
            int kLen = key.Length;
            var result = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i += kLen)
            {
                char[] transposition = new char[kLen];
                for (int j = 0; j < kLen; j++)
                    transposition[j] = input[i + key[j] - 1];

                for (int j = 0; j < kLen; j++)
                    result.Append(transposition[j]);
            }

            return result.ToString();
        }

        // Множественная перестановка
        public static string Encrypt(string input, int[] key1, int[] key2)
        {
            // ИСПРАВЛЕНИЕ: Дополняем входную строку пробелами до Наименьшего Общего Кратного 
            // длин обоих ключей, чтобы при двойном шифровании длина не сбивалась.
            int lcm = Lcm(key1.Length, key2.Length);
            int remainder = input.Length % lcm;
            if (remainder != 0)
            {
                input = input.PadRight(input.Length + (lcm - remainder), ' ');
            }

            string step1 = SingleEncrypt(input, key1);
            string step2 = SingleEncrypt(step1, key2);
            return step2;
        }

        public static string Decrypt(string input, int[] key1, int[] key2)
        {
            string step1 = SingleDecrypt(input, key2); // Сначала снимаем ключ 2
            string step2 = SingleDecrypt(step1, key1); // Затем снимаем ключ 1
            return step2;
        }
    }
}

