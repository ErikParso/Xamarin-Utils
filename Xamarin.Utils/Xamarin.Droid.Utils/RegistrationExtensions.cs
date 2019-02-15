using Android.Content;
using Autofac;
using Xamarin.Auth;
using Xamarin.Droid.Utils.Services;
using Xamarin.Forms.Utils;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Droid.Utils
{
    public static class RegistrationExtensions
    {
        public static void UseAuthentication(
            this AppBase app,
            Context context,
            string accountStorePassword,
            string applicationUrl,
            string uriScheme,
            string customRegistrationControllerName = "CustomRegistration",
            string customLoginControllerName = "CustomLogin",
            string verificationControllerName = "Verification",
            string accountsControllerName = "Accounts")
        {
            app.RegisterTypes((builder) =>
            {
                builder.RegisterAuthentication(applicationUrl);
                builder.RegisterContext(context);
                builder.RegisterAccountStore(AccountStore.Create(context, accountStorePassword));
                builder.RegisterAccountStoreService(applicationUrl);
                builder.RegisterCustomLoginService(customRegistrationControllerName, customLoginControllerName);
                builder.RegisterProviderLoginService(uriScheme);
                builder.RegisterAuthenticationService(customLoginControllerName);
                builder.RegisterVerificationService(verificationControllerName);
                builder.RegisterAccountInformationService(accountsControllerName);
            });
        }

        private static void RegisterContext(this ContainerBuilder containerBuilder, Context context)
            => containerBuilder.RegisterInstance(context);

        private static void RegisterAccountStore(this ContainerBuilder containerBuilder, AccountStore accountStore)
            => containerBuilder.RegisterInstance(accountStore);

        private static void RegisterAccountStoreService(this ContainerBuilder containerBuilder, string serviceId)
            => containerBuilder.RegisterType<AccountStoreService>().As<IAccountStoreService>()
               .WithParameter("serviceId", serviceId)
               .SingleInstance();

        private static void RegisterCustomLoginService(
            this ContainerBuilder containerBuilder,
            string customRegistrationControllerName,
            string customLoginControllerName)
            => containerBuilder.RegisterType<CustomLoginService>().As<ICustomLoginService>()
               .WithParameter("customRegistrationControllerName", customRegistrationControllerName)
               .WithParameter("customLoginControllerName", customLoginControllerName)
               .SingleInstance();

        private static void RegisterProviderLoginService(this ContainerBuilder containerBuilder, string uriScheme)
            => containerBuilder.RegisterType<ProviderLoginService>().As<IProviderLoginService>()
               .WithParameter("uriScheme", uriScheme)
               .SingleInstance();

        private static void RegisterAuthenticationService(
            this ContainerBuilder containerBuilder,
            string customLoginControllerName)
            => containerBuilder.RegisterType<AuthenticationService>().As<IAuthenticationService>()
               .WithParameter("customLoginControllerName", customLoginControllerName)
               .SingleInstance();

        private static void RegisterVerificationService(
            this ContainerBuilder containerBuilder,
            string verificationControllerName)
            => containerBuilder.RegisterType<VerificationService>().As<IVerificationService>()
               .WithParameter("verificationControllerName", verificationControllerName)
               .SingleInstance();

        private static void RegisterAccountInformationService(
            this ContainerBuilder containerBuilder,
            string accountsControllerName)
            => containerBuilder.RegisterType<AccountInformationService>().As<IAccountInformationService>()
               .WithParameter("accountsControllerName", accountsControllerName)
               .SingleInstance();
    }
}