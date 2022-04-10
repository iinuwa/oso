using Xunit;

namespace Oso.Tests;

public class EnforcementTests
{
    private Oso _oso;
    public EnforcementTests()
    {
        _oso = new Oso();
        _oso.RegisterClass(typeof(User), "User");
        _oso.RegisterClass(typeof(Widget), "Widget");
    }
    public record class User(string Name);
    public record class Widget(int Id);
    public record class Request(string Method, string Path);
  [Fact]
  public void TestAuthorize()
{
    User guest = new User("guest");
    User admin = new User("admin");
    Widget widget0 = new Widget(0);
    Widget widget1 = new Widget(1);

    _oso.LoadStr(@"
        allow(_actor: User, ""read"", widget: Widget) if
            widget.Id = 0;
        allow(actor: User, ""update"", _widget: Widget) if
            actor.Name = ""admin"";
    ");

    _oso.Authorize(guest, "read", widget0);
    _oso.Authorize(admin, "update", widget1);

    // Throws a forbidden exception when user can read resource
    Assert.Throws<ForbiddenException>(() => _oso.Authorize(guest, "update", widget0));

    // Throws a not found exception when user cannot read resource
    Assert.Throws<NotFoundException>(() => _oso.Authorize(guest, "read", widget1));
    Assert.Throws<NotFoundException>(() => _oso.Authorize(guest, "update", widget1));

    // With checkRead = false, returns a forbidden exception
    Assert.Throws<ForbiddenException>(() => _oso.Authorize(guest, "read", widget1, false));
    Assert.Throws<ForbiddenException>(() => _oso.Authorize(guest, "update", widget1, false));
  }

}