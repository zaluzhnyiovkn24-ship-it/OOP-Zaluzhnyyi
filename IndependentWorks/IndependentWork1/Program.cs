var rectangle = new Rectangle(8, 5);
Console.WriteLine($"Rectangle: area={rectangle.CalculateArea()}, square={rectangle.IsSquare()}");
rectangle.Height = 8;
Console.WriteLine($"Resized: area={rectangle.CalculateArea()}, square={rectangle.IsSquare()}");

var employee = new Employee("Alex Morgan", 20_000);
Console.WriteLine($"{employee.FullName}: annual bonus = {employee.CalculateAnnualBonus(10):0.##} UAH");

var playlist = new Playlist("Evening", ["First Light", "Night Train", "Home"]);
playlist[1] = "Blue Hour";
Console.WriteLine($"Track at index 1: {playlist[1]}");
playlist.PrintAllTracks();
try { _ = playlist[3]; }
catch (ArgumentOutOfRangeException) { Console.WriteLine("Track index 3 is out of range."); }

sealed class Rectangle
{
    private double _width;
    private double _height;

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public double Width { get => _width; set => _width = Positive(value); }
    public double Height { get => _height; set => _height = Positive(value); }
    public double CalculateArea() => Width * Height;
    public bool IsSquare() => Width == Height;

    private static double Positive(double value) =>
        double.IsFinite(value) && value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
}

sealed class Employee
{
    private readonly string _fullName;
    private double _monthlySalary;

    public Employee(string fullName, double monthlySalary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        _fullName = fullName;
        MonthlySalary = monthlySalary;
    }

    public string FullName => _fullName;
    public double MonthlySalary
    {
        get => _monthlySalary;
        set => _monthlySalary = double.IsFinite(value) && value >= 0
            ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }

    public double CalculateAnnualBonus(double bonusPercent)
    {
        if (!double.IsFinite(bonusPercent) || bonusPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(bonusPercent));
        return MonthlySalary * 12 * (bonusPercent / 100);
    }
}

sealed class Playlist
{
    private string _title;
    private readonly string[] _tracks;

    public Playlist(string title, string[] tracks)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(tracks);
        _title = title;
        _tracks = (string[])tracks.Clone();
        foreach (string track in _tracks)
            ArgumentException.ThrowIfNullOrWhiteSpace(track);
    }

    public string Title
    {
        get => _title;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _title = value;
        }
    }

    public string this[int index]
    {
        get => _tracks[CheckIndex(index)];
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _tracks[CheckIndex(index)] = value;
        }
    }

    public void PrintAllTracks()
    {
        Console.WriteLine(Title);
        for (int i = 0; i < _tracks.Length; i++)
            Console.WriteLine($"{i + 1}. {_tracks[i]}");
    }

    private int CheckIndex(int index) => index >= 0 && index < _tracks.Length
        ? index : throw new ArgumentOutOfRangeException(nameof(index));
}
