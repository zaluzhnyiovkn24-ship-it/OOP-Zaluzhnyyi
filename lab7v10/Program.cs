using System.Globalization;

var product = new Product(1000m);
Product electronics = new Electronics(1000m);
Product clothing = new Clothing(1000m);

Console.WriteLine("Base reference:");
Print("Product", product.GetPrice());
Print("Electronics as Product", electronics.GetPrice());
Print("Clothing as Product", clothing.GetPrice());

Console.WriteLine();
Console.WriteLine("Derived reference after cast:");
Print("Electronics", ((Electronics)electronics).GetPrice());
Print("Clothing", ((Clothing)clothing).GetPrice());

static void Print(string label, decimal price) =>
    Console.WriteLine($"{label}: {price.ToString("0.00", CultureInfo.InvariantCulture)} UAH");

public class Product(decimal basePrice)
{
    protected decimal BasePrice { get; } = basePrice;

    public virtual decimal GetPrice() => BasePrice;
}

public sealed class Electronics(decimal basePrice) : Product(basePrice)
{
    public override decimal GetPrice() => base.GetPrice() * 1.10m;
}

public sealed class Clothing(decimal basePrice) : Product(basePrice)
{
    public new decimal GetPrice() => base.GetPrice() * 0.80m;
}
