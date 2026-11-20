var products = Enumerable.Range(1, 10).Select(id => new Product(id, $"Product {id:D2}",
[
    new(id * 10 + 1, "Color", [new(id * 100 + 1, id % 2 == 0 ? "Blue" : "Green")]),
    new(id * 10 + 2, id % 2 == 0 ? "Size" : "Material", [new(id * 100 + 2, id % 2 == 0 ? "M" : "Cotton")])
])).ToList();

var options = products.SelectMany(product => product.Features, (product, feature) => new { product, feature })
    .SelectMany(row => row.feature.Options, (row, option) => $"{row.product.Name} / {row.feature.Name}: {option.Value}")
    .ToList();
Console.WriteLine($"All options ({options.Count}):");
foreach (string option in options)
    Console.WriteLine(option);

const int pageSize = 3;
Console.WriteLine();
Console.WriteLine("Pages (3 products each):");
for (int offset = 0; offset < products.Count; offset += pageSize)
{
    var page = products.Skip(offset).Take(pageSize);
    Console.WriteLine($"Page {offset / pageSize + 1}: {string.Join(", ", page.Select(product => product.Name))}");
}

Console.WriteLine();
Console.WriteLine($"Has Size: {string.Join(", ", products.Where(product => product.HasFeature("size")).Select(product => product.Name))}");
Console.WriteLine($"Product 01 has Size: {products[0].HasFeature("Size")}");

public sealed record Product(int Id, string Name, List<Feature> Features);
public sealed record Feature(int Id, string Name, List<Option> Options);
public sealed record Option(int Id, string Value);

public static class ProductExtensions
{
    public static bool HasFeature(this Product product, string featureName)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);
        return product.Features.Any(feature => string.Equals(feature.Name, featureName.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
