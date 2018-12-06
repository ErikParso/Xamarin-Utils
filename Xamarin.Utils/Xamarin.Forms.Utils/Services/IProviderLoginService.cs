using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public interface IProviderLoginService
    {
        Task<bool> Login(MobileServiceAuthenticationProvider provider);
    }
}
