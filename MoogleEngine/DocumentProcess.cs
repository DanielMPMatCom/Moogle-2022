namespace MoogleEngine
{
    public static class DocumentProcess
    {
        public static string[] names;
        public static string[] text;
        public static Dictionary<string, double>[] WordInDoc;
        public static Dictionary<string, double>[] MatrixTF_IDF;
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
            WordInDoc = DocumentProcess.Normalize(names, text);
            //Diccionario que contiene cada palabra en cada documento con la cantidad de veces que aparece en dicho documento
            //Llama al metodo Normalize de la clase DocumentProcess
            MatrixTF_IDF = DocumentProcess.TF_IDF(WordInDoc);
            //Diccionario con cada palabra en cada documento y su correspondiente TF-IDF en dicho documento
        }

        public static Dictionary<string, double>[] Normalize(string[] names, string[] text)
        {
            char[] SpecialChar = new char[] { ' ', ',', '.', '/', '?', '<', '>', '"', ':', ';', '}', ']', '{', '[', '=', '+', '-', '_', ')', '(', '*', '&', '^', '^', '%', '$', '#', '@', '!', '~', '`', '\n', '\t', '|', '\r', '\a', '\f', '\b', '\v', (char)92 };
            //Caracteres especiales a eliminar
            Dictionary<string, double>[] WordInDoc = new Dictionary<string, double>[names.Length];
            //Array de diccionarios, para hacer un diccionario en cada txt
            //y saber cuántas veces se repite cada palabra en cada txt
            List<Dictionary<string, List<int>>> WordIndex = new List<Dictionary<string, List<int>>>();
            string[] text2 = new string[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                text2[i] = text[i];
            }
            for (int i = 0; i < names.Length; i++)
            //Ciclo que recorre los documentos, los lee, hace el split, crea los diccionarios
            //y los inserta en el array de diccionarios
            {
                text2[i] = text2[i].ToLower();
                string[] words = text2[i].Split(SpecialChar, StringSplitOptions.RemoveEmptyEntries);
                //Array de palabras de cada txt
                Dictionary<string, double> DicTemp = new Dictionary<string, double>();
                WordIndex[i] = new Dictionary<string, List<int>>();
                for (int k = 0; k < words.Length; k++)
                //Ciclo para poner las palabras en los diccionarios y contar las veces que se 
                //repiten en cada documento
                {
                    if (DicTemp.ContainsKey(words[k]))
                    //Compruebo si la palabra(Key) está ya en el diccionario
                    {
                        DicTemp[words[k]]++;
                        //si ya está le sumo uno al contador(value)
                    }
                    else
                    {
                        DicTemp.Add(words[k], 1);
                        //Si no está previamente en el diccionario, la agrego;
                        WordIndex[i].Add(words[k], new List<int>());
                    }
                    WordIndex[i][words[k]].Add(k);
                }
                WordInDoc[i] = DicTemp;
            }
            return WordInDoc;
        }
        static public Dictionary<string, double>[] TF_IDF(Dictionary<string, double>[] WordInDoc)
        //Método para calcular TF-IDF, recibe un array de diccionarios donde cada diccionario
        //es un documento
        //Devuelve un arreglo de diccionarios donde cada diccionario es un documento 
        //con cada palabra relacionada con su TF-IDF
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
            //Array de Diccionarios donde cada Diccionario es un documento, 
            //donde cada palabra se relaciona con su TF-IDF 
        }
    }
}