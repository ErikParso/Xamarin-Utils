using Azure.Server.Utils.Communication.Authentication;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public interface IVerificationService
    {
        Task Verify();
    }
}
