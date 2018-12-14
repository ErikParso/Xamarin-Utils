using System.Security.Claims;
using System.Web.Http;

namespace Azure.Server.Utils.Extensions
{
    public static class ApiControllerExtensions
    {
        public static string GetCurrentUserClaim(this ApiController controller, string claim)
            => ((ClaimsPrincipal)controller.User).FindFirst(claim)?.Value;
    }
}
