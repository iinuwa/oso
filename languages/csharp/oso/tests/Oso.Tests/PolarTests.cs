using Xunit;
using Oso;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Oso.Tests;

public class PolarTests
{
    [Fact]
    public void TestLoadAndQueryStr()
    {
        var polar = new Polar();
        polar.Load("f(1);");
        Query query = polar.NewQuery("f(x)", 0);
        // List<Dictionary<string, int>> expected = new () { new () { { "x", 1 } } };
        // List<string> expected = new() { @"""x"": 1" };
        string expected = @"[""x"": 1]";
        // TODO: Are any of these strings actually nullable? If not, we should go back and mark them as non-nullable.
        var ev = query.NextEvent()!;
        System.Console.WriteLine(ev);
        var doc = JsonDocument.Parse(ev);
        var actual  = doc.RootElement
            .GetProperty("Result")
            .GetProperty("bindings")
            .GetProperty("x")
            .GetProperty("value")
            .GetProperty("Number")
            .GetProperty("Integer")
            .GetInt32();
        Assert.Equal(1, actual);
    }
}