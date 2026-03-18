using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System.Text;

namespace lab2
{
    public class lab02
    {

        static readonly char[] TurkishAlphabet =
{
    'A','B','C','Ç','D','E','F','G','Ğ','H','I','İ',
    'J','K','L','M','N','O','Ö','P','R','S','Ş','T',
    'U','Ü','V','Y','Z',
    'a','b','c','ç','d','e','f','g','ğ','h','ı','i',
    'j','k','l','m','n','o','ö','p','r','s','ş','t',
    'u','ü','v','y','z'
};
        static readonly char[] RussianAlphabet =
{
    'А','Б','В','Г','Д','Е','Ё','Ж','З','И','Й','К','Л','М','Н','О','П',
    'Р','С','Т','У','Ф','Х','Ц','Ч','Ш','Щ','Ъ','Ы','Ь','Э','Ю','Я',
    'а','б','в','г','д','е','ё','ж','з','и','й','к','л','м','н','о','п',
    'р','с','т','у','ф','х','ц','ч','ш','щ','ъ','ы','ь','э','ю','я'
};
        static void Main()
        {
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

            string curdir = @"D:\BSTU\6sem\IB\lab2\";
            Directory.SetCurrentDirectory(curdir);
            string filePath = "lat.txt";
            string filePath2 = "kir.txt";
            string filePath3 = "bin.txt";


            if (File.Exists(filePath) && File.Exists(filePath2) && File.Exists(filePath3))
            {
                string text = File.ReadAllText(filePath, Encoding.UTF8);

                string text2 = File.ReadAllText(filePath2, Encoding.UTF8);

                string text3 = File.ReadAllText(filePath3, Encoding.UTF8);


                var latinFrequencies = GetCharacterFrequencies(text, TurkishAlphabet);
                var cyrillicFrequencies = GetCharacterFrequencies(text2, RussianAlphabet);
                var binFrequencies = GetCharacterFrequencies(text3, '0', '1');

                var filePathExcel = "Frequencies.xlsx";

                if (File.Exists(filePathExcel))
                {
                    File.Delete(filePathExcel);
                }

                var fileInfo = new FileInfo("Frequencies.xlsx");
                using (var package = new ExcelPackage(fileInfo))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Frequencies");

                    WriteFrequenciesToWorksheet(worksheet, latinFrequencies, "Латиница", 1);

                    WriteFrequenciesToWorksheet(worksheet, cyrillicFrequencies, "Кириллица", 30);

                    WriteFrequenciesToWorksheet(worksheet, binFrequencies, "Бинарный", 70);
                    package.Save();
                }

                Console.WriteLine("Excel файл создан: Frequencies.xlsx");

                //  энтропия Шеннона
                double entropyLatin = CalculateShannonEntropy(latinFrequencies, text.Length);
                double entropyCyrillic = CalculateShannonEntropy(cyrillicFrequencies, text2.Length);
                double entropyBin = CalculateShannonEntropy(binFrequencies, text3.Length);

                Console.WriteLine($"Энтропия латиницы: {entropyLatin}");
                Console.WriteLine($"Энтропия кириллицы: {entropyCyrillic}");
                Console.WriteLine($"Энтропия бинарного алфавита: {entropyBin}");


                string fullName = "KucherukNikolayPetrovich";
                string fullName1 = "КучерукНиколайПетрович";
                double infoAmountLatin = fullName.Length * entropyLatin;
                double infoAmountCyrillic = fullName1.Length * entropyCyrillic;
                double ascii = fullName1.Length * 8 * entropyBin;

                Console.WriteLine($"\nКоличество информации (латиница): {infoAmountLatin} бит");
                Console.WriteLine($"\nКоличество информации (кириллица): {infoAmountCyrillic} бит");
                Console.WriteLine($"\nКоличество информации (ASCII): {ascii} бит");


                double[] errorProbabilities = { 0.1, 0.26, 0.5, 1.0 };

                foreach (double p in errorProbabilities)
                {
                    if (p < 0 || p > 1)
                        continue;

                    string bitsLatinStr = ToBitString(fullName);
                    string bitsCyrStr = ToBitString(fullName1);

                    int NbitsLatin = bitsLatinStr.Length;
                    int NbitsCyr = bitsCyrStr.Length;

                    double HXLatin = CalculateBinaryEntropy(bitsLatinStr);
                    double HXCyr = CalculateBinaryEntropy(bitsCyrStr);

                    double Hp = 0;
                    if (p != 0 && p != 1)
                        Hp = -(p * Log2(p) + (1 - p) * Log2(1 - p));

                    double ILatinPerBit = Math.Max(0, HXLatin - Hp);
                    double ICyrPerBit = Math.Max(0, HXCyr - Hp);

                    double totalLatin = NbitsLatin * ILatinPerBit;
                    double totalCyr = NbitsCyr * ICyrPerBit;

                    Console.WriteLine($"\np = {p}");
                    Console.WriteLine($"Биты латиница: {NbitsLatin}");
                    Console.WriteLine($"Биты кириллица: {NbitsCyr}");
                    Console.WriteLine($"H(X) латиница: {Math.Round(HXLatin, 5)}");
                    Console.WriteLine($"H(X) кириллица: {Math.Round(HXCyr, 5)}");
                    Console.WriteLine($"H(p): {Math.Round(Hp, 5)}");
                    Console.WriteLine($"Hе(Cyr): {Math.Round(HXLatin - Hp, 5)}");
                    Console.WriteLine($"Hе(Latin): {Math.Round(HXLatin - Hp, 5)}");
                    Console.WriteLine($"I латиница: {Math.Round(totalLatin, 5)} бит");
                    Console.WriteLine($"I кириллица: {Math.Round(totalCyr, 5)} бит");
                }
            }
            else
            {
                Console.WriteLine("\nФайл не найден.");
            }
        }

        static string ToBitString(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            StringBuilder sb = new StringBuilder();

            foreach (byte b in bytes)
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));

            return sb.ToString();
        }

        static double CalculateBinaryEntropy(string bitString)
        {
            int zeros = bitString.Count(c => c == '0');
            int ones = bitString.Length - zeros;

            double p0 = (double)zeros / bitString.Length;
            double p1 = (double)ones / bitString.Length;

            return -(p0 * Log2(p0) + p1 * Log2(p1));
        }

        static Dictionary<char, int> GetCharacterFrequencies(string text, char[] alphabet)
        {
            var freq = new Dictionary<char, int>();

            foreach (char c in text)
            {
                if (alphabet.Contains(c))
                {
                    if (!freq.ContainsKey(c))
                        freq[c] = 0;
                    freq[c]++;
                }
            }

            return freq;
        }

        static Dictionary<char, int> GetCharacterFrequencies(string text, char start, char end)
        {
            var freq = new Dictionary<char, int>();
            foreach (char c in text)
            {
                if (c >= start && c <= end)
                {
                    if (!freq.ContainsKey(c))
                        freq[c] = 0;
                    freq[c]++;
                }
            }
            return freq;
        }


        public static Dictionary<char, int> GetSymbolAppearances(string str)
        {
            var symbolAppearances = new Dictionary<char, int>();
            foreach (char c in str)
            {
                if (!symbolAppearances.ContainsKey(c))
                    symbolAppearances.Add(c, 1);
                else
                    symbolAppearances[c] += 1;
            }
            return symbolAppearances;
        }

        static double CalculateShannonEntropy(Dictionary<char, int> frequencies, int total)
        {
            double entropy = 0;
            foreach (var kvp in frequencies)
            {
                double probability = (double)kvp.Value / total;
                entropy -= probability * Log2(probability);
            }
            return Math.Round(entropy, 3);
        }


        public static double Log2(double value)
        {
            if (value <= 0)
            {
                return 0;
            }
            return Math.Log(value) / Math.Log(2);
        }


        static void WriteFrequenciesToWorksheet(ExcelWorksheet worksheet, Dictionary<char, int> frequencies, string title, int startRow)
        {
            worksheet.Cells[startRow, 1].Value = title;
            worksheet.Cells[startRow, 1, startRow, 2].Merge = true;

            int row = startRow + 1;
            foreach (var kvp in frequencies)
            {
                worksheet.Cells[row, 1].Value = kvp.Key;
                worksheet.Cells[row, 2].Value = kvp.Value;
                row++;
            }

            // Создание гистограммы
            var chart = worksheet.Drawings.AddChart(title + " Chart", eChartType.ColumnClustered);
            chart.SetPosition(startRow + 1, 0, 3, 0);
            chart.SetSize(400, 250);
            chart.Series.Add(worksheet.Cells[startRow + 1, 2, row - 1, 2], worksheet.Cells[startRow + 1, 1, row - 1, 1]);
            chart.Title.Text = title + " Гистограмма";
        }



    }


}
