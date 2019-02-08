using Autofac;
using Microsoft.WindowsAzure.MobileServices;
using System;
using Xamarin.Forms.Utils.ViewModel;

namespace Xamarin.Forms.Utils
{
    public abstract class AppBase : Application
    {
        protected readonly IContainer Container;

        public string ApplicationUrl { get; }

        public static IContainer CurrentAppContainer => ((AppBase)Current).Container;

        public AppBase(string applicationUrl, Action<ContainerBuilder> registerPlatformSpecificTypes)
        {
            ApplicationUrl = applicationUrl;

            ContainerBuilder builder = new ContainerBuilder();
            //Register types defined by Xamarin.Forms.Utils.
            RegisterUtilsTypes(builder);
            //Register types defined in shared library.
            RegisterSharedTypes(builder);
            //Register types defined by platform specific project.
            registerPlatformSpecificTypes?.Invoke(builder);
            Container = builder.Build();

            // set the root page of your application
            MainPage = GetMainPage();
        }

        private void RegisterUtilsTypes(ContainerBuilder builder)
        {
            builder.RegisterType<MobileServiceClient>()
                .WithParameter(new TypedParameter(typeof(string), ApplicationUrl))
                .SingleInstance();
            builder.RegisterType<LoginViewModel>()
                .SingleInstance();
            builder.RegisterType<ProfileBarViewModel>()
                .SingleInstance();
        }

        protected abstract void RegisterSharedTypes(ContainerBuilder builder);

        /// <summary>
        /// Provide startup page. You can use <see cref="CurrentAppContainer"/>
        /// </summary>
        protected abstract Page GetMainPage();
    }
}
