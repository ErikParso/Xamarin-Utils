using Microsoft.WindowsAzure.MobileServices;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Forms.Utils.ViewModel
{
    internal class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authenticationService;
        private bool _workInProgress;
        private string _email;
        private string _password;

        public LoginViewModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            LoginCommand = new Command<MobileServiceAuthenticationProvider>(Login);
            LoginCustomCommand = new Command(LoginCustom);
        }

        public Action UserAuthenticated { get; set; }

        public ICommand LoginCommand { get; set; }

        public ICommand LoginCustomCommand { get; set; }

        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
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
            WorkInProgress = true;
            if (await _authenticationService.Login(Email, Password))
            {
                UserAuthenticated?.Invoke();
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
