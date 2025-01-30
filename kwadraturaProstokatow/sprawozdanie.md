# Sprawozdanie

## 1. Treść zadania

**TEMAT nr 6.**  
> Napisz program obliczający wartość całki oznaczonej, wykorzystujący złożoną kwadraturę prostokątów.
> Należy wybrać przykłady funkcji podcałkowej, przedział całkowania oraz liczbę podziału przedziału całkowania.  
> Obliczenia wykonane za pomocą programu można uzupełnić obliczeniami analitycznymi w celu sprawdzenia dokładności obliczeń i szacowania błędu.

W ramach tematu:

1. Opracowano program (w języku C#) do obliczania wartości całki oznaczonej zadanego wyrażenia \( f(x) \) na przedziale \([a, b]\) przy użyciu **złożonej kwadratury prostokątów (wersja środkowa)**.  
2. Dodano możliwość **automatycznego szacowania błędu** na podstawie **ekstrapolacji Richardson** – co pozwala uzyskać przybliżenie \(\int f(x)\,dx\) i estymację dokładności bez konieczności podawania wartości analitycznej.  
3. Wprowadzono obsługę różnych funkcji (w tym potęgowanie, pierwiastkowanie, funkcje trygonometryczne i logarytm).  
4. Zapewniono przykładowe testy i wyniki obliczeń.

---

## 2. Opis użytych metod numerycznych

### 2.1 Złożona kwadratura prostokątów (środkowa)

Metoda złożonych prostokątów polega na:

1. Podziale przedziału \([a,b]\) na \(n\) równych podprzedziałów o szerokości
   \[
   h = \frac{b-a}{n}.
   \]
2. Wybieraniu **punktu środkowego** w każdym podprzedziale:
   \[
   x_k^* = a + \left(k + 0.5\right) h,
   \quad k = 0, 1, \dots, n-1.
   \]
3. Sumowaniu wartości funkcji w tych punktach środkowych, pomnożonych przez \(h\):
   \[
   I_n = \sum_{k=0}^{n-1} f\!\bigl(x_k^*\bigr)\,h.
   \]
Ta metoda jest relatywnie prosta w implementacji i daje przyzwoite przybliżenia dla funkcji ciągłych.

### 2.2 Ekstrapolacja Richardson – szacowanie błędu

Chcąc **oszacować** błąd metody, obliczamy całkę dwukrotnie:

- \(I_n\) przy \(n\) podprzedziałach,  
- \(I_{2n}\) przy \(2n\) podprzedziałach (czyli dwa razy gęstszy podział).

Dla prostokątów środkowych błąd maleje w przybliżeniu 4-krotnie przy podwojeniu \(n\). Pozwala to wyznaczyć tzw. **ulepszone** przybliżenie:
\[
I_{R} \;=\; \frac{4\,I_{2n} - I_{n}}{3},
\]
a **szacowany błąd**:
\[
\varepsilon \;\approx\; \frac{\lvert I_{2n} - I_{n}\rvert}{3}.
\]
W ten sposób, nawet bez znajomości \(\int f(x)\) metodami analitycznymi, możemy ustalić, jak dokładny jest wynik.

---

## 3. Omówienie (i treść) wszystkich napisanych programów obliczeniowych

Kod w języku C# został rozbity na kilka plików, aby klarownie oddzielić poszczególne elementy aplikacji:

1. **Program.cs**  
   - Zawiera metodę `Main`, w której zachodzi:
     1. Wczytanie wyrażenia `f(x)` od użytkownika,  
     2. Weryfikacja poprawności nawiasów (Parser.ValidateParentheses),  
     3. Tokenizacja (Tokenizer) i parsowanie (Parser -> RPN),  
     4. Wczytanie danych całkowania (`a`, `b`, `n`),  
     5. Obliczenie \(I_n\) oraz \(I_{2n}\) metodą prostokątów,  
     6. Ekstrapolacja Richardson w celu uzyskania `I_R` i błędu,  
     7. Wyświetlenie wyniku i zapisanie do pliku `wynik.md`.

2. **Tokenizer.cs**  
   - Skanuje wyrażenie w postaci string i rozbija je na tzw. „tokeny”: liczby, `x`, nawiasy, operatory, nazwy funkcji.  
   - Obsługuje funkcje: `sqrt`, `sin`, `cos`, `tan`, `log` oraz operator potęgowania `^`.

3. **Parser.cs**  
   - Zawiera metodę **ValidateParentheses** (sprawdza, czy nawiasy są poprawnie ułożone)  
   - Zawiera metodę **ConvertToRPN** implementującą algorytm *shunting yard* (Dijkstra) – konwersja z notacji infiksowej na odwrotną polską (RPN).

4. **Counter.cs**  
   - Metoda **EvaluateRPN**: interpretuje listę tokenów w RPN, wykorzystując stos do obsługi operatorów i funkcji.  
   - Metoda **CompositeRectangle**: realizuje złożoną kwadraturę prostokątów (środkową) na zadanym przedziale [a,b] z n podprzedziałami.

5. **Analyzer.cs**  
   - Metoda **AnalyzeError**: oblicza błąd bezwzględny i względny względem wartości analitycznej (opcjonalnie, jeśli znamy \(\int f(x)\)).  
   - Metoda **SaveResults**: generuje plik markdown z informacjami o obliczonej całce, błędzie i parametrach zadania.

### Fragment przykładowego kodu (Program.cs)

```csharp
static void Main(string[] args)
{
    Console.WriteLine("=== Złożona kwadratura prostokątów z automatycznym szacowaniem błędu ===\n");

    // 1. Wczytanie wyrażenia
    string expression = PromptExpression();

    // 2. Sprawdzenie nawiasów
    if (!Parser.ValidateParentheses(expression))
    {
        Console.WriteLine("Błąd nawiasów!");
        return;
    }

    // 3. Tokenizacja + Parser -> RPN
    var tokens = Tokenizer.Tokenize(expression);
    var rpn = Parser.ConvertToRPN(tokens);

    // 4. Wczytanie danych całkowania (a,b,n)
    double a = ReadDouble("Podaj a: ");
    double b = ReadDouble("Podaj b: ");
    int n = ReadInt("Podaj n: ");

    // 5. Obliczenie I_n i I_2n
    double I_n = Counter.CompositeRectangle(x => Counter.EvaluateRPN(rpn, x), a, b, n);
    double I_2n = Counter.CompositeRectangle(x => Counter.EvaluateRPN(rpn, x), a, b, 2*n);

    // 6. Ekstrapolacja Richardson
    double I_R = (4*I_2n - I_n)/3.0;
    double estError = Math.Abs(I_2n - I_n)/3.0;

    // 7. Wyświetlenie i zapis
    Console.WriteLine($"I_n   = {I_n}");
    Console.WriteLine($"I_2n  = {I_2n}");
    Console.WriteLine($"I_R   = {I_R}");
    Console.WriteLine($"Błąd  = {estError}");
    Analyzer.SaveResults("wynik.md", expression, a, b, n, I_R, null, estError, null);

    Console.WriteLine("\nZapisano do pliku wynik.md");
}
