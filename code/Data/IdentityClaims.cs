using Microsoft.AspNetCore.Identity;

namespace Koi.Web.Data;

// Identity defines an integer claim Id; the derived property is mapped as a Guid key.
public class UserClaim : IdentityUserClaim<Guid>
{
    public new Guid Id { get; set; }
}

public class RoleClaim : IdentityRoleClaim<Guid>
{
    public new Guid Id { get; set; }
}
