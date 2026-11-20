var inventory = new InventoryService();
const int productId = 10;
Show(inventory.AddStock(productId, 8));
inventory.DecreaseStock(productId, 3);
Console.WriteLine($"Залишок товару {productId}: {inventory.GetStock(productId)}");
Show(inventory.AddStock(productId, 0));

try
{
    inventory.DecreaseStock(productId, 6);
}
catch (ProductOutOfStockException ex)
{
    Console.WriteLine($"Виняток: товар {ex.ProductId}, запитано {ex.RequestedQuantity}.");
}

static void Show(Result<bool> result) =>
    Console.WriteLine(result.IsSuccess ? $"Додано: {result.Value}" : $"Відмова: {result.Error}");

public sealed class ProductOutOfStockException : Exception
{
    public int ProductId { get; }
    public int RequestedQuantity { get; }

    public ProductOutOfStockException() { }
    public ProductOutOfStockException(string message) : base(message) { }
    public ProductOutOfStockException(string message, Exception inner) : base(message, inner) { }

    public ProductOutOfStockException(int productId, int requestedQuantity)
        : base($"Недостатньо одиниць товару {productId}.")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
    }
}

public sealed class Result<T>
{
    private Result(T? value, bool isSuccess, string? error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value) => new(value, true, null);
    public static Result<T> Failure(string error) => new(default, false, error);
    public static Result<T> Failure(Exception ex) => Failure(ex.Message);
}

public sealed class InventoryService
{
    private readonly Dictionary<int, int> _stock = new();

    public int GetStock(int productId) => _stock.GetValueOrDefault(productId);

    public void DecreaseStock(int productId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        int available = GetStock(productId);
        if (quantity > available)
            throw new ProductOutOfStockException(productId, quantity);

        _stock[productId] = available - quantity;
    }

    public Result<bool> AddStock(int productId, int quantity)
    {
        if (quantity <= 0)
            return Result<bool>.Failure("Кількість має бути додатною.");

        int available = GetStock(productId);
        if (quantity > int.MaxValue - available)
            return Result<bool>.Failure("Перевищено максимальний залишок.");

        _stock[productId] = available + quantity;
        return Result<bool>.Success(true);
    }
}
