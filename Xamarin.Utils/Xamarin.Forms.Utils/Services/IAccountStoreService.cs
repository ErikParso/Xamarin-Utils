using Microsoft.WindowsAzure.MobileServices;

namespace Xamarin.Forms.Utils.Services
{
    public interface IAccountStoreService
    {
        MobileServiceUser RetrieveTokenFromSecureStore();

        void StoreTokenInSecureStore(MobileServiceUser user);

        void ClearSecureStore();
    }
}
