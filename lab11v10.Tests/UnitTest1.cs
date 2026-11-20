public class RepositoryTests
{
    private readonly IRepository<Book> _books = new InMemoryRepository<Book>();
    private static Book Sample(int id = 1) => new(id, "Dune", "Frank Herbert", 1965);

    [Fact]
    public void AddStoresBook()
    {
        var book = Sample();
        _books.Add(book);
        Assert.Same(book, Assert.Single(_books.GetAll()));
    }

    [Fact]
    public void DuplicateIdIsRejected()
    {
        _books.Add(Sample());
        Assert.Throws<ArgumentException>(() => _books.Add(Sample()));
    }

    [Fact]
    public void GetByIdFindsMatchingBook()
    {
        _books.Add(Sample(1));
        var second = Sample(2);
        _books.Add(second);
        Assert.Same(second, _books.GetById(2));
    }

    [Fact]
    public void GetAllReturnsAllBooks()
    {
        _books.Add(Sample(1));
        _books.Add(Sample(2));
        Assert.Equal(new[] { 1, 2 }, _books.GetAll().Select(book => book.Id));
    }

    [Fact]
    public void UpdateReplacesMatchingBook()
    {
        _books.Add(Sample());
        _books.Update(new(1, "Dune Messiah", "Frank Herbert", 1969));
        Assert.Equal("Dune Messiah", _books.GetById(1).Title);
    }

    [Fact]
    public void DeleteRemovesOnlyMatchingBook()
    {
        _books.Add(Sample(1));
        _books.Add(Sample(2));
        _books.Delete(1);
        Assert.Equal(2, Assert.Single(_books.GetAll()).Id);
    }

    [Fact]
    public void MissingIdIsRejectedByGetUpdateAndDelete()
    {
        Assert.Throws<KeyNotFoundException>(() => _books.GetById(99));
        Assert.Throws<KeyNotFoundException>(() => _books.Update(Sample(99)));
        Assert.Throws<KeyNotFoundException>(() => _books.Delete(99));
    }
}
