using Android.Content;
using Autofac;
using Xamarin.Droid.Utils.Services;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Droid.Utils
{
    public static class RegistrationExtensions
    {
        public static void RegisterAuthenticationService(
            this ContainerBuilder containerBuilder,
            Context context,
            string uriScheme,
            string accountStorePassword,
            string customLoginController = "CustomLogin",
            string customRegistrationController = "CustomRegistration")
        {
            containerBuilder.RegisterType<AuthenticationService>().As<IAuthenticationService>()
            .WithParameter(new TypedParameter(typeof(Context), context))
            .WithParameter("uriScheme", uriScheme)
            .WithParameter("accountStorePassword", accountStorePassword)
            .WithParameter("customLoginController", customLoginController)
            .WithParameter("customRegistrationController", customRegistrationController)
            .SingleInstance();
        }
    }
}