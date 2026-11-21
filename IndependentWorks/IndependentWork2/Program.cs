using System.Globalization;

var full = new Product(101, "Notebook", 119.50m, "Stationery", 12);
var shortForm = new Product(102, "Marker", 35m);
var copy = new Product(full);
Console.WriteLine($"Full: {full}");
Console.WriteLine($"Short: {shortForm}");
Console.WriteLine($"Copy: {copy}");
Console.WriteLine($"Distinct instances: {!ReferenceEquals(full, copy)}");

sealed class Product
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public string Category { get; }
    public int StockCount { get; }

    public Product(int id, string name, decimal price, string category, int stockCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentOutOfRangeException.ThrowIfNegative(stockCount);
        Id = id;
        Name = name;
        Price = price;
        Category = category;
        StockCount = stockCount;
    }

    public Product(int id, string name, decimal price)
        : this(id, name, price, "Uncategorized", 0) { }

    public Product(Product source)
        : this((source ?? throw new ArgumentNullException(nameof(source))).Id,
            source.Name, source.Price, source.Category, source.StockCount) { }

    public override string ToString() =>
        $"#{Id} {Name} | {Price.ToString("C", CultureInfo.GetCultureInfo("uk-UA"))} | {Category} | stock: {StockCount}";
}
