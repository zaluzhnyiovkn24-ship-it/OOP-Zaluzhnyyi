using System.Diagnostics;

var random = new Random(13);
Console.WriteLine($"Логічних процесорів: {Environment.ProcessorCount}");
Console.WriteLine("Елементи | Операція | LINQ, мс | PLINQ, мс | Прискорення");
foreach (int size in new[] { 1_000_000, 5_000_000, 10_000_000 })
{
    var values = new List<int>(size);
    for (int i = 0; i < size; i++)
        values.Add(random.Next(2, 100_000));

    Compare(values, "Квадрати", n => n % 2 == 0, n => (long)n * n);
    Compare(values, "Прості", IsPrime, n => n);
}

const int count = 100_000;
int unsafeCount = 0;
int atomicCount = 0;
ParallelEnumerable.Range(0, count).ForAll(_ =>
{
    int before = unsafeCount;
    Thread.SpinWait(30);
    unsafeCount = before + 1;
});
ParallelEnumerable.Range(0, count).ForAll(_ => Interlocked.Increment(ref atomicCount));
int reduced = ParallelEnumerable.Range(0, count).Select(_ => 1).Sum();
if (atomicCount != count || reduced != count)
    throw new InvalidOperationException("Помилка підрахунку.");

Console.WriteLine($"Очікувано: {count}");
Console.WriteLine($"Без синхронізації: {unsafeCount}");
Console.WriteLine($"Interlocked: {atomicCount}");
Console.WriteLine($"Паралельна сума: {reduced}");

static void Compare(List<int> values, string label, Func<int, bool> filter, Func<int, long> transform)
{
    long Sequential() => values.Where(filter).Select(transform).Sum();
    long Parallel() => values.AsParallel().Where(filter).Select(transform).Sum();

    long expected = Sequential();
    _ = Measure(Parallel, expected);
    var sequential = new double[3];
    var parallel = new double[3];
    for (int i = 0; i < sequential.Length; i++)
    {
        if (i % 2 == 0)
        {
            sequential[i] = Measure(Sequential, expected);
            parallel[i] = Measure(Parallel, expected);
        }
        else
        {
            parallel[i] = Measure(Parallel, expected);
            sequential[i] = Measure(Sequential, expected);
        }
    }

    Array.Sort(sequential);
    Array.Sort(parallel);
    Console.WriteLine($"{values.Count,8} | {label,-8} | {sequential[1],8:F3} | {parallel[1],9:F3} | {sequential[1] / parallel[1]:F2}x");
}

static double Measure(Func<long> query, long expected)
{
    var timer = Stopwatch.StartNew();
    long actual = query();
    timer.Stop();
    if (actual != expected)
        throw new InvalidOperationException("Результати LINQ та PLINQ різні.");
    return timer.Elapsed.TotalMilliseconds;
}

static bool IsPrime(int n)
{
    if (n < 2)
        return false;
    if (n % 2 == 0)
        return n == 2;
    for (int divisor = 3; divisor <= n / divisor; divisor += 2)
    {
        if (n % divisor == 0)
            return false;
    }
    return true;
}
