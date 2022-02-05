using System.Collections.Generic;
using System.Text.Json;
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
        var result = query.NextResult(); // TODO: This should be an enumerator?
        Assert.Equal(new() { { "x", 1 } }, result);
    }

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

    [Fact]
    public void TestNaN()
    {
        var polar = new Polar();
        // TODO: 
        // polar.registerConstant(Double.NaN, "nan");

        Dictionary<string, object> result = polar.NewQuery("x = nan", 0).NextResult();
        object x = result["x"];
        Assert.True(x is double);
        double y = (double)x;
        Assert.True(double.IsNaN(y));

        Assert.True(polar.NewQuery("nan = nan", 0).NextResult().Count == 0, "NaN != NaN");
        // assertTrue(p.query("nan = nan").results().isEmpty(), "NaN != NaN");
    }

    [Fact]
    public void TestInfinities()
    {
        p.registerConstant(Double.POSITIVE_INFINITY, "inf");

        List<HashMap<String, object>> inf_results = p.query("x = inf").results();
        HashMap<String, object> inf_result = inf_results.get(0);
        object inf = inf_result.get("x");
        Assert.True((Double)inf == Double.POSITIVE_INFINITY);

        Assert.False(p.query("inf = inf").results().isEmpty(), "Infinity == Infinity");

        p.registerConstant(Double.NEGATIVE_INFINITY, "neg_inf");

        List<HashMap<String, object>> neg_inf_results = p.query("x = neg_inf").results();
        HashMap<String, object> neg_inf_result = neg_inf_results.get(0);
        object neg_inf = neg_inf_result.get("x");
        Assert.True((Double)neg_inf == Double.NEGATIVE_INFINITY);

        Assert.False(p.query("neg_inf = neg_inf").results().isEmpty(), "-Infinity == -Infinity");

        Assert.True(p.query("inf = neg_inf").results().isEmpty(), "Infinity != -Infinity");
        Assert.True(p.query("inf < neg_inf").results().isEmpty(), "Infinity > -Infinity");
        Assert.False(p.query("neg_inf < inf").results().isEmpty(), "-Infinity < Infinity");
    }

    [Fact]
    // test_nil
    public void TestNil()
    {
        p.loadStr("null(nil);");

        // Map.of() can't handle a null value.
        HashMap<String, object> expected = new HashMap<String, Object>();
        expected.put("x", null);
        Assert.Equal(p.query("null(x)").results(), List.of(expected));
        Assert.True(p.queryRule("null", (object)null).results().equals(List.of(Map.of())));
        Assert.True(p.queryRule("null", List.of()).results().isEmpty());
    }
    */
}