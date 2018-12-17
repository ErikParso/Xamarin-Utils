using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public class AccountInformationService : IAccountInformationService
    {
        private readonly MobileServiceClient _mobileServiceClient;
        private readonly string _accountsControllerName;

        public AccountInformationService(MobileServiceClient mobileServiceClient, string accountsControllerName)
        {
            _mobileServiceClient = mobileServiceClient;
            _accountsControllerName = accountsControllerName;
        }

        public async Task<JToken> GetCurrentAccountInformation()
            => await _mobileServiceClient.InvokeApiAsync(_accountsControllerName);
    }
}
