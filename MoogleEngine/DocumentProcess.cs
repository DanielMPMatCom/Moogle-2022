namespace MoogleEngine
{
    public static class DocumentProcess
    {
        public static string[] names;
        public static string[] text;
        public static Dictionary<string, double>[] MatrixTF_IDF;
        public static List<Dictionary<string, List<int>>> WordIndex = new List<Dictionary<string, List<int>>>();
        public static void LoadData()
        {
            names = System.IO.Directory.GetFiles(Path.Join("..", "Content"));
            //Array de nombres
            text = new string[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                text[i] = System.IO.File.ReadAllText(names[i]);
            }
            //Textos completos de archivos
            Dictionary<string, double>[] WordInDoc = DocumentProcess.Normalize(names, text, true);
            //Diccionario que contiene cada palabra en cada documento con la cantidad de veces que aparece en dicho documento
            MatrixTF_IDF = DocumentProcess.TF_IDF(WordInDoc);
            //Diccionario con cada palabra en cada documento y su correspondiente TF-IDF en dicho documento
        }
        public static Dictionary<string, double>[] Normalize(string[] names, string[] text, bool NormalizeWithIndexation)
        {
            Dictionary<string, double>[] WordInDoc = new Dictionary<string, double>[names.Length];
            //Array de diccionarios, para hacer un diccionario en cada txt
            //y saber cuántas veces se repite cada palabra en cada txt
            string[] text2 = new string[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                text2[i] = text[i];
            }
            for (int i = 0; i < names.Length; i++)
            //Ciclo para separar los textos en palabras, guardar sus posiciones dentro de cada texto y la cantidad de veces que se repiten
            {
                text2[i] = text2[i].ToLower();
                if (NormalizeWithIndexation)
                {
                    Dictionary<string, double> DicTemp = new Dictionary<string, double>();
                    WordIndex.Add(new Dictionary<string, List<int>>());
                    List<int> ListTemp = new List<int>();
                    string temp = "";
                    bool makingword = false;
                    int startofword = 0;
                    for (int t = 0; t < text2[i].Length; t++)
                    {
                        if (Char.IsLetterOrDigit(text2[i][t]))
                        //Si encuentra una letra o un dígito
                        {
                            if (!makingword)
                            //Si no había empezado a formar una palabra
                            {
                                startofword = t;
                            }
                            makingword = true;
                            temp += text2[i][t];
                            if (t == text2[i].Length - 1)
                            //Si llega al final del documento
                            {
                                if (WordIndex[i].ContainsKey(temp))
                                {
                                    WordIndex[i][temp].Add(startofword);
                                    DicTemp[temp]++;
                                }
                                else
                                {
                                    WordIndex[i].Add(temp, new List<int>());
                                    WordIndex[i][temp].Add(startofword);
                                    DicTemp.Add(temp, 1);
                                }
                            }
                        }
                        else if (makingword)
                        {
                            if (WordIndex[i].ContainsKey(temp))
                            //Compruebo si la palabra(Key) está ya en el diccionario
                            {
                                WordIndex[i][temp].Add(startofword);
                                DicTemp[temp]++;
                                makingword = false;
                                startofword = 0;
                                temp = "";
                            }
                            else
                            // Si no está, la agrego
                            {
                                WordIndex[i].Add(temp, new List<int>());
                                WordIndex[i][temp].Add(startofword);
                                DicTemp.Add(temp, 1);
                                makingword = false;
                                startofword = 0;
                                temp = "";
                            }
                        }
                    }
                    WordInDoc[i] = DicTemp;
                }
                else
                {
                    Dictionary<string, double> DicTemp = new Dictionary<string, double>();
                    string temp = "";
                    bool makingword = false;
                    for (int t = 0; t < text2[i].Length; t++)
                    {
                        if (Char.IsLetterOrDigit(text2[i][t]))
                        //Si encuentra una letra o un dígito
                        {
                            makingword = true;
                            temp += text2[i][t];
                            if (t == text2[i].Length - 1)
                            {
                                if (DicTemp.ContainsKey(temp))
                                //Compruebo si la palabra(Key) está ya en el diccionario
                                {
                                    DicTemp[temp]++;
                                    //si ya está le sumo uno al contador(value)
                                }
                                else
                                {
                                    DicTemp.Add(temp, 1);
                                    //Si no está previamente en el diccionario, la agrego;
                                }
                            }
                        }
                        else if (makingword)
                        {
                            if (DicTemp.ContainsKey(temp))
                            //Compruebo si la palabra(Key) está ya en el diccionario
                            {
                                DicTemp[temp]++;
                                //si ya está le sumo uno al contador(value)
                                makingword = false;
                                temp = "";
                            }
                            else
                            {
                                DicTemp.Add(temp, 1);
                                //Si no está previamente en el diccionario, la agrego;
                                makingword = false;
                                temp = "";
                            }
                        }
                    }
                    WordInDoc[i] = DicTemp;
                }
            }
            return WordInDoc;
        }
        static public Dictionary<string, double>[] TF_IDF(Dictionary<string, double>[] WordInDoc)
        //Método para calcular TF-IDF, recibe un array de diccionarios donde cada diccionario
        //es un documento
        //Devuelve un arreglo de diccionarios con cada palabra relacionada con su TF-IDF
        {
            double cant = 0;
            Dictionary<string, double>[] TF = new Dictionary<string, double>[WordInDoc.Length];
            //Array de diccionarios para guardar el TF de cada palabra en cada documento
            for (int i = 0; i < WordInDoc.Length; i++)
            {
                TF[i] = new Dictionary<string, double>();
                foreach (double n in WordInDoc[i].Values)
                //Calcula la cantidad total de palabras en el documento
                {
                    cant += n;
                }
                foreach (string word2 in WordInDoc[i].Keys)
                //Calcula el TF de cada palabra
                {
                    TF[i].Add(word2, WordInDoc[i][word2] / cant);
                }
                cant = 0;
            }
            if (WordInDoc.Length == 1) return TF;
            //Si solo existe un documento no tiene sentido calcular IDF, solo TF. Un ejemplo de esto es la Query
            Dictionary<string, double> IDF = new Dictionary<string, double>();
            //Para almacenar el IDF
            for (int i = 0; i < WordInDoc.Length; i++)
            //Ciclo para contar la cantidad de documentos que contienen cierta palabra
            {
                foreach (string word in WordInDoc[i].Keys)
                {
                    if (IDF.ContainsKey(word))
                    {
                        IDF[word]++;
                    }
                    else
                    {
                        IDF.Add(word, 1);
                    }
                }
            }
            foreach (string word in IDF.Keys)
            //Ciclo para calcular el IDF y TF-IDF
            {
                IDF[word] = Math.Log10((TF.Length / IDF[word]));
                //Calcula IDF
                for (int i = 0; i < TF.Length; i++)
                //Calcula TF-IDF
                {
                    if (TF[i].ContainsKey(word))
                    {
                        TF[i][word] *= IDF[word];
                    }
                }
            }
            return TF;
        }
        public static string SnippetMaker(string TextResult, int ResultIndex, string VIW)
        //Elabora los snippets alrededor de la palabra más importante seleccionada
        {
            string Snippet = "";
            int start = 0;
            int end = 0;
            if (DocumentProcess.WordIndex[ResultIndex][VIW][0] < 200) start = 0;
            else start = DocumentProcess.WordIndex[ResultIndex][VIW][0] - 200;
            if (TextResult.Length - DocumentProcess.WordIndex[ResultIndex][VIW][0] < 200) end = TextResult.Length;
            else end = DocumentProcess.WordIndex[ResultIndex][VIW][0] + 200;
            Snippet = DocumentProcess.text[ResultIndex].Substring(start, end - start);
            return Snippet;
        }
    }
}
