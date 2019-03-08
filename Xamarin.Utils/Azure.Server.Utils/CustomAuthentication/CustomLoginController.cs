using Azure.Server.Utils.Communication.Authentication;
using Azure.Server.Utils.Extensions;
using Microsoft.Azure.Mobile.Server.Login;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    /// <summary>
    /// Custom authentication login controller with token refreshing.
    /// </summary>
    /// <typeparam name="C">Db context.</typeparam>
    /// <typeparam name="A">Account type.</typeparam>
    /// <seealso cref="ApiController" />
    public abstract class CustomLoginController<C, A> : ApiController
        where C : DbContext
        where A : AccountBase
    {
        private readonly C _context;
        private readonly string _siteUrl;
        private readonly TimeSpan _accessTokenLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomLoginController{T}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="siteUrl">The site URL.</param>
        /// <param name="accessTokenLifetime">The access token lifetime.</param>
        public CustomLoginController(C context, string siteUrl, TimeSpan accessTokenLifetime)
        {
            _context = context;
            _siteUrl = siteUrl;
            _accessTokenLifetime = accessTokenLifetime;
        }

        /// <summary>
        /// Logins user specified by <paramref name="loginRequest"/> and initializes <see cref="AccountBase.RefreshToken"/>.
        /// </summary>
        /// <param name="loginRequest"> User credentials. </param>
        /// <returns>
        /// Login result (access and refresh token).
        /// </returns>
        [HttpPost]
        public IHttpActionResult Login(CustomLoginRequest loginRequest)
        {
            A account = GetAccountsDbSet(_context).GetUserAccount(loginRequest.UserId, "Federation");
            if (account != null)
            {
                byte[] incoming = CustomLoginProviderUtils.Hash(loginRequest.Password, account.Salt);
                if (CustomLoginProviderUtils.SlowEquals(incoming, account.Hash))
                {
                    var accessToken = GetAuthenticationTokenForUser(account.Sid);
                    account.RefreshToken = CustomLoginProviderUtils.GenerateRefreshToken();
                    _context.SaveChanges();
                    return Ok(new CustomLoginResult()
                    {
                        UserId = account.Sid,
                        MobileServiceAuthenticationToken = accessToken.RawData,
                        RefreshToken = account.RefreshToken
                    });
                }
            }
            return BadRequest("Invalid name or password.");
        }

        /// <summary>
        /// Refreshes the token.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>New login result with access and refresh token.</returns>
        /// <exception cref="SecurityTokenException">Invalid refresh token</exception>
        [HttpGet]
        public IHttpActionResult RefreshToken(string userId, string refreshToken)
        {
            A account = GetAccountsDbSet(_context).GetUserAccount(userId, "Federation");
            if (account == null || account.RefreshToken != refreshToken)
            {
                return BadRequest("Invalid account or refresh token.");
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

        /// <summary>
        /// Provides db set of accounts from <paramref name="fromContext"/>.
        /// </summary>
        /// <param name="fromContext">Db ontext.</param>
        /// <returns>Db set of accounts.</returns>
        protected abstract DbSet<A> GetAccountsDbSet(C fromContext);

        private JwtSecurityToken GetAuthenticationTokenForUser(string userId)
            => AppServiceLoginHandler.CreateToken(
                   GetClaims(userId),
                   Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY"),
                   _siteUrl,
                   _siteUrl,
                   _accessTokenLifetime);

        private IEnumerable<Claim> GetClaims(string userId) => new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.NameIdentifier, "custom")
        };
    }
}
