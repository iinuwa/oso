using System;
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

#region Test Query
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
    public void TestInlineQueries()
    {
        var polar = new Polar();
        polar.Load("f(1); ?= f(1);");
        polar.ClearRules();
        try
        {
            var exception = Assert.Throws<OsoException>(() => polar.Load("f(1); ?= f(2);"));
        }
        catch (Exception e)
        {
            throw new Exception("Expected inline query to fail but it didn't.", e);
        }
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
        // test query with .NET Object
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

    [Fact]
    public void TestQueryPredWithVariable()
    {
        // test query with Variable
        var polar = new Polar();
        polar.Load("f(a, b) if a = b;");
        var expected = new List<Dictionary<string, object>>() { new() { { "result", 1 } } };
        try
        {
            Assert.Equal(expected, polar.QueryRule("f", 1, new Variable("result")).Results);
        }
        catch(Exception e)
        {
            throw new Exception("Predicate query with Variable failed.", e);
        }
    }

#endregion
#region Test FFI Conversions

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

    [Fact]
    public void TestJavaClassFFIRoundTrip()
    {
        var polar = new Polar();
        MyClass instance = new MyClass("test", 1);
        JsonElement p = polar.Host.SerializePolarTerm(instance);
        object o = polar.Host.ParsePolarTerm(p);
        Assert.Equal(instance, o);
    }

    [Fact]
    public void TestPredicateFFIRoundTrip()
    {
        var polar = new Polar(); 
        Predicate pred = new Predicate("name", new List<object>() { 1, "hello" });
        JsonElement t = polar.Host.SerializePolarTerm(pred);
        object o = polar.Host.ParsePolarTerm(t);
        Assert.Equal(pred, o);
    }

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
    #endregion
    #region Test Externals

    [Fact]
    public void TestRegisterAndMakeClass()
    {
        var polar = new Polar();
        polar.RegisterClass(typeof(MyClass), "MyClass");
        MyClass instance = (MyClass)polar.Host.MakeInstance("MyClass", new() { "testName", 1 }, 0UL);
        Assert.Equal("testName", instance.Name);
        Assert.Equal(1, instance.Id);
        // TODO: test that errors when given invalid constructor
        // TODO: test that errors when registering same class twice
        // TODO: test that errors if same alias used twice
        // TODO: test inheritance
    }

  [Fact]
  public void TestDuplicateRegistration()
    {
        var polar = new Polar();
        polar.RegisterClass(typeof(MyClass), "MyClass");
        var exception = Assert.Throws<OsoException>(() => polar.RegisterClass(typeof(MyClass), "MyClass"));
        Assert.Equal("Attempted to alias MyClass as Oso.Tests.MyClass, but Oso.Tests.MyClass already has that alias.", exception.Message);
        // TODO: Should exceptions end with periods?
  }

    [Fact]
    public void TestMakeInstanceFromPolar()
    {
        var polar = new Polar();
        polar.RegisterClass(typeof(MyClass), "MyClass");
        polar.Load("f(x) if x = new MyClass(\"test\", 1);");
        Query query = polar.NewQuery("f(x)", 0);
        MyClass ret = (MyClass)query.Results.First()["x"];
        Assert.Equal("test", ret.Name);
        Assert.Equal(1, ret.Id);
    }

    /*
        [Fact]
        public void TestNoKeywordArgs()
        {
            var polar = new Polar();
            polar.RegisterConstant(true, "MyClass");
            var e1 = Assert.Throws<OsoException>(() => polar.NewQuery("x = new MyClass(\"test\", id: 1)", 0).Results.First());
            Assert.Equal("Failed to instantiate external class MyClass; named arguments are not supported in .NET", e1.Message);
            var e2 = Assert.Throws<OsoException>(() => polar.NewQuery("x = (new MyClass(\"test\", 1)).Foo(\"test\", id: 1)", 0).Results.First());
            Assert.Equal("Invalid call `{callName}` on class {className}, with argument types `{argTypes}`", e2.Message);
        }
        */

    [Fact]
    public void TestExternalCall()
    {
        // Test get attribute
        var polar = new Polar();
        polar.RegisterClass(typeof(MyClass), "MyClass");
        polar.Load("id(x) if x = new MyClass(\"test\", 1).Id;");
        var expected1 = new List<Dictionary<string, object>>() { new() { { "x", (object)1 } } };
        Assert.True(
            polar.NewQuery("id(x)", 0).Results.First().SequenceEqual(expected1.First()),
            "Failed to get attribute on external instance.");

        polar.ClearRules();

        // Test call method
        polar.Load("method(x) if x = new MyClass(\"test\", 1).MyMethod(\"hello world\");");
        var expected2 = new Dictionary<string, object>() { { "x", "hello world" } };
        Assert.True(
            polar.NewQuery("method(x)", 0).Results.First().SequenceEqual(expected2),
            "Failed to get attribute on external instance.");
    }

/*
  [Fact]
  public void TestReturnJavaInstanceFromCall()
    {
    MyClass c = new MyClass("test", 1);
    p.loadStr("test(c: MyClass) if x = c.mySubClass(c.name, c.id) and x.id = c.id;");
    Assert.False(p.queryRule("test", c).results().isEmpty());
  }

  [Fact]
  public void TestEnumerationCallResults()
    {
    MyClass c = new MyClass("test", 1);
    p.loadStr("test(c: MyClass, x) if x in c.myEnumeration();");
    List<HashMap<String, Object>> results = p.queryRule("test", c, new Variable("x")).results();
    Assert.True(results.equals(List.of(Map.of("x", "hello"), Map.of("x", "world"))));
  }

  [Fact]
  public void TestStringMethods()
    {
    p.loadStr("f(x) if x.length() = 3;");
    Assert.False(p.query("f(\"oso\")").results().isEmpty());
    Assert.True(p.query("f(\"notoso\")").results().isEmpty());
  }

  [Fact]
  public void TestListMethods()
    {
    p.loadStr("f(x) if x.size() = 3;");
    Assert.False(p.queryRule("f", new ArrayList(Arrays.asList(1, 2, 3))).results().isEmpty());
    Assert.True(p.queryRule("f", new ArrayList(Arrays.asList(1, 2, 3, 4))).results().isEmpty());

    Assert.False(p.queryRule("f", new int[] {1, 2, 3}).results().isEmpty());
    Assert.True(p.queryRule("f", new int[] {1, 2, 3, 4}).results().isEmpty());
  }

  [Fact]
  public void TestExternalIsa()
    {
    p.loadStr("f(a: MyClass, x) if x = a.id;");
    List<HashMap<String, Object>> result =
        p.queryRule("f", new MyClass("test", 1), new Variable("x")).results();
    Assert.True(result.equals(List.of(Map.of("x", 1))));
    p.clearRules();

    p.loadStr("f(a: MySubClass, x) if x = a.id;");
    result = p.queryRule("f", new MyClass("test", 1), new Variable("x")).results();
    Assert.True(result.isEmpty(), "Failed to filter rules by specializers.");
    p.clearRules();

    p.loadStr("f(a: OtherClass, x) if x = a.id;");
    assertThrows(
        Exceptions.UnregisteredClassError.class,
        () -> p.queryRule("f", new MyClass("test", 1), new Variable("x")).results());
  }

  [Fact]
  public void TestExternalIsSubSpecializer()
    {
    String policy = "f(_: MySubClass, x) if x = 1;\n" + "f(_: MyClass, x) if x = 2;";
    p.loadStr(policy);
    List<HashMap<String, Object>> result =
        p.queryRule("f", new MySubClass("test", 1), new Variable("x")).results();
    Assert.True(
        result.equals(List.of(Map.of("x", 1), Map.of("x", 2))),
        "Failed to order rules based on specializers.");

    result = p.queryRule("f", new MyClass("test", 1), new Variable("x")).results();
    Assert.True(
        result.equals(List.of(Map.of("x", 2))), "Failed to order rules based on specializers.");
  }

  [Fact]
  public void TestExternalUnify()
    {
    Assert.False(p.query("new MyClass(\"foo\", 1) = new MyClass(\"foo\", 1)").results().isEmpty());
    Assert.True(p.query("new MyClass(\"foo\", 1) = new MyClass(\"foo\", 2)").results().isEmpty());
    Assert.True(p.query("new MyClass(\"foo\", 1) = new MyClass(\"bar\", 1)").results().isEmpty());
    Assert.True(p.query("new MyClass(\"foo\", 1) = {foo: 1}").results().isEmpty());
  }

  [Fact]
  public void TestExternalInternalUnify()
    {
    Assert.False(p.query("new String(\"foo\") = \"foo\"").results().isEmpty());
  }

  [Fact]
  public void TestReturnListFromCall()
    {
    p.loadStr("test(c: MyClass) if \"hello\" in c.myList();");
    MyClass c = new MyClass("test", 1);
    Assert.False(p.queryRule("test", c).results().isEmpty());
  }

  [Fact]
  public void TestClassMethods()
    {
    p.loadStr("test(x) if x=1 and MyClass.myStaticMethod() = \"hello world\";");

    Assert.False(p.query("test(1)").results().isEmpty());
  }

  [Fact]
  public void TestExternalOp()
    {
    Assert.False(p.query("new String(\"foo\") == new String(\"foo\")").results().isEmpty());
  }
  */
#endregion
}