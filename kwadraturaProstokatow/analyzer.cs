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
        /// Metoda <c>SaveResults</c> zapisuje główne wyniki obliczeń do pliku 
        /// w formacie Markdown (z rozszerzeniem .md).
        /// 
        /// W pliku zapisujemy:
        /// 1. Wyrażenie, które całkujemy,
        /// 2. Przedział całkowania [a, b] i liczbę podprzedziałów (n),
        /// 3. Wynik numeryczny <paramref name="numericResult"/> (np. I_R),
        /// 4. Ewentualny szacowany błąd <paramref name="absError"/> – jeśli nie jest null,
        /// 5. Datę i godzinę wygenerowania pliku.
        /// 
        /// Parametry:
        /// - <paramref name="fileName"/> to nazwa pliku wyjściowego (np. "wynik.md").
        /// - <paramref name="expression"/> to ciąg znaków opisujący funkcję f(x).
        /// - <paramref name="a"/>, <paramref name="b"/>: granice całkowania.
        /// - <paramref name="n"/>: liczba podprzedziałów (pomocnicza informacja).
        /// - <paramref name="numericResult"/>: obliczona wartość całki (np. z ekstrapolacji Richardson).
        /// - <paramref name="knownValue"/>: wartość analityczna, o ile istnieje (tutaj może być null).
        /// - <paramref name="absError"/>, <paramref name="relError"/>: obliczony błąd bezwzględny 
        ///   i względny (mogą być null, jeśli nie dotyczy).
        /// 
        /// Plik wynikowy pozwala na szybki wgląd w to, co policzyliśmy i jak duży błąd oszacowaliśmy.
        /// </summary>
        /// <param name="fileName">Nazwa pliku wyjściowego, np. "wynik.md".</param>
        /// <param name="expression">Tekst opisujący wyrażenie f(x) całkowane w programie.</param>
        /// <param name="a">Początek przedziału całkowania.</param>
        /// <param name="b">Koniec przedziału całkowania.</param>
        /// <param name="n">Liczba podprzedziałów (n).</param>
        /// <param name="numericResult">
        /// Ostateczny wynik numeryczny całkowania, np. wartość z ekstrapolacji Richardsona.
        /// </param>
        /// <param name="knownValue">
        /// Opcjonalna wartość analityczna (jeśli użytkownik/kod ją zna).
        /// Można wykorzystać do porównania z <paramref name="numericResult"/>.
        /// </param>
        /// <param name="absError">
        /// Ewentualny błąd bezwzględny (z ekstrapolacji lub w porównaniu z <paramref name="knownValue"/>).
        /// </param>
        /// <param name="relError">
        /// Ewentualny błąd względny (z ekstrapolacji lub w porównaniu z <paramref name="knownValue"/>).
        /// </param>
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
            sw.WriteLine("- `I_n`: wynikiem całkowania z n podprzedziałami.");
            sw.WriteLine("- `I_2n`: wynik z 2n podprzedziałami (podwojona dokładność).");
            sw.WriteLine("- `I_R`: jeszcze lepsze przybliżenie obliczone dzięki ekstrapolacji Richardson.");
            sw.WriteLine("- `error`: szacowany błąd numeryczny (pokazuje, o ile się możemy mylić).");

            sw.WriteLine();
            sw.WriteLine("---");
            sw.WriteLine($"Plik wygenerowany: {DateTime.Now}");
        }
    }
}
