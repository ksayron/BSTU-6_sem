using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

RsaPrngParameters parameters = RsaPrngParameters.Generate(256);

while (true)
{
    Console.WriteLine("\n=== Приложение 1. Генератор ПСП RSA ===");
    Console.WriteLine("1 - Сгенерировать новые параметры p, q, e, x0");
    Console.WriteLine("2 - Показать текущие параметры");
    Console.WriteLine("3 - Вывести первые N бит ПСП");
    Console.WriteLine("4 - Зашифровать и расшифровать текст");
    Console.WriteLine("5 - Замерить скорость генерации");
    Console.WriteLine("0 - Выход");
    Console.Write("Выбор: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.WriteLine("Генерация 256-битных параметров...");
            parameters = RsaPrngParameters.Generate(256);
            Console.WriteLine("Параметры успешно сгенерированы.");
            break;

        case "2":
            ShowParameters(parameters);
            break;

        case "3":
            Console.Write("Сколько бит вывести? ");
            if (int.TryParse(Console.ReadLine(), out int bitCount) && bitCount > 0)
            {
                var prng = new RsaPrng(parameters.N, parameters.E, parameters.X0);
                StringBuilder bits = new();
                for (int i = 0; i < bitCount; i++)
                {
                    bits.Append(prng.NextBit());
                }

                Console.WriteLine($"ПСП ({bitCount} бит):");
                Console.WriteLine(bits.ToString());
            }
            else
            {
                Console.WriteLine("Некорректное число.");
            }
            break;

        case "4":
            Console.Write("Введите текст: ");
            string plainText = Console.ReadLine() ?? string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = RsaStreamCipher.Transform(plainBytes, parameters);
            byte[] decryptedBytes = RsaStreamCipher.Transform(cipherBytes, parameters);

            Console.WriteLine($"Шифртекст (HEX): {Convert.ToHexString(cipherBytes)}");
            Console.WriteLine($"Расшифрованный текст: {Encoding.UTF8.GetString(decryptedBytes)}");
            break;

        case "5":
            Console.Write("Сколько байт сгенерировать для теста скорости? ");
            if (!int.TryParse(Console.ReadLine(), out int byteCount) || byteCount <= 0)
            {
                byteCount = 100_000;
            }

            var generator = new RsaPrng(parameters.N, parameters.E, parameters.X0);
            Stopwatch sw = Stopwatch.StartNew();
            generator.GenerateBytes(byteCount);
            sw.Stop();

            double bytesPerSecond = byteCount / sw.Elapsed.TotalSeconds;
            Console.WriteLine($"Сгенерировано: {byteCount} байт");
            Console.WriteLine($"Время: {sw.Elapsed.TotalMilliseconds:F3} мс");
            Console.WriteLine($"Скорость: {bytesPerSecond:F2} байт/с");
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Неизвестная команда.");
            break;
    }
}

static void ShowParameters(RsaPrngParameters parameters)
{
    Console.WriteLine("\n--- Текущие параметры ---");
    Console.WriteLine($"p   = {parameters.P}");
    Console.WriteLine($"q   = {parameters.Q}");
    Console.WriteLine($"e   = {parameters.E}");
    Console.WriteLine($"n   = {parameters.N}");
    Console.WriteLine($"phi = {parameters.Phi}");
    Console.WriteLine($"x0  = {parameters.X0}");

    Console.WriteLine("\nHEX-представление:");
    Console.WriteLine($"p   = {BigIntUtils.ToHex(parameters.P)}");
    Console.WriteLine($"q   = {BigIntUtils.ToHex(parameters.Q)}");
    Console.WriteLine($"e   = {BigIntUtils.ToHex(parameters.E)}");
    Console.WriteLine($"n   = {BigIntUtils.ToHex(parameters.N)}");
    Console.WriteLine($"x0  = {BigIntUtils.ToHex(parameters.X0)}");

    Console.WriteLine("\nОбоснование выбора:");
    Console.WriteLine("1. p и q выбраны 256-битными простыми числами в соответствии с заданием.");
    Console.WriteLine("2. Тогда модуль n = p * q имеет размер около 512 бит, что делает пример неигрушечным.");
    Console.WriteLine("3. e также выбрано 256-битным нечётным простым числом, взаимно простым с phi(n).");
    Console.WriteLine("4. x0 выбирается случайно и взаимно просто с n, чтобы генератор корректно работал и не попадал в вырожденные состояния.");
    Console.WriteLine("5. В реальном RSA часто берут e = 65537, но в этой лабораторной e делаем 256-битным, потому что это прямо указано в задании.");
}

public sealed record RsaPrngParameters(
    BigInteger P,
    BigInteger Q,
    BigInteger E,
    BigInteger N,
    BigInteger Phi,
    BigInteger X0)
{
    public static RsaPrngParameters Generate(int bitLength)
    {
        BigInteger p = PrimeUtils.GeneratePrime(bitLength);

        BigInteger q;
        do
        {
            q = PrimeUtils.GeneratePrime(bitLength);
        }
        while (q == p);

        BigInteger n = p * q;
        BigInteger phi = (p - 1) * (q - 1);

        BigInteger e;
        do
        {
            e = PrimeUtils.GeneratePrime(bitLength);
        }
        while (BigInteger.GreatestCommonDivisor(e, phi) != 1);

        BigInteger x0 = BigIntUtils.RandomCoprime(n);

        return new RsaPrngParameters(p, q, e, n, phi, x0);
    }
}

public sealed class RsaPrng
{
    private readonly BigInteger _n;
    private readonly BigInteger _e;

    public BigInteger State { get; private set; }

    public RsaPrng(BigInteger n, BigInteger e, BigInteger x0)
    {
        _n = n;
        _e = e;
        State = x0;
    }

    public int NextBit()
    {
        State = BigInteger.ModPow(State, _e, _n);
        return State.IsEven ? 0 : 1;
    }

    public byte NextByte()
    {
        int value = 0;
        for (int i = 0; i < 8; i++)
        {
            value = (value << 1) | NextBit();
        }

        return (byte)value;
    }

    public byte[] GenerateBytes(int count)
    {
        byte[] result = new byte[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = NextByte();
        }

        return result;
    }
}

public static class RsaStreamCipher
{
    public static byte[] Transform(byte[] input, RsaPrngParameters parameters)
    {
        var prng = new RsaPrng(parameters.N, parameters.E, parameters.X0);
        byte[] gamma = prng.GenerateBytes(input.Length);

        byte[] output = new byte[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = (byte)(input[i] ^ gamma[i]);
        }

        return output;
    }
}

public static class PrimeUtils
{
    public static BigInteger GeneratePrime(int bitLength)
    {
        while (true)
        {
            BigInteger candidate = BigIntUtils.RandomOddBigInteger(bitLength);
            if (IsProbablePrime(candidate, rounds: 20))
            {
                return candidate;
            }
        }
    }

    public static bool IsProbablePrime(BigInteger n, int rounds)
    {
        if (n < 2)
            return false;

        int[] smallPrimes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };

        foreach (int p in smallPrimes)
        {
            if (n == p)
                return true;

            if (n % p == 0)
                return false;
        }

        BigInteger d = n - 1;
        int s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s++;
        }

        for (int i = 0; i < rounds; i++)
        {
            BigInteger a = BigIntUtils.RandomInRange(2, n - 2);
            BigInteger x = BigInteger.ModPow(a, d, n);

            if (x == 1 || x == n - 1)
                continue;

            bool passedRound = false;

            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, n);
                if (x == n - 1)
                {
                    passedRound = true;
                    break;
                }
            }

            if (!passedRound)
                return false;
        }

        return true;
    }
}

public static class BigIntUtils
{
    public static BigInteger RandomOddBigInteger(int bitLength)
    {
        int byteCount = bitLength / 8;
        byte[] bytes = new byte[byteCount];
        RandomNumberGenerator.Fill(bytes);

        bytes[0] |= 0b1000_0000; // гарантируем нужную битовую длину
        bytes[^1] |= 0b0000_0001; // нечётное число

        return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
    }

    public static BigInteger RandomBelow(BigInteger upperExclusive)
    {
        if (upperExclusive <= 1)
            return 0;

        byte[] bytes = upperExclusive.ToByteArray(isUnsigned: true, isBigEndian: true);
        BigInteger result;

        do
        {
            RandomNumberGenerator.Fill(bytes);
            result = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        }
        while (result >= upperExclusive);

        return result;
    }

    public static BigInteger RandomInRange(BigInteger minInclusive, BigInteger maxInclusive)
    {
        BigInteger range = maxInclusive - minInclusive + 1;
        return minInclusive + RandomBelow(range);
    }

    public static BigInteger RandomCoprime(BigInteger n)
    {
        BigInteger x;
        do
        {
            x = RandomInRange(2, n - 1);
        }
        while (BigInteger.GreatestCommonDivisor(x, n) != 1);

        return x;
    }

    public static string ToHex(BigInteger value)
    {
        return Convert.ToHexString(value.ToByteArray(isUnsigned: true, isBigEndian: true));
    }
}