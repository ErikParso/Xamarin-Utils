using Autofac;
using System;

namespace Xamarin.Forms.Utils
{
    public abstract class AppBase : Application
    {
        protected readonly IContainer Container;

        public static IContainer CurrentAppContainer => ((AppBase)Current).Container;

        public AppBase(Action<ContainerBuilder> registerPlatformSpecific)
        {
            // Fill container with types.
            ContainerBuilder builder = new ContainerBuilder();
            RegisterShared(builder);
            registerPlatformSpecific?.Invoke(builder);
            Container = builder.Build();

            // set the root page of your application
            MainPage = GetMainPage();
        }

        protected abstract void RegisterShared(ContainerBuilder builder);

        /// <summary>
        /// Provide startup page. You can use <see cref="CurrentAppContainer"/>
        /// </summary>
        protected abstract Page GetMainPage();
    }
}
