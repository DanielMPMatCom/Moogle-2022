using System.IO;
namespace MoogleEngine;


public static class Moogle
{
    public static int cantResult;
    public static TimeSpan SearchTime = new TimeSpan();
    public static string[] search = new string[1];
    public static string[] TextQuery = new string[1];
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda
        DateTime begin = DateTime.Now;
        //Inicio del cronómetro
        search[0] = "Búsqueda";
        TextQuery[0] = query;
        Dictionary<string, int> RelevantWords = SearchOperators.HighlightWords();
        Dictionary<string, string> NearWords = SearchOperators.NearbyWords();
        List<string> NeededWord = SearchOperators.NecessaryWords();
        List<string> SpamWord = SearchOperators.UnwishedWords();
        //Detecta los posibles operadores de búsqueda y agrupa las palabras sobre las que estos se ejecutan
        Dictionary<string, double>[] VectorQuery = DocumentProcess.Normalize(search, TextQuery, false);
        //Query normalizada
        Dictionary<string, double>[] VectorQueryTF = new Dictionary<string, double>[VectorQuery.Length];
        string Suggestion = "";
        Dictionary<string, int> notfoundwords = LevenshteinDistance.NotFoundWord(DocumentProcess.MatrixTF_IDF, VectorQuery[0]);
        //Palabras que no se encuentran en el Content
        if (notfoundwords.Count == 0)
        {
            VectorQueryTF = DocumentProcess.TF_IDF(VectorQuery);
            //Diccionario de la Query con cada palabra de la misma relacionada con su TF
        }
        else
        {
            VectorQueryTF = DocumentProcess.TF_IDF(LevenshteinDistance.VectorQueryFixed(notfoundwords, VectorQuery[0]));
            //Se buscan palabras cercanas a las que no se pueden encontrar y se ofrecen resultados y sugerencias. 
            //Se calcula el TF-IDF del vector de la query con esas palabras arregladas
            Suggestion = query;
            for (int i = 0; i < LevenshteinDistance.SimilarWords.Count; i++)
            {
                Suggestion = Suggestion.Replace(notfoundwords.Keys.ElementAt(i), LevenshteinDistance.SimilarWords[i]);
            }
            //Ciclo que elabora la sugerencia
            LevenshteinDistance.SimilarWords.Clear();
        }
        List<int> ResultIndex = ModelSpaceVector.CosineSimilarity(VectorQueryTF[0], SpamWord, NeededWord, NearWords, RelevantWords);
        //Lista de los índices ordenados por relevancia de los documentos que son resultados de la búsqueda
        string[] TextResult = new string[ResultIndex.Count];
        //Textos que son resultado de la búsqueda
        for (int i = 0; i < ResultIndex.Count; i++)
        {
            TextResult[i] = DocumentProcess.text[ResultIndex[i]].ToLower();
        }
        string[] VIW = ModelSpaceVector.MostImportantWord(VectorQueryTF[0], ResultIndex);
        //Very Important Word, para elaborar el snippet alrededor de la palabra más importante del resultado, y  que se encuentra en la query
        string[] Snippet = new string[TextResult.Length];
        for (int i = 0; i < TextResult.Length; i++)
        //Ciclo que elabora los snippets
        {
            Snippet[i] = DocumentProcess.SnippetMaker(TextResult[i], ResultIndex[i], VIW[i]);
        }
        SearchItem[] items = new SearchItem[ResultIndex.Count];
        for (int i = 0; i < ResultIndex.Count; i++)
        //Ciclo que elabora los resultados a mostrar
        {
            items[i] = new SearchItem(Path.GetFileNameWithoutExtension(DocumentProcess.names[ResultIndex[i]]), Snippet[i], ResultIndex.Count - i);
        }
        cantResult = ResultIndex.Count;
        //Cantidad de resultados
        SearchTime = DateTime.Now - begin;
        //Fin del cronómetro
        return new SearchResult(items, Suggestion);
    }
}