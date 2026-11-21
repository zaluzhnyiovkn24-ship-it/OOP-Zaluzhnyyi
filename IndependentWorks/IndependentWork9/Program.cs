using System.Text.RegularExpressions;

var cache = new FileMetadataCache();
cache.Add(new("notes.txt", 512, new DateTime(2026, 11, 20), "demo-001"));
Console.WriteLine($"Кеш: {cache.Get("NOTES.TXT")?.Size} байт");
Console.WriteLine($"Видалено: {cache.Remove("notes.txt")}, залишився: {cache.Contains("notes.txt")}");

var tags = new TagManager();
tags.Add(1, "C#");
tags.Add(1, "oop");
tags.Add(2, "c#");
tags.Add(2, "linq");
Console.WriteLine($"Об’єднання: {Show(tags.Union(1, 2))}");
Console.WriteLine($"Перетин: {Show(tags.Intersection(1, 2))}");
Console.WriteLine($"Різниця: {Show(tags.Difference(1, 2))}");

var index = new SearchIndex();
index.Add(1, "C# та ООП");
index.Add(2, "C# і LINQ");
index.Add(3, "Історія ООП");
Console.WriteLine($"C#: {string.Join(", ", index.Search("C#"))}");
Console.WriteLine($"C# + ООП: {string.Join(", ", index.SearchMultiple("C# ООП"))}");

static string Show(IEnumerable<string> values) =>
    string.Join(", ", values.Order(StringComparer.OrdinalIgnoreCase));

record FileMetadata(string Name, long Size, DateTime Modified, string Hash);

sealed class FileMetadataCache
{
    private readonly Dictionary<string, FileMetadata> _files = new(StringComparer.OrdinalIgnoreCase);

    public void Add(FileMetadata file) => _files[file.Name] = file;
    public FileMetadata? Get(string name) => _files.GetValueOrDefault(name);
    public bool Remove(string name) => _files.Remove(name);
    public bool Contains(string name) => _files.ContainsKey(name);
}

sealed class TagManager
{
    private readonly Dictionary<int, HashSet<string>> _tags = new();

    public void Add(int articleId, string tag)
    {
        if (!_tags.TryGetValue(articleId, out var values))
        {
            values = new(StringComparer.OrdinalIgnoreCase);
            _tags.Add(articleId, values);
        }

        values.Add(tag);
    }

    private HashSet<string> Copy(int id) =>
        new(_tags.GetValueOrDefault(id) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> Union(int first, int second)
    {
        var result = Copy(first);
        result.UnionWith(Copy(second));
        return result;
    }

    public IEnumerable<string> Intersection(int first, int second)
    {
        var result = Copy(first);
        result.IntersectWith(Copy(second));
        return result;
    }

    public IEnumerable<string> Difference(int first, int second)
    {
        var result = Copy(first);
        result.ExceptWith(Copy(second));
        return result;
    }
}

sealed class SearchIndex
{
    private readonly Dictionary<int, string[]> _documents = new();
    private readonly Dictionary<string, HashSet<int>> _postings = new(StringComparer.OrdinalIgnoreCase);

    public void Add(int id, string content)
    {
        var tokens = Tokenize(content);
        if (_documents.TryGetValue(id, out var previous))
        {
            foreach (string token in previous)
            {
                var ids = _postings[token];
                ids.Remove(id);
                if (ids.Count == 0)
                    _postings.Remove(token);
            }
        }

        _documents[id] = tokens;
        foreach (string token in tokens)
        {
            if (!_postings.TryGetValue(token, out var ids))
            {
                ids = new();
                _postings.Add(token, ids);
            }

            ids.Add(id);
        }
    }

    public IEnumerable<int> Search(string query) => SearchMultiple(query);

    public IEnumerable<int> SearchMultiple(string query)
    {
        var terms = Tokenize(query);
        if (terms.Length == 0 || !_postings.TryGetValue(terms[0], out var first))
            return Array.Empty<int>();

        var result = new HashSet<int>(first);
        foreach (string term in terms.Skip(1))
        {
            if (!_postings.TryGetValue(term, out var ids))
                return Array.Empty<int>();

            result.IntersectWith(ids);
        }

        return result.Order().ToArray();
    }

    private static string[] Tokenize(string text) =>
        Regex.Matches(text, @"[\p{L}\p{Nd}]+#?")
            .Select(match => match.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
