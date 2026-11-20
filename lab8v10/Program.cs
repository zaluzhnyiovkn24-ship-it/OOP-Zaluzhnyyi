List<Game> games =
[
    new BoardGame("Forest Trail", 2, 4),
    new VideoGame("Orbit Survey", "PC"),
    new CardGame("Market Day", 2)
];

var played = new List<Game>();
foreach (Game game in games)
{
    game.Play();
    played.Add(game);
}

Console.WriteLine();
Console.WriteLine($"Played games ({played.Count}):");
foreach (Game game in played)
    Console.WriteLine($"- {game.Title}");

public class Game(string title)
{
    public string Title { get; } = title;

    public virtual void Play() => Console.WriteLine($"Playing {Title}.");
}

public sealed class BoardGame(string title, int minPlayers, int maxPlayers) : Game(title)
{
    public int MinPlayers { get; } = minPlayers;
    public int MaxPlayers { get; } = maxPlayers;

    public override void Play() =>
        Console.WriteLine($"Board game: {Title} ({MinPlayers}-{MaxPlayers} players).");
}

public sealed class VideoGame(string title, string platform) : Game(title)
{
    public string Platform { get; } = platform;

    public override void Play() =>
        Console.WriteLine($"Video game: {Title} on {Platform}.");
}

public sealed class CardGame(string title, int numDecks) : Game(title)
{
    public int NumDecks { get; } = numDecks;

    public override void Play() =>
        Console.WriteLine($"Card game: {Title} ({NumDecks} decks).");
}
