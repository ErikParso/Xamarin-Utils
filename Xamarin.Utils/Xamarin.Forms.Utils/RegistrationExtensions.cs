using Autofac;
using Xamarin.Forms.Utils.Services;
using Xamarin.Forms.Utils.ViewModel;

namespace Xamarin.Forms.Utils
{
    public static class RegistrationExtensions
    {
        public static void RegisterAuthentication(this ContainerBuilder builder, string applicationUrl)
        {
            builder.RegisterType<RefreshTokenHandler>()
                .OnActivated(e => e.Instance.AuthenticationService = e.Context.Resolve<IAuthenticationService>())
                .SingleInstance();
            builder.RegisterType<AuthMobileServiceClient>()
                .WithParameter(new TypedParameter(typeof(string), applicationUrl))
                .SingleInstance();
            builder.RegisterType<LoginViewModel>()
                .SingleInstance();
            builder.RegisterType<ProfileBarViewModel>()
                .SingleInstance();
        }
    }
}
