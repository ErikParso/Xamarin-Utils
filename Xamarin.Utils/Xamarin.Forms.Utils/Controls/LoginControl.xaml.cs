using Autofac;
using System;
using Xamarin.Forms.Utils.ViewModel;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Utils.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginControl : ContentView
    {

        private readonly LoginViewModel _viewModel;

        public event EventHandler UserAuthenticated;

        public LoginControl()
        {
            InitializeComponent();
            _viewModel = AppBase.CurrentAppContainer.Resolve<LoginViewModel>();
            BindingContext = _viewModel;
            _viewModel.UserAuthenticated += () => UserAuthenticated(this, EventArgs.Empty);
        }

        public void Authenticate()
        {
            _viewModel.Authenticate();
        }
    }
}