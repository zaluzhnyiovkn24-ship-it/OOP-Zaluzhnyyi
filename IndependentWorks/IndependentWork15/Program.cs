var policy = new RetryPolicy(3, TimeSpan.FromMilliseconds(100), 0.2);

int reads = 0;
Run("1. Тимчасовий збій читання", () =>
{
    if (++reads < 3)
        throw new IOException("Звіт тимчасово заблокований.");
    return $"Звіт прочитано після {reads} спроб.";
});

Run("2. Постійний мережевий збій",
    () => throw new HttpRequestException("Сервіс недоступний."));

Run("3. Некоректний аргумент",
    () => throw new ArgumentException("Шлях до звіту порожній."));

void Run(string title, Func<string> operation)
{
    Console.WriteLine(title);
    try
    {
        Console.WriteLine(policy.Execute(operation));
    }
    catch (Exception ex) when (ex is IOException or HttpRequestException or ArgumentException)
    {
        Console.WriteLine($"Остаточна помилка: {ex.Message}");
    }
    Console.WriteLine();
}

public sealed class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _jitterFactor;

    public RetryPolicy(int maxRetries, TimeSpan initialDelay, double jitterFactor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);
        if (initialDelay < TimeSpan.Zero || initialDelay.TotalMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(initialDelay));
        if (!double.IsFinite(jitterFactor) || jitterFactor is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(jitterFactor));

        _maxRetries = maxRetries;
        _initialDelay = initialDelay;
        _jitterFactor = jitterFactor;
    }

    public T Execute<T>(Func<T> operation, Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        shouldRetry ??= ex => ex is IOException or HttpRequestException;

        for (int retry = 0; ; retry++)
        {
            Console.WriteLine($"Спроба {(long)retry + 1}");
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                if (retry == _maxRetries || !shouldRetry(ex))
                    throw;

                int delay = CalculateDelay(retry);
                Console.WriteLine($"Затримка перед повтором: {delay} мс");
                Thread.Sleep(delay);
            }
        }
    }

    private int CalculateDelay(int retry)
    {
        if (_initialDelay == TimeSpan.Zero)
            return 0;

        double factor = 1 + (2 * Random.Shared.NextDouble() - 1) * _jitterFactor;
        double milliseconds = Math.ScaleB(_initialDelay.TotalMilliseconds * factor, retry);
        return (int)Math.Min(int.MaxValue, Math.Round(milliseconds));
    }
}
