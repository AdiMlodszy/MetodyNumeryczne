using System;
using System.Collections.Generic;

namespace CompositeRectangleIntegration.Counting
{
    /// <summary>
    /// Klasa <c>Counter</c> zawiera:
    /// 1) Funkcję <c>EvaluateRPN</c> – służącą do ewaluacji (obliczania wartości) wyrażenia
    ///    w notacji odwrotnej polskiej (RPN) dla konkretnej wartości x.
    /// 2) Funkcję <c>CompositeRectangle</c> – implementującą metodę złożonych prostokątów (w wersji środkowej)
    ///    do numerycznego obliczania całki oznaczonej <c>∫(a,b) f(x) dx</c>.
    /// </summary>
    public static class Counter
    {
        /// <summary>
        /// Metoda <c>EvaluateRPN</c> służy do obliczania wartości wyrażenia zapisanego w notacji odwrotnej polskiej (RPN).
        /// 
        /// Obsługiwane elementy:
        /// - Liczby (np. 3.14),
        /// - Zmienna 'x' (podstawiana wartością <paramref name="xValue"/>),
        /// - Funkcje: <c>sqrt, sin, cos, tan, log</c>,
        /// - Operatory arytmetyczne: <c>+, -, *, /, ^</c> (potęgowanie).
        /// 
        /// Algorytm:
        /// 1. Tworzymy stos typu <c>Stack&lt;double&gt;</c>.
        /// 2. Dla każdego tokena w <paramref name="rpnTokens"/>:
        ///    - Jeśli token jest liczbą (double.TryParse), wrzucamy ją na stos.
        ///    - Jeśli token to "x", wrzucamy na stos wartość <paramref name="xValue"/>.
        ///    - Jeśli token to nazwa funkcji (np. "sin"), pobieramy jeden argument ze stosu (top),
        ///      obliczamy <c>Math.Sin(argument)</c> i odkładamy wynik na stos.
        ///    - W wypadku operatora (np. "+", "-", "^"), zdejmujemy dwa argumenty ze stosu 
        ///      (uwaga na kolejność!) i wykonujemy działanie, po czym wynik wraca na stos.
        /// 3. Po przetworzeniu wszystkich tokenów na stosie powinna zostać dokładnie jedna wartość
        ///    (wynik całego wyrażenia). Jeśli jest inaczej, rzucamy wyjątek.
        /// 
        /// Przykład:
        /// RPN = ["2", "x", "*", "3", "+"] i xValue=4 => stosowa ewaluacja daje (2*4+3)=11.
        /// 
        /// Wyjątki:
        /// - Rzucane, gdy np. brakuje argumentów w stosie dla operatora/funkcji,
        ///   lub gdy występuje próba sqrt z liczby ujemnej, log z &lt;= 0, itp.
        /// 
        /// </summary>
        /// <param name="rpnTokens">
        /// Lista tokenów w notacji odwrotnej polskiej, np. ["2","x","*", "sin"].
        /// Mogą to być też tokeny operatorów i funkcji. 
        /// </param>
        /// <param name="xValue">
        /// Wartość zmiennej 'x', która zostanie wstawiona do wyrażenia 
        /// w miejsce wystąpienia tokena "x".
        /// </param>
        /// <returns>
        /// Pojedyncza liczba <c>double</c> stanowiąca wynik obliczenia 
        /// wyrażenia zapisanego w RPN.
        /// </returns>
        public static double EvaluateRPN(List<string> rpnTokens, double xValue)
        {
            // Stos do przechowywania liczb w trakcie ewaluacji
            var stack = new Stack<double>();

            // Główna pętla przetwarzająca kolejne tokeny
            foreach (var token in rpnTokens)
            {
                // 1. Liczba? (double)
                if (double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                // 2. Zmienna x
                else if (token == "x")
                {
                    stack.Push(xValue);
                }
                // 3. Funkcja sqrt
                else if (token == "sqrt")
                {
                    // Pobieramy jeden argument ze stosu
                    if (stack.Count < 1) 
                        throw new Exception("Błąd: sqrt() bez argumentu.");

                    double arg = stack.Pop();
                    if (arg < 0) 
                        throw new Exception("Błąd: sqrt z liczby ujemnej.");

                    // Wykonanie funkcji i zapis wyniku na stosie
                    stack.Push(Math.Sqrt(arg));
                }
                // 4. Funkcja sin
                else if (token == "sin")
                {
                    if (stack.Count < 1) 
                        throw new Exception("Błąd: sin() bez argumentu.");

                    double arg = stack.Pop();
                    stack.Push(Math.Sin(arg));
                }
                // 5. Funkcja cos
                else if (token == "cos")
                {
                    if (stack.Count < 1) 
                        throw new Exception("Błąd: cos() bez argumentu.");

                    double arg = stack.Pop();
                    stack.Push(Math.Cos(arg));
                }
                // 6. Funkcja tan
                else if (token == "tan")
                {
                    if (stack.Count < 1) 
                        throw new Exception("Błąd: tan() bez argumentu.");

                    double arg = stack.Pop();
                    stack.Push(Math.Tan(arg));
                }
                // 7. Funkcja log
                else if (token == "log")
                {
                    if (stack.Count < 1) 
                        throw new Exception("Błąd: log() bez argumentu.");

                    double arg = stack.Pop();
                    if (arg <= 0) 
                        throw new Exception("Błąd: log z argumentu <= 0.");

                    stack.Push(Math.Log(arg)); // Math.Log => logarytm naturalny
                }
                // 8. Zakładamy, że to operator: +, -, *, /, ^
                else
                {
                    // Dla operatora potrzebujemy dwóch argumentów na stosie
                    if (stack.Count < 2) 
                        throw new Exception($"Błąd: za mało argumentów dla operatora '{token}'.");

                    // Pobieramy argumenty w odwrotnej kolejności: 
                    // najpierw 'b' (drugi w kolejności), potem 'a' (pierwszy)
                    double b = stack.Pop();
                    double a = stack.Pop();

                    switch (token)
                    {
                        case "+": stack.Push(a + b); break;
                        case "-": stack.Push(a - b); break;
                        case "*": stack.Push(a * b); break;
                        case "/": stack.Push(a / b); break;
                        case "^": stack.Push(Math.Pow(a, b)); break;
                        default: 
                            throw new Exception($"Nieznany operator: {token}");
                    }
                }
            }

            // Po przetworzeniu wszystkich tokenów sprawdzamy,
            // czy na stosie jest dokładnie jedna wartość – wynik końcowy.
            if (stack.Count != 1)
                throw new Exception("Błąd: nieprawidłowa liczba elementów na stosie po EvaluateRPN.");

            // Zdejmujemy wynik i zwracamy
            return stack.Pop();
        }

        /// <summary>
        /// Metoda <c>CompositeRectangle</c> implementuje złożoną kwadraturę prostokątów (wersję środkową)
        /// na przedziale <c>[a, b]</c> z <c>n</c> równymi podprzedziałami.
        /// 
        /// Wzór:
        /// <code>
        /// Δx = (b - a) / n;
        /// x_k* = a + (k + 0.5)*Δx;
        /// I ≈ Σ (f(x_k*) * Δx)  dla  k=0..n-1
        /// </code>
        /// 
        /// Algorytm:
        /// 1. Dzielimy przedział [a,b] na n podprzedziałów o jednakowej szerokości h = (b-a)/n.
        /// 2. W każdej iteracji k od 0 do n-1 wyznaczamy środek podprzedziału:
        ///    xMid = a + (k+0.5)*h
        /// 3. Sumujemy wartości f(xMid) i na końcu mnożymy przez h, otrzymując przybliżenie całki.
        /// 
        /// Zastosowanie:
        /// - <paramref name="f"/> to delegat funkcji f(x), np. EvaluateRPN.
        /// - <paramref name="a"/> i <paramref name="b"/> – granice całkowania.
        /// - <paramref name="n"/> – liczba podziałów (im większa, tym dokładniejsze przybliżenie,
        ///   ale czas obliczeń rośnie).
        /// 
        /// Przykład:
        /// Jeśli f(x)=x^2, a=0, b=1 i n=10, funkcja obliczy sumę f(środek każdego z 10 podprzedziałów)*h.
        /// </summary>
        /// <param name="f">
        /// Delegat przyjmujący <c>double</c> i zwracający <c>double</c>,
        /// reprezentujący naszą funkcję podcałkową f(x).
        /// </param>
        /// <param name="a">Początek przedziału całkowania.</param>
        /// <param name="b">Koniec przedziału całkowania.</param>
        /// <param name="n">Liczba podprzedziałów (musi być dodatnia).</param>
        /// <returns>
        /// Przybliżona wartość całki <c>∫(a,b) f(x) dx</c> obliczona metodą złożonych prostokątów (środkową).
        /// </returns>
        public static double CompositeRectangle(Func<double, double> f, double a, double b, int n)
        {
            // h = szerokość każdego podprzedziału
            double h = (b - a) / n;
            double sum = 0.0;

            // Pętla od k=0 do k=n-1
            // xMid – środek k-tego podprzedziału
            for (int k = 0; k < n; k++)
            {
                double xMid = a + (k + 0.5) * h;
                // Dodajemy f(środek) do sumy
                sum += f(xMid);
            }

            // Całka ≈ sum * h
            return sum * h;
        }
    }
}
