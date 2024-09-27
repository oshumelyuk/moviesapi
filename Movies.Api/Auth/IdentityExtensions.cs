namespace Movies.Api.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var userIdClaim = context.User.Claims.SingleOrDefault(x => x.Type == "userid");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            return userId;
        }
        return null;
    }
}