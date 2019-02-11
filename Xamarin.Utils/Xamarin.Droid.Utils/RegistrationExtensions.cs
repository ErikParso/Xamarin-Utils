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

        public static void RegisterCustomLoginService(
            this ContainerBuilder containerBuilder,
            string customRegistrationControllerName = "CustomRegistration",
            string customLoginControllerName = "CustomLogin")
            => containerBuilder.RegisterType<CustomLoginService>().As<ICustomLoginService>()
               .WithParameter("customRegistrationControllerName", customRegistrationControllerName)
               .WithParameter("customLoginControllerName", customLoginControllerName)
               .SingleInstance();

        public static void RegisterProviderLoginService(this ContainerBuilder containerBuilder, string uriScheme)
            => containerBuilder.RegisterType<ProviderLoginService>().As<IProviderLoginService>()
               .WithParameter("uriScheme", uriScheme)
               .SingleInstance();

        public static void RegisterAuthenticationService(
            this ContainerBuilder containerBuilder,
            string customLoginControllerName = "CustomLogin")
            => containerBuilder.RegisterType<AuthenticationService>().As<IAuthenticationService>()
               .WithParameter("customLoginControllerName", customLoginControllerName)
               .SingleInstance();

        public static void RegisterVerificationService(
            this ContainerBuilder containerBuilder,
            string verificationControllerName = "Verification")
            => containerBuilder.RegisterType<VerificationService>().As<IVerificationService>()
               .WithParameter("verificationControllerName", verificationControllerName)
               .SingleInstance();

        public static void RegisterAccountInformationService(
            this ContainerBuilder containerBuilder,
            string accountsControllerName = "Accounts")
            => containerBuilder.RegisterType<AccountInformationService>().As<IAccountInformationService>()
               .WithParameter("accountsControllerName", accountsControllerName)
               .SingleInstance();
    }
}