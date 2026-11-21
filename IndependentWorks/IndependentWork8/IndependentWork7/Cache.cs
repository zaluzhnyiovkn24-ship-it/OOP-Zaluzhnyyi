namespace IndependentWork7;

public sealed class Cache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cache = new();
    private readonly Queue<TKey> _keysOrder = new();

    public Cache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        _capacity = capacity;
    }

    public int Count => _cache.Count;

    public void Add(TKey key, TValue value)
    {
        if (_cache.ContainsKey(key))
        {
            _cache[key] = value;
            return;
        }

        if (Count == _capacity)
        {
            _cache.Remove(_keysOrder.Dequeue());
        }

        _cache.Add(key, value);
        _keysOrder.Enqueue(key);
    }

    public TValue Get(TKey key) => _cache[key];

    public bool ContainsKey(TKey key) => _cache.ContainsKey(key);
}
