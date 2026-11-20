using System.Diagnostics;

var locations = Enumerable.Range(1, 12)
    .Select(id => new Location(id, $"Zone {id:D2}", id <= 6 ? "Main Warehouse" : "Reserve Warehouse"))
    .ToArray();
var items = Enumerable.Range(1, 20)
    .Select(id => new Item(id, $"Item {id:D2}", id * 7 % 32, (id - 1) % locations.Length + 1))
    .ToArray();

var joined = items.Join(locations, item => item.LocationId, location => location.Id,
    (item, location) => new { item, location });

Report("1. Low stock in Main Warehouse",
    () => from item in items
          join location in locations on item.LocationId equals location.Id
          where item.Quantity < 10 && location.Warehouse == "Main Warehouse"
          select $"{item.Name}: {item.Quantity}",
    () => joined.Where(x => x.item.Quantity < 10 && x.location.Warehouse == "Main Warehouse")
        .Select(x => $"{x.item.Name}: {x.item.Quantity}"));

Report("2. Items and locations",
    () => from item in items
          join location in locations on item.LocationId equals location.Id
          select $"{item.Name}: {location.Name}",
    () => joined.Select(x => $"{x.item.Name}: {x.location.Name}"));

Report("3. Total stock by location",
    () => from row in joined
          group row.item by row.location.Name into stock
          orderby stock.Key
          select $"{stock.Key}: {stock.Sum(item => item.Quantity)}",
    () => joined.GroupBy(x => x.location.Name, x => x.item)
        .OrderBy(stock => stock.Key).Select(stock => $"{stock.Key}: {stock.Sum(item => item.Quantity)}"));

Report("4. Highest stock",
    () => (from item in items orderby item.Quantity descending, item.Id select item)
        .Take(1).Select(item => $"{item.Name}: {item.Quantity}"),
    () => new[] { items.MaxBy(item => (item.Quantity, -item.Id))! }
        .Select(item => $"{item.Name}: {item.Quantity}"));

static void Report(string title, Func<IEnumerable<string>> query, Func<IEnumerable<string>> methods)
{
    var expected = query().ToArray();
    if (!expected.SequenceEqual(methods()))
        throw new InvalidOperationException($"Results differ in {title}.");

    Console.WriteLine(title);
    foreach (string row in expected)
        Console.WriteLine(row);
    Console.WriteLine($"Query: {Measure(query):F4} ms | Methods: {Measure(methods):F4} ms");
    Console.WriteLine();
}

static double Measure(Func<IEnumerable<string>> run)
{
    const int repeats = 200;
    var timer = Stopwatch.StartNew();
    for (int i = 0; i < repeats; i++)
        _ = run().ToArray();
    return timer.Elapsed.TotalMilliseconds / repeats;
}

record Item(int Id, string Name, int Quantity, int LocationId);
record Location(int Id, string Name, string Warehouse);
