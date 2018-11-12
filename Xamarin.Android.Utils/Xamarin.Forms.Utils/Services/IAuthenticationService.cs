using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public interface IAuthenticationService
    {
        Task<bool> Login();

        Task Logout();
    }
}
