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
    /// Authenticates current user stored in AccountStore. Tries to refresh account and chcecks token activity.
    /// Authentication is successfull, if stored users token is valid.
    /// Otherwise call Login method to log user and store his account in AccountStore.
    /// Call Logout method to logout user and remove his account from account store.
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
        /// Tries to find user registered in Account store.
        /// If successfull, tries to refresh his token and stores a new token in Account store.
        /// If token refresh failed (facebook), checks token expiration.
        /// If user not registerd in AccountStore or Refresh of expired token failed, authentication is unsuccessfull.
        /// In this case call <see cref="Login(MobileServiceAuthenticationProvider)"/> method.
        /// </summary>
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
        /// Logins user using specidied provider and stores his account in AccountStore for <see cref="Authenticate"/> method.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>If Login successfull, returns true, otherwise false.</returns>
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


        public async Task<RegistrationResult> Register(string email, string password)
        {
            var registrationRequest = new RegistrationRequest() { Email = email, Password = password };
            var ret = await _mobileServiceClient.InvokeApiAsync<RegistrationRequest, RegistrationResult>(
                _customRegistrationController, registrationRequest);
            return ret;
        }


        public async Task<bool> Login(string email, string password)
        {
            var ret = await _mobileServiceClient.InvokeApiAsync<CustomLoginResult>(
                _customLoginController, HttpMethod.Post, new Dictionary<string, string> {
                { "email", email}, { "password", password}
            });
            _mobileServiceClient.CurrentUser = new MobileServiceUser(ret.UserId)
            {
                MobileServiceAuthenticationToken = ret.MobileServiceAuthenticationToken
            };
            StoreTokenInSecureStore(_mobileServiceClient.CurrentUser);
            return true;
        }


        /// <summary>
        /// Logouts user and removes his account from account store.
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