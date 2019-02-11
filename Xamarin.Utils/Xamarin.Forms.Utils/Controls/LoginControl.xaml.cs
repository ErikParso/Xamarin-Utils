using Autofac;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Utils.Services;
using Xamarin.Forms.Utils.ViewModel;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Utils.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginControl : ContentView
    {

        #region Bindable properties

        public static readonly BindableProperty TestUrlProperty = BindableProperty.Create(
            nameof(TestUrl), typeof(string), typeof(LineSplitter), "TestUrl");
        public static readonly BindableProperty TestProperty = BindableProperty.Create(
            nameof(Test), typeof(bool), typeof(LineSplitter), true);

        public string TestUrl
        {
            get { return (string)GetValue(TestUrlProperty); }
            set { SetValue(TestUrlProperty, value); }
        }

        public bool Test
        {
            get { return (bool)GetValue(TestProperty); }
            set { SetValue(TestProperty, value); }
        }

        #endregion

        private readonly LoginViewModel _viewModel;

        public event EventHandler UserAuthenticated;

        public LoginControl()
        {
            InitializeComponent();
            _viewModel = AppBase.CurrentAppContainer.Resolve<LoginViewModel>();
            BindingContext = _viewModel;
            _viewModel.UserAuthenticated += () => UserAuthenticated(this, EventArgs.Empty);
            _viewModel.ActionPerformed += (s) => TestResult.Text = s;
        }

        public void Authenticate()
        {
            _viewModel.Authenticate();
        }

        private async void TestRequestClick(object sender, EventArgs e)
        {
            try
            {
                var client = AppBase.CurrentAppContainer.Resolve<MobileServiceClient>();
                var result = await client.InvokeApiAsync<string>(TestUrl);
                TestResult.Text = $"MobileServiceClient.InvokeApiAsync('{TestUrl}') result:{Environment.NewLine}{result}";
            }
            catch (Exception ex)
            {
                TestResult.Text = $"MobileServiceClient.InvokeApiAsync('{TestUrl}') failed:{Environment.NewLine}{ex.Message}";
            }
        }

        private async void TestAuthenticateUser(object sender, EventArgs e)
        {
            try
            {
                var service = AppBase.CurrentAppContainer.Resolve<IAuthenticationService>();
                string result = await service.Authenticate();
                TestResult.Text = $"Autthentication result {result}";
            }
            catch (Exception ex)
            {
                TestResult.Text = ex.Message;
            }
        }

        private async void TestLogoutUser(object sender, EventArgs e)
        {
            try
            {
                var service = AppBase.CurrentAppContainer.Resolve<IAuthenticationService>();
                await service.Logout();
                TestResult.Text = "Logout executed";
            }
            catch (Exception ex)
            {
                TestResult.Text = ex.Message;
            }
        }

        private async void TestVerify(object sender, EventArgs e)
        {
            try
            {
                var service = AppBase.CurrentAppContainer.Resolve<IVerificationService>();
                await service.Verify();
                TestResult.Text = "Verification method executed";
            }
            catch (Exception ex)
            {
                TestResult.Text = ex.Message;
            }
        }

        private async void TestGetCurrentUserInfo(object sender, EventArgs e)
        {
            try
            {
                var service = AppBase.CurrentAppContainer.Resolve<IAccountInformationService>();
                TestResult.Text = (await service.GetCurrentAccountInformation()).ToString();
            }
            catch (Exception ex)
            {
                TestResult.Text = ex.Message;
            }
        }

        private void CreateNewAccountClick(object sender, EventArgs e)
        {
            _viewModel.IsRegistration = true;
        }

        private void UseExistingAccountClick(object sender, EventArgs e)
        {
            _viewModel.IsRegistration = false;
        }

        #region Showing validation errors


        private async void ShowError(object sender, IEnumerable<string> e)
        {
            if (validationLabel.IsVisible)
                return; //showing previous errors
            validationLabel.IsVisible = true;
            foreach (string error in e)
            {
                await ShowErrorSlowly(error);
            }
            validationLabel.IsVisible = false;
        }

        private async Task ShowErrorSlowly(string e)
        {
            validationLabel.Opacity = 0;
            validationLabel.Text = e;
            await validationLabel.FadeTo(1, 100);
            await validationLabel.FadeTo(1, (uint)e.Length * 40);
            await validationLabel.FadeTo(0, 1000);
        }

        #endregion

    }
}