using Azure.Server.Utils.Email;
using Azure.Server.Utils.Extensions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    public class VerificationController<T> : ApiController
        where T : AccountBase, new()
    {
        private readonly CustomAuthenticationContext<T> _context;
        private readonly IEmailService _emailService;

        public VerificationController(CustomAuthenticationContext<T> context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [Authorize]
        public void PostVerify()
        {
            var email = this.GetCurrentUserClaim(ClaimTypes.NameIdentifier);
            T account = _context.Accounts
                .Where(a => a.Provider == Provider.Custom)
                .Where(a => a.Sid == email)
                .SingleOrDefault();
            if (account != null)
            {
                string confirmationKey = CustomLoginProviderUtils.RandomString(32);
                account.ConfirmationHash = CustomLoginProviderUtils.Hash(confirmationKey, account.Salt);
                account.Verified = false;
                _context.SaveChanges();
                _emailService.SendEmail("Account confirmation", CreateConfirmationLink(email, confirmationKey), email);
            }
        }

        public HttpResponseMessage GetVerify(string email, string key)
        {
            string result = null;
            T account = _context.Accounts
                .Where(a => a.Provider == Provider.Custom)
                .Where(a => a.Sid == email)
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

        private string CreateConfirmationLink(string email, string confirmationKey)
            => Request.RequestUri + $"?ZUMO-API-VERSION=2.0.0&email={email}" + $"&key={confirmationKey}";
    }
}
