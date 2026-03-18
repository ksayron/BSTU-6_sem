package main

import (
	"fmt"
	"math"
	"sort"
)

func gcd(a, b int) int {
	if a < 0 {
		a = -a
	}
	if b < 0 {
		b = -b
	}

	for b != 0 {
		a, b = b, a%b
	}
	return a
}

func gcd3(a, b, c int) int {
	return gcd(a, gcd(b, c))
}

func findPrimes(limit int) []int {
	if limit < 2 {
		return []int{}
	}
	isComposite := make([]bool, limit+1)

	for i := 2; i*i <= limit; i++ {
		if !isComposite[i] {
			for j := i * i; j <= limit; j += i {
				isComposite[j] = true
			}
		}
	}

	var primes []int
	for i := 2; i <= limit; i++ {
		if !isComposite[i] {
			primes = append(primes, i)
		}
	}
	return primes
}

type PrimeFactor struct {
	Prime    int
	Exponent int
}

func factorize(n int) []PrimeFactor {
	if n < 2 {
		return nil
	}

	var factors []PrimeFactor

	count := 0
	for n%2 == 0 {
		count++
		n /= 2
	}
	if count > 0 {
		factors = append(factors, PrimeFactor{Prime: 2, Exponent: count})
	}

	for i := 3; i*i <= n; i += 2 {
		count = 0
		for n%i == 0 {
			count++
			n /= i
		}
		if count > 0 {
			factors = append(factors, PrimeFactor{Prime: i, Exponent: count})
		}
	}

	if n > 2 {
		factors = append(factors, PrimeFactor{Prime: n, Exponent: 1})
	}

	return factors
}

func main() {
	var choice int

	for {
		printMenu()
		_, err := fmt.Scan(&choice)
		if err != nil {
			fmt.Println("Ошибка ввода. Пожалуйста, введите число.")
			var dump string
			fmt.Scanln(&dump)
			continue
		}

		switch choice {
		case 1:
			handleGCD2()
		case 2:
			handleGCD3()
		case 3:
			handlePrimesSearch()
		case 4:
			handlePrimesSearch2()
		case 5:
			handleFactorize()
		case 0:
			fmt.Println("Выход из программы. До свидания!")
			return
		default:
			fmt.Println("Неверный пункт меню. Попробуйте снова.")
		}
	}
}

func printMenu() {
	fmt.Println("\n--- ГЛАВНОЕ МЕНЮ ---")
	fmt.Println("1. Вычислить НОД двух чисел")
	fmt.Println("2. Вычислить НОД трех чисел")
	fmt.Println("3. Найти простые числа до N")
	fmt.Println("0. Выход")
	fmt.Print("-> Ваш выбор: ")
}

func handleGCD2() {
	var a, b int
	fmt.Print("Введите два числа через пробел: ")
	_, err := fmt.Scan(&a, &b)
	if err != nil {
		fmt.Println("Ошибка: введите целые числа.")
		return
	}
	result := gcd(a, b)
	fmt.Printf("НОД(%d, %d) = %d\n", a, b, result)
}

func handleGCD3() {
	var a, b, c int
	fmt.Print("Введите три числа через пробел: ")
	_, err := fmt.Scan(&a, &b, &c)
	if err != nil {
		fmt.Println("Ошибка: введите целые числа.")
		return
	}
	result := gcd3(a, b, c)
	fmt.Printf("НОД(%d, %d, %d) = %d\n", a, b, c, result)
}

func handlePrimesSearch() {
	var n int
	fmt.Print("Введите верхнюю границу поиска (N): ")
	_, err := fmt.Scan(&n)
	if err != nil || n < 2 {
		fmt.Println("Ошибка: введите целое число больше 1.")
		return
	}

	primes := findPrimes(n)

	fmt.Printf("Найдены простые числа до %d (%d шт.):\n", n, len(primes))
	fmt.Printf("Соотношение к логарифму: %f \n", math.Round(float64(n)/math.Log(float64(n))))

	fmt.Println(primes)
}

func handlePrimesSearch2() {
	var min, max int

	fmt.Print("Введите нижнюю границу поиска: ")
	_, err := fmt.Scan(&min)
	if err != nil {
		fmt.Println("Ошибка: введите корректное целое число для нижней границы.")
		return
	}

	fmt.Print("Введите верхнюю границу поиска (N): ")
	_, err = fmt.Scan(&max)
	if err != nil || max < 2 {
		fmt.Println("Ошибка: верхняя граница должна быть целым числом больше 1.")
		return
	}

	if min > max {
		fmt.Println("Ошибка: нижняя граница не может быть больше верхней.")
		return
	}

	primes := findPrimes(max)

	startIndex := sort.SearchInts(primes, min)

	result := primes[startIndex:]

	fmt.Printf("Найдены простые числа от %d до %d (%d шт.):\n", min, max, len(result))
	fmt.Printf("Соотношение к логарифму: %f \n", math.Round(float64(max)/math.Log(float64(max))))
	fmt.Println(result)
}

func handleFactorize() {
	var n int
	fmt.Print("Введите число для канонического разложения: ")
	_, err := fmt.Scan(&n)
	if err != nil || n < 2 {
		fmt.Println("Ошибка: введите целое число больше 1.")
		return
	}

	factors := factorize(n)

	fmt.Printf("Каноническое разложение числа %d:\n", n)
	for i, f := range factors {
		if f.Exponent == 1 {
			fmt.Printf("%d", f.Prime)
		} else {
			fmt.Printf("%d^%d", f.Prime, f.Exponent)
		}
		if i < len(factors)-1 {
			fmt.Print(" * ")
		}
	}
	fmt.Println()
}
