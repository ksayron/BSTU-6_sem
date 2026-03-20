using lab6.Entities;

namespace lab6
{
    public class Program
    {
        // Проводки из таблицы
        const string WIRING_BETA = "LEYJVCNIXWPBQMDRTAKZGFUHOS";
        const string WIRING_GAMMA = "FSOKANUERHMBTIYCWLQPZXVGJD";
        const string WIRING_V = "VZBRGITYUPSDNHLXAWMJQOFECK";
        const string WIRING_REF_B = "YRUHQSLDPXNGOKMIEBFZCWVJAT";

        // Ввод: начальные позиции L, M, R (0–25)
        static EnigmaMachine CreateMachine(int posL, int posM, int posR)
        {
            var left = new Rotor(WIRING_BETA, stepSize: 1, initialPosition: posL);
            var middle = new Rotor(WIRING_GAMMA, stepSize: 2, initialPosition: posM);
            var right = new Rotor(WIRING_V, stepSize: 2, initialPosition: posR);
            var reflector = new Reflector(WIRING_REF_B);
            return new EnigmaMachine(left, middle, right, reflector);
        }

        static Dictionary<char, double> GetFrequency(string text)
        {
            var freq = new Dictionary<char, double>();
            foreach (char c in text)
                freq[c] = freq.GetValueOrDefault(c) + 1;

            foreach (char key in freq.Keys.ToList())
                freq[key] = Math.Round(freq[key] / text.Length * 100, 2);

            return freq;
        }

        static void PrintFrequency(string label, string text)
        {
            Console.WriteLine($"\n  Частота символов ({label}):");
            var freq = GetFrequency(text);
            foreach (var kv in freq.OrderByDescending(x => x.Value))
                Console.WriteLine($"    {kv.Key}: {kv.Value}%");
        }

        static string ReadKey()
        {
            while (true)
            {
                Console.Write("\nВведите ключ (3 латинские буквы, например WYX): ");
                string? input = Console.ReadLine()?.ToUpper().Trim();

                if (input?.Length == 3 && input.All(char.IsLetter))
                    return input;

                Console.WriteLine("Неверный формат. Нужно ровно 3 латинские буквы.");
            }
        }

        static void Main()
        {
            Console.WriteLine("=== ENIGMA SIMULATOR ===");
            Console.WriteLine($"Роторы: L=Beta | M=Gamma | R=V | Reflector=B");
            Console.WriteLine($"Шаг роторов: L=+1 (по обороту M) | M=+2 (по обороту R) | R=+2 (каждый символ)");
            Console.WriteLine(new string('=', 40));

            while (true)
            {
                // Ввод ключа
                string key = ReadKey();
                int posL = key[0] - 'A';
                int posM = key[1] - 'A';
                int posR = key[2] - 'A';

                // Ввод текста
                Console.Write("\nВведите текст: ");
                string input = Console.ReadLine()?.ToUpper() ?? "";
                string cleanInput = new string(input.Where(char.IsLetter).ToArray());

                if (cleanInput.Length == 0)
                {
                    Console.WriteLine("Текст не содержит букв. Попробуйте снова.");
                    continue;
                }

                // Шифрование
                var machine = CreateMachine(posL, posM, posR);
                string output = machine.Encrypt(cleanInput);

                Console.WriteLine($"\nИсходный текст:  {cleanInput}");
                Console.WriteLine($"Результат:       {output}");

                // Частотный анализ
                PrintFrequency("исходный", cleanInput);
                PrintFrequency("результат", output);

                // Проверка симметрии
                machine.Reset();
                string check = machine.Encrypt(output);
                Console.WriteLine($"\nПроверка (обратное):  {check}");
                Console.WriteLine($"Совпадает с вводом:   {check == cleanInput}");

                Console.WriteLine("\n" + new string('-', 40));
            }
        }
    }
}
