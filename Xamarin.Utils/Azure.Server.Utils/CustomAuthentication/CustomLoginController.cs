using Azure.Server.Utils.Communication.Authentication;
using Microsoft.Azure.Mobile.Server.Login;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    public abstract class CustomLoginController<T> : ApiController
        where T : AccountBase
    {
        private readonly CustomAuthenticationContext<T> _context;
        private readonly string _siteUrl;
        private readonly TimeSpan _accessTokenLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomLoginController{T}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="siteUrl">The site URL 'https://myservice.azurewebsites.net/'.</param>
        /// <param name="accessTokenLifetime">The lifetime.</param>
        public CustomLoginController(CustomAuthenticationContext<T> context, string siteUrl, TimeSpan accessTokenLifetime)
        {
            _context = context;
            _siteUrl = siteUrl;
            _accessTokenLifetime = accessTokenLifetime;
        }

        [HttpPost]
        public CustomLoginResult Login(CustomLoginRequest loginRequest)
        {
            T account = _context.Accounts.Where(a => a.Sid == loginRequest.Email).SingleOrDefault();
            if (account != null)
            {
                byte[] incoming = CustomLoginProviderUtils.Hash(loginRequest.Password, account.Salt);
                if (CustomLoginProviderUtils.SlowEquals(incoming, account.Hash))
                {
                    var accessToken = GetAuthenticationTokenForUser(account.Sid);
                    account.RefreshToken = CustomLoginProviderUtils.GenerateRefreshToken();
                    _context.SaveChanges();
                    return new CustomLoginResult()
                    {
                        UserId = account.Sid,
                        MobileServiceAuthenticationToken = accessToken.RawData,
                        RefreshToken = account.RefreshToken
                    };
                }
            }
            return null;
        }

        [HttpGet]
        public IHttpActionResult RefreshToken(string userId, string refreshToken)
        {
            T account = _context.Accounts.Where(a => a.Sid == userId).SingleOrDefault();
            if (account.RefreshToken != refreshToken)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }
            else
            {
                var newAccessToken = GetAuthenticationTokenForUser(userId);
                account.RefreshToken = CustomLoginProviderUtils.GenerateRefreshToken();
                _context.SaveChanges();
                return Ok(new CustomLoginResult()
                {
                    UserId = account.Sid,
                    MobileServiceAuthenticationToken = newAccessToken.RawData,
                    RefreshToken = account.RefreshToken
                });
            }
        }

        private JwtSecurityToken GetAuthenticationTokenForUser(string email)
            => AppServiceLoginHandler.CreateToken(
                   GetClaims(email),
                   Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY"),
                   _siteUrl,
                   _siteUrl,
                   _accessTokenLifetime);

        private IEnumerable<Claim> GetClaims(string email) => new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(ClaimTypes.NameIdentifier, "custom"),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };
    }
}
