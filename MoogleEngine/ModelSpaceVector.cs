namespace MoogleEngine;
public static class ModelSpaceVector
{
    public static List<Dictionary<string, double>> DocWithWordList = new List<Dictionary<string, double>>();
    static List<int> SelectVector(Dictionary<string, double> VectorQueryTF, List<string> SpamWord, List<string> NeededWord)
    //Método para filtrar los documentos que contienen al menos una palabra de la Query
    {
        List<int> Index = new List<int>();
        foreach (string word in VectorQueryTF.Keys)
        //Ciclo para almacenar los índices de dichos documentos
        {
            for (int i = 0; i < DocumentProcess.MatrixTF_IDF.Length; i++)
            {
                if (DocumentProcess.MatrixTF_IDF[i].ContainsKey(word))
                {
                    if (!Index.Contains(i))
                        Index.Add(i);
                }
            }
        }
        foreach (string unwishedword in SpamWord)
        //Ciclo que elimina los documentos que contienen las palabras a las que se les aplicó el operador !
        {
            for (int i = Index.Count - 1; i >= 0; i--)
            {
                if (DocumentProcess.MatrixTF_IDF[Index[i]].ContainsKey(unwishedword))
                {
                    Index.RemoveAt(i);
                }
            }
        }
        foreach (string necessaryword in NeededWord)
        //Ciclo que elimina los documentos que no contienen las palabras a las que se les aplicó el operador ^
        {
            for (int i = Index.Count - 1; i >= 0; i--)
            {
                if (!DocumentProcess.MatrixTF_IDF[Index[i]].ContainsKey(necessaryword))
                {
                    Index.RemoveAt(i);
                }
            }
        }
        return Index;
    }
    public static Dictionary<string, double>[] NormalizeVectors(Dictionary<string, double> VectorQueryTF, List<int> Index, Dictionary<string, double>[] MatrixTF_IDF)
    //Método para normalizar los vectores de los documentos, 
    //los transforma en vectores con la misma cantidad de componentes de la Query
    //Relaciona cada palabra con su TF-IDF, si una palabra de la Querry no aparece en el documento, la relaciona con un 0
    {
        Dictionary<string, double>[] DocWithWord = new Dictionary<string, double>[Index.Count];
        for (int i = 0; i < Index.Count; i++)
        {
            Dictionary<string, double> DocWithWord2 = new Dictionary<string, double>();
            foreach (string word2 in VectorQueryTF.Keys)
            {
                if (MatrixTF_IDF[Index[i]].ContainsKey(word2))
                {
                    DocWithWord2.Add(word2, MatrixTF_IDF[Index[i]][word2]);
                }
                else
                    DocWithWord2.Add(word2, 0);
            }
            DocWithWord[i] = DocWithWord2;
        }
        return DocWithWord;
    }
    static double[] DotProducts(Dictionary<string, double>[] DocWithWord, Dictionary<string, double> VectorQueryTF)
    //Método para calcular Producto Punto entre dos vectores
    {
        double[] dotproducts = new double[DocWithWord.Length];
        for (int i = 0; i < DocWithWord.Length; i++)
        {
            dotproducts[i] = 0;
            foreach (string word in DocWithWord[i].Keys)
            {
                dotproducts[i] += DocWithWord[i][word] * VectorQueryTF[word];
                //Sumatoria de la multiplicación de pares de componentes de ambos vectores
            }
        }
        return dotproducts;
    }
    static double ModuleVector(Dictionary<string, double> Vector)
    //Método para calcular el Módulo de un vector
    {
        double Module = 0;
        foreach (string word in Vector.Keys)
        {
            Module += Vector[word] * Vector[word];
        }
        return Module = Math.Sqrt(Module);
        //Raíz cuadrada de la sumatoria del cuadrado de los componentes del vector 
    }
    static double[] ModuleProducts(Dictionary<string, double>[] DocWithWord, Dictionary<string, double> VectorQueryTF)
    //Método para calcular el producto de los módulos de dos vectores
    {
        double ModuleQuery = ModuleVector(VectorQueryTF);
        double[] moduleproducts = new double[DocWithWord.Length];
        for (int i = 0; i < DocWithWord.Length; i++)
        {
            moduleproducts[i] = ModuleVector(DocWithWord[i]) * ModuleQuery;
        }
        return moduleproducts;
    }
    static List<int> SortCosines(List<double> Cosines, List<int> Index, double[] ModProd, Dictionary<string, double>[] DocWithWord)
    /*Método para ordenar los documentos según su similaridad de coseno respecto a la Query
    Mientras mayor sea el coseno, más cercano a 1 será, por lo que el ángulo entre los vectores será más cercano a 0
    Mientras más cercano a 0 sea el ángulo entre los vectores, mayor coincidencia habrá entre ellos
    Por lo tanto el documento más cercano a la búsqueda será el que corresponda al mayor coseno 
    entre sus respectivos vectores
    Si existieran al menos 2 cosenos iguales, estos se ordenarán por sus respectivos módulos
    Si previamente fueron aplicados operadores que modifican el score, los "cosenos" no necesariamente serán menores que 1
    El algoritmo de ordenación siguiente seguirá siendo efectivo para obtener los resultados de foma descendente atendiendo al "score"*/
    {
        DocWithWordList = DocWithWord.ToList();
        for (int i = 0; i < Cosines.Count - 1; i++)
        {
            for (int j = 0; j < Cosines.Count - 1; j++)
            {
                if (Cosines[j] < Cosines[j + 1])
                {
                    Cosines.Insert(j, Cosines[j + 1]);
                    Cosines.RemoveAt(j + 2);
                    Index.Insert(j, Index[j + 1]);
                    Index.RemoveAt(j + 2);
                    DocWithWordList.Insert(j, DocWithWordList[j + 1]);
                    DocWithWordList.RemoveAt(j + 2);
                }
                else if (Cosines[j] == Cosines[j + 1])
                {
                    if (ModProd[j] < ModProd[j + 1])
                    {
                        Index.Insert(j, Index[j + 1]);
                        Index.RemoveAt(j + 2);
                        DocWithWordList.Insert(j, DocWithWordList[j + 1]);
                        DocWithWordList.RemoveAt(j + 2);
                    }
                }
            }
        }
        return Index;
    }
    static void CosinesAfterNearbyWords(string[] text, List<int> Index, List<double> Cosines, Dictionary<string, string> NearWords)
    //Método para modificar el "score" de los documentos al aplicar el operador de cercanía (~)
    {
        foreach (string word in NearWords.Keys)
        //Ciclo que recorre los pares de palabras que deben aparecer cercanas
        {
            List<int> Proximity = new List<int>();
            List<int> ProximityIndex = new List<int>();
            for (int i = 0; i < Index.Count; i++)
            //Ciclo que distingue los documentos en los que aparecen ambas palabras
            {
                int proximity;
                if (DocumentProcess.WordIndex[Index[i]].ContainsKey(word) && DocumentProcess.WordIndex[Index[i]].ContainsKey(NearWords[word]))
                {
                    List<int> word1Indexes = DocumentProcess.WordIndex[Index[i]][word];
                    List<int> word2Indexes = DocumentProcess.WordIndex[Index[i]][NearWords[word]];
                    proximity = Math.Abs(word1Indexes[0] - word2Indexes[0]);
                    int p = 0;
                    int q = 0;
                    while (p != word1Indexes.Count - 1 || q != word2Indexes.Count - 1)
                    //Calcula la proximidad y almacena la menor que encuentre por cada documento
                    {
                        if (Math.Abs(word1Indexes[p] - word2Indexes[q]) < proximity)
                        {
                            proximity = Math.Abs(word1Indexes[p] - word2Indexes[q]);
                        }
                        if (p + 1 < word1Indexes.Count && q + 1 < word2Indexes.Count && word1Indexes[p + 1] < word2Indexes[q + 1]) p++;
                        else if (p + 1 < word1Indexes.Count && q + 1 < word2Indexes.Count) q++;
                        else if (p + 1 < word1Indexes.Count) p++;
                        else q++;
                    }
                    Proximity.Add(proximity);
                    ProximityIndex.Add(i);
                }
            }
            for (int j = Proximity.Count; j > 0; j--)
            //Ciclo que aumenta el score de los documentos con ambas palabras
            {
                Cosines[ProximityIndex[Proximity.IndexOf(Proximity.Min())]] += j;
                Proximity[Proximity.IndexOf(Proximity.Min())] = int.MaxValue;
            }
        }
    }
    static void VectorsAfterHighlightsWords(Dictionary<string, double>[] DocWithWord, Dictionary<string, int> RelevantWords)
    //Método para modificar la "importancia" de las palabras al aplicar el operador de importancia (*) 
    {
        for (int i = 0; i < DocWithWord.Length; i++)
        {
            foreach (string word in RelevantWords.Keys)
            {
                if (DocWithWord[i].ContainsKey(word) && DocWithWord[i][word] != 0)
                {
                    DocWithWord[i][word] += RelevantWords[word];
                    //Suma un coeficiente al TF-IDF de la palabra en cada documento que la contenga
                }
            }
        }
    }
    public static List<int> CosineSimilarity(Dictionary<string, double> VectorQueryTF, List<string> SpamWords, List<string> NeededWord, Dictionary<string, string> NearWords, Dictionary<string, int> RelevantWords)
    //Método para hallar la similaridad de coseno
    //devuelve una lista con los índices ordenados de los documentos del resultado de la búsqueda. El orden es descendente atendiendo a su "score"
    {
        List<int> Index = SelectVector(VectorQueryTF, SpamWords, NeededWord);
        //Selecciona los vectores luego de un primer filtrado
        Dictionary<string, double>[] DocWithWord = NormalizeVectors(VectorQueryTF, Index, DocumentProcess.MatrixTF_IDF);
        //Una vez normalizados los vectores, podemos operar con ellos y calcular, por ejemplo, el coseno entre 2 de ellos
        if (RelevantWords.Count > 0)
        {
            VectorsAfterHighlightsWords(DocWithWord, RelevantWords);
            //Influencia del operador (*) en el "score" 
        }
        double[] DotProd = DotProducts(DocWithWord, VectorQueryTF);
        double[] ModProd = ModuleProducts(DocWithWord, VectorQueryTF);
        List<double> Cosines = new List<double>();
        for (int i = 0; i < DocWithWord.Length; i++)
        {
            Cosines.Add(DotProd[i] / ModProd[i]);
        }
        //Ciclo para calcular el coseno entre cada vector o documento y el vector de la Query
        if (NearWords.Count > 0)
        {
            CosinesAfterNearbyWords(DocumentProcess.text, Index, Cosines, NearWords);
            //Influencia del operador (~) en el "score"
        }
        Index = SortCosines(Cosines, Index, ModProd, DocWithWord);
        return Index;
    }
    public static string[] MostImportantWord(Dictionary<string, double> VectorQueryTF, List<int> Index)
    //Método para obtener la palabra "más importante" de cada resultado para elaborar el snippet, atendiendo a su TF-IDF
    {
        Dictionary<string, double>[] DocWithWord = DocWithWordList.ToArray();
        string[] mostimportantword = new string[Index.Count];
        for (int i = 0; i < Index.Count; i++)
        {
            mostimportantword[i] = DocWithWord[i].Keys.ElementAt(0);
            foreach (string word in DocWithWord[i].Keys)
            {
                if (DocWithWord[i][word] > DocWithWord[i][mostimportantword[i]])
                {
                    mostimportantword[i] = word;
                }
            }
        }
        return mostimportantword;
    }

}