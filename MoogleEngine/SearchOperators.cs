namespace MoogleEngine
{
    public class SearchOperators
    {
        public static List<string> WordsWithOperator(char symbol)
        //Almacena las palabras sobre las que se ejecutan los operadores (!) ó (^) en dependencia del parámetro symbol que se le pase al método
        {
            List<string> wordwithoperator = new List<string>();
            char endofword = ' ';
            bool foundendofword = false;
            for (int i = 0; i < Moogle.TextQuery[0].Length - 1; i++)
            {
                if (Moogle.TextQuery[0][i] == symbol && Char.IsLetterOrDigit(Moogle.TextQuery[0][i + 1]))
                {
                    for (int j = i + 2; j < Moogle.TextQuery[0].Length; j++)
                    {
                        if (!Char.IsLetterOrDigit(Moogle.TextQuery[0][j]))
                        {
                            endofword = Moogle.TextQuery[0][j];
                            foundendofword = true;
                            break;
                        }
                    }
                    if (!foundendofword)
                    {
                        wordwithoperator.Add(Moogle.TextQuery[0].Substring(i + 1, Moogle.TextQuery[0].Length - i - 1));
                    }
                    else
                    {
                        wordwithoperator.Add(Moogle.TextQuery[0].Substring(i + 1, Moogle.TextQuery[0].IndexOf(endofword, i + 1) - i - 1));
                        foundendofword = false;
                    }
                }
            }
            return wordwithoperator;
        }
        public static List<string> UnwishedWords()
        //Operador (!)
        {
            List<string> Spamword = WordsWithOperator('!');
            foreach (string word in Spamword)
            //Ciclo para modificar la query, eliminando ls palabras sobre las que se aplica el operador (!)
            {
                while (Moogle.TextQuery[0].Contains(word))
                {
                    Moogle.TextQuery[0] = Moogle.TextQuery[0].Remove(Moogle.TextQuery[0].IndexOf(word), word.Length);
                }
            }
            return Spamword;
        }
        public static List<string> NecessaryWords()
        //Operador (^)
        {
            List<string> Neededword = WordsWithOperator('^');
            return Neededword;
        }
        public static Dictionary<string, string> NearbyWords()
        //Operador (~)
        {
            string InverseQuery = "";
            for (int i = Moogle.TextQuery[0].Length - 1; i >= 0; i--)
            //Invierte los caracteres de la query para utilizar el método Substring "en sentidos opuestos", a ambos lados del operador
            {
                InverseQuery += Moogle.TextQuery[0][i];
            }
            Dictionary<string, string> Nearwords = new Dictionary<string, string>();
            char endofword = ' ';
            char startofword = ' ';
            bool foundstartofword = false;
            bool foundendofword = false;
            for (int i = 1; i < Moogle.TextQuery[0].Length - 1; i++)
            //Ciclo para seleccionar los pares de palabras sobre las que se ejecuta el operador (~)
            {
                if (Moogle.TextQuery[0][i] == '~' && Char.IsLetterOrDigit(Moogle.TextQuery[0][i + 1]) && Char.IsLetterOrDigit(Moogle.TextQuery[0][i - 1]))
                {
                    for (int j = i + 2; j < Moogle.TextQuery[0].Length; j++)
                    {
                        if (!Char.IsLetterOrDigit(Moogle.TextQuery[0][j]))
                        {
                            endofword = Moogle.TextQuery[0][j];
                            foundendofword = true;
                            break;
                        }
                    }
                    for (int k = i - 2; k >= 0; k--)
                    {
                        if (!Char.IsLetterOrDigit(Moogle.TextQuery[0][k]))
                        {
                            startofword = Moogle.TextQuery[0][k];
                            foundstartofword = true;
                            break;
                        }
                    }
                    int start;
                    switch ((foundendofword, foundstartofword))
                    {
                        case (false, false):
                            Nearwords.Add(Moogle.TextQuery[0].Substring(i + 1, Moogle.TextQuery[0].Length - i - 1), Moogle.TextQuery[0].Substring(0, i));
                            break;
                        case (false, true):
                            start = InverseQuery.Length - (InverseQuery.IndexOf(startofword, Moogle.TextQuery[0].Length - (i - 1) - 1));
                            Nearwords.Add(Moogle.TextQuery[0].Substring(i + 1, Moogle.TextQuery[0].Length - i - 1), Moogle.TextQuery[0].Substring(start, i - start));
                            foundstartofword = false;
                            break;
                        case (true, false):
                            Nearwords.Add(Moogle.TextQuery[0].Substring(i + 1, Moogle.TextQuery[0].IndexOf(endofword, i + 1) - i - 1), Moogle.TextQuery[0].Substring(0, i));
                            foundendofword = false;
                            break;
                        case (true, true):
                            start = InverseQuery.Length - (InverseQuery.IndexOf(startofword, Moogle.TextQuery[0].Length - (i - 1) - 1));
                            Nearwords.Add(Moogle.TextQuery[0].Substring(i + 1, Moogle.TextQuery[0].IndexOf(endofword, i + 1) - i - 1), Moogle.TextQuery[0].Substring(start, i - start));
                            foundendofword = false;
                            foundstartofword = false;
                            break;
                    }
                }
            }
            return Nearwords;
        }
        public static Dictionary<string, int> HighlightWords()
        //Operador (*)
        {
            char endofword = ' ';
            bool foundendofword = false;
            int cantoperators = 0;
            Dictionary<string, int> RelevantWords = new Dictionary<string, int>();
            for (int i = 0; i < Moogle.TextQuery[0].Length - 1; i++)
            //Ciclo para seleccionar las palabras sobre las que se ejecuta el operador (*)
            //Cuenta y almacena la cantidad de operadores (*) que se ejecutan sobre una misma palabra
            {
                if (Moogle.TextQuery[0][i] == '*' && Char.IsLetterOrDigit(Moogle.TextQuery[0][i + 1]))
                {
                    cantoperators++;
                    for (int j = i + 2; j < Moogle.TextQuery[0].Length; j++)
                    {
                        if (!Char.IsLetterOrDigit(Moogle.TextQuery[0][j]))
                        {
                            endofword = Moogle.TextQuery[0][j];
                            foundendofword = true;
                            break;
                        }
                    }
                    if (!foundendofword)
                    {
                        RelevantWords.Add(Moogle.TextQuery[0].Substring(i + 1, Moogle.TextQuery[0].Length - i - 1), cantoperators);
                        cantoperators = 0;
                    }
                    else
                    {
                        RelevantWords.Add(Moogle.TextQuery[0].Substring(i + 1, Moogle.TextQuery[0].IndexOf(endofword, i + 1) - i - 1), cantoperators);
                        foundendofword = false;
                        cantoperators = 0;
                    }
                }
                else if (Moogle.TextQuery[0][i] == '*')
                {
                    cantoperators++;
                }
            }
            return RelevantWords;
        }
    }
}