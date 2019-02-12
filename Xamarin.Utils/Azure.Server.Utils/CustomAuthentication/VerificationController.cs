using Azure.Server.Utils.Email;
using Azure.Server.Utils.Extensions;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    public abstract class VerificationController<C, A> : ApiController
        where C : DbContext
        where A : AccountBase, new()
    {
        private readonly C _context;
        private readonly IEmailService<A> _emailService;

        public VerificationController(C context, IEmailService<A> emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [Authorize]
        public void PostVerify()
        {
            var userId = this.GetCurrentUserClaim(ClaimTypes.NameIdentifier);
            A account = GetAccountsDbSet(_context)
                .Where(a => a.Provider == "Federation")
                .Where(a => a.Sid == userId)
                .SingleOrDefault();
            if (account != null)
            {
                string confirmationKey = CustomLoginProviderUtils.RandomString(32);
                account.ConfirmationHash = CustomLoginProviderUtils.Hash(confirmationKey, account.Salt);
                account.Verified = false;
                _context.SaveChanges();
                _emailService.SendEmail("Account confirmation", CreateConfirmationLink(userId, confirmationKey), account);
            }
        }

        public HttpResponseMessage GetVerify(string userId, string key)
        {
            string result = null;
            A account = GetAccountsDbSet(_context)
                .Where(a => a.Provider == "Federation")
                .Where(a => a.Sid == userId)
                .SingleOrDefault();
            if (account != null)
            {
                if (account.Verified)
                {
                    result = "Account is already verified.";
                }
                else
                {
                    var hash = CustomLoginProviderUtils.Hash(key, account.Salt);
                    if (CustomLoginProviderUtils.SlowEquals(hash, account.ConfirmationHash))
                    {
                        account.Verified = true;
                        _context.SaveChanges();
                        result = "Account was successfuly verified.";
                    }
                    else
                    {
                        result = "Wrong verification key.";
                    }
                }
            }
            else
            {
                result = "Account was not found.";
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent($"<html><body>{result}</body></html>");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        protected abstract DbSet<A> GetAccountsDbSet(C fromContext);

        private string CreateConfirmationLink(string userId, string confirmationKey)
            => Request.RequestUri + $"?ZUMO-API-VERSION=2.0.0&userId={userId}" + $"&key={confirmationKey}";
    }
}
