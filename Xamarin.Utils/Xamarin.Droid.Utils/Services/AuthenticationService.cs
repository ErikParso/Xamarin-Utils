using Android.Content;
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
    public class AuthenticationService : IAuthenticationService
    {
        private readonly MobileServiceClient _mobileServiceClient;
        private readonly Context _context;
        private readonly AccountStore _accountStore;
        private readonly string _uriScheme;

        public AuthenticationService(
            Context context,
            MobileServiceClient mobileServiceClient,
            string uriScheme,
            string accountStorePassword)
        {
            _mobileServiceClient = mobileServiceClient;
            _context = context;
            _uriScheme = uriScheme;
            _accountStore = AccountStore.Create(context, accountStorePassword);
        }

        #region Login, Logout

        public async Task<bool> Login()
        {
            _mobileServiceClient.CurrentUser = RetrieveTokenFromSecureStore();
            if (_mobileServiceClient.CurrentUser != null)
            {
                try
                {
                    var refreshed = await _mobileServiceClient.RefreshUserAsync();
                    if (refreshed != null)
                    {
                        _mobileServiceClient.CurrentUser = refreshed;
                        StoreTokenInSecureStore(refreshed);
                        return true;
                    }
                }
                catch (Exception)
                {

                }
            }

            if (_mobileServiceClient.CurrentUser != null && !IsTokenExpired(_mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken))
            {
                // User has previously been authenticated, no refresh is required
                return true;
            }

            // We need to ask for credentials at this point
            await LoginAsync(_mobileServiceClient);
            if (_mobileServiceClient.CurrentUser != null)
            {
                // We were able to successfully log in
                StoreTokenInSecureStore(_mobileServiceClient.CurrentUser);
            }

            return _mobileServiceClient.CurrentUser != null;
        }

        public async Task Logout()
        {
            if (_mobileServiceClient.CurrentUser == null || _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken == null)
                return;

            // Invalidate the token on the mobile backend
            var authUri = new Uri($"{_mobileServiceClient.MobileAppUri}/.auth/logout");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken);
                await httpClient.GetAsync(authUri);
            }

            // Remove the token from the cache
            RemoveTokenFromSecureStore();

            // Remove the token from the MobileServiceClient
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
            var accounts = _accountStore.FindAccountsForService("myjobdiary");
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    _accountStore.Delete(acct, "myjobdiary");
                }
            }
        }

        #endregion


        #region Private helpers

        private async Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            var parameters = new Dictionary<string, string>{
                { "access_type", "offline" },
                { "prompt", "consent" }
            };
            return await client.LoginAsync(_context, MobileServiceAuthenticationProvider.Google, _uriScheme, parameters);
        }

        private bool IsTokenExpired(string token)
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
            return (expire < DateTime.UtcNow);
        }

        #endregion

    }
}