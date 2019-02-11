using Xamarin.Auth;
using Xamarin.Forms.Utils.Models;

namespace Xamarin.Forms.Utils.Services
{
    public class AccountStoreService : IAccountStoreService
    {
        private readonly AccountStore _accountStore;
        private readonly string _serviceId;

        public AccountStoreService(AccountStore accountStore, string serviceId)
        {
            _accountStore = accountStore;
            _serviceId = serviceId;
        }

        public RefreshTokenInfo RetrieveTokenFromSecureStore()
        {
            var accounts = _accountStore.FindAccountsForService(_serviceId);
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    if (acct.Properties.TryGetValue("token", out string token) &&
                        acct.Properties.TryGetValue("provider", out string provider))
                    {
                        return new RefreshTokenInfo()
                        {
                            UserId = acct.Username,
                            RefreshToken = token,
                            Provider = provider
                        };
                    }
                }
            }
            return null;
        }

        public void StoreTokenInSecureStore(RefreshTokenInfo user)
        {
            ClearSecureStore();
            var account = new Account(user.UserId);
            account.Properties.Add("token", user.RefreshToken);
            account.Properties.Add("provider", user.Provider);
            _accountStore.Save(account, _serviceId);
        }

        public void ClearSecureStore()
        {
            var accounts = _accountStore.FindAccountsForService(_serviceId);
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    _accountStore.Delete(acct, _serviceId);
                }
            }
        }
    }
}