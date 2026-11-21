using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

Console.WriteLine("1. Retry: читання каталогу");
int attempts = 0;
var retry = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<IOException>(),
    MaxRetryAttempts = 3,
    Delay = TimeSpan.FromMilliseconds(100),
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,
    OnRetry = args =>
    {
        Console.WriteLine($"Повтор {args.AttemptNumber + 1}: {args.Outcome.Exception?.Message}. Затримка {args.RetryDelay.TotalMilliseconds:F0} мс.");
        return default;
    }
}).Build();

string catalog = retry.Execute(() =>
{
    if (++attempts < 3)
        throw new IOException("Тимчасовий збій читання");
    return "12 товарів";
});
Console.WriteLine($"Каталог: {catalog}. Спроб: {attempts}.");

Console.WriteLine("\n2. Circuit Breaker: черга повідомлень");
var state = new CircuitBreakerStateProvider();
var breaker = new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
    MinimumThroughput = 2,
    FailureRatio = 1,
    SamplingDuration = TimeSpan.FromSeconds(5),
    BreakDuration = TimeSpan.FromSeconds(1),
    StateProvider = state,
    OnOpened = _ => LogState("OPEN"),
    OnHalfOpened = _ => LogState("HALF-OPEN"),
    OnClosed = _ => LogState("CLOSED")
}).Build();

bool queueOnline = false;
int queueCalls = 0;
void TryQueue()
{
    try
    {
        breaker.Execute(() =>
        {
            queueCalls++;
            if (!queueOnline)
                throw new HttpRequestException("Черга недоступна.");
            Console.WriteLine("Черга прийняла повідомлення.");
        });
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine(ex.Message);
    }
    catch (BrokenCircuitException)
    {
        Console.WriteLine("Запит заблоковано до виклику черги.");
    }
}
TryQueue();
TryQueue();
TryQueue();
await Task.Delay(TimeSpan.FromMilliseconds(1200));
queueOnline = true;
TryQueue();
Console.WriteLine($"Стан: {state.CircuitState}, викликів черги: {queueCalls}.");

Console.WriteLine("\n3. Timeout: експорт звіту");
var timeout = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(1)).Build();
try
{
    await timeout.ExecuteAsync(token => new ValueTask(Task.Delay(TimeSpan.FromSeconds(3), token)));
}
catch (TimeoutRejectedException)
{
    Console.WriteLine("Експорт скасовано: перевищено ліміт 1 с.");
}

static ValueTask LogState(string value)
{
    Console.WriteLine($"Стан: {value}");
    return default;
}
