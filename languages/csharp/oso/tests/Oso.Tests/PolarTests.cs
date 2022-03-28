using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;
namespace Oso.Tests;

public class MyClass
{
    public string Name { get; set; }
    public int Id { get; set; }

    public MyClass(string name, int id)
    {
        Name = name;
        Id = id;
    }
    public string MyMethod(string arg) => arg;
    public List<string> MyList() => new () { "hello", "world" };
    public MySubClass MySubClass(string name, int id) => new MySubClass(name, id);
    public static string MyStaticMethod() => "hello world";
    public string? MyReturnNull() => null;
}

public class MySubClass : MyClass
{
    public MySubClass(string name, int id) : base(name, id) { }
}

public class PolarTests
{
    [Fact]
    public void TestLoadAndQueryStr()
    {
        var polar = new Polar();
        polar.Load("f(1);");
        Query query = polar.NewQuery("f(x)", 0);
        // TODO: Are any of these strings actually nullable? If not, we should go back and mark them as non-nullable.
        var result = query.Results.ToList()[0];
        Assert.Equal(new() { { "x", 1 } }, result);
    }

    [Fact]
    public void TestBasicQueryPredicate()
    {
        // test basic query
        var polar = new Polar();
        polar.Load("f(a, b) if a = b;");
        Assert.True(polar.QueryRule("f", 1, 1).Results.Any(), "Basic predicate query failed.");
        Assert.False(
            polar.QueryRule("f", 1, 2).Results.Any(),
            "Basic predicate query expected to fail but didn't.");
    }
    [Fact]
    public void TestQueryPredWithObject()
    {
        // test query with Java Object
        var polar = new Polar();
        polar.RegisterClass(typeof(MyClass), "MyClass");
        polar.Load("g(x) if x.Id = 1;");
        Assert.True(
            polar.QueryRule("g", new MyClass("test", 1)).Results.Any(),
            "Predicate query with .NET object failed.");
        Assert.False(
            polar.QueryRule("g", new MyClass("test", 2)).Results.Any(),
            "Predicate query with .NET object expected to fail but didn't.");
    }

    /*

    [Fact]
    public void TestQueryPredWithVariable()
    {
        // test query with Variable
        var polar = new Polar();
        polar.Load("f(a, b) if a = b;");
        Assert.True(
            polar.QueryRule("f", 1, new Variable("result")).Results.Equals(new List<Dictionary<string, object>>() { new() { { "result", 1 } } }),
            "Predicate query with Variable failed.");
    }
    */

    /*** TEST FFI CONVERSIONS ***/

    [Fact]
    public void TestBoolFFIRoundTrip()
    {
        var polar = new Polar();
        bool b = true;
        JsonElement jsonTerm = polar.Host.SerializePolarTerm(b);
        object objectTerm = polar.Host.ParsePolarTerm(jsonTerm);
        Assert.Equal(b, objectTerm);
    }

    [Fact]
    public void TestIntFFIRoundTrip()
    {
        var polar = new Polar();
        int i = 3;
        JsonElement p = polar.Host.SerializePolarTerm(i);
        object o = polar.Host.ParsePolarTerm(p);
        Assert.Equal(i, o);
    }

    [Fact]
    public void TestFloatFFIRoundTrip()
    {
        var polar = new Polar();
        double f = 3.50;
        JsonElement p = polar.Host.SerializePolarTerm(f);
        object o = polar.Host.ParsePolarTerm(p);
        Assert.Equal(f, o);
    }

    [Fact]
    public void TestListFFIRoundTrip()
    {
        var polar = new Polar();
        List<int> l = new() { 1, 2, 3, 4 };
        JsonElement p = polar.Host.SerializePolarTerm(l);
        object o = polar.Host.ParsePolarTerm(p);
        Assert.Equal(l, o);
    }

    [Fact]
    public void TestArrayFFIRoundTrip()
    {
        var polar = new Polar();
        int[] a1 = { 1, 2, 3, 4 };
        JsonElement p = polar.Host.SerializePolarTerm(a1);
        object o = polar.Host.ParsePolarTerm(p);
        Assert.Equal(new List<int>() { 1, 2, 3, 4 }, o);

        double[] a2 = { 1.2, 3.5 };
        p = polar.Host.SerializePolarTerm(a2);
        o = polar.Host.ParsePolarTerm(p);

        Assert.Equal(new List<double>() { 1.2, 3.5 }, o);

        string[] a3 = { "hello", "world" };
        p = polar.Host.SerializePolarTerm(a3);
        o = polar.Host.ParsePolarTerm(p);
        Assert.Equal(new List<string>() { "hello", "world" }, o);
    }

    [Fact]
    public void TestDictFFIRoundTrip()
    {
        var polar = new Polar();
        Dictionary<string, dynamic> m = new() { { "a", 1 }, { "b", "two" } };
        JsonElement p = polar.Host.SerializePolarTerm(m);
        object o = polar.Host.ParsePolarTerm(p);
        Assert.Equal(m, o);
    }

/*
    [Fact]
    public void TestJavaClassFFIRoundTrip()
    {
        MyClass instance = new MyClass("test", 1);
        JsonElement polar = p.host.toPolarTerm(instance);
        object java = p.host.toJava(polar);
        Assert.Equal(instance, java);
    }

    [Fact]
    public void TestPredicateFFIRoundTrip()
    {
        Predicate pred = new Predicate("name", List.of(1, "hello"));
        JsonElement polar = p.host.toPolarTerm(pred);
        object java = p.host.toJava(polar);
        Assert.Equal(pred, java);
    }
    */

    [Fact]
    public void TestNaN()
    {
        var polar = new Polar();
        polar.RegisterConstant(double.NaN, "nan");

        Dictionary<string, object>? result = polar.NewQuery("x = nan", 0).Results.First();
        object x = result["x"];
        Assert.True(x is double);
        double y = (double)x;
        Assert.True(double.IsNaN(y));

        Assert.True(!polar.NewQuery("nan = nan", 1).Results.Any(), "NaN != NaN");
    }

    [Fact]
    public void TestInfinities()
    {
        var polar = new Polar();
        polar.RegisterConstant(double.PositiveInfinity, "inf");

        Dictionary<string, object> infResult = polar.NewQuery("x = inf", 0).Results.First();
        object inf = infResult["x"];
        Assert.True((double)inf == double.PositiveInfinity);

        Assert.True(polar.NewQuery("inf = inf", 0).Results.Any(), "Infinity == Infinity");

        polar.RegisterConstant(double.NegativeInfinity, "neg_inf");

        Dictionary<string, object> negInfResult = polar.NewQuery("x = neg_inf", 0).Results.First();
        var negInf = (double)negInfResult["x"];
        Assert.True(negInf == double.NegativeInfinity);

        Assert.True(polar.NewQuery("neg_inf = neg_inf", 0).Results.Any(), "-Infinity == -Infinity");

        Assert.False(polar.NewQuery("inf = neg_inf", 0).Results.Any(), "Infinity != -Infinity");
        Assert.False(polar.NewQuery("inf < neg_inf", 0).Results.Any(), "Infinity > -Infinity");
        Assert.True(polar.NewQuery("neg_inf < inf", 0).Results.Any(), "-Infinity < Infinity");
    }

    [Fact]
    public void TestNil()
    {
        var polar = new Polar();
        polar.Load("null(nil);");

        List<Dictionary<string, object>> expected = new () { new () { { "x", null! } } };
        Assert.Equal(polar.NewQuery("null(x)", 0).Results, expected);
        Assert.Equal(new List<Dictionary<string, object>>() { new () }, polar.QueryRule("null", args: new object?[] { null }).Results);
        Assert.False(polar.QueryRule("null", bindings: new ()).Results.Any());
    }
}