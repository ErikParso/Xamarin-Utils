using Android.Content;
using Autofac;
using Microsoft.WindowsAzure.MobileServices;
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
            string customLoginController,
            string customRegistrationController)
        {
            containerBuilder.RegisterType<AuthenticationService>().As<IAuthenticationService>()
            .WithParameter(new TypedParameter(typeof(Context), context))
            .WithParameter("uriScheme", "balanse")
            .WithParameter("accountStorePassword", "1234")
            .WithParameter("customLoginController", "CustomLogin")
            .WithParameter("customRegistrationController", "CustomRegistration")
            .SingleInstance();
        }
    }
}