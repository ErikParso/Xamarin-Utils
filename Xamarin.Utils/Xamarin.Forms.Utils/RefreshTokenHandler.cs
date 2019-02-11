using Autofac;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Forms.Utils
{
    public class RefreshTokenHandler : DelegatingHandler
    {
        private readonly IAccountStoreService _accountStoreService;

        public RefreshTokenHandler(IAccountStoreService accountStoreService)
        {
            _accountStoreService = accountStoreService;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }
            var newAccessToken = await AppBase.CurrentAppContainer.Resolve<IAuthenticationService>().Authenticate();
            if (string.IsNullOrWhiteSpace(newAccessToken))
            {
                return response;
            }
            else
            {
                response.Dispose();
                request.Headers.Remove("X-ZUMO-AUTH");
                request.Headers.Add("X-ZUMO-AUTH", newAccessToken);
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
