using System.Diagnostics;
using System.Text.RegularExpressions;

string[] samples = [
    "C# та об’єктно орієнтоване програмування", "LINQ і колекції C#",
    "Пошук документів та інвертований індекс", "Порівняння C++ і C#",
    "Словники та множини"
];

var demo = new InvertedIndex();
for (int i = 0; i < samples.Length; i++)
    demo.AddDocument(new(i + 1, samples[i]));
Print("C#", demo.SearchSingleWord("C#"));
Print("C# AND LINQ", demo.SearchMultipleWords("C# LINQ"));
Print("C++ OR індекс", demo.SearchAnyWord("C++ індекс"));
Console.WriteLine($"Без стоп-слів: {string.Join(", ", InvertedIndex.RemoveStopWords(samples[0]))}");

Console.WriteLine("Документів | Побудова, мс | Лінійний, мс/запит | Індекс, мс/запит | Знайдено");
foreach (int count in new[] { 10, 100, 1_000, 10_000 })
{
    Document[] docs = Enumerable.Range(1, count).Select(id => new Document(id, samples[(id - 1) % samples.Length])).ToArray();
    var index = new InvertedIndex();
    var timer = Stopwatch.StartNew();
    foreach (Document document in docs)
        index.AddDocument(document);
    double build = timer.Elapsed.TotalMilliseconds;

    int[] Linear() => docs.Where(doc => InvertedIndex.Tokenize(doc.Content).Contains("c#")).Select(doc => doc.Id).ToArray();
    int[] Indexed() => index.SearchSingleWord("C#").Select(doc => doc.Id).ToArray();
    int[] ids = Linear();
    if (!ids.SequenceEqual(Indexed()))
        throw new InvalidOperationException("Результати пошуку відрізняються.");

    Console.WriteLine($"{count,10} | {build,12:F3} | {Measure(Linear),19:F3} | {Measure(Indexed),17:F3} | {ids.Length}");
}

if (!args.Contains("--demo"))
{
    Console.WriteLine("Введіть слова для AND-пошуку або exit.");
    while (Console.ReadLine() is { } query && !query.Equals("exit", StringComparison.OrdinalIgnoreCase))
        Print(query, demo.SearchMultipleWords(query));
}

static double Measure(Func<int[]> search)
{
    const int repeats = 20;
    var timer = Stopwatch.StartNew();
    for (int i = 0; i < repeats; i++)
        search();
    return timer.Elapsed.TotalMilliseconds / repeats;
}

static void Print(string query, IEnumerable<Document> results) =>
    Console.WriteLine($"{query}: {string.Join(", ", results.Select(doc => doc.Id))}");

public sealed record Document(int Id, string Content);

public sealed class InvertedIndex
{
    private readonly Dictionary<int, Document> _documents = new();
    private readonly Dictionary<string, HashSet<int>> _postings = new();
    private static readonly HashSet<string> StopWords = ["і", "та", "в", "у", "на", "з"];

    public void AddDocument(Document document)
    {
        if (_documents.TryGetValue(document.Id, out var previous))
        {
            foreach (string word in Tokenize(previous.Content))
            {
                var ids = _postings[word];
                ids.Remove(document.Id);
                if (ids.Count == 0)
                    _postings.Remove(word);
            }
        }

        _documents[document.Id] = document;
        foreach (string word in Tokenize(document.Content))
        {
            if (!_postings.TryGetValue(word, out var ids))
            {
                ids = new();
                _postings.Add(word, ids);
            }
            ids.Add(document.Id);
        }
    }

    public IReadOnlyList<Document> SearchSingleWord(string query)
    {
        string[] words = Tokenize(query);
        return words.Length == 1 ? SearchMultipleWords(words[0]) : [];
    }

    public IReadOnlyList<Document> SearchMultipleWords(string query)
    {
        string[] words = Tokenize(query);
        if (words.Length == 0 || !_postings.TryGetValue(words[0], out var first))
            return [];

        var ids = new HashSet<int>(first);
        foreach (string word in words.Skip(1))
        {
            if (!_postings.TryGetValue(word, out var matches))
                return [];
            ids.IntersectWith(matches);
        }
        return Resolve(ids);
    }

    public IReadOnlyList<Document> SearchAnyWord(string query)
    {
        var ids = new HashSet<int>();
        foreach (string word in Tokenize(query))
        {
            if (_postings.TryGetValue(word, out var matches))
                ids.UnionWith(matches);
        }
        return Resolve(ids);
    }

    private Document[] Resolve(IEnumerable<int> ids) =>
        ids.Order().Select(id => _documents[id]).ToArray();

    public static IEnumerable<string> RemoveStopWords(string text) =>
        Tokenize(text).Where(word => !StopWords.Contains(word));

    public static string[] Tokenize(string text) =>
        Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{Nd}]+(?:\+\+|#)?")
            .Select(match => match.Value)
            .Distinct()
            .ToArray();
}
