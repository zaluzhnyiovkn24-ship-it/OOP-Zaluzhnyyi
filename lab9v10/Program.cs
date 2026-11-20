using System.Text.RegularExpressions;

List<Delivery> deliveries =
[
    new(new TelegramSender(), "-1002345678901", "Class starts at nine."),
    new(new WhatsAppSender(), "invalid", "Class starts at nine."),
    new(new SlackSender(), "C012AB3CD", "Class starts at nine."),
    new(new WhatsAppSender(), "+380501234567", "Class starts at nine.")
];
new NotificationService().SendAll(deliveries);

public abstract class MessageSender
{
    public abstract void SendMessage(string chatId, string message);

    protected static void Simulate(string platform, string chatId, string message, string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (!Regex.IsMatch(chatId, pattern))
            throw new ArgumentException($"{platform}: invalid chat ID.", nameof(chatId));

        Console.WriteLine($"{platform} simulation -> {chatId}: {message}");
    }
}

public sealed class TelegramSender : MessageSender
{
    public override void SendMessage(string chatId, string message) =>
        Simulate("Telegram", chatId, message, @"\A-?[1-9][0-9]{0,18}\z");
}

public sealed class WhatsAppSender : MessageSender
{
    public override void SendMessage(string chatId, string message) =>
        Simulate("WhatsApp", chatId, message, @"\A\+[1-9][0-9]{7,14}\z");
}

public sealed class SlackSender : MessageSender
{
    public override void SendMessage(string chatId, string message) =>
        Simulate("Slack", chatId, message, @"\A[CGD][A-Z0-9]{8,}\z");
}

public sealed record Delivery(MessageSender Sender, string ChatId, string Message);

public sealed class NotificationService
{
    public void SendAll(List<Delivery> items)
    {
        int successful = 0;
        foreach (Delivery item in items)
        {
            try
            {
                item.Sender.SendMessage(item.ChatId, item.Message);
                successful++;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Rejected: {ex.Message}");
            }
        }

        Console.WriteLine($"Simulated deliveries: {successful}/{items.Count}");
    }
}
