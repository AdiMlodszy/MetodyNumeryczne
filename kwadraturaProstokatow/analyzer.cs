using System;
using System.IO;

namespace CompositeRectangleIntegration.Analyzing
{
    /// <summary>
    /// Klasa <c>Analyzer</c> przechowuje metody:
    /// 1. <c>AnalyzeError</c>: służącą do wyliczenia błędu bezwzględnego i względnego 
    ///    względem wartości analitycznej (jeśli jest znana).
    /// 2. <c>SaveResults</c>: służącą do zapisu wyników obliczeń (np. całki i błędu) 
    ///    do pliku tekstowego w formacie Markdown.
    /// </summary>
    public static class Analyzer
    {
        /// <summary>
        /// Metoda <c>AnalyzeError</c> oblicza błąd bezwzględny i względny:
        /// <code>
        /// absError = |numericVal - exactVal|
        /// relError = absError / |exactVal|
        /// </code>
        /// 
        /// Parametr <paramref name="numericVal"/> to obliczona wartość metodą numeryczną (np. prostokątów),
        /// a <paramref name="exactVal"/> to wartość analityczna, jeśli ją znamy. 
        /// Metoda zwraca krotkę <c>(absError, relError)</c>, gdzie:
        /// - <c>absError</c> to błąd bezwzględny,
        /// - <c>relError</c> to błąd względny.
        /// 
        /// Uwaga: W programie wykorzystujemy często szacowanie błędu metodą ekstrapolacji Richardsona 
        /// (porównanie I_n i I_2n), ale gdy posiadamy wartość analityczną, możemy porównać też 
        /// z tą wartością – do tego służy niniejsza metoda.
        /// </summary>
        /// <param name="numericVal">Wynik obliczony numerycznie (np. I_R z ekstrapolacji).</param>
        /// <param name="exactVal">Dokładna (analityczna) wartość całki (jeśli znana).</param>
        /// <returns>
        /// Krotka <c>(double absError, double relError)</c>, 
        /// gdzie <c>absError</c> = |numericVal - exactVal|, 
        /// a <c>relError</c> = absError / |exactVal|.
        /// </returns>
        public static (double absError, double relError) AnalyzeError(double numericVal, double exactVal)
        {
            double absErr = Math.Abs(numericVal - exactVal);
            double relErr = absErr / Math.Abs(exactVal);
            return (absErr, relErr);
        }

        /// <summary>
        /// Metoda <c>SaveResults</c> zapisuje szczegółowe wyniki obliczeń do pliku 
        /// w formacie Markdown (z rozszerzeniem .md).
        /// 
        /// W pliku zapisujemy m.in.:
        /// - Wyrażenie, które całkujemy,
        /// - Przedział całkowania [a, b] i liczbę podprzedziałów (n),
        /// - Wyniki pośrednie I_n, I_2n,
        /// - Ulepszone przybliżenie I_R (ekstrapolacja Richardson),
        /// - Szacowany błąd (|I_2n - I_n| / 3),
        /// - Datę i godzinę wygenerowania pliku.
        /// </summary>
        /// <param name="fileName">Nazwa pliku wyjściowego (np. "wynik.md").</param>
        /// <param name="expression">Tekst wyrażenia (np. "sqrt(x)+log(x)").</param>
        /// <param name="a">Początek przedziału całkowania.</param>
        /// <param name="b">Koniec przedziału całkowania.</param>
        /// <param name="n">Liczba podprzedziałów (n).</param>
        /// <param name="i_n">Wynik całki z n podprzedziałami.</param>
        /// <param name="i_2n">Wynik całki z 2n podprzedziałami.</param>
        /// <param name="iR">Ulepszone przybliżenie (Richardson): (4*I_2n - I_n)/3.</param>
        /// <param name="error">Szacowany błąd, czyli |I_2n - I_n| / 3.</param>
        public static void SaveResults(
            string fileName,
            string expression,
            double a,
            double b,
            int n,
            double i_n,
            double i_2n,
            double iR,
            double error
        )
        {
            using var sw = new StreamWriter(fileName);

            // Nagłówek pliku
            sw.WriteLine("# Wyniki obliczeń – metoda złożonych prostokątów (środkowych)");
            sw.WriteLine();

            // 1. Dane wejściowe
            sw.WriteLine("## 1. Dane wejściowe");
            sw.WriteLine($"**Wyrażenie f(x):** `{expression}`");
            sw.WriteLine($"**Przedział całkowania [a,b]:** [{a}, {b}]");
            sw.WriteLine($"**Liczba podprzedziałów (n):** {n}");
            sw.WriteLine();

            // 2. Obliczenia krok po kroku
            sw.WriteLine("## 2. Obliczenia krok po kroku");
            sw.WriteLine($"- Podstawowe przybliżenie (n = {n}): `I_n = {i_n}`");
            sw.WriteLine($"- Bardziej zagęszczone (2n = {2*n}): `I_2n = {i_2n}`");
            sw.WriteLine();

            // 3. Ekstrapolacja Richardson
            sw.WriteLine("## 3. Ekstrapolacja Richardson");
            sw.WriteLine("Zgodnie z formułą:");
            sw.WriteLine("```\nI_R = (4 * I_2n - I_n) / 3\nerror = |I_2n - I_n| / 3\n```");
            sw.WriteLine();
            sw.WriteLine($"- **Ulepszone I_R** = `{iR}`");
            sw.WriteLine($"- **Szacowany błąd** = `{error}`");
            sw.WriteLine();

            // 4. Interpretacja wyników
            sw.WriteLine("## 4. Interpretacja wyników");
            sw.WriteLine("- `I_n`: wynik całkowania z n podprzedziałami.");
            sw.WriteLine("- `I_2n`: wynik całkowania z 2n podprzedziałami (podwojona liczba podprzedziałów).");
            sw.WriteLine("- `I_R`: jeszcze lepsze przybliżenie obliczone dzięki ekstrapolacji Richardson.");
            sw.WriteLine("- `error`: szacowany błąd numeryczny (pokazuje, o ile możemy się mylić).");

            sw.WriteLine();
            sw.WriteLine("---");
            sw.WriteLine($"Plik wygenerowany: {DateTime.Now}");
        }
    }
}
