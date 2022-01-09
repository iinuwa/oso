using Xunit;
namespace Oso.Tests;

public class PolarTests
{
    [Fact]
    public void TestLoadAndQueryStr()
    {
        var polar = new Polar();
        polar.Load("f(1);");
        Query query = polar.NewQuery("f(x)", 0);
        // TODO: Are any of these strings actually nullable? If not, we should go back and mark them as non-nullable.
        var result = query.NextResult()!; // TODO: This should be an enumerator?
        Assert.Equal(new() { { "x", 1 } }, result);
    }
}