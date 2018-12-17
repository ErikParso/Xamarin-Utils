using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Utils.Services
{
    public interface IAccountInformationService
    {
        Task<JToken> GetCurrentAccountInformation();
    }
}
