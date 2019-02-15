using Autofac;
using System;

namespace Xamarin.Forms.Utils
{
    public abstract class AppBase : Application
    {
        private IContainer _container;
        private ContainerBuilder _containerBuilder;

        public static IContainer CurrentAppContainer => ((AppBase)Current)._container;

        public AppBase()
        {
            _containerBuilder = new ContainerBuilder();
        }

        public void RegisterTypes(Action<ContainerBuilder> registrator)
        {
            if (_container != null)
                throw new Exception("Container already initialized.");
            if (registrator == null)
                throw new ArgumentException("Registrator is not specified");
            registrator(_containerBuilder);
        }

        public void Build()
        {
            _container = _containerBuilder.Build();
            _containerBuilder = null;
        }
    }
}
