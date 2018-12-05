using Android.Content;
using Azure.Server.Utils.Communication.Authentication;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Droid.Utils.Services
{
    /// <summary>
    /// Authentication service specified for android platform.
    /// Use <see cref="RegistrationExtensions.RegisterAuthenticationService"/> to register this service.
    /// </summary>
    /// <seealso cref="IAuthenticationService" />
    public class AuthenticationService : IAuthenticationService
    {
        private readonly MobileServiceClient _mobileServiceClient;
        private readonly Context _context;
        private readonly AccountStore _accountStore;
        private readonly string _uriScheme;

        private readonly string _customRegistrationController;
        private readonly string _customLoginController;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        /// <param name="context">The context (use your main activity).</param>
        /// <param name="mobileServiceClient">The mobile service client.</param>
        /// <param name="uriScheme">The URI scheme of your azure mobile app.</param>
        /// <param name="accountStorePassword">The account store password.</param>
        /// <param name="authControllerName">Name of the custom authentication controller.</param>
        public AuthenticationService(
            Context context,
            MobileServiceClient mobileServiceClient,
            string uriScheme,
            string accountStorePassword,
            string customLoginController,
            string customRegistrationController)
        {
            _mobileServiceClient = mobileServiceClient;
            _context = context;
            _uriScheme = uriScheme;
            _accountStore = AccountStore.Create(context, accountStorePassword);
            _customRegistrationController = customRegistrationController;
            _customLoginController = customLoginController;
        }


        #region Authenticate, Login, Logout

        /// <summary>
        /// Finds first token stored in account store.
        /// If successfull, tries to refresh and store token.
        /// If token refresh failed (custom authentication or facebook obviously), checks token expiration.
        /// If no token stored, authentication is unsuccessfull. In this case execute one of login methods.
        /// </summary>
        /// <returns>Authentication result.</returns>
        public async Task<bool> Authenticate()
        {
            _mobileServiceClient.CurrentUser = RetrieveTokenFromSecureStore();
            if (_mobileServiceClient.CurrentUser != null)
            {
                try
                {
                    var refreshed = await _mobileServiceClient.RefreshUserAsync();
                    _mobileServiceClient.CurrentUser = refreshed;
                    StoreTokenInSecureStore(refreshed);
                    return true;
                }
                catch (Exception e) //some providers doesn't support refresh
                {
                    return IsTokenActive(_mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken);
                }
            }
            else //No stored account
            {
                return false;
            }
        }

        /// <summary>
        /// Logins user using provider and stores token in acount store.
        /// Stored token will be used in <see cref="Authenticate"/> method.
        /// Sets logged user to <see cref="MobileServiceClient.CurrentUser"/> and
        /// access to authorized requests should be gratned.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>Login result.</returns>
        public async Task<bool> Login(MobileServiceAuthenticationProvider provider)
        {
            var parameters = new Dictionary<string, string>{
                { "access_type", "offline" },
                { "prompt", "consent" }
            };
            await _mobileServiceClient.LoginAsync(_context, provider, _uriScheme, parameters);

            if (_mobileServiceClient.CurrentUser != null)
            {
                StoreTokenInSecureStore(_mobileServiceClient.CurrentUser);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Registers new user using custom register controller. Access is to authorized requests is not granted.
        /// Call <see cref="Login(string, string)"/> method for this purpose.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <returns>Registration result.</returns>
        public async Task<RegistrationResult> Register(string email, string password)
        {
            var registrationRequest = new RegistrationRequest() { Email = email, Password = password };
            var ret = await _mobileServiceClient.InvokeApiAsync<RegistrationRequest, RegistrationResult>(
                _customRegistrationController, registrationRequest);
            return ret;
        }

        /// <summary>
        /// Logins user using custom login controller and stores token in account store.
        /// Stored token will be used in <see cref="Authenticate"/> method.
        /// Sets logged user to <see cref="MobileServiceClient.CurrentUser"/> and
        /// access to authorized requests should be gratned.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <returns>Login result.</returns>
        public async Task<bool> Login(string email, string password)
        {
            var loginRequest = new CustomLoginRequest() { Email = email, Password = password };
            var ret = await _mobileServiceClient.InvokeApiAsync<CustomLoginRequest, CustomLoginResult>(
                _customLoginController, loginRequest);
            if (ret != null)
            {
                _mobileServiceClient.CurrentUser = new MobileServiceUser(ret.UserId)
                {
                    MobileServiceAuthenticationToken = ret.MobileServiceAuthenticationToken
                };
                StoreTokenInSecureStore(_mobileServiceClient.CurrentUser);
                return true;
            }
            else
            {
                return false;
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

            RemoveTokenFromSecureStore();
            await _mobileServiceClient.LogoutAsync();
        }

        #endregion


        #region Account store

        private MobileServiceUser RetrieveTokenFromSecureStore()
        {
            var accounts = _accountStore.FindAccountsForService(_uriScheme);
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    if (acct.Properties.TryGetValue("token", out string token))
                    {
                        return new MobileServiceUser(acct.Username)
                        {
                            MobileServiceAuthenticationToken = token
                        };
                    }
                }
            }
            return null;
        }

        private void StoreTokenInSecureStore(MobileServiceUser user)
        {
            var account = new Account(user.UserId);
            account.Properties.Add("token", user.MobileServiceAuthenticationToken);
            _accountStore.Save(account, _uriScheme);
        }

        private void RemoveTokenFromSecureStore()
        {
            var accounts = _accountStore.FindAccountsForService(_uriScheme);
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    _accountStore.Delete(acct, _uriScheme);
                }
            }
        }

        #endregion


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