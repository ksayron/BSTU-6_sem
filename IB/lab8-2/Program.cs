using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

const int n = 6;
int[] key = { 10, 11, 12, 13, 14, 15 };

int[]? lastCipherWords = null;
int lastBitLength = 0;

while (true)
{
    Console.WriteLine("\n=== Приложение 2. RC4 (n = 6) ===");
    Console.WriteLine("1 - Показать первые N слов гаммы");
    Console.WriteLine("2 - Зашифровать текст");
    Console.WriteLine("3 - Расшифровать последний шифртекст");
    Console.WriteLine("4 - Оценить скорость генерации ПСП");
    Console.WriteLine("0 - Выход");
    Console.Write("Выбор: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.Write("Сколько 6-битных слов вывести? ");
            if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
            {
                var rc4 = new Rc4N(n, key);
                int[] gamma = rc4.GenerateWords(count);
                Console.WriteLine("Гамма (десятичные 6-битные слова):");
                Console.WriteLine(string.Join(", ", gamma));
            }
            else
            {
                Console.WriteLine("Некорректное число.");
            }
            break;

        case "2":
            Console.Write("Введите текст: ");
            string text = Console.ReadLine() ?? string.Empty;

            int[] plainWords = BitWordCodec.TextToWords(text, n, out lastBitLength);

            var rc4Enc = new Rc4N(n, key);
            int[] gammaWords = rc4Enc.GenerateWords(plainWords.Length);

            lastCipherWords = XorArrays(plainWords, gammaWords);

            Console.WriteLine($"Исходный текст: {text}");
            Console.WriteLine($"Длина исходного битового потока: {lastBitLength} бит");
            Console.WriteLine($"Открытые 6-битные слова: {string.Join(", ", plainWords)}");
            Console.WriteLine($"Гамма: {string.Join(", ", gammaWords)}");
            Console.WriteLine($"Шифртекст: {string.Join(", ", lastCipherWords)}");

            var rc4DecDemo = new Rc4N(n, key);
            int[] gammaForDecryptDemo = rc4DecDemo.GenerateWords(lastCipherWords.Length);
            int[] demoDecryptedWords = XorArrays(lastCipherWords, gammaForDecryptDemo);
            string demoDecryptedText = BitWordCodec.WordsToText(demoDecryptedWords, n, lastBitLength);

            Console.WriteLine($"Проверка расшифрования: {demoDecryptedText}");
            break;

        case "3":
            if (lastCipherWords is null)
            {
                Console.WriteLine("Сначала нужно выполнить шифрование.");
                break;
            }

            var rc4Dec = new Rc4N(n, key);
            int[] gammaForDecrypt = rc4Dec.GenerateWords(lastCipherWords.Length);
            int[] decryptedWords = XorArrays(lastCipherWords, gammaForDecrypt);
            string decryptedText = BitWordCodec.WordsToText(decryptedWords, n, lastBitLength);

            Console.WriteLine($"Шифртекст: {string.Join(", ", lastCipherWords)}");
            Console.WriteLine($"Расшифрованный текст: {decryptedText}");
            break;

        case "4":
            Console.Write("Сколько слов ПСП генерировать в одном тесте? ");
            if (!int.TryParse(Console.ReadLine(), out int wordsPerTest) || wordsPerTest <= 0)
            {
                wordsPerTest = 200_000;
            }

            Console.Write("Сколько повторов теста выполнить? ");
            if (!int.TryParse(Console.ReadLine(), out int iterations) || iterations <= 0)
            {
                iterations = 10;
            }

            double totalMs = 0.0;
            long totalWords = 0;

            for (int i = 0; i < iterations; i++)
            {
                var rc4 = new Rc4N(n, key);

                Stopwatch sw = Stopwatch.StartNew();
                rc4.GenerateWords(wordsPerTest);
                sw.Stop();

                totalMs += sw.Elapsed.TotalMilliseconds;
                totalWords += wordsPerTest;
            }

            double avgMs = totalMs / iterations;
            double wordsPerSecond = totalWords / (totalMs / 1000.0);

            Console.WriteLine($"Параметры: n = {n}, ключ = [{string.Join(", ", key)}]");
            Console.WriteLine($"Слов за тест: {wordsPerTest}");
            Console.WriteLine($"Повторов: {iterations}");
            Console.WriteLine($"Среднее время: {avgMs:F3} мс");
            Console.WriteLine($"Скорость генерации: {wordsPerSecond:F2} слов/с");
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Неизвестная команда.");
            break;
    }
}

static int[] XorArrays(int[] left, int[] right)
{
    if (left.Length != right.Length)
        throw new ArgumentException("Массивы должны иметь одинаковую длину.");

    int[] result = new int[left.Length];
    for (int i = 0; i < left.Length; i++)
    {
        result[i] = left[i] ^ right[i];
    }

    return result;
}

public sealed class Rc4N
{
    private readonly int _n;
    private readonly int _mod;
    private readonly int[] _s;
    private int _i;
    private int _j;

    public Rc4N(int n, int[] key)
    {
        if (n <= 0 || n > 30)
            throw new ArgumentOutOfRangeException(nameof(n));

        _n = n;
        _mod = 1 << n;
        _s = new int[_mod];

        for (int i = 0; i < _mod; i++)
        {
            _s[i] = i;
        }

        int[] k = new int[_mod];
        for (int i = 0; i < _mod; i++)
        {
            k[i] = key[i % key.Length] % _mod;
        }

        int j = 0;
        for (int i = 0; i < _mod; i++)
        {
            j = (j + _s[i] + k[i]) % _mod;
            Swap(i, j);
        }

        _i = 0;
        _j = 0;
    }

    public int NextWord()
    {
        _i = (_i + 1) % _mod;
        _j = (_j + _s[_i]) % _mod;

        Swap(_i, _j);

        int a = (_s[_i] + _s[_j]) % _mod;
        return _s[a];
    }

    public int[] GenerateWords(int count)
    {
        int[] result = new int[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = NextWord();
        }

        return result;
    }

    private void Swap(int i, int j)
    {
        (_s[i], _s[j]) = (_s[j], _s[i]);
    }
}

public static class BitWordCodec
{
    public static int[] TextToWords(string text, int wordSize, out int originalBitLength)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        originalBitLength = bytes.Length * 8;

        int wordCount = (originalBitLength + wordSize - 1) / wordSize;
        int[] words = new int[wordCount];

        int bitIndex = 0;

        for (int w = 0; w < wordCount; w++)
        {
            int value = 0;

            for (int b = 0; b < wordSize; b++)
            {
                value <<= 1;

                if (bitIndex < originalBitLength)
                {
                    int byteIndex = bitIndex / 8;
                    int bitInByte = 7 - (bitIndex % 8);
                    int bit = (bytes[byteIndex] >> bitInByte) & 1;
                    value |= bit;
                }

                bitIndex++;
            }

            words[w] = value;
        }

        return words;
    }

    public static string WordsToText(int[] words, int wordSize, int originalBitLength)
    {
        int byteCount = originalBitLength / 8;
        byte[] bytes = new byte[byteCount];

        int bitIndex = 0;

        foreach (int word in words)
        {
            for (int b = wordSize - 1; b >= 0 && bitIndex < originalBitLength; b--)
            {
                int bit = (word >> b) & 1;

                int byteIndex = bitIndex / 8;
                int bitInByte = 7 - (bitIndex % 8);

                bytes[byteIndex] |= (byte)(bit << bitInByte);
                bitIndex++;
            }
        }

        return Encoding.UTF8.GetString(bytes);
    }
}