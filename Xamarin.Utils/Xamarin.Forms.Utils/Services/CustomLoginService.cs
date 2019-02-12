using Azure.Server.Utils.Communication.Authentication;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Xamarin.Forms.Utils.Models;

namespace Xamarin.Forms.Utils.Services
{
    public class CustomLoginService : ICustomLoginService
    {
        private readonly MobileServiceClient _mobileServiceClient;
        private readonly IAccountStoreService _accountStoreService;
        private readonly string _customRegistrationControllerName;
        private readonly string _customLoginControllerName;

        public CustomLoginService(
            MobileServiceClient mobileServiceClient,
            IAccountStoreService accountStoreService,
            string customRegistrationControllerName,
            string customLoginControllerName)
        {
            _mobileServiceClient = mobileServiceClient;
            _accountStoreService = accountStoreService;
            _customRegistrationControllerName = customRegistrationControllerName;
            _customLoginControllerName = customLoginControllerName;
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
            var registrationRequest = new RegistrationRequest() { UserId = email, Password = password };
            var ret = await _mobileServiceClient.InvokeApiAsync<RegistrationRequest, RegistrationResult>(
                _customRegistrationControllerName, registrationRequest);
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
            var loginRequest = new CustomLoginRequest() { UserId = email, Password = password };
            var ret = await _mobileServiceClient.InvokeApiAsync<CustomLoginRequest, CustomLoginResult>(
                _customLoginControllerName, loginRequest);
            if (ret != null)
            {
                _mobileServiceClient.CurrentUser = new MobileServiceUser(ret.UserId)
                {
                    MobileServiceAuthenticationToken = ret.MobileServiceAuthenticationToken
                };
                _accountStoreService.StoreTokenInSecureStore(new RefreshTokenInfo()
                {
                    UserId = ret.UserId,
                    Provider = "custom",
                    RefreshToken = ret.RefreshToken,
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
