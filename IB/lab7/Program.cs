using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace lab7
{
    class Program
    {
        static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding cp1251 = Encoding.GetEncoding("windows-1251");

            // 1. Подготовка ключевой информации
            byte[] key1 = cp1251.GetBytes("Информац");
            byte[] key2 = cp1251.GetBytes("ионнаябе");
            byte[] key3 = cp1251.GetBytes("зопаснос");

            Console.WriteLine("=== ЧАСТЬ 1: DES-EEE3, ЛАВИННЫЙ ЭФФЕКТ, СКОРОСТЬ ===");

            // --- Проверка корректности работы алгоритма ---
            byte[] message = cp1251.GetBytes("Секретные данные");
            Console.WriteLine($"Шифртекст : {message}");
            byte[] ciphertext = EncryptEEE3(message, key1, key2, key3);
            Console.WriteLine($"Шифртекст (HEX): {BitConverter.ToString(ciphertext).Replace("-", "")}");

            byte[] plaintext = DecryptEEE3(ciphertext, key1, key2, key3);
            Console.WriteLine($"Расшифрованное сообщение: {cp1251.GetString(plaintext)}");

            // --- Анализ лавинного эффекта ---
            byte[] testWord = cp1251.GetBytes("Анализ");
            AnalyzeAvalancheEffect(testWord, key1, key2, key3);

            // --- Оценка скорости ---
            BenchmarkSpeed(key1, key2, key3);

            Console.WriteLine("\n=== ЧАСТЬ 2: АНАЛИЗ СЛАБЫХ И ПОЛУСЛАБЫХ КЛЮЧЕЙ ===");
            AnalyzeWeakKeys();

            Console.WriteLine("\n=== ЧАСТЬ 3: ОЦЕНКА СТЕПЕНИ СЖАТИЯ ===");
            AnalyzeCompression();
        }

        // Зашифрование: E(k3, E(k2, E(k1, M)))
        static byte[] EncryptEEE3(byte[] data, byte[] k1, byte[] k2, byte[] k3)
        {
            byte[] paddedData = PadPKCS7(data, 8);
            byte[] result = new byte[paddedData.Length];

            var d1 = new DesEngine(); d1.Init(true, new KeyParameter(k1));
            var d2 = new DesEngine(); d2.Init(true, new KeyParameter(k2));
            var d3 = new DesEngine(); d3.Init(true, new KeyParameter(k3));

            byte[] temp1 = new byte[8];
            byte[] temp2 = new byte[8];

            for (int i = 0; i < paddedData.Length; i += 8)
            {
                d1.ProcessBlock(paddedData, i, temp1, 0);
                d2.ProcessBlock(temp1, 0, temp2, 0);
                d3.ProcessBlock(temp2, 0, result, i);
            }
            return result;
        }

        // Расшифрование: D(k1, D(k2, D(k3, C)))
        static byte[] DecryptEEE3(byte[] data, byte[] k1, byte[] k2, byte[] k3)
        {
            byte[] result = new byte[data.Length];

            var d1 = new DesEngine(); d1.Init(false, new KeyParameter(k1));
            var d2 = new DesEngine(); d2.Init(false, new KeyParameter(k2));
            var d3 = new DesEngine(); d3.Init(false, new KeyParameter(k3));

            byte[] temp1 = new byte[8];
            byte[] temp2 = new byte[8];

            for (int i = 0; i < data.Length; i += 8)
            {
                d3.ProcessBlock(data, i, temp1, 0);
                d2.ProcessBlock(temp1, 0, temp2, 0);
                d1.ProcessBlock(temp2, 0, result, i);
            }

            return UnpadPKCS7(result);
        }

        static void AnalyzeAvalancheEffect(byte[] word, byte[] k1, byte[] k2, byte[] k3)
        {
            Console.WriteLine("\n--- Анализ лавинного эффекта ---");
            Encoding cp1251 = Encoding.GetEncoding("windows-1251");
            Console.WriteLine($"Исходное слово: {cp1251.GetString(word)}");

            byte[] paddedWord = PadPKCS7(word, 8);
            byte[] modifiedWord = (byte[])paddedWord.Clone();
            modifiedWord[0] ^= 1;
            Console.WriteLine("Изменен 1 бит в исходном блоке (дополненном).");

            var d1 = new DesEngine(); d1.Init(true, new KeyParameter(k1));
            var d2 = new DesEngine(); d2.Init(true, new KeyParameter(k2));
            var d3 = new DesEngine(); d3.Init(true, new KeyParameter(k3));

            byte[] c1_orig = new byte[8], c1_mod = new byte[8];
            d1.ProcessBlock(paddedWord, 0, c1_orig, 0);
            d1.ProcessBlock(modifiedWord, 0, c1_mod, 0);
            Console.WriteLine($"После 1-го DES (K1) изменилось бит: {GetBitDiff(c1_orig, c1_mod)} из 64");

            byte[] c2_orig = new byte[8], c2_mod = new byte[8];
            d2.ProcessBlock(c1_orig, 0, c2_orig, 0);
            d2.ProcessBlock(c1_mod, 0, c2_mod, 0);
            Console.WriteLine($"После 2-го DES (K2) изменилось бит: {GetBitDiff(c2_orig, c2_mod)} из 64");

            byte[] c3_orig = new byte[8], c3_mod = new byte[8];
            d3.ProcessBlock(c2_orig, 0, c3_orig, 0);
            d3.ProcessBlock(c2_mod, 0, c3_mod, 0);
            Console.WriteLine($"После 3-го DES (K3) изменилось бит: {GetBitDiff(c3_orig, c3_mod)} из 64");
        }

        static void BenchmarkSpeed(byte[] k1, byte[] k2, byte[] k3)
        {
            Console.WriteLine("\n--- Оценка скорости зашифрования/расшифрования ---");
            int dataSizeMb = 5;
            byte[] testData = new byte[dataSizeMb * 1024 * 1024];
            Array.Fill(testData, (byte)0x41);

            Stopwatch sw = new Stopwatch();

            sw.Start();
            byte[] encryptedData = EncryptEEE3(testData, k1, k2, k3);
            sw.Stop();
            double encTime = sw.Elapsed.TotalSeconds;

            sw.Reset();

            sw.Start();
            byte[] decryptedData = DecryptEEE3(encryptedData, k1, k2, k3);
            sw.Stop();
            double decTime = sw.Elapsed.TotalSeconds;

            Console.WriteLine($"Размер данных: {dataSizeMb} МБ");
            Console.WriteLine($"Время зашифрования: {encTime:F4} сек. Скорость: {dataSizeMb / encTime:F2} МБ/с");
            Console.WriteLine($"Время расшифрования: {decTime:F4} сек. Скорость: {dataSizeMb / decTime:F2} МБ/с");
        }

        // --- Вспомогательные методы ---

        static byte[] PadPKCS7(byte[] data, int blockSize)
        {
            int padLength = blockSize - (data.Length % blockSize);
            byte[] padded = new byte[data.Length + padLength];
            Buffer.BlockCopy(data, 0, padded, 0, data.Length);
            for (int i = data.Length; i < padded.Length; i++)
                padded[i] = (byte)padLength;
            return padded;
        }

        static byte[] UnpadPKCS7(byte[] data)
        {
            int padLength = data[data.Length - 1];
            if (padLength <= 0 || padLength > 8) return data;
            byte[] unpadded = new byte[data.Length - padLength];
            Buffer.BlockCopy(data, 0, unpadded, 0, unpadded.Length);
            return unpadded;
        }

        static int GetBitDiff(byte[] b1, byte[] b2)
        {
            int diff = 0;
            for (int i = 0; i < Math.Min(b1.Length, b2.Length); i++)
            {
                int xor = b1[i] ^ b2[i];
                while (xor > 0)
                {
                    diff += xor & 1;
                    xor >>= 1;
                }
            }
            return diff;
        }

        // --- Методы из второй части (Слабые ключи и Сжатие) ---

        static void AnalyzeWeakKeys()
        {
            byte[] weakKey = { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };
            byte[] semiWeak1 = { 0x01, 0xFE, 0x01, 0xFE, 0x01, 0xFE, 0x01, 0xFE };
            byte[] semiWeak2 = { 0xFE, 0x01, 0xFE, 0x01, 0xFE, 0x01, 0xFE, 0x01 };
            byte[] plaintext = Encoding.GetEncoding("windows-1251").GetBytes("Секреты!");

            Console.WriteLine($"Исходный текст: {BitConverter.ToString(plaintext)}");

            Console.WriteLine("\n[Слабый ключ: 0101010101010101]");
            byte[] cipher1 = ProcessDesBlock(plaintext, weakKey, encrypt: true);
            Console.WriteLine($"Зашифрование (Шаг 1): {BitConverter.ToString(cipher1)}");

            byte[] cipher2 = ProcessDesBlock(cipher1, weakKey, encrypt: true);
            Console.WriteLine($"Повторное ЗАШИФРОВАНИЕ тем же ключом (Шаг 2): {BitConverter.ToString(cipher2)}");
            Console.WriteLine($"Совпадает с исходным: {BitConverter.ToString(plaintext) == BitConverter.ToString(cipher2)}");

            Console.WriteLine("\n[Полуслабые ключи: пара 01FE... и FE01...]");
            byte[] swCipher1 = ProcessDesBlock(plaintext, semiWeak1, encrypt: true);
            Console.WriteLine($"Зашифрование ключом 1: {BitConverter.ToString(swCipher1)}");

            byte[] swCipher2 = ProcessDesBlock(swCipher1, semiWeak2, encrypt: true);
            Console.WriteLine($"Зашифрование ключом 2: {BitConverter.ToString(swCipher2)}");
            Console.WriteLine($"Совпадает с исходным: {BitConverter.ToString(plaintext) == BitConverter.ToString(swCipher2)}");

            Console.WriteLine("\n[Лавинный эффект при использовании слабого ключа]");
            byte[] modifiedText = (byte[])plaintext.Clone();
            modifiedText[0] ^= 1;

            byte[] cipherMod = ProcessDesBlock(modifiedText, weakKey, encrypt: true);
            int diff = GetBitDiff(cipher1, cipherMod);
            Console.WriteLine($"Изменен 1 бит открытого текста.");
            Console.WriteLine($"Изменилось бит в шифртексте: {diff} из 64");
        }

        static void AnalyzeCompression()
        {
            string pattern = "Анализ алгоритма DES. ";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5000; i++) sb.Append(pattern);

            byte[] originalData = Encoding.GetEncoding("windows-1251").GetBytes(sb.ToString());
            byte[] strongKey = { 0x1A, 0x2B, 0x3C, 0x4D, 0x5E, 0x6F, 0x7A, 0x8B };
            byte[] encryptedData = new byte[originalData.Length];

            var engine = new DesEngine();
            engine.Init(true, new KeyParameter(strongKey));

            for (int i = 0; i < originalData.Length; i += 8)
            {
                if (i + 8 <= originalData.Length)
                    engine.ProcessBlock(originalData, i, encryptedData, i);
                else
                    Buffer.BlockCopy(originalData, i, encryptedData, i, originalData.Length - i);
            }

            byte[] compressedOriginal = CompressData(originalData);
            byte[] compressedEncrypted = CompressData(encryptedData);

            Console.WriteLine($"Исходный текст (сжатый / до сжатия): {compressedOriginal.Length,6} / {originalData.Length} байт. Коэффициент: {(double)originalData.Length / compressedOriginal.Length:F2}");
            Console.WriteLine($"Шифртекст      (сжатый / до сжатия): {compressedEncrypted.Length,6} / {encryptedData.Length} байт. Коэффициент: {(double)encryptedData.Length / compressedEncrypted.Length:F2}");
        }

        static byte[] ProcessDesBlock(byte[] input, byte[] key, bool encrypt)
        {
            var engine = new DesEngine();
            engine.Init(encrypt, new KeyParameter(key));
            byte[] output = new byte[8];
            engine.ProcessBlock(input, 0, output, 0);
            return output;
        }

        static byte[] CompressData(byte[] data)
        {
            using var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionLevel.Optimal))
                gzip.Write(data, 0, data.Length);
            return ms.ToArray();
        }
    }
}