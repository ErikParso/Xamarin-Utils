using Azure.Server.Utils.CustomAuthentication;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;

namespace Azure.Server.Utils.Extensions
{
    public static class ApiControllerExtensions
    {
        public static string GetAuthenticatedByClaim(this IPrincipal user)
            => GetCurrentUserClaim(user, Constants.AUTHENTICATED_BY_CLAIM);

        public static string GetCurrentUserClaim(this IPrincipal user, string claim)
            => ((ClaimsPrincipal)user).FindFirst(claim)?.Value;

        public static T GetCurrentUserAccount<T>(this IPrincipal user, DbSet<T> accountDbSet)
            where T : AccountBase
            => accountDbSet.GetUserAccount(
                user.GetCurrentUserClaim(ClaimTypes.NameIdentifier),
                user.Identity.AuthenticationType);

        public static T GetUserAccount<T>(this DbSet<T> accountDbSet, string sid, string provider)
            where T : AccountBase
            => accountDbSet.Where(a => a.Provider == provider)
                 .Where(a => a.Sid == sid)
                 .SingleOrDefault();
    }
}
