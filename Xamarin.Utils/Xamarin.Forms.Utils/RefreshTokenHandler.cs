using Microsoft.WindowsAzure.MobileServices;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Forms.Utils
{
    public class RefreshTokenHandler : DelegatingHandler
    {
        public MobileServiceClient MobileServiceClient { get; set; }

        public IAuthenticationService AuthenticationService { get; set; }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }
            var newAccessToken = await AuthenticationService.Authenticate();
            if (string.IsNullOrWhiteSpace(newAccessToken))
            {
                return response;
            }
            else
            {
                response.Dispose();
                MobileServiceClient.CurrentUser.MobileServiceAuthenticationToken = newAccessToken;
                request.Headers.Remove("X-ZUMO-AUTH");
                request.Headers.Add("X-ZUMO-AUTH", newAccessToken);
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
