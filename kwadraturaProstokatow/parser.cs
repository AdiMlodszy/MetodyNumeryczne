using System;
using System.Collections.Generic;
using System.Globalization;

namespace CompositeRectangleIntegration.Parsing
{
    /// <summary>
    /// Klasa Parser zawiera metody do:
    /// 1) Walidacji poprawności nawiasów w wyrażeniu,
    /// 2) Konwersji listy tokenów infiksowych na listę tokenów w notacji odwrotnej (RPN)
    ///    przy pomocy algorytmu Shunting Yard.
    /// 
    /// Metody:
    /// - ValidateParentheses(expression): sprawdza, czy każde '(' ma odpowiadające ')'.
    /// - ConvertToRPN(tokens): implementuje algorytm Dijkstry (shunting yard) 
    ///   i zwraca listę tokenów w formie RPN.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// ValidateParentheses sprawdza, czy w łańcuchu 'expression' 
        /// nawiasy '(' i ')' występują w poprawnych parach.
        /// 
        /// W skrócie:
        /// 1. Tworzymy stos znaków,
        /// 2. Przechodzimy przez kolejne znaki:
        ///    - jeśli napotkamy '(', wrzucamy go na stos,
        ///    - jeśli napotkamy ')', zdejmujemy '(' ze stosu (jeśli brak, zwracamy false),
        /// 3. Jeśli po przejrzeniu całego ciągu stos jest pusty, nawiasy są dopasowane;
        ///    w przeciwnym razie – nie.
        /// </summary>
        /// <param name="expression">Łańcuch znaków, np. "2*(x+1)"</param>
        /// <returns>
        /// True, jeśli nawiasy '(' i ')' są poprawnie zagnieżdżone i występują w parach;
        /// False w przeciwnym wypadku.
        /// </returns>
        public static bool ValidateParentheses(string expression)
        {
            // Tworzymy stos do przechowywania nawiasów otwierających '('
            var stack = new Stack<char>();

            // Przechodzimy znak po znaku
            foreach (char c in expression)
            {
                // Jeśli to nawias otwierający, wrzucamy na stos
                if (c == '(') 
                    stack.Push(c);
                // Jeśli to nawias zamykający ')', sprawdzamy, czy mamy co zdjąć ze stosu
                else if (c == ')')
                {
                    // Stos pusty => brak odpowiadającego '('
                    if (stack.Count == 0) 
                        return false;

                    // Zdejmujemy '(' - dopasowana para '(' ')'
                    stack.Pop();
                }
            }

            // Na końcu, jeśli stos nie jest pusty, to zostały nam jakieś '(' bez pary
            return (stack.Count == 0);
        }

        /// <summary>
        /// ConvertToRPN: konwertuje listę tokenów infiksowych (zwykła notacja np. 2 + 3) 
        /// na listę tokenów w notacji odwrotnej polskiej (RPN).
        /// 
        /// Realizuje klasyczny algorytm Shunting Yard E.W. Dijkstry:
        /// 1. Tworzymy dwie struktury: listę 'output' i stos 'stack'.
        /// 2. Przechodzimy przez tokeny:
        ///    - Jeśli token to liczba lub zmienna x, wrzucamy go do output,
        ///    - Jeśli to funkcja (sqrt, sin, cos, tan, log), wrzucamy ją na stos,
        ///    - Jeśli to operator (+, -, *, /, ^):
        ///      * Ściągaj ze stosu operatory o wyższym/podobnym priorytecie (uwzględniając łączność ^), 
        ///        dodawaj je do output,
        ///      * Wrzucaj bieżący operator na stos,
        ///    - Jeśli to nawias '(' -> wrzucamy go na stos,
        ///    - Jeśli to nawias ')' -> ściągaj operatory ze stosu do output aż do '(',
        ///      następnie, jeśli na stosie jest funkcja, też przerzuć ją do output,
        ///    - Jeśli token nieznany – zgłoś wyjątek.
        /// 3. Po przetworzeniu wszystkich tokenów ściągamy pozostałe ze stosu do output 
        ///    (jeśli spotkamy '(', oznacza to błąd w nawiasach).
        /// 
        /// Wynikowa lista 'output' to wyrażenie w RPN gotowe do ewaluacji metodą "stosową".
        /// </summary>
        /// <param name="tokens">
        /// Lista tokenów infiksowych, np. ["2", "*", "x", "+", "sin", "(", "x", ")"].
        /// </param>
        /// <returns>
        /// Lista tokenów w notacji RPN, np. ["2", "x", "*", "x", "sin", "+"]
        /// (w zależności od analizowanego wyrażenia).
        /// </returns>
        public static List<string> ConvertToRPN(List<string> tokens)
        {
            // output - wynikowa lista w notacji RPN
            var output = new List<string>();

            // stack - stos do przechowywania operatorów i funkcji
            var stack = new Stack<string>();

            // Funkcje pomocnicze do rozpoznawania operatorów i funkcji
            bool IsOperator(string t) => t == "+" || t == "-" || t == "*" || t == "/" || t == "^";
            bool IsFunction(string t) => (t == "sqrt" || t == "sin" || t == "cos" || t == "tan" || t == "log");

            // Prec - priorytet operatora (większa wartość => wyższy priorytet)
            // '^' ma najwyższy priorytet, a '+' i '-' najniższy w tym kontekście.
            int Prec(string op)
            {
                return op switch
                {
                    "^" => 4, // potęgowanie (prawostronna łączność)
                    "*" => 3,
                    "/" => 3,
                    "+" => 2,
                    "-" => 2,
                    _ => 0
                };
            }

            // Przechodzimy przez wszystkie tokeny infiksowe
            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];

                // 1. Sprawdź, czy to liczba (double) - jeśli tak, od razu dodaj do output
                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    output.Add(token);
                }
                // 2. Zmienna x - też trafia bezpośrednio do output
                else if (token == "x")
                {
                    output.Add(token);
                }
                // 3. Funkcja (sqrt, sin, cos, tan, log) - wrzucamy na stos
                else if (IsFunction(token))
                {
                    stack.Push(token);
                }
                // 4. Operator (+, -, *, /, ^)
                else if (IsOperator(token))
                {
                    // Ściągaj ze stosu operatory o wyższym/podobnym priorytecie 
                    // (zależnie od łączności potęgowania '^').
                    while (stack.Count > 0 && IsOperator(stack.Peek()))
                    {
                        if (token == "^")
                        {
                            // '^' - prawostronna łączność => operator o równym priorytecie nie jest zdejmowany 
                            // (zdejmujemy tylko jeśli priorytet wierzchu jest wyższy)
                            if (Prec(stack.Peek()) > Prec(token))
                                output.Add(stack.Pop());
                            else
                                break;
                        }
                        else
                        {
                            // Lewostronna łączność (dla +, -, *, /):
                            // Jeśli operator na stosie ma >= priorytet, ściągamy go
                            if (Prec(stack.Peek()) >= Prec(token))
                                output.Add(stack.Pop());
                            else
                                break;
                        }
                    }
                    // Wrzuć bieżący operator na stos
                    stack.Push(token);
                }
                // 5. Nawias '('
                else if (token == "(")
                {
                    stack.Push(token);
                }
                // 6. Nawias ')'
                else if (token == ")")
                {
                    // Ściągaj ze stosu do output aż napotkasz '('
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                    if (stack.Count == 0)
                        throw new Exception("Brak '(' w wyrażeniu.");

                    // Zdejmij '('
                    stack.Pop();

                    // Jeśli na stosie bezpośrednio po '(' jest jeszcze funkcja (np. sin), 
                    // też przerzuć ją do output
                    if (stack.Count > 0 && IsFunction(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                }
                else
                {
                    // Jeżeli jakiś token nie pasuje do żadnej kategorii (np. literówka),
                    // zgłaszamy wyjątek
                    throw new Exception($"Nieznany token: {token}");
                }
            }

            // Po przetworzeniu wszystkich tokenów - ściągamy pozostałe elementy ze stosu
            while (stack.Count > 0)
            {
                string top = stack.Pop();
                // Jeśli znajdziemy '(' lub ')' - to błąd w nawiasach
                if (top == "(" || top == ")")
                    throw new Exception("Niedopasowane nawiasy w wyrażeniu.");
                // Wrzucamy operator lub funkcję do wyniku RPN
                output.Add(top);
            }

            return output;
        }
    }
}
