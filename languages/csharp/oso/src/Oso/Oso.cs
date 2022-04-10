namespace Oso;

public class Oso : Polar
{
    /// <summary> Used to differentiate between a <see cref="NotFoundException" /> and a
    /// <see cref="ForbiddenException" /> on authorization failures.
    /// </summary>
    public object ReadAction { get; set; } = "read";

    public Oso() : base() { }

    public bool IsAllowed(object actor, object action, object resource) => QueryRuleOnce("allow", actor, action, resource);

    /// <summary>
    /// Return the allowed actions for the given actor and resource, if any.
    /// </summary>
    /// 
    /// <code>
    /// Oso oso = new Oso();
    /// o.loadStr("allow(\"guest\", \"get\", \"widget\");");
    /// HashSet actions = o.getAllowedActions("guest", "widget");
    /// assert actions.contains("get");
    /// </code>
    /// 
    /// <param name="actor">The actor performing the request</param>
    /// <param name="resource">The resource being accessed</param>
    /// <returns cref="HashSet&lt;object&gt;" />
    /// <throws cref="OsoException" />
    public HashSet<object> GetAllowedActions(object actor, object resource)
    {
        return AuthorizedActions(actor, resource, false);
    }

    /// <summary>
    /// Determine the actions <paramref name="actor" /> is allowed to take on <paramref name="resource" />.
    /// 
    /// Collects all actions allowed by allow rules in the Polar policy for the given combination of
    /// actor and resource.
    /// </summary>
    /// 
    /// <param name="actor">The actor for whom to collect allowed actions</param>
    /// <param name="resource">The resource being accessed</param>
    /// <param name="allowWildcard">
    ///   Flag to determine behavior if the policy includes a wildcard action.
    ///   E.g., given a rule allowing any action:
    ///   <code>
    ///     allow(_actor, _action, _resource)
    ///   </code>
    ///   If <c>true</c>, the method will return <c>["*"]</c>.
    ///   if <c>false</c>, the method will raise an exception.
    /// </param>
    /// <returns> A list of the unique allowed actions.</returns>
    /// <throws cref="OsoException" />
    public HashSet<object> AuthorizedActions(object actor, object resource, bool allowWildcard = false)
    {
        return QueryRule("allow", actor, new Variable("action"), resource).Results
            .Select(action =>
            {
                if (action["action"] is not Variable) return action["action"];
                return allowWildcard
                    ? "*"
                    : throw new OsoException(
                        "\"The result of authorizedActions contained an \"unconstrained\" action that" +
                            " could represent any\n" +
                            " action, but allowWildcard was set to false. To fix,\n" +
                            " set allowWildcard to true and compare with the \"*\"\n" +
                            " string.\"");
            }).ToHashSet();
    }

    /// <summary>
    /// Ensure that <paramref name="actor" /> is allowed to perform <paramref name="action" /> on <paramref name="resource" />.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// If the action is permitted with an <c>allow</c> rule in the policy, then this method returns
    /// without error. If the action is not permitted by the policy, this method will raise an error.
    /// </para>
    /// 
    /// <para>
    /// The error raised by this method depends on whether the actor can perform the <c>"read"</c> action
    /// on the resource. If they cannot read the resource, then a <see cref="NotFoundException" /> is raised.
    /// Otherwise, a <see cref="ForbiddenException" /> is raised.
    /// </para>
    /// </remarks>
    /// 
    /// <param name="actor">The actor performing the request.</param>
    /// <param name="action">The action the actor is attempting to perform.</param>
    /// <param name="resource">The resource being accessed.</param>
    /// <param name="checkRead">
    ///     If set to <c>false</c>, a <see cref="ForbiddenException" /> is always thrown on authorization
    ///     failures, regardless of whether the actor can read the resource. Default is <c>true</c>.</param>
    /// <throws name="OsoException" />
    public void Authorize(object actor, object action, object resource, bool checkRead = true)
    {
        bool authorized = QueryRuleOnce("allow", actor, action, resource);
        if (authorized)
        {
            return;
        }
        // Authorization failure. Determine whether to throw a NotFoundException or
        // a ForbiddenException.
        if (checkRead)
        {
            if (action == ReadAction || !QueryRuleOnce("allow", actor, ReadAction, resource))
            {
                throw new NotFoundException();
            }
        }
        throw new ForbiddenException();
    }
}