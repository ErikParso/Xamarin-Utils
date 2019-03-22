using Microsoft.WindowsAzure.MobileServices;

namespace Xamarin.Forms.Utils
{
    public class AuthMobileServiceClient : MobileServiceClient
    {
        public AuthMobileServiceClient(string appUrl, RefreshTokenHandler refreshTokenHandler)
            : base(appUrl, refreshTokenHandler)
        {
            
        }
    }
}
