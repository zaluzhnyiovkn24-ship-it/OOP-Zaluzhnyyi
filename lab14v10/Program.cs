var events = new EventProcessor();
events.Enqueue(new("Order", "Order #101"));
events.Enqueue(new("Stock", "Restock #20"));
events.Enqueue(new("Order", "Order #102"));
Console.WriteLine($"Unique types: {events.TypeCount}; contains Order: {events.HasType("Order")}");
Console.WriteLine($"Removed Stock type: {events.RemoveType("Stock")}");
Console.WriteLine("FIFO:");
while (events.ProcessNext() is { } next)
    Console.WriteLine(next.Description);
Console.WriteLine("LIFO history:");
while (events.PopHistory() is { } recent)
    Console.WriteLine(recent.Description);

record Event(string Type, string Description);

sealed class EventProcessor
{
    private readonly HashSet<string> _types = new(StringComparer.OrdinalIgnoreCase);
    private readonly Queue<Event> _pending = new();
    private readonly Stack<Event> _history = new();

    public int TypeCount => _types.Count;
    public bool HasType(string type) => _types.Contains(type);
    public bool RemoveType(string type) => _types.Remove(type);

    public void Enqueue(Event item)
    {
        _types.Add(item.Type);
        _pending.Enqueue(item);
    }

    public Event? ProcessNext()
    {
        if (!_pending.TryDequeue(out var item))
            return null;
        _history.Push(item);
        return item;
    }

    public Event? PopHistory() => _history.TryPop(out var item) ? item : null;
}
