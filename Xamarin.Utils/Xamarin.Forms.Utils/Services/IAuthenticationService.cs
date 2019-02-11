using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public interface IAuthenticationService
    {
        Task<string> Authenticate();

        Task Logout();
    }
}
