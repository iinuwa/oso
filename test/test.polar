allow("a","b","c");

a(a_var, x_val) if a_var = new A{x: x_val};
?= a(a_instance, "hello") and a_instance.x = "hello";

c(instance, y) if instance = new C{y: y};
?= c(instance, "hello") and instance.y = "hello";

specializers(a: A, c: C) if
    a.x = c.y;

builtinSpecializers(x: Boolean, "Boolean") if x = true;
builtinSpecializers(x: Integer, "Integer") if x > 0;
builtinSpecializers(x: Float, "Float") if x > 0.0;
builtinSpecializers(x: List, "List") if x = ["foo", *_rest];
builtinSpecializers(x: Dictionary, "Dictionary") if x.foo = "foo";
builtinSpecializers(x: String, "String") if x = "foo";

floatLists() if 3.14159 in ["pi", 3.14159];

intDicts() if {a: 42}.a = 42;

comparisons() if
    2 > 1
    and not (1 > 2)
    and 1 < 2
    and not (2 < 1)
    and 1 >= 1
    and 2 >= 1
    and not (0 >= 1)
    and 1 <= 1
    and 1 <= 2
    and not (3 <= 2)
    and 1 == 1
    # and not (-1 == 1)
    # and 1 != -1
    and not (1 != 1);

testForall() if
    forall(x in [1, 1], x = 1)
    and not forall(x in [1, 2], x = 1);

testRest() if
    [_, *tail] = [1, 2, 3]
    and tail = [2, 3];

testMatches(a) if
    a matches {x: "hello"}
    and a matches A{x: "hello"}
    and a matches A
    and {x: 1, y: 2} matches {x: 1}
    and {x: 1, y: 3} matches {y: 3}
    and {x: 1, y: 3} matches {x:1, y: 3}
    and not {x: 1, y: 3} matches {x:1, y: 4}
    and new A{x: "hello"} matches A
    and new A{x: "hello"} matches A{x: "hello"};

testMethodCalls(a, c) if
    a.foo() == c.foo();

testOr() if false or true;

testCut() if
    x in [1, 2, 3]
    and x > 1
    and cut
    and x == 3;

testHttpAndPathMapper() if
    new Http{hostname: "foo", path: "/", query: {x: 1}}.hostname = "foo"
    and new PathMapper{template: "/foo/{id}/bar/{ego}"}.map("/foo/1/bar/2") = {id: "1", ego: "2"};
