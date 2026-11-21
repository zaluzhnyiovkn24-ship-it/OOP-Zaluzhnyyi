int[] numbers = [3, 8, 2, 6];
string[] words = ["кеш", "словник", "індекс"];
Product[] products = [new("Чай", 80), new("Кава", 125), new("Какао", 95)];

Console.WriteLine("Числа");
Show(numbers, n => n * n, n => n >= 5, n => n);
Console.WriteLine("Слова");
Show(words, w => w.ToUpperInvariant(), w => w.Length >= 5, w => w.Length);
Console.WriteLine("Товари");
Show(products, p => p.Name, p => p.Price < 100, p => p.Price);

static void Show<T, TResult, TKey>(IEnumerable<T> items, Func<T, TResult> map,
    Predicate<T> filter, Func<T, TKey> key)
{
    Console.WriteLine($"Map: {string.Join(", ", CollectionUtils.Map(items, map))}");
    Console.WriteLine($"Filter: {string.Join(", ", CollectionUtils.Filter(items, filter))}");
    Console.WriteLine($"MaxBy: {CollectionUtils.MaxBy(items, key)}");
    Console.WriteLine("ForEach:");
    CollectionUtils.ForEach(items, item => Console.WriteLine($"  {item}"));
}

record Product(string Name, decimal Price);

public static class CollectionUtils
{
    public static List<TResult> Map<T, TResult>(IEnumerable<T> source, Func<T, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        var result = new List<TResult>();
        foreach (T item in source)
            result.Add(selector(item));
        return result;
    }

    public static List<T> Filter<T>(IEnumerable<T> source, Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        var result = new List<T>();
        foreach (T item in source)
        {
            if (predicate(item))
                result.Add(item);
        }
        return result;
    }

    public static T MaxBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        using var items = source.GetEnumerator();
        if (!items.MoveNext())
            throw new InvalidOperationException("Колекція порожня.");

        T best = items.Current;
        TKey bestKey = selector(best);
        var comparer = Comparer<TKey>.Default;
        while (items.MoveNext())
        {
            T candidate = items.Current;
            TKey key = selector(candidate);
            if (comparer.Compare(key, bestKey) > 0)
            {
                best = candidate;
                bestKey = key;
            }
        }
        return best;
    }

    public static void ForEach<T>(IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);
        foreach (T item in source)
            action(item);
    }
}
