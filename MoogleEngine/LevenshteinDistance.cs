namespace MoogleEngine
{
    public class LevenshteinDistance
    {
        public static List<string> SimilarWords = new List<string>();
        public static Dictionary<string, int> NotFoundWord(Dictionary<string, double>[] MatrixTF_IDF, Dictionary<string, double> VectorQuery)
        //Método para almacenar las palabras de la query que no son encontradas en el Content
        {
            Dictionary<string, int> notfoundword = new Dictionary<string, int>();
            int cant = 0;
            foreach (string word in VectorQuery.Keys)
            {
                for (int i = 0; i < MatrixTF_IDF.Length; i++)
                {
                    if (MatrixTF_IDF[i].ContainsKey(word))
                    {
                        break;
                    }
                    else
                    {
                        cant++;
                    }
                }
                if (cant == MatrixTF_IDF.Length)
                {
                    notfoundword.Add(word, (int)VectorQuery[word]);
                }
                cant = 0;
            }
            return notfoundword;
        }
        // static int EditDistance(string queryword, string docword, int querywordLength, int docwordLength)
        // //Edit Distance Recursivo
        // {
        //     if (querywordLength == 0)
        //         return docwordLength;
        //     if (docwordLength == 0)
        //         return querywordLength;
        //     if (queryword[querywordLength - 1] == docword[docwordLength - 1])
        //         return EditDistance(queryword, docword, querywordLength - 1, docwordLength - 1);
        //     return 1
        //         + Math.Min(Math.Min(EditDistance(queryword, docword, querywordLength, docwordLength - 1),
        //               EditDistance(queryword, docword, querywordLength - 1, docwordLength)),
        //               EditDistance(queryword, docword, querywordLength - 1, docwordLength - 1));
        // }
        static int EditDistance(string queryword, string docword)
        //Edit Distance Iterativo
        {
            int[,] EditDistanceMatrix = new int[queryword.Length + 1, docword.Length + 1];
            for (int i = 0; i <= queryword.Length; i++)
            {
                for (int j = 0; j <= docword.Length; j++)
                {
                    
                    if (i == 0)
                    {
                        EditDistanceMatrix[i, j] = j;
                    }
                    else if (j == 0)
                    {
                        EditDistanceMatrix[i, j] = i;
                    }
                    else if (queryword[i - 1] == docword[j - 1])
                    {
                        EditDistanceMatrix[i, j] = EditDistanceMatrix[i - 1, j - 1];
                    }
                    else
                    {
                        EditDistanceMatrix[i, j] = 1 + Math.Min(Math.Min(EditDistanceMatrix[i, j - 1], EditDistanceMatrix[i - 1, j]), EditDistanceMatrix[i - 1, j - 1]);
                    }
                }
            }
            return EditDistanceMatrix[queryword.Length, docword.Length];
        }
        
        public static void FixedWords(Dictionary<string, int> notfoundword, Dictionary<string, double> VectorQuery)
        //Método para almacenar la palabra más cercana a cada una de las palabras de la Query que no aparecen en el Content
        {
            List<int> Distance = new List<int>();
            List<double> Coefficient = new List<double>();
            for (int i = 0; i < notfoundword.Count; i++)
            {
                Distance.Add(EditDistance(notfoundword.Keys.ElementAt(i), DocumentProcess.MatrixTF_IDF[0].Keys.ElementAt(0)));
                //Distance.Add(EditDistance(notfoundword[i], DocumentProcess.MatrixTF_IDF[0].Keys.ElementAt(0),notfoundword[i].Length,DocumentProcess.MatrixTF_IDF[0].Keys.ElementAt(0).Length));
                SimilarWords.Add(DocumentProcess.MatrixTF_IDF[0].Keys.ElementAt(0));
                Coefficient.Add(DocumentProcess.MatrixTF_IDF[0][DocumentProcess.MatrixTF_IDF[0].Keys.ElementAt(0)]);
                for (int j = 0; j < DocumentProcess.MatrixTF_IDF.Length; j++)
                {
                    foreach (string word in DocumentProcess.MatrixTF_IDF[j].Keys)
                    //Ciclo para comparar los resultados del Edit Distance y obtener el término más similar
                    {
                        int x = EditDistance(notfoundword.Keys.ElementAt(i), word);
                        //int x = EditDistance(notfoundword[i], word,notfoundword[i].Length,word.Length);
                        if (Distance.ElementAt(i) > x)
                        //Se compara primero según la "distancia", quedándose con la menor posible
                        {
                            Distance.RemoveAt(i);
                            Distance.Add(x);
                            SimilarWords.RemoveAt(i);
                            SimilarWords.Add(word);
                            Coefficient.RemoveAt(i);
                            Coefficient.Add(DocumentProcess.MatrixTF_IDF[j][word]);
                        }
                        else if (Distance.ElementAt(i) == x)
                        //Segundo criterio de comparación, si existen dos términos con igual "distancia", se ordenan por el de mayor coeficiente de importancia (TF-IDF)
                        {
                            if (Coefficient.ElementAt(i) < DocumentProcess.MatrixTF_IDF[j][word])
                            {
                                SimilarWords.RemoveAt(i);
                                SimilarWords.Add(word);
                                Coefficient.RemoveAt(i);
                                Coefficient.Add(DocumentProcess.MatrixTF_IDF[j][word]);
                            }
                        }
                    }
                }
            }
        }
        public static Dictionary<string, double>[] VectorQueryFixed(Dictionary<string, int> notfoundword, Dictionary<string, double> VectorQuery)
        //Método para reelaborar el vector de la Query con las sugerencias encontradas
        {
            FixedWords(notfoundword, VectorQuery);
            List<string> SortedSimilarWords = new List<string>();
            foreach (string word in SimilarWords)
            {
                SortedSimilarWords.Add(word);
            }
            Dictionary<string, double> vectorqueryfixed = new Dictionary<string, double>();
            bool NotCoincidence = true;
            foreach (string word in VectorQuery.Keys)
            //Ciclo para rearmar la query sustituyendo los términos arreglados
            {
                foreach (string word2 in notfoundword.Keys)
                {
                    if (word == word2 && !vectorqueryfixed.ContainsKey(SortedSimilarWords[0]))
                    {
                        vectorqueryfixed.Add(SortedSimilarWords[0], notfoundword[word]);
                        SortedSimilarWords.RemoveAt(0);
                        NotCoincidence = false;
                        break;
                    }
                }
                if (NotCoincidence)
                {
                    if (vectorqueryfixed.ContainsKey(word))
                    {
                        vectorqueryfixed[word] += VectorQuery[word];
                    }
                    else
                    {
                        vectorqueryfixed.Add(word, VectorQuery[word]);
                    }
                }
                NotCoincidence = true;
            }

            Dictionary<string, double>[] VectorQueryFixed = new Dictionary<string, double>[1];
            VectorQueryFixed[0] = vectorqueryfixed;
            return VectorQueryFixed;
        }
    }
}