Show("Numbers", new List<int> { 2, 5, 9, 12 }, n => n * n, n => n > 4, n => n == 9);
Show("Words", new List<string> { "cat", "cloud", "wind", "rain" },
    s => s.Length, s => s.Length >= 4, s => s == "wind");
Show("Books", new List<Book> { new("Dune", 1965), new("Solaris", 1961), new("Neuromancer", 1984) },
    b => b.Title, b => b.Year >= 1965, b => b.Title == "Solaris");

Console.WriteLine($"No match: {FunctionalUtils.FirstOrDefault(new List<string>(), _ => true) ?? "(default)"}");
ShowSingleFailure("No matches", new List<int> { 1, 3 });
ShowSingleFailure("Multiple matches", new List<int> { 2, 4 });

static void Show<T, TResult>(string title, List<T> items, Func<T, TResult> map,
    Func<T, bool> filter, Func<T, bool> unique)
{
    Console.WriteLine(title);
    Console.WriteLine($"  Map: {string.Join(", ", FunctionalUtils.Map(items, map))}");
    Console.WriteLine($"  Filter: {string.Join(", ", FunctionalUtils.Filter(items, filter))}");
    Console.WriteLine($"  First: {FunctionalUtils.FirstOrDefault(items, filter)}");
    Console.WriteLine($"  Last: {FunctionalUtils.LastOrDefault(items, filter)}");
    Console.WriteLine($"  Single: {FunctionalUtils.Single(items, unique)}");
}

static void ShowSingleFailure(string label, List<int> items)
{
    try
    {
        _ = FunctionalUtils.Single(items, n => n % 2 == 0);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"{label}: {ex.Message}");
    }
}

public sealed record Book(string Title, int Year)
{
    public override string ToString() => $"{Title} ({Year})";
}

public static class FunctionalUtils
{
    public static List<TResult> Map<T, TResult>(List<T> items, Func<T, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selector);
        var result = new List<TResult>(items.Count);
        foreach (T item in items)
            result.Add(selector(item));
        return result;
    }

    public static List<T> Filter<T>(List<T> items, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicate);
        var result = new List<T>();
        foreach (T item in items)
        {
            if (predicate(item))
                result.Add(item);
        }
        return result;
    }

    public static T? FirstOrDefault<T>(List<T> items, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicate);
        foreach (T item in items)
        {
            if (predicate(item))
                return item;
        }
        return default;
    }

    public static T? LastOrDefault<T>(List<T> items, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicate);
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (predicate(items[i]))
                return items[i];
        }
        return default;
    }

    public static T Single<T>(List<T> items, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicate);
        int match = -1;
        for (int i = 0; i < items.Count; i++)
        {
            if (!predicate(items[i]))
                continue;
            if (match >= 0)
                throw new InvalidOperationException("More than one matching item.");
            match = i;
        }
        return match >= 0 ? items[match] : throw new InvalidOperationException("No matching item.");
    }
}
