using System.Diagnostics;

var rng = new Random(10);
string[] cities = ["Рівне", "Луцьк", "Київ", "Львів", "Одеса"];
string[] categories = ["Книги", "Електроніка", "Дім", "Спорт", "Одяг"];
var products = Enumerable.Range(1, 100)
    .Select(id => new Product(id, categories[(id - 1) % 5], rng.Next(50, 1500))).ToArray();
var customers = Enumerable.Range(1, 100)
    .Select(id => new Customer(id, cities[(id - 1) % 5])).ToArray();
var sales = Enumerable.Range(1, 100)
    .Select(_ => new Sale(rng.Next(1, 101), rng.Next(1, 101), rng.Next(1, 8),
        new DateOnly(2025, 1, 1).AddDays(rng.Next(730)))).ToArray();

var priced = sales.Join(products, s => s.ProductId, p => p.Id,
    (s, p) => new { Sale = s, Revenue = s.Quantity * p.Price });

Report("1. Топ-3 товари за категоріями", () =>
    sales.GroupBy(s => s.ProductId)
        .Select(g => new { Id = g.Key, Quantity = g.Sum(s => s.Quantity) })
        .Join(products, s => s.Id, p => p.Id, (s, p) => new { Product = p, s.Quantity })
        .GroupBy(x => x.Product.Category).OrderBy(g => g.Key)
        .SelectMany(g => g.OrderByDescending(x => x.Quantity).ThenBy(x => x.Product.Id).Take(3))
        .Select(x => $"{x.Product.Category}: товар {x.Product.Id}, {x.Quantity} од."));

Report("2. Дохід за містами", () =>
    priced.Join(customers, x => x.Sale.CustomerId, c => c.Id, (x, c) => new { c.City, x.Revenue })
        .GroupBy(x => x.City).Select(g => new { City = g.Key, Revenue = g.Sum(x => x.Revenue) })
        .OrderByDescending(x => x.Revenue).ThenBy(x => x.City)
        .Select(x => $"{x.City}: {x.Revenue:F2} грн"));

Report("3. Продажі за місяцями", () =>
    priced.GroupBy(x => new { x.Sale.Date.Year, x.Sale.Date.Month })
        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
        .Select(g => $"{g.Key.Year}-{g.Key.Month:D2}: {g.Sum(x => x.Sale.Quantity)} од., {g.Sum(x => x.Revenue):F2} грн"));

Report("4. Топ-5 клієнтів за витратами у 2026 році", () =>
    sales.Where(s => s.Date.Year == 2026)
        .Join(products, s => s.ProductId, p => p.Id, (s, p) => new { s.CustomerId, Revenue = s.Quantity * p.Price })
        .GroupBy(x => x.CustomerId)
        .Select(g => new { Id = g.Key, Revenue = g.Sum(x => x.Revenue) })
        .Join(customers, x => x.Id, c => c.Id, (x, c) => new { c.Id, c.City, x.Revenue })
        .OrderByDescending(x => x.Revenue).ThenBy(x => x.Id).Take(5)
        .Select(x => $"Клієнт {x.Id}, {x.City}: {x.Revenue:F2} грн"));

static void Report(string title, Func<IEnumerable<string>> query)
{
    _ = query().ToArray();
    var watch = Stopwatch.StartNew();
    var rows = query().ToArray();
    watch.Stop();
    Console.WriteLine(title);
    foreach (string row in rows)
        Console.WriteLine(row);
    Console.WriteLine($"Час: {watch.Elapsed.TotalMilliseconds:F3} мс");
    Console.WriteLine();
}

record Product(int Id, string Category, decimal Price);
record Customer(int Id, string City);
record Sale(int ProductId, int CustomerId, int Quantity, DateOnly Date);
