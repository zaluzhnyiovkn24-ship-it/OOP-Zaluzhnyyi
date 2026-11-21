using IndependentWork7;

var cache = new Cache<string, string>(2);
cache.Add("a", "first");
cache.Add("b", "second");
Console.WriteLine(cache.Get("a"));
cache.Add("c", "third");
Console.WriteLine($"a still cached: {cache.ContainsKey("a")}");
Console.WriteLine($"Entries: {cache.Count}");
