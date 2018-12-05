using Azure.Server.Utils.Communication.Authentication;
using Microsoft.Azure.Mobile.Server.Login;
using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    public abstract class CustomLoginController<T> : ApiController
        where T : AccountBase
    {
        private readonly CustomAuthenticationContext<T> _context;
        private readonly string _siteUrl;
        private readonly TimeSpan _lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomLoginController{T}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="siteUrl">The site URL 'https://myservice.azurewebsites.net/'.</param>
        /// <param name="lifetime">The lifetime.</param>
        public CustomLoginController(CustomAuthenticationContext<T> context, string siteUrl, TimeSpan lifetime)
        {
            _context = context;
            _siteUrl = siteUrl;
            _lifetime = lifetime;
        }

        public CustomLoginResult Post(CustomLoginRequest loginRequest)
        {
            T account = _context.Accounts.Where(a => a.Sid == loginRequest.Email).SingleOrDefault();
            if (account != null)
            {
                byte[] incoming = CustomLoginProviderUtils.Hash(loginRequest.Password, account.Salt);
                if (CustomLoginProviderUtils.SlowEquals(incoming, account.Hash))
                {
                    var token = GetAuthenticationTokenForUser(account.Sid);
                    return new CustomLoginResult()
                    {
                        UserId = account.Sid,
                        MobileServiceAuthenticationToken = token.RawData
                    };
                }
            }
            return null;
        }

        private JwtSecurityToken GetAuthenticationTokenForUser(string email)
            => AppServiceLoginHandler.CreateToken(new Claim[] { new Claim(JwtRegisteredClaimNames.Sub, email) },
                    Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY"), _siteUrl, _siteUrl, _lifetime);
    }
}
