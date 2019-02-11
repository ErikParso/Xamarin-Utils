using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms.Utils.Models;

namespace Xamarin.Forms.Utils.Services
{
    public interface IAccountStoreService
    {
        RefreshTokenInfo RetrieveTokenFromSecureStore();

        void StoreTokenInSecureStore(RefreshTokenInfo refreshTokenInfo);

        void ClearSecureStore();
    }
}
