using System;
using System.Globalization;
using CompositeRectangleIntegration.Parsing;
using CompositeRectangleIntegration.Tokenizing;
using CompositeRectangleIntegration.Counting;
using CompositeRectangleIntegration.Analyzing;

namespace CompositeRectangleIntegration
{
    /// <summary>
    /// Klasa Program zawierająca metodę Main, która uruchamia proces:
    /// 1. Wczytania wyrażenia matematycznego f(x),
    /// 2. Walidacji nawiasów, tokenizacji i konwersji na RPN (shunting yard),
    /// 3. Numerycznego obliczenia całki metodą złożonych prostokątów (dla n i 2n podprzedziałów),
    /// 4. Szacowania błędu poprzez ekstrapolację Richardson,
    /// 5. Zapisania wyników do pliku i wyświetlenia informacji w konsoli.
    /// 
    /// Dzięki temu program w pełni automatycznie prezentuje przybliżenie I_R 
    /// oraz błąd ~ |I_(2n) - I_n| / 3, bez konieczności podawania wartości analitycznej przez użytkownika.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Metoda Main – punkt startowy programu. 
        /// 1. Wyświetla nagłówek informacyjny.
        /// 2. Prosi użytkownika o podanie wyrażenia f(x) w postaci infiksowej.
        /// 3. Waliduje poprawność nawiasów.
        /// 4. Tokenizuje wyrażenie na listę tokenów (Tokenizer).
        /// 5. Konwertuje listę tokenów na notację odwrotną (Parser – shunting yard).
        /// 6. Wczytuje dane całkowania: a, b, n.
        /// 7. Liczy całkę dwukrotnie: dla n i 2n (złożona kwadratura prostokątów środkowych).
        /// 8. Stosuje ekstrapolację Richardson, uzyskując lepsze przybliżenie I_R i oszacowanie błędu.
        /// 9. Wyświetla wyniki i zapisuje je do pliku (Analyzer).
        /// 
        /// Program kończy działanie po zapisaniu wyników.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("=== Złożona kwadratura prostokątów (środkowych) z automatycznym szacowaniem błędu ===\n");

            // 1. Wczytanie wyrażenia
            string expression = PromptExpression();

            // 2. Walidacja nawiasów
            if (!Parser.ValidateParentheses(expression))
            {
                Console.WriteLine("Błąd: niepoprawne nawiasy w wyrażeniu!");
                return;
            }

            // 3. Tokenizacja
            var tokens = Tokenizer.Tokenize(expression);

            // 4. Shunting yard -> RPN
            var rpn = Parser.ConvertToRPN(tokens);

            // 5. Wczytanie danych całkowania
            double a = ReadDouble("Podaj początek przedziału całkowania (a): ");
            double b = ReadDouble("Podaj koniec przedziału całkowania (b): ");
            int n = ReadInt("Podaj liczbę podprzedziałów (n): ");

            // 6. Metoda prostokątów (środkowych) – oblicz z n i 2n
            double I_n = Counter.CompositeRectangle(x => Counter.EvaluateRPN(rpn, x), a, b, n);
            double I_2n = Counter.CompositeRectangle(x => Counter.EvaluateRPN(rpn, x), a, b, 2 * n);

            // 7. Ekstrapolacja Richardson
            //    lepsze przybliżenie I_R  = (4 * I_2n - I_n) / 3
            //    błąd   ~ |I_2n - I_n| / 3
            double I_R = (4.0 * I_2n - I_n) / 3.0;
            double estimatedError = Math.Abs(I_2n - I_n) / 3.0;

            // Wyświetlamy wyniki w konsoli
            Console.WriteLine($"\nWyrażenie: f(x) = {expression}");
            Console.WriteLine($"Przedział całkowania: [{a}, {b}]");
            Console.WriteLine($"\nPodstawowe przybliżenie (n = {n}): I_n   = {I_n}");
            Console.WriteLine($"Bardziej zagęszczone (2n = {2*n}): I_2n  = {I_2n}");
            Console.WriteLine($"\nEkstrapolacja Richardson: I_R = {I_R}");
            Console.WriteLine($"Szacowany błąd (metodą R.): E  = {estimatedError}");

            // 8. Zapis do pliku (raport w formacie Markdown)
            Analyzer.SaveResults(
                fileName: "wynik.md",
                expression: expression,
                a: a,
                b: b,
                n: n,
                numericResult: I_R,        // jako "najlepszy" wynik
                knownValue: null,         // nie mamy wartości analitycznej (automatyczne szacowanie)
                absError: estimatedError, // wstawiamy do rubryki "absError"
                relError: null            // relError pomijamy, jeśli nie chcemy go liczyć
            );

            Console.WriteLine("\nWyniki zapisano do pliku 'wynik.md'.");
            Console.WriteLine("=== Koniec programu ===");
        }

        /// <summary>
        /// Metoda pomocnicza PromptExpression:
        /// 1. Wyświetla komunikat o dopuszczalnych operatorach i funkcjach.
        /// 2. Wczytuje wyrażenie od użytkownika jako string.
        /// 3. Usuwa wszystkie spacje (expr.Replace(" ", "")).
        /// 4. Zwraca przetworzony łańcuch.
        /// 
        /// Przykład:
        /// Użytkownik wpisuje: " 2*x + sqrt( x+1 ) "
        /// Metoda zwraca:      "2*x+sqrt(x+1)"
        /// </summary>
        /// <returns>Wyrażenie f(x) bez spacji, w postaci np. "2*x+log(x^2)"</returns>
        static string PromptExpression()
        {
            Console.WriteLine("Podaj wyrażenie f(x) do całkowania.");
            Console.WriteLine("Obsługiwane: +, -, *, /, ^, sqrt(...), sin(...), cos(...), tan(...), log(...)");
            Console.Write("f(x) = ");
            string expr = Console.ReadLine() ?? "";

            // Usuwanie zbędnych spacji
            expr = expr.Replace(" ", "");
            return expr;
        }

        /// <summary>
        /// Metoda pomocnicza ReadDouble:
        /// W pętli:
        /// 1. Wyświetla 'prompt' (np. "Podaj a: "),
        /// 2. Odczytuje string s z konsoli,
        /// 3. Próbuje sparsować s jako double (TryParse z InvariantCulture),
        ///    jeśli się uda – zwraca wartość double,
        ///    jeśli nie – wyświetla komunikat błędu i ponawia pętlę.
        /// </summary>
        /// <param name="prompt">Tekst wyświetlany użytkownikowi, np. "Podaj b: "</param>
        /// <returns>Liczba typu double w formacie InvariantCulture</returns>
        static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine()?.Trim() ?? "";
                // TryParse sprawdza, czy 's' da się zamienić na double
                // z wykorzystaniem reguł dla liczb zmiennoprzecinkowych (NumberStyles.Float)
                // i kropki jako separatora dziesiętnego (CultureInfo.InvariantCulture).
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                {
                    return val;
                }
                Console.WriteLine("Niepoprawna liczba, spróbuj ponownie.");
            }
        }

        /// <summary>
        /// Metoda pomocnicza ReadInt:
        /// 1. Wyświetla 'prompt' (np. "Podaj n: "),
        /// 2. Odczytuje string s z konsoli,
        /// 3. Próbuje sparsować s jako liczbę całkowitą int,
        /// 4. Sprawdza, czy wartość jest dodatnia.
        /// 
        /// Jeśli wszystko jest poprawne, zwraca wynik,
        /// w przeciwnym wypadku wyświetla komunikat o błędzie i ponawia zapytanie.
        /// </summary>
        /// <param name="prompt">Tekst wyświetlany użytkownikowi, np. "Podaj liczbę podprzedziałów (n): "</param>
        /// <returns>Dodatnia liczba całkowita (n > 0)</returns>
        static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine()?.Trim() ?? "";

                // Próbujemy sparsować jako int (CultureInfo.InvariantCulture gwarantuje brak kolizji z locale)
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
                {
                    if (val <= 0)
                    {
                        Console.WriteLine("Wartość musi być dodatnia!");
                        continue;
                    }
                    return val;
                }
                Console.WriteLine("Niepoprawna liczba całkowita, spróbuj ponownie.");
            }
        }
    }
}
