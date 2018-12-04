using Azure.Server.Utils.Communication.Authentication;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public interface IAuthenticationService
    {
        Task<bool> Authenticate();

        Task<bool> Login(MobileServiceAuthenticationProvider provider);

        Task<RegistrationResult> Register(string email, string password);

        Task<bool> Login(string email, string password);

        Task Logout();
    }
}
