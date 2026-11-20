var morning = new TimeTable { ["Понеділок"] = "ООП" };
morning.AddLesson("Вівторок", "Математика");
var afternoon = new TimeTable { ["Понеділок"] = "Вебтехнології", ["Середа"] = "Англійська" };
var combined = morning + afternoon;
var reordered = new TimeTable
{
    ["Середа"] = "Англійська",
    ["Понеділок"] = "ООП / Вебтехнології",
    ["Вівторок"] = "Математика"
};

foreach (string day in new[] { "Понеділок", "Вівторок", "Середа", "Четвер" })
    Console.WriteLine($"{day}: {combined[day] ?? "занять немає"}");
Console.WriteLine($"Початковий понеділок: {morning["Понеділок"]}");
Console.WriteLine($"Однаковий зміст: {combined == reordered}");
Console.WriteLine($"Різний зміст: {combined != morning}");
Console.WriteLine($"Хеші рівних розкладів збігаються: {combined.GetHashCode() == reordered.GetHashCode()}");

public sealed class TimeTable : IEquatable<TimeTable>
{
    private readonly Dictionary<string, string> _schedule = new(StringComparer.OrdinalIgnoreCase);

    public string? this[string day]
    {
        get => _schedule.GetValueOrDefault(NormalizeDay(day));
        set
        {
            string key = NormalizeDay(day);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _schedule[key] = value.Trim();
        }
    }

    public void AddLesson(string day, string lesson)
    {
        string key = NormalizeDay(day);
        ArgumentException.ThrowIfNullOrWhiteSpace(lesson);
        string name = lesson.Trim();
        _schedule[key] = _schedule.TryGetValue(key, out string? existing)
            ? $"{existing} / {name}"
            : name;
    }

    public static TimeTable operator +(TimeTable left, TimeTable right)
    {
        var combined = new TimeTable();
        foreach (var entry in left._schedule)
            combined[entry.Key] = entry.Value;
        foreach (var entry in right._schedule)
            combined.AddLesson(entry.Key, entry.Value);
        return combined;
    }

    public bool Equals(TimeTable? other) =>
        other is not null && _schedule.Count == other._schedule.Count &&
        _schedule.All(entry => other._schedule.TryGetValue(entry.Key, out string? lesson) &&
            StringComparer.Ordinal.Equals(entry.Value, lesson));

    public override bool Equals(object? obj) => obj is TimeTable other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var (day, lesson) in _schedule.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(day, StringComparer.OrdinalIgnoreCase);
            hash.Add(lesson, StringComparer.Ordinal);
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(TimeTable? left, TimeTable? right) =>
        ReferenceEquals(left, right) || (left is not null && left.Equals(right));

    public static bool operator !=(TimeTable? left, TimeTable? right) => !(left == right);

    private static string NormalizeDay(string day)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(day);
        return day.Trim();
    }
}
