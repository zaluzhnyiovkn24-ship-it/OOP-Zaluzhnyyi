using System.Text;

string directory = Directory.CreateTempSubdirectory("oop-lab10-").FullName;
try
{
    IStorage[] stores = [new LocalStorage(directory), new SessionStorage()];
    foreach (IStorage store in stores)
    {
        store.Save("subject", "OOP");
        Console.WriteLine($"{store.GetType().Name}: {store.Load("subject")}");
    }

    Console.WriteLine($"Reopened LocalStorage: {new LocalStorage(directory).Load("subject")}");
    try
    {
        _ = new SessionStorage().Load("subject");
    }
    catch (KeyNotFoundException)
    {
        Console.WriteLine("New SessionStorage: (empty)");
    }
}
finally
{
    File.Delete(Path.Combine(directory, LocalStorage.FileName("subject")));
    Directory.Delete(directory);
}

MessageSender[] senders = [new EmailSender(), new SmsSender()];
foreach (MessageSender sender in senders)
{
    sender.Connect();
    try
    {
        sender.Send("Lab 10 complete.");
    }
    finally
    {
        sender.Disconnect();
    }
}

public interface IStorage
{
    void Save(string key, string value);
    string Load(string key);
}

public sealed class LocalStorage(string directory) : IStorage
{
    public void Save(string key, string value) =>
        File.WriteAllText(Path.Combine(directory, FileName(key)), value);

    public string Load(string key) => File.ReadAllText(Path.Combine(directory, FileName(key)));

    internal static string FileName(string key) =>
        Convert.ToHexString(Encoding.UTF8.GetBytes(key)) + ".txt";
}

public sealed class SessionStorage : IStorage
{
    private readonly Dictionary<string, string> _values = new();

    public void Save(string key, string value) => _values[key] = value;
    public string Load(string key) => _values[key];
}

public abstract class MessageSender
{
    private bool _connected;

    public abstract void Connect();
    public abstract void Send(string message);

    protected void SetConnected(string channel)
    {
        _connected = true;
        Console.WriteLine($"{channel}: connected.");
    }

    protected void EnsureConnected()
    {
        if (!_connected)
            throw new InvalidOperationException("Connect before sending.");
    }

    public void Disconnect()
    {
        _connected = false;
        Console.WriteLine("Disconnected.");
    }
}

public sealed class EmailSender : MessageSender
{
    public override void Connect() => SetConnected("Email");
    public override void Send(string message)
    {
        EnsureConnected();
        Console.WriteLine($"Email simulation: {message}");
    }
}

public sealed class SmsSender : MessageSender
{
    public override void Connect() => SetConnected("SMS");
    public override void Send(string message)
    {
        EnsureConnected();
        if (message.Length > 160)
            throw new ArgumentException("Expected at most 160 characters.", nameof(message));
        Console.WriteLine($"SMS simulation: {message}");
    }
}

