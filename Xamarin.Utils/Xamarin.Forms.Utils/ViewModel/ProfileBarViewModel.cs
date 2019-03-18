using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms.Utils.Services;

namespace Xamarin.Forms.Utils.ViewModel
{
    public class ProfileBarViewModel : INotifyPropertyChanged
    {
        private readonly IAccountInformationService _accountInformationService;

        private string _name;
        private string _photo;

        public ProfileBarViewModel(IAccountInformationService accountInformationService)
        {
            _accountInformationService = accountInformationService;
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public string Photo
        {
            get => _photo;
            set => SetField(ref _photo, value);
        }


        public async Task LoadAccountInformation()
        {
            var account = await _accountInformationService.GetCurrentAccountInformation();
            Name = account.Value<string>("name");
            Photo = account.Value<string>("photoUrl");
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
