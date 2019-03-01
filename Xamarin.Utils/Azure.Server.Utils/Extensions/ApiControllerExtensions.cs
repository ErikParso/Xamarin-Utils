using Azure.Server.Utils.CustomAuthentication;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Azure.Server.Utils.Extensions
{
    public static class ApiControllerExtensions
    {
        public static string GetCurrentUserClaim(this ApiController controller, string claim)
            => ((ClaimsPrincipal)controller.User).FindFirst(claim)?.Value;

        public static T GetCurrentUserAccount<T>(this ApiController controller, DbSet<T> accountDbSet)
            where T : AccountBase
            => accountDbSet.GetUserAccount(
                controller.GetCurrentUserClaim(ClaimTypes.NameIdentifier),
                controller.User.Identity.AuthenticationType);

        public static T GetUserAccount<T>(this DbSet<T> accountDbSet, string sid, string provider)
            where T : AccountBase
            => accountDbSet.Where(a => a.Provider == provider)
                 .Where(a => a.Sid == sid)
                 .SingleOrDefault();
    }
}
