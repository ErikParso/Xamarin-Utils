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
        private readonly ICustomLoginService _customLoginService;
        private readonly IProviderLoginService _providerLoginService;
        private bool _workInProgress;
        private bool _isRegistration;
        private ValidatableObject<string> _email;
        private ValidatableObject<string> _password;
        private ValidatableObject<string> _confirmPassword;

        public LoginViewModel(
            IAuthenticationService authenticationService,
            ICustomLoginService customLoginService,
            IProviderLoginService providerLoginService)
        {
            _authenticationService = authenticationService;
            _customLoginService = customLoginService;
            _providerLoginService = providerLoginService;
            LoginCommand = new Command<MobileServiceAuthenticationProvider>(Login);
            LoginCustomCommand = new Command(LoginCustom);
            RegisterCommand = new Command(Register);

            AddValidations();
        }

        private void AddValidations()
        {
            Email = new ValidatableObject<string>(true) { Value = "" };
            Email.RegisterValidationRule(new IsNotNullOrEmptyRule() { ValidationMessage = "Email is required." });
            Email.RegisterValidationRule(new EmailRule() { ValidationMessage = "Provide The valid email address." });
            Password = new ValidatableObject<string>(true) { Value = "" };
            Password.RegisterValidationRule(new IsNotNullOrEmptyRule() { ValidationMessage = "Password is required." });
            Password.RegisterValidationRule(new PasswordRule(false, true, true, true, 8) { ValidationMessage = "Passwords must be 8 more characters in length. Must contain at least 1 upper, 1 numeric and 1 special character." });
            ConfirmPassword = new ValidatableObject<string>(true) { Value = "" };
            ConfirmPassword.RegisterValidationRule(new IsNotNullOrEmptyRule() { ValidationMessage = "Confirm password is required." });
            ConfirmPassword.RegisterValidationRule(new ConfirmPasswordRule(() => Password.Value) { ValidationMessage = "Password and confirm password are not same." });
        }

        public Action UserAuthenticated { get; set; }

        public Action<string> ActionPerformed { get; set; }

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
            try
            {
                if (await _providerLoginService.Login(provider))
                {
                    UserAuthenticated?.Invoke();
                    ActionPerformed("provider login successfull");
                }
                else
                {
                    ActionPerformed("provider login unsuccessfull");
                }
            }
            catch (Exception e)
            {
                ActionPerformed("provider login failed with exception " + e.Message);
            }
            finally
            {
                WorkInProgress = false;
            }
        }

        private async void LoginCustom()
        {
            WorkInProgress = true;
            try
            {
                if (!Email.Validate() | !Password.Validate())
                    return;

                if (await _customLoginService.Login(Email.Value, Password.Value))
                {
                    UserAuthenticated?.Invoke();
                    ActionPerformed("custom login successfull");
                }
                else
                {
                    ActionPerformed("custom login unsuccessfull");
                }
            }
            catch (Exception e)
            {
                ActionPerformed("custom login failed with exception " + e.Message);
            }
            finally
            {
                WorkInProgress = false;
            }
        }

        private async void Register()
        {
            WorkInProgress = true;
            try
            {
                if (!Email.Validate() | !Password.Validate() | !ConfirmPassword.Validate())
                    return;

                if (await _customLoginService.Register(Email.Value, Password.Value) == RegistrationResult.Registered)
                {
                    IsRegistration = false;
                    ActionPerformed("registration successfull");
                }
                else
                {
                    ActionPerformed("registration unsuccessfull");
                }
            }
            catch (Exception e)
            {
                ActionPerformed("registration failed with exception " + e.Message);
            }
            finally
            {
                WorkInProgress = false;
            }
        }

        public async void Authenticate()
        {
            WorkInProgress = true;
            if (!string.IsNullOrWhiteSpace(await _authenticationService.Authenticate()))
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
