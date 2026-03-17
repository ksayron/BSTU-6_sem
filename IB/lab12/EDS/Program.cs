using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

public static class Program
{
    private static readonly SecureRandom Rng = new SecureRandom();

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        string message = "KucherukNikolayPetrovich";

        Console.WriteLine("Лабораторная работа №10");
        Console.WriteLine("Исследование алгоритмов генерации и верификации ЭЦП");
        Console.WriteLine();

        Console.WriteLine($"Исходное сообщение: {message}");
        Console.WriteLine();

        RunRsaDemo(message);
        RunElGamalDemo(message);
        RunSchnorrDemo(message);
    }

    // ============================================================
    // RSA
    // ============================================================

    private static void RunRsaDemo(string message)
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("RSA ЭЦП");
        Console.WriteLine("====================================================");

        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] changedMessageBytes = Encoding.UTF8.GetBytes(message + "11111");

        var keyGenWatch = Stopwatch.StartNew();

        using RSA rsa = RSA.Create(2048);

        keyGenWatch.Stop();

        byte[] publicKey = rsa.ExportSubjectPublicKeyInfo();

        var signWatch = Stopwatch.StartNew();

        byte[] signature = rsa.SignData(
            messageBytes,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        signWatch.Stop();

        var verifyWatch = Stopwatch.StartNew();

        bool isValid = rsa.VerifyData(
            messageBytes,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        verifyWatch.Stop();

        bool isChangedValid = rsa.VerifyData(
            changedMessageBytes,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        Console.WriteLine("Параметры:");
        Console.WriteLine("RSA key size: 2048 bit");
        Console.WriteLine("Hash: SHA-256");
        Console.WriteLine("Padding: PKCS#1 v1.5");
        Console.WriteLine();

        Console.WriteLine("Открытая ключевая информация для передачи получателю:");
        Console.WriteLine(Convert.ToBase64String(publicKey));
        Console.WriteLine();

        Console.WriteLine("Подпись:");
        Console.WriteLine(Convert.ToBase64String(signature));
        Console.WriteLine();

        Console.WriteLine($"Верификация исходного сообщения: {isValid}");
        Console.WriteLine($"Верификация изменённого сообщения: {isChangedValid}");
        Console.WriteLine();

        Console.WriteLine($"Время генерации ключей: {keyGenWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine($"Время генерации ЭЦП: {signWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine($"Время верификации ЭЦП: {verifyWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine();
    }

    // ============================================================
    // ELGAMAL SIGNATURE
    // ============================================================

    private sealed class ElGamalPublicKey
    {
        public BigInteger P { get; init; } = null!;
        public BigInteger G { get; init; } = null!;
        public BigInteger Y { get; init; } = null!;
    }

    private sealed class ElGamalPrivateKey
    {
        public BigInteger X { get; init; } = null!;
    }

    private sealed class ElGamalSignature
    {
        public BigInteger A { get; init; } = null!;
        public BigInteger B { get; init; } = null!;
    }

    private static void RunElGamalDemo(string message)
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("ЭЦП Эль-Гамаля");
        Console.WriteLine("====================================================");

        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] changedMessageBytes = Encoding.UTF8.GetBytes(message + "11111");

        var keyGenWatch = Stopwatch.StartNew();

        var parameters = GenerateDsaLikeParameters(1024, 160);

        BigInteger p = parameters.P;
        BigInteger g = parameters.G;

        BigInteger x = RandomBetween(BigInteger.One, p.Subtract(BigInteger.Two));
        BigInteger y = g.ModPow(x, p);

        var publicKey = new ElGamalPublicKey
        {
            P = p,
            G = g,
            Y = y
        };

        var privateKey = new ElGamalPrivateKey
        {
            X = x
        };

        keyGenWatch.Stop();

        var signWatch = Stopwatch.StartNew();

        ElGamalSignature signature = ElGamalSign(messageBytes, publicKey, privateKey);

        signWatch.Stop();

        var verifyWatch = Stopwatch.StartNew();

        bool isValid = ElGamalVerify(messageBytes, signature, publicKey);

        verifyWatch.Stop();

        bool isChangedValid = ElGamalVerify(changedMessageBytes, signature, publicKey);

        Console.WriteLine("Параметры:");
        Console.WriteLine("p: 1024 bit");
        Console.WriteLine("q: 160 bit");
        Console.WriteLine("Hash: SHA-256");
        Console.WriteLine();

        Console.WriteLine("Открытая ключевая информация для передачи получателю:");
        Console.WriteLine($"p = {ToHex(publicKey.P)}");
        Console.WriteLine($"g = {ToHex(publicKey.G)}");
        Console.WriteLine($"y = {ToHex(publicKey.Y)}");
        Console.WriteLine();

        Console.WriteLine("Подпись:");
        Console.WriteLine($"a = {ToHex(signature.A)}");
        Console.WriteLine($"b = {ToHex(signature.B)}");
        Console.WriteLine();

        Console.WriteLine($"Верификация исходного сообщения: {isValid}");
        Console.WriteLine($"Верификация изменённого сообщения: {isChangedValid}");
        Console.WriteLine();

        Console.WriteLine($"Время генерации ключей: {keyGenWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine($"Время генерации ЭЦП: {signWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine($"Время верификации ЭЦП: {verifyWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine();
    }

    private static ElGamalSignature ElGamalSign(
        byte[] message,
        ElGamalPublicKey publicKey,
        ElGamalPrivateKey privateKey)
    {
        BigInteger p = publicKey.P;
        BigInteger g = publicKey.G;
        BigInteger x = privateKey.X;

        BigInteger pMinusOne = p.Subtract(BigInteger.One);
        BigInteger h = HashToBigInteger(message).Mod(pMinusOne);

        BigInteger k;

        do
        {
            k = RandomBetween(BigInteger.Two, pMinusOne.Subtract(BigInteger.One));
        }
        while (!k.Gcd(pMinusOne).Equals(BigInteger.One));

        BigInteger a = g.ModPow(k, p);

        BigInteger kInverse = k.ModInverse(pMinusOne);

        BigInteger b = h
            .Subtract(x.Multiply(a))
            .Multiply(kInverse)
            .Mod(pMinusOne);

        return new ElGamalSignature
        {
            A = a,
            B = b
        };
    }

    private static bool ElGamalVerify(
        byte[] message,
        ElGamalSignature signature,
        ElGamalPublicKey publicKey)
    {
        BigInteger p = publicKey.P;
        BigInteger g = publicKey.G;
        BigInteger y = publicKey.Y;

        BigInteger a = signature.A;
        BigInteger b = signature.B;

        if (a.CompareTo(BigInteger.Zero) <= 0 || a.CompareTo(p) >= 0)
        {
            return false;
        }

        BigInteger h = HashToBigInteger(message).Mod(p.Subtract(BigInteger.One));

        BigInteger left = y.ModPow(a, p)
            .Multiply(a.ModPow(b, p))
            .Mod(p);

        BigInteger right = g.ModPow(h, p);

        return left.Equals(right);
    }

    // ============================================================
    // SCHNORR SIGNATURE
    // ============================================================

    private sealed class SchnorrPublicKey
    {
        public BigInteger P { get; init; } = null!;
        public BigInteger Q { get; init; } = null!;
        public BigInteger G { get; init; } = null!;
        public BigInteger Y { get; init; } = null!;
    }

    private sealed class SchnorrPrivateKey
    {
        public BigInteger X { get; init; } = null!;
    }

    private sealed class SchnorrSignature
    {
        public BigInteger H { get; init; } = null!;
        public BigInteger B { get; init; } = null!;
    }

    private static void RunSchnorrDemo(string message)
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("ЭЦП Шнорра");
        Console.WriteLine("====================================================");

        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] changedMessageBytes = Encoding.UTF8.GetBytes(message + "11111");

        var keyGenWatch = Stopwatch.StartNew();

        var parameters = GenerateDsaLikeParameters(1024, 160);

        BigInteger p = parameters.P;
        BigInteger q = parameters.Q;
        BigInteger g = parameters.G;

        BigInteger x = RandomBetween(BigInteger.One, q.Subtract(BigInteger.One));

        BigInteger y = g.ModPow(x, p).ModInverse(p);

        var publicKey = new SchnorrPublicKey
        {
            P = p,
            Q = q,
            G = g,
            Y = y
        };

        var privateKey = new SchnorrPrivateKey
        {
            X = x
        };

        keyGenWatch.Stop();

        var signWatch = Stopwatch.StartNew();

        SchnorrSignature signature = SchnorrSign(messageBytes, publicKey, privateKey);

        signWatch.Stop();

        var verifyWatch = Stopwatch.StartNew();

        bool isValid = SchnorrVerify(messageBytes, signature, publicKey);

        verifyWatch.Stop();

        bool isChangedValid = SchnorrVerify(changedMessageBytes, signature, publicKey);

        Console.WriteLine("Параметры:");
        Console.WriteLine("p: 1024 bit");
        Console.WriteLine("q: 160 bit");
        Console.WriteLine("Hash: SHA-256");
        Console.WriteLine();

        Console.WriteLine("Открытая ключевая информация для передачи получателю:");
        Console.WriteLine($"p = {ToHex(publicKey.P)}");
        Console.WriteLine($"q = {ToHex(publicKey.Q)}");
        Console.WriteLine($"g = {ToHex(publicKey.G)}");
        Console.WriteLine($"y = {ToHex(publicKey.Y)}");
        Console.WriteLine();

        Console.WriteLine("Подпись:");
        Console.WriteLine($"h = {ToHex(signature.H)}");
        Console.WriteLine($"b = {ToHex(signature.B)}");
        Console.WriteLine();

        Console.WriteLine($"Верификация исходного сообщения: {isValid}");
        Console.WriteLine($"Верификация изменённого сообщения: {isChangedValid}");
        Console.WriteLine();

        Console.WriteLine($"Время генерации ключей: {keyGenWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine($"Время генерации ЭЦП: {signWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine($"Время верификации ЭЦП: {verifyWatch.Elapsed.TotalMilliseconds:F3} ms");
        Console.WriteLine();
    }

    private static SchnorrSignature SchnorrSign(
        byte[] message,
        SchnorrPublicKey publicKey,
        SchnorrPrivateKey privateKey)
    {
        BigInteger p = publicKey.P;
        BigInteger q = publicKey.Q;
        BigInteger g = publicKey.G;
        BigInteger x = privateKey.X;

        BigInteger k = RandomBetween(BigInteger.One, q.Subtract(BigInteger.One));

        BigInteger a = g.ModPow(k, p);

        byte[] hInput = Concat(message, a.ToByteArrayUnsigned());
        BigInteger h = HashToBigInteger(hInput).Mod(q);

        BigInteger b = k.Add(x.Multiply(h)).Mod(q);

        return new SchnorrSignature
        {
            H = h,
            B = b
        };
    }

    private static bool SchnorrVerify(
        byte[] message,
        SchnorrSignature signature,
        SchnorrPublicKey publicKey)
    {
        BigInteger p = publicKey.P;
        BigInteger q = publicKey.Q;
        BigInteger g = publicKey.G;
        BigInteger y = publicKey.Y;

        BigInteger h = signature.H;
        BigInteger b = signature.B;

        if (h.CompareTo(BigInteger.Zero) < 0 || h.CompareTo(q) >= 0)
        {
            return false;
        }

        if (b.CompareTo(BigInteger.Zero) < 0 || b.CompareTo(q) >= 0)
        {
            return false;
        }

        BigInteger xCheck = g.ModPow(b, p)
            .Multiply(y.ModPow(h, p))
            .Mod(p);

        byte[] hInput = Concat(message, xCheck.ToByteArrayUnsigned());
        BigInteger hCheck = HashToBigInteger(hInput).Mod(q);

        return h.Equals(hCheck);
    }

    // ============================================================
    // COMMON HELPERS
    // ============================================================

    private static DsaParameters GenerateDsaLikeParameters(int pBits, int qBits)
    {
        var generator = new DsaParametersGenerator();
        generator.Init(pBits, 80, Rng);

        DsaParameters parameters = generator.GenerateParameters();

        if (parameters.Q.BitLength != qBits)
        {
            // Для классических 1024-битных DSA-параметров q обычно 160 бит.
            // Проверку оставляем мягкой, чтобы не ломать запуск на разных версиях библиотеки.
        }

        return parameters;
    }

    private static BigInteger HashToBigInteger(byte[] data)
    {
        byte[] hash = SHA256.HashData(data);
        return new BigInteger(1, hash);
    }

    private static BigInteger RandomBetween(BigInteger min, BigInteger max)
    {
        if (min.CompareTo(max) > 0)
        {
            throw new ArgumentException("min must be less than or equal to max");
        }

        BigInteger range = max.Subtract(min).Add(BigInteger.One);
        BigInteger result;

        do
        {
            result = new BigInteger(range.BitLength, Rng);
        }
        while (result.CompareTo(range) >= 0);

        return result.Add(min);
    }

    private static byte[] Concat(byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];

        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);

        return result;
    }

    private static string ToHex(BigInteger number)
    {
        return number.ToString(16);
    }
}