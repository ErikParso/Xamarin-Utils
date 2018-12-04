using Azure.Server.Utils.Communication.Authentication;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms.Utils.Services;
using Xamarin.Forms.Utils.Validation;
using Xamarin.Forms.Utils.Validation.Core;

namespace Xamarin.Forms.Utils.ViewModel
{
    internal class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authenticationService;
        private bool _workInProgress;
        private bool _isRegistration;
        private ValidatableObject<string> _email;
        private ValidatableObject<string> _password;
        private ValidatableObject<string> _confirmPassword;

        public LoginViewModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            LoginCommand = new Command<MobileServiceAuthenticationProvider>(Login);
            LoginCustomCommand = new Command(LoginCustom);
            RegisterCommand = new Command(Register);

            AddValidations();
        }

        private void AddValidations()
        {
            Email = new ValidatableObject<string>(true) { Value = "parsoerik@gmail.com" };
            Email.RegisterValidationRule(new IsNotNullOrEmptyRule() { ValidationMessage = "Email is required." });
            Email.RegisterValidationRule(new EmailRule() { ValidationMessage = "Provide The valid email address." });
            Password = new ValidatableObject<string>(true) { Value = "@ReneMladencaStrastiASkusenost1" };
            Password.RegisterValidationRule(new IsNotNullOrEmptyRule() { ValidationMessage = "Password is required." });
            Password.RegisterValidationRule(new PasswordRule(false, true, true, true, 8) { ValidationMessage = "Passwords must be 8 more characters in length. Must contain at least 1 upper, 1 numeric and 1 special character." });
            ConfirmPassword = new ValidatableObject<string>(true) { Value = "@ReneMladencaStrastiASkusenost1" };
            ConfirmPassword.RegisterValidationRule(new IsNotNullOrEmptyRule() { ValidationMessage = "Confirm password is required." });
            ConfirmPassword.RegisterValidationRule(new ConfirmPasswordRule(() => Password.Value) { ValidationMessage = "Password and confirm password are not same." });
        }

        public Action UserAuthenticated { get; set; }

        public ICommand LoginCommand { get; set; }

        public ICommand LoginCustomCommand { get; set; }

        public ICommand RegisterCommand { get; set; }

        public bool IsRegistration
        {
            get => _isRegistration;
            set => SetField(ref _isRegistration, value);
        }

        public ValidatableObject<string> Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        public ValidatableObject<string> Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        public ValidatableObject<string> ConfirmPassword
        {
            get => _confirmPassword;
            set => SetField(ref _confirmPassword, value);
        }

        public bool WorkInProgress
        {
            get => _workInProgress;
            set => SetField(ref _workInProgress, value);
        }

        private async void Login(MobileServiceAuthenticationProvider provider)
        {
            WorkInProgress = true;
            if (await _authenticationService.Login(provider))
            {
                UserAuthenticated?.Invoke();
            }
            WorkInProgress = false;
        }

        private async void LoginCustom()
        {
            if (!Email.Validate() | !Password.Validate())
                return;

            WorkInProgress = true;
            if (await _authenticationService.Login(Email.Value, Password.Value))
            {
                UserAuthenticated?.Invoke();
            }
            WorkInProgress = false;
        }

        private async void Register()
        {
            if (!Email.Validate() | !Password.Validate() | !ConfirmPassword.Validate())
                return;

            WorkInProgress = true;
            if (await _authenticationService.Register(Email.Value, Password.Value) == RegistrationResult.Registered)
            {
                IsRegistration = false;
            }
            WorkInProgress = false;
        }

        public async void Authenticate()
        {
            WorkInProgress = true;
            if (await _authenticationService.Authenticate())
            {
                UserAuthenticated?.Invoke();
            }
            WorkInProgress = false;
        }

        #region Property changed

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaiseProperyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            RaiseProperyChanged(propertyName);
        }

        #endregion

    }
}
