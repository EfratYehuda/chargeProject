using System.Security.Authentication;
using System.Security.Claims;

namespace chargeProject.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // The logged-in user's id. Throws AuthenticationException (-> 401) if the claim is missing.
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(value, out var id))
                throw new AuthenticationException("User id claim is missing or invalid.");

            return id;
        }

        // Null if the user has no driver profile.
        public static int? GetDriverId(this ClaimsPrincipal user) =>
            int.TryParse(user.FindFirstValue("driverId"), out var id) ? id : null;

        // Null if the user has no owner profile.
        public static int? GetOwnerId(this ClaimsPrincipal user) =>
            int.TryParse(user.FindFirstValue("ownerId"), out var id) ? id : null;

        // Use these in actions that need the profile. Throws (-> 403) if the profile is missing.
        public static int GetRequiredDriverId(this ClaimsPrincipal user) =>
            user.GetDriverId() ?? throw new UnauthorizedAccessException("A driver profile is required for this action.");

        public static int GetRequiredOwnerId(this ClaimsPrincipal user) =>
            user.GetOwnerId() ?? throw new UnauthorizedAccessException("A station owner profile is required for this action.");

        public static bool IsAdmin(this ClaimsPrincipal user) =>
            user.IsInRole("Admin");
    }
}