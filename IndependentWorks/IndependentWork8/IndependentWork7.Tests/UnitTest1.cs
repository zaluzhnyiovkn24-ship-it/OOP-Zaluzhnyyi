using IndependentWork7;

namespace IndependentWork7.Tests;

public class CacheTests
{
    [Fact]
    public void AddMakesKeyAvailable()
    {
        var cache = new Cache<string, int>(2);
        cache.Add("a", 1);
        Assert.True(cache.ContainsKey("a"));
        Assert.False(cache.ContainsKey("b"));
    }

    [Fact]
    public void GetReturnsStoredValue()
    {
        var cache = new Cache<string, int>(2);
        cache.Add("a", 42);
        Assert.Equal(42, cache.Get("a"));
    }

    [Fact]
    public void MissingKeyThrows()
    {
        var cache = new Cache<string, int>(2);
        Assert.Throws<KeyNotFoundException>(() => cache.Get("missing"));
    }

    [Fact]
    public void OverflowEvictsOldestEvenAfterReadingIt()
    {
        var cache = new Cache<string, int>(2);
        cache.Add("a", 1);
        cache.Add("b", 2);
        Assert.Equal(1, cache.Get("a"));
        cache.Add("c", 3);

        Assert.False(cache.ContainsKey("a"));
        Assert.Equal(2, cache.Get("b"));
        Assert.Equal(3, cache.Get("c"));
        Assert.Equal(2, cache.Count);
    }

    [Fact]
    public void UpdatingKeyKeepsItsOriginalPosition()
    {
        var cache = new Cache<string, int>(2);
        Assert.Equal(0, cache.Count);
        cache.Add("a", 1);
        cache.Add("b", 2);
        cache.Add("a", 10);
        Assert.Equal(10, cache.Get("a"));
        Assert.Equal(2, cache.Count);

        cache.Add("c", 3);
        Assert.False(cache.ContainsKey("a"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CapacityMustBePositive(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cache<string, int>(capacity));
    }
}
