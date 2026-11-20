var files = new FileProcessor();
var client = new NetworkClient();
Func<Exception, bool> transient = e => e is IOException or HttpRequestException;
var delay = TimeSpan.FromMilliseconds(100);

string body = RetryHelper.ExecuteWithRetry(
    () => files.GetEmailTemplate("welcome.txt"), initialDelay: delay, shouldRetry: transient);
RetryHelper.ExecuteWithRetry(() =>
{
    client.SendEmail("student@example.test", "Вітання", body);
    return true;
}, initialDelay: delay, shouldRetry: transient);

try
{
    RetryHelper.ExecuteWithRetry(() => files.GetEmailTemplate(""), shouldRetry: transient);
}
catch (ArgumentException)
{
    Console.WriteLine("Порожній шлях: повторів немає.");
}

public static class RetryHelper
{
    public static T ExecuteWithRetry<T>(Func<T> operation, int retryCount = 3,
        TimeSpan initialDelay = default, Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);
        if (initialDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(initialDelay));

        for (int retry = 0; ; retry++)
        {
            Console.WriteLine($"Спроба {retry + 1}");
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Збій: {ex.Message}");
                if (retry == retryCount || shouldRetry?.Invoke(ex) == false)
                    throw;

                double milliseconds = Math.Min(
                    initialDelay.TotalMilliseconds * Math.Pow(2, retry), int.MaxValue);
                if (initialDelay > TimeSpan.Zero)
                {
                    Console.WriteLine($"Затримка: {milliseconds} мс");
                    Thread.Sleep(TimeSpan.FromMilliseconds(milliseconds));
                }
            }
        }
    }
}

public sealed class FileProcessor
{
    private int _attempts;

    public string GetEmailTemplate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (++_attempts <= 3)
            throw new IOException("Шаблон тимчасово заблоковано.");

        Console.WriteLine($"Шаблон {path} прочитано.");
        return "Вітаємо на курсі ООП!";
    }
}

public sealed class NetworkClient
{
    private int _attempts;

    public void SendEmail(string recipient, string subject, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        if (++_attempts <= 2)
            throw new HttpRequestException("Сервер тимчасово недоступний.");

        Console.WriteLine($"Симуляція надсилання до {recipient}: {subject} — {body}");
    }
}
