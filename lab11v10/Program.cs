IRepository<Book> books = new InMemoryRepository<Book>();
books.Add(new(1, "The Hobbit", "J. R. R. Tolkien", 1937));
books.Add(new(2, "Dune", "Frank Herbert", 1965));
Console.WriteLine($"Added: {books.GetAll().Count()} books");
Console.WriteLine($"Found: {books.GetById(1).Title}");
books.Update(new(2, "Dune Messiah", "Frank Herbert", 1969));
Console.WriteLine($"Updated: {books.GetById(2).Title}");
books.Delete(1);
Console.WriteLine($"Remaining: {string.Join(", ", books.GetAll().Select(book => book.Title))}");

public interface IEntity
{
    int Id { get; set; }
}

public interface IRepository<T> where T : IEntity
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T item);
    void Update(T item);
    void Delete(int id);
}

public sealed class InMemoryRepository<T> : IRepository<T> where T : IEntity
{
    private readonly List<T> _items = new();

    public T GetById(int id) => _items[FindIndex(id)];
    public IEnumerable<T> GetAll() => _items.ToArray();

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.Exists(existing => existing.Id == item.Id))
            throw new ArgumentException($"ID {item.Id} already exists.", nameof(item));

        _items.Add(item);
    }

    public void Update(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items[FindIndex(item.Id)] = item;
    }

    public void Delete(int id) => _items.RemoveAt(FindIndex(id));

    private int FindIndex(int id)
    {
        int index = _items.FindIndex(item => item.Id == id);
        return index >= 0 ? index : throw new KeyNotFoundException($"ID {id} does not exist.");
    }
}

public sealed class Book(int id, string title, string author, int publicationYear) : IEntity
{
    public int Id { get; set; } = id;
    public string Title { get; } = title;
    public string Author { get; } = author;
    public int PublicationYear { get; } = publicationYear;
}
