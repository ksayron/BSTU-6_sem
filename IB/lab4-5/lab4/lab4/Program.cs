using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace CryptoEnglish
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using OfficeOpenXml;
    using OfficeOpenXml.Drawing.Chart;

    class Program
    {
        static void Main()
        {
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");
            
            string inputPath = "input.txt";          

            string surname = "kucheruk";
            int a = 24;
            string name = "nikolay";


            string text = File.ReadAllText(inputPath, Encoding.UTF8);

            var (enc1, tEnc1) = Measure(s => CaesarKeywordCipher.Encrypt(s, surname, a), text);
            var (dec1, tDec1) = Measure(s => CaesarKeywordCipher.Decrypt(s, surname, a), enc1);

            File.WriteAllText("enc_caesar_keyword.txt", enc1, Encoding.UTF8);
            File.WriteAllText("dec_caesar_keyword.txt", dec1, Encoding.UTF8);

            var (enc2, tEnc2) = Measure(s => TrisemusCipher.Encrypt(s, name), text);
            var (dec2, tDec2) = Measure(s => TrisemusCipher.Decrypt(s, name), enc2);

            File.WriteAllText("enc_trisemus.txt", enc2, Encoding.UTF8);
            File.WriteAllText("dec_trisemus.txt", dec2, Encoding.UTF8);


            // --- Frequencies ---
            var freqOriginal = FrequencyAnalyzer.CountFrequencies(text);
            var freqCaesar = FrequencyAnalyzer.CountFrequencies(enc1);
            var freqTrisemus = FrequencyAnalyzer.CountFrequencies(enc2);
            var freqCaeserDec = FrequencyAnalyzer.CountFrequencies(dec1);
            var freqTrisemusDec = FrequencyAnalyzer.CountFrequencies(dec2);

            // --- Excel with histograms ---
            CreateExcelHistogram("histograms.xlsx", freqOriginal, freqCaesar, freqTrisemus, freqCaeserDec, freqTrisemusDec);

            Console.WriteLine("Готово.");
            Console.WriteLine($"Caesar (Keyword): encrypt {tEnc1} ms, decrypt {tDec1} ms");
            Console.WriteLine($"Trisemus: encrypt {tEnc2} ms, decrypt {tDec2} ms");
        }

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
            Dictionary<char, int> caesar,
            Dictionary<char, int> trisemus,
            Dictionary<char, int> dec1,
            Dictionary<char, int> dec2
            )
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Frequencies");

            ws.Cells[1, 1].Value = "Letter";
            ws.Cells[1, 2].Value = "Original";
            ws.Cells[1, 3].Value = "Caesar (Keyword) Encrypted";
            ws.Cells[1, 4].Value = "Trisemus Encrypted";
            ws.Cells[1, 5].Value = "Caesar (Keyword) Decrypted";
            ws.Cells[1, 6].Value = "Trisemus Decrypted";

            int row = 2;
            foreach (var c in EnglishAlphabet.Letters)
            {
                ws.Cells[row, 1].Value = c.ToString();
                ws.Cells[row, 2].Value = orig[c];
                ws.Cells[row, 3].Value = caesar[c];
                ws.Cells[row, 4].Value = trisemus[c];
                ws.Cells[row, 5].Value = dec1[c];
                ws.Cells[row, 6].Value = dec2[c];
                row++;
            }

            // Chart
            var chart = ws.Drawings.AddChart("hist", eChartType.ColumnClustered) as ExcelBarChart;
            chart.Title.Text = "Частоты символов";
            chart.SetPosition(0, 0, 10, 0);
            chart.SetSize(900, 600);

            var rangeLetters = ws.Cells[2, 1, row - 1, 1];
            chart.Series.Add(ws.Cells[2, 2, row - 1, 2], rangeLetters).Header = "Original";
            chart.Series.Add(ws.Cells[2, 3, row - 1, 3], rangeLetters).Header = "Caesar (Keyword) Encrypted";
            chart.Series.Add(ws.Cells[2, 4, row - 1, 4], rangeLetters).Header = "Trisemus Encrypted";
            chart.Series.Add(ws.Cells[2, 5, row - 1, 5], rangeLetters).Header = "Caesar (Keyword) Decrypted";
            chart.Series.Add(ws.Cells[2, 6, row - 1, 6], rangeLetters).Header = "Trisemus Decrypted";

            package.SaveAs(new FileInfo(fileName));
        }
    }

    // -------------------- English Alphabet Base --------------------
    public static class EnglishAlphabet
    {
        public static readonly char[] Letters =
        {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
        'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
    };

        public static int IndexOf(char c)
        {
            c = char.ToLower(c);
            for (int i = 0; i < Letters.Length; i++)
                if (Letters[i] == c) return i;
            return -1;
        }
    }

    // -------------------- 1. Caesar Cipher with Keyword --------------------
    public static class CaesarKeywordCipher
    {
        private static char[] BuildMixedAlphabet(string keyword)
        {
            // Убираем дубликаты из ключа и добавляем оставшиеся буквы алфавита
            var distinctKeyword = keyword.ToLower().Where(c => EnglishAlphabet.IndexOf(c) != -1).Distinct();
            var remaining = EnglishAlphabet.Letters.Except(distinctKeyword);
            return distinctKeyword.Concat(remaining).ToArray();
        }

        private static Dictionary<char, char> BuildMap(string keyword, int a, bool decrypt)
        {
            var mixed = BuildMixedAlphabet(keyword);
            var map = new Dictionary<char, char>();

            for (int i = 0; i < 26; i++)
            {
                char plain = EnglishAlphabet.Letters[i];

                // a = 24 означает, что смешанный алфавит начинает подставляться под 24-й индекс обычного
                int shiftedIndex = (i - a + 26 * 10) % 26;
                char cipher = mixed[shiftedIndex];

                if (decrypt)
                    map[cipher] = plain;
                else
                    map[plain] = cipher;
            }
            return map;
        }

        public static string Encrypt(string text, string keyword, int a)
        {
            return ApplyMap(text, BuildMap(keyword, a, false));
        }

        public static string Decrypt(string text, string keyword, int a)
        {
            return ApplyMap(text, BuildMap(keyword, a, true));
        }

        private static string ApplyMap(string text, Dictionary<char, char> map)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                char lower = char.ToLower(c);
                if (map.TryGetValue(lower, out char mapped))
                    sb.Append(char.IsUpper(c) ? char.ToUpper(mapped) : mapped);
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }

    // -------------------- 2. Trisemus Table Cipher --------------------
    public static class TrisemusCipher
    {
        private static char[] BuildMixedAlphabet(string keyword)
        {
            // Формируем уникальную последовательность букв (Ключ + остальные)
            var distinctKeyword = keyword.ToLower().Where(c => EnglishAlphabet.IndexOf(c) != -1).Distinct();
            var remaining = EnglishAlphabet.Letters.Except(distinctKeyword);
            return distinctKeyword.Concat(remaining).ToArray();
        }

        private static Dictionary<char, char> BuildMap(string keyword, int cols, bool decrypt)
        {
            var mixed = BuildMixedAlphabet(keyword);
            var map = new Dictionary<char, char>();

            for (int i = 0; i < mixed.Length; i++)
            {
                int r = i / cols;
                int c = i % cols;
                int targetIdx;

                if (!decrypt)
                {
                    // При шифровании берем букву ПОД текущей
                    int nextR = r + 1;
                    targetIdx = nextR * cols + c;
                    if (targetIdx >= mixed.Length) targetIdx = c; // Если вышли за пределы колонки — возвращаемся в начало колонки
                }
                else
                {
                    // При дешифровании берем букву НАД текущей
                    int prevR = r - 1;
                    targetIdx = prevR * cols + c;
                    if (targetIdx < 0)
                    {
                        // Ищем самый низ колонки
                        int maxRow = (mixed.Length - 1) / cols;
                        int bottomIdx = maxRow * cols + c;
                        if (bottomIdx >= mixed.Length) bottomIdx -= cols;
                        targetIdx = bottomIdx;
                    }
                }

                map[mixed[i]] = mixed[targetIdx];
            }
            return map;
        }

        // Для английского алфавита удобно использовать ширину 5 (получится сетка 6x5, последняя буква сдвинута)
        public static string Encrypt(string text, string keyword, int cols = 5)
        {
            return ApplyMap(text, BuildMap(keyword, cols, false));
        }

        public static string Decrypt(string text, string keyword, int cols = 5)
        {
            return ApplyMap(text, BuildMap(keyword, cols, true));
        }

        private static string ApplyMap(string text, Dictionary<char, char> map)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                char lower = char.ToLower(c);
                if (map.TryGetValue(lower, out char mapped))
                    sb.Append(char.IsUpper(c) ? char.ToUpper(mapped) : mapped);
                else
                    sb.Append(c);
            }
            return sb.ToString();
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
}
