using Autofac;
using System;
using Xamarin.Forms.Utils.ViewModel;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Utils.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfileBar : ContentView
    {
        private readonly ProfileBarViewModel _viewModel;

        public event EventHandler LogoutClicked;

        public ProfileBar()
        {
            InitializeComponent();
            _viewModel = AppBase.CurrentAppContainer.Resolve<ProfileBarViewModel>();
            BindingContext = _viewModel;
        }

        public void LoadAccountInformation()
        {
            _viewModel.LoadAccountInformation();
        }

        private void LogoutButton_Clicked(object sender, System.EventArgs e)
        {
            LogoutClicked(this, EventArgs.Empty);
        }
    }
}