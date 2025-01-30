using System;
using System.Collections.Generic;
using System.Globalization;

namespace CompositeRectangleIntegration.Tokenizing
{
    /// <summary>
    /// Klasa Tokenizer odpowiedzialna jest za rozbicie (tokenizację) łańcucha znaków
    /// na podstawowe elementy języka matematycznego: liczby, zmienną x, operatory, nawiasy, 
    /// oraz rozpoznawane funkcje (sqrt, sin, cos, tan, log).
    /// 
    /// Głównym zadaniem klasy jest zapewnienie, że parsowanie wyrażenia będzie 
    /// przebiegać poprawnie i generować listę "tokenów", które można dalej 
    /// przetwarzać np. algorytmem shunting yard.
    /// </summary>
    public static class Tokenizer
    {
        /// <summary>
        /// Metoda Tokenize pobiera wyrażenie matematyczne (expression)
        /// i zwraca listę tokenów (List&lt;string&gt;). 
        /// 
        /// Krok po kroku:
        /// 1. Inicjalizuje listę 'tokens' na przechowanie poszczególnych elementów wyrażenia.
        /// 2. Przechodzi po każdym znaku wejściowego (w pętli while).
        /// 3. Sprawdza, czy fragment tekstu zaczyna się od:
        ///    - Liczby (np. 3.14, 2e-1, -1.2e3 itp.)
        ///    - Zmiennej x
        ///    - Nawiasów (i dodaje je do listy)
        ///    - Operatorów arytmetycznych ^, +, -, *, /
        ///    - Słowa kluczowego będącego nazwą funkcji: sqrt, sin, cos, tan, log
        ///    - Jeśli żaden warunek nie jest spełniony, wyrzuca wyjątek (nieznany token).
        ///
        /// W efekcie otrzymujemy sekwencję tokenów tekstowych, np.:
        /// "2*x + sin(x)" => ["2", "*", "x", "+", "sin", "(", "x", ")"].
        /// </summary>
        /// <param name="expression">Łańcuch znaków reprezentujący wyrażenie matematyczne, np. "2*x+sin(x)"</param>
        /// <returns>Lista tokenów typu string, np. ["2", "*", "x", "+", "sin", "(", "x", ")"]</returns>
        public static List<string> Tokenize(string expression)
        {
            // Inicjalizacja listy na tokeny
            var tokens = new List<string>();

            // 'i' będzie wskaźnikiem na aktualnie przetwarzany znak w wyrażeniu
            int i = 0;

            // Pętla, dopóki nie przejdziemy całego łańcucha
            while (i < expression.Length)
            {
                // Bieżący znak
                char c = expression[i];

                // 1. Liczba (np. 3.14, 2e-1)
                //    Sprawdzamy: czy c jest cyfrą, albo czy jest kropką, po której następuje cyfra
                if (char.IsDigit(c) || (c == '.' && i + 1 < expression.Length && char.IsDigit(expression[i + 1])))
                {
                    // startIndex - początek ciągu reprezentującego liczbę
                    int startIndex = i;
                    i++;

                    // Pętla wewnętrzna, która wchłania wszystkie znaki należące do liczby:
                    // cyfry, kropkę dziesiętną '.', notację naukową 'e'/'E', znak '-' po 'e' itd.
                    while (i < expression.Length &&
                           (char.IsDigit(expression[i]) || expression[i] == '.' ||
                            expression[i] == 'e' || expression[i] == 'E' ||
                            // Dopuszczamy znak '-' BEZPOŚREDNIO po 'e'/'E', aby rozpoznać np. 2e-1
                            (expression[i] == '-' && (expression[i - 1] == 'e' || expression[i - 1] == 'E'))))
                    {
                        i++;
                    }

                    // Odczytany fragment od startIndex do i-1 to jedna liczba
                    tokens.Add(expression.Substring(startIndex, i - startIndex));
                    // Kontynuuj główną pętlę z aktualnym 'i'
                    continue;
                }
                // 2. Zmienna x
                //    Jeśli pojedynczy znak to 'x', traktujemy go jako token reprezentujący zmienną
                else if (c == 'x')
                {
                    tokens.Add("x");
                    i++;
                }
                // 3. Nawiasy (otwierający '(' lub zamykający ')')
                else if (c == '(' || c == ')')
                {
                    tokens.Add(c.ToString());
                    i++;
                }
                // 4. Operatory ^, +, -, *, /
                //    Sprawdzamy, czy dany znak zawiera się w stringu "^+-*/".
                else if ("^+-*/".Contains(c))
                {
                    tokens.Add(c.ToString());
                    i++;
                }
                // 5. Funkcje: sqrt, sin, cos, tan, log
                //    Sprawdzamy, czy w tym miejscu występuje dana sekwencja znaków.
                else if (CheckFunction(expression, i, "sqrt"))
                {
                    tokens.Add("sqrt");
                    i += 4; // przesuwamy wskaźnik o długość słowa "sqrt"
                }
                else if (CheckFunction(expression, i, "sin"))
                {
                    tokens.Add("sin");
                    i += 3;
                }
                else if (CheckFunction(expression, i, "cos"))
                {
                    tokens.Add("cos");
                    i += 3;
                }
                else if (CheckFunction(expression, i, "tan"))
                {
                    tokens.Add("tan");
                    i += 3;
                }
                else if (CheckFunction(expression, i, "log"))
                {
                    tokens.Add("log");
                    i += 3;
                }
                else
                {
                    // Jeśli żaden z powyższych warunków nie został spełniony,
                    // to natrafiliśmy na nieznany ciąg znaków (np. literówka w nazwie funkcji).
                    throw new Exception($"Nieznany fragment wyrażenia w okolicy: '{expression.Substring(i)}'.");
                }
            }

            return tokens;
        }

        /// <summary>
        /// Metoda pomocnicza CheckFunction służy do sprawdzenia, czy w łańcuchu wejściowym (expr)
        /// na pozycji 'index' występuje sekwencja znaków równa 'funcName'.
        /// 
        /// Przykład: CheckFunction("sin(x)+cos(x)", 0, "sin") -> true
        ///           CheckFunction("sqrt(x)", 0, "sin")       -> false
        /// 
        /// Dodatkowe uwagi:
        /// 1. Jeśli 'index + funcName.Length' wykracza poza długość expr, zwracamy false
        ///    (nie ma miejsca w tekście na całą nazwę funkcji).
        /// 2. Sprawdzamy, czy substring w expr od 'index' o długości 'funcName.Length'
        ///    jest identyczny ze słowem 'funcName', porównując literę po literze
        ///    (StringComparison.OrdinalIgnoreCase ignoruje wielkość liter).
        /// 
        /// Jeśli ta metoda zwróci true, to znaczy, że na tej pozycji 'expr[index..]' mamy 
        /// dokładnie szukane słowo (np. "sqrt", "sin", itp.).
        /// </summary>
        /// <param name="expr">Całe wyrażenie, np. "2*sin(x)".</param>
        /// <param name="index">Pozycja w łańcuchu, od której chcemy sprawdzić obecność słowa.</param>
        /// <param name="funcName">Nazwa funkcji, np. "sin", "sqrt", "log".</param>
        /// <returns>
        /// True, jeśli od 'index' w górę w 'expr' występuje 'funcName', 
        /// false - w przeciwnym wypadku.
        /// </returns>
        private static bool CheckFunction(string expr, int index, string funcName)
        {
            // Sprawdzenie, czy jesteśmy w stanie w ogóle zmieścić 'funcName'
            // w pozostałej części 'expr' (czy indeks nie wychodzi poza długość łańcucha).
            if (index + funcName.Length > expr.Length)
                return false;

            // Porównanie substringa 'expr' (od index, długość funcName.Length) 
            // z 'funcName' w sposób ignorujący wielkość liter (OrdinalIgnoreCase).
            return string.Compare(expr, index, funcName, 0, funcName.Length, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
