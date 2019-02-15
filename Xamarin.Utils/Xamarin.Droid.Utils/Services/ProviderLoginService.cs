using Android.Content;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Utils;
using Xamarin.Forms.Utils.Models;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Droid.Utils.Services
{
    public class ProviderLoginService : IProviderLoginService
    {
        private readonly AuthMobileServiceClient _mobileServiceClient;
        private readonly Context _context;
        private readonly IAccountStoreService _accountStoreService;
        private readonly string _uriScheme;

        public ProviderLoginService(
            Context context,
            AuthMobileServiceClient mobileServiceClient,
            IAccountStoreService accountStoreService,
            string uriScheme)
        {
            _mobileServiceClient = mobileServiceClient;
            _context = context;
            _accountStoreService = accountStoreService;
            _uriScheme = uriScheme;
        }

        /// <summary>
        /// Logins user using provider and stores token in acount store.
        /// Stored token will be used in <see cref="Authenticate"/> method.
        /// Sets logged user to <see cref="AuthMobileServiceClient.CurrentUser"/> and
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
                _accountStoreService.StoreTokenInSecureStore(new RefreshTokenInfo()
                {
                    UserId = _mobileServiceClient.CurrentUser.UserId,
                    Provider = provider.ToString(),
                    RefreshToken = _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken
                });
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}