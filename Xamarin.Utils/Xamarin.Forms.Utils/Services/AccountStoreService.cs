using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Auth;

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

        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            var accounts = _accountStore.FindAccountsForService(_serviceId);
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

        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            var account = new Account(user.UserId);
            account.Properties.Add("token", user.MobileServiceAuthenticationToken);
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