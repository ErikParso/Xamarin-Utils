using Android.Content;
using Autofac;
using Xamarin.Auth;
using Xamarin.Droid.Utils.Services;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Droid.Utils
{
    public static class RegistrationExtensions
    {
        public static void RegisterContext(this ContainerBuilder containerBuilder, Context context)
            => containerBuilder.RegisterInstance(context);

        public static void RegisterAccountStore(this ContainerBuilder containerBuilder, AccountStore accountStore)
            => containerBuilder.RegisterInstance(accountStore);

        public static void RegisterAccountStoreService(this ContainerBuilder containerBuilder, string serviceId)
            => containerBuilder.RegisterType<AccountStoreService>().As<IAccountStoreService>()
               .WithParameter("serviceId", serviceId)
               .SingleInstance();

        public static void RegisterAuthenticationService(
            this ContainerBuilder containerBuilder,
            string uriScheme,
            string customLoginController = "CustomLogin",
            string customRegistrationController = "CustomRegistration")
        {
            containerBuilder.RegisterType<AuthenticationService>().As<IAuthenticationService>()
            .WithParameter("uriScheme", uriScheme)
            .WithParameter("customLoginController", customLoginController)
            .WithParameter("customRegistrationController", customRegistrationController)
            .SingleInstance();
        }
    }
}