using Azure.Server.Utils.Communication.Authentication;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public interface ICustomLoginService
    {
        Task<RegistrationResult> Register(string email, string password);

        Task<bool> Login(string email, string password);

    }
}
