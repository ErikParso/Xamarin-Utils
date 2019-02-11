using Azure.Server.Utils.Communication.Authentication;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Utils.Models;

namespace Xamarin.Forms.Utils.Services
{
    /// <summary>
    /// Authentication service specified for android platform.
    /// Use <see cref="RegistrationExtensions.RegisterAuthenticationService"/> to register this service.
    /// </summary>
    /// <seealso cref="IAuthenticationService" />
    public class AuthenticationService : IAuthenticationService
    {
        private readonly MobileServiceClient _mobileServiceClient;
        private readonly IAccountStoreService _accountStoreService;
        private readonly string _customLoginControllerName;

        public AuthenticationService(
            MobileServiceClient mobileServiceClient,
            IAccountStoreService accountStoreService,
            string customLoginControllerName)
        {
            _mobileServiceClient = mobileServiceClient;
            _accountStoreService = accountStoreService;
            _customLoginControllerName = customLoginControllerName;
        }

        /// <summary>
        /// Finds first token stored in account store.
        /// If successfull, tries to refresh and store token.
        /// If token refresh failed (custom authentication or facebook obviously), checks token expiration.
        /// If no token stored, authentication is unsuccessfull. In this case execute one of login methods.
        /// </summary>
        /// <returns>Authentication result.</returns>
        public async Task<string> Authenticate()
        {
            var refreshTokenInfo = _accountStoreService.RetrieveTokenFromSecureStore();
            if (refreshTokenInfo == null)
                return string.Empty;
            if (refreshTokenInfo.Provider == "custom")
                return await CustomProviderAuthentication(refreshTokenInfo);
            else
                return await IdentityProviderAuthentication(refreshTokenInfo);
        }

        private async Task<string> CustomProviderAuthentication(RefreshTokenInfo refreshTokenInfo)
        {
            var ret = await _mobileServiceClient.InvokeApiAsync<CustomLoginResult>(
                _customLoginControllerName, HttpMethod.Get, new Dictionary<string, string> {
                    { "userId", refreshTokenInfo.UserId }, { "refreshToken", refreshTokenInfo.RefreshToken }
                });
            if (ret != null)
            {
                _mobileServiceClient.CurrentUser = new MobileServiceUser(ret.UserId)
                {
                    MobileServiceAuthenticationToken = ret.MobileServiceAuthenticationToken
                };
                refreshTokenInfo.RefreshToken = ret.RefreshToken;
                _accountStoreService.StoreTokenInSecureStore(refreshTokenInfo);
                return _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken;
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task<string> IdentityProviderAuthentication(RefreshTokenInfo refreshTokenInfo)
        {
            _mobileServiceClient.CurrentUser = new MobileServiceUser(refreshTokenInfo.UserId)
            {
                MobileServiceAuthenticationToken = refreshTokenInfo.RefreshToken
            };
            try
            {
                var refreshed = await _mobileServiceClient.RefreshUserAsync();
                _mobileServiceClient.CurrentUser = refreshed;
                refreshTokenInfo.RefreshToken = refreshed.MobileServiceAuthenticationToken;
                _accountStoreService.StoreTokenInSecureStore(refreshTokenInfo);
                return _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken;
            }
            catch //some providers doesn't support refresh
            {
                return IsTokenActive(_mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken) ?
                    _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken : string.Empty;
            }
        }

        /// <summary>
        /// Logouts user and removes his account from account store.
        /// Client is no more grantet to execute authorized requests.
        /// </summary>
        public async Task Logout()
        {
            if (_mobileServiceClient.CurrentUser == null || _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken == null)
                return;

            var authUri = new Uri($"{_mobileServiceClient.MobileAppUri}/.auth/logout");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken);
                await httpClient.GetAsync(authUri);
            }

            _accountStoreService.ClearSecureStore();
            await _mobileServiceClient.LogoutAsync();
        }


        #region Private helpers

        private bool IsTokenActive(string token)
        {
            // Get just the JWT part of the token (without the signature).
            var jwt = token.Split(new Char[] { '.' })[1];

            // Undo the URL encoding.
            jwt = jwt.Replace('-', '+').Replace('_', '/');
            switch (jwt.Length % 4)
            {
                case 0: break;
                case 2: jwt += "=="; break;
                case 3: jwt += "="; break;
                default:
                    throw new ArgumentException("The token is not a valid Base64 string.");
            }
            var bytes = Convert.FromBase64String(jwt);
            string jsonString = UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            // Parse as JSON object and get the exp field value,
            // which is the expiration date as a JavaScript primative date.
            JObject jsonObj = JObject.Parse(jsonString);
            var exp = Convert.ToDouble(jsonObj["exp"].ToString());

            // Calculate the expiration by adding the exp value (in seconds) to the
            // base date of 1/1/1970.
            DateTime minTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expire = minTime.AddSeconds(exp);
            return (expire > DateTime.UtcNow);
        }

        #endregion

    }
}