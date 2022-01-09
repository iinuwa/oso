using System.Text.Json;

namespace Oso;

public class Host
{
    private readonly Dictionary<ulong, object> _instances = new();

    public bool AcceptExpression { get; set; }
    public Dictionary<string, object> DeserializePolarDictionary(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new OsoException($"Expected a JSON object element, received {element.ValueKind}");

        return element.EnumerateObject()
                        .Select(property => new KeyValuePair<string, object>(property.Name, ParsePolarTerm(property.Value)))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public List<object> DeserializePolarList(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
            throw new OsoException($"Expected a JSON array element, received {element.ValueKind}");

        return element.EnumerateArray().Select(ParsePolarTerm).ToList();
    }

    /// <summary>
    /// Make an instance of a class from a <see cref="List&lt;object&gt;" /> of fields. 
    /// </summary>
    internal void MakeInstance(string className, List<object> constructorArgs, ulong instanceId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    internal bool IsA(JsonElement instance, string className)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    internal bool IsSubclass(string leftTag, string rightTag)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    internal bool Subspecializer(ulong instanceId, string leftTag, string rightTag)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Turn a Polar term passed across the FFI boundary into an <see cref="object" />.
    /// </summary>
    internal object ParsePolarTerm(JsonElement term)
    {
        /*
            {
                "value": {"String": "someValue" }
            }

         */
        // TODO: Would this be better as a JsonConverter?
        JsonElement value = term.GetProperty("value");
        var property = value.EnumerateObject().First();
        string tag = property.Name;
        switch (tag)
        {
            case "String":
                return property.Value.GetBoolean();
            case "Boolean":
                return property.Value.GetBoolean();
            case "Number":
                JsonProperty numProperty = property.Value.EnumerateObject().First();
                string numType = numProperty.Name;
                switch (numType)
                {
                    case "Integer":
                        return numProperty.Value.GetInt32();
                    case "Float":
                        if (numProperty.Value.ValueKind == JsonValueKind.String)
                        {
                            return numProperty.Value.GetString() switch
                            {
                                "Infinity" => double.PositiveInfinity,
                                "-Infinity" => double.NegativeInfinity,
                                "NaN" => double.NaN,
                                var f => throw new OsoException($"Expected a floating point number, got `{f}`"),
                            };
                        }
                        return numProperty.Value.GetDouble();
                }
                throw new OsoException("Unexpected Number type: {numType}");
            case "List":
                return DeserializePolarList(property.Value);
            case "Dictionary":
                return DeserializePolarDictionary(property.Value.GetProperty("fields"));
            case "ExternalInstance":
                throw new NotImplementedException();
            // return getInstance(property.Value.GetProperty("instance_id").GetUInt64());
            case "Call":
                List<object> args = DeserializePolarList(property.Value.GetProperty("args"));
                throw new NotImplementedException();
            // return new Predicate(property.Value.GetProperty("name").GetString(), args);
            case "Variable":
                throw new NotImplementedException();
            // return new Variable(property.Value.GetString());
            case "Expression":
                if (!AcceptExpression)
                {
                    // TODO: More specific exceptions?
                    // throw new Exceptions.UnexpectedPolarTypeError(Exceptions.UNEXPECTED_EXPRESSION_MESSAGE);
                    const string unexpectedExpressionMessage = "Received Expression from Polar VM. The Expression type is only supported when\n"
                        + "using data filtering features. Did you perform an "
                        + "operation over an unbound variable in your policy?\n\n"
                        + "To silence this error and receive an Expression result, pass\n"
                        + "acceptExpression as true to Oso.query.";
                    throw new OsoException(unexpectedExpressionMessage);
                }
                throw new NotImplementedException();
            /*
            return new Expression(
                Enum.Parse<Operator>(property.Value.GetProperty("operator").GetString()),
                DeserializePolarList(property.Value.GetProperty("args")));
                */
            case "Pattern":
                throw new NotImplementedException();
            /*
            JsonProperty pattern = value.GetProperty("Pattern");
            string patternTag = pattern.Name;
            return patternTag switch
            {
                "Instance" => new Pattern(
                                            pattern.Value.GetProperty("tag").GetString,
                                            DeserializePolarDictionary(pattern.Value.GetProperty("fields").GetProperty("fields"))),
                "Dictionary" => new Pattern(null, DeserializePolarDictionary(pattern.Value)),
                _ => throw new Exceptions.UnexpectedPolarTypeError("Pattern: " + patternTag),
            };
            */
            default:
                // throw new Exceptions.UnexpectedPolarTypeError(tag);
                // TODO: Rename PolarException to OsoException.
                throw new OsoException($"Unexpected polar type: {tag}");
        }
    }

    public JsonElement SerializePolarTerm(object term)
    {
        throw new NotImplementedException();
    }


    public bool Operator(string op, List<object> args)
    {
        throw new NotImplementedException();
        /*
        Object left = args.get(0), right = args.get(1);
        if (op.equals("Eq")) {
            if (left == null) return left == right;
            else return left.equals(right);
        }
        throw new Exceptions.UnimplementedOperation(op);
        */
    }

    /// <summary>
    /// Determine if an instance has been cached.
    /// </summary>
    public bool HasInstance(ulong instanceId) => _instances.ContainsKey(instanceId);

private object GetInstance(ulong instanceId)
{
    return _instances.TryGetValue(instanceId, out object? value)
        ? value
        : throw new OsoException($"Unregistered instance: {instanceId}");
}
}