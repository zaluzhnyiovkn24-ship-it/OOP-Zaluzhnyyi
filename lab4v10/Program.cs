var first = new Percentage(25);
var second = new Percentage(40);
var same = new Percentage(25);
Console.WriteLine($"Максимум: {Percentage.MaxPercentage}");
Console.WriteLine($"{first} + {second} = {first + second}");
Console.WriteLine($"{second} × 2 = {second * 2}");
Console.WriteLine($"Рівність: {first == same}; нерівність: {first != second}");
try
{
    first.Value = 101;
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine($"101% відхилено. Залишилося: {first}");
}
try
{
    _ = second * 3;
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine("Результат понад 100% відхилено.");
}

public sealed class Percentage : IEquatable<Percentage>
{
    private double _value;

    public Percentage(double value) => Value = value;

    public double Value
    {
        get => _value;
        set
        {
            if (!double.IsFinite(value) || value is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(value), "Значення має бути від 0 до 100.");
            _value = value;
        }
    }

    public static Percentage MaxPercentage => new(100);

    public static Percentage operator +(Percentage left, Percentage right) =>
        new(left.Value + right.Value);

    public static Percentage operator *(Percentage percentage, double scalar) =>
        new(percentage.Value * scalar);

    public override string ToString() => $"{Value:0.##}%";
    public bool Equals(Percentage? other) => other is not null && Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is Percentage other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Percentage? left, Percentage? right) =>
        ReferenceEquals(left, right) || (left is not null && left.Equals(right));

    public static bool operator !=(Percentage? left, Percentage? right) => !(left == right);
}
