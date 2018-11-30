using System;
using System.Collections.Generic;
using Xamarin.Forms.Utils.Extensions;
using Xamarin.Forms.Utils.Validation.Core;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Utils.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ValidationEntry : ContentView
	{
        public static readonly BindableProperty ValidatableStringProperty = BindableProperty.Create(
            nameof(ValidatableString), typeof(ValidatableObject<string>), typeof(ValidationEntry), null);

        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
            nameof(Placeholder), typeof(string), typeof(ValidationEntry));

        public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
            nameof(IsPassword), typeof(bool), typeof(ValidationEntry));

        public ValidatableObject<string> ValidatableString
        {
            get { return (ValidatableObject<string>)GetValue(ValidatableStringProperty); }
            set { SetValue(ValidatableStringProperty, value); }
        }

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }

        public event EventHandler<IEnumerable<string>> DisplayErrors;

        public ValidationEntry()
		{
			InitializeComponent();
		}

        private void ErrorTapped(object sender, EventArgs e)
        {
            if (ValidatableString.ValidationResult == ValidationResult.Invalid)
            {
                if (DisplayErrors == null)
                    this.GetPage().DisplayAlert(null, string.Join(Environment.NewLine, ValidatableString.Errors), "Ok");
                else
                    DisplayErrors(this, ValidatableString.Errors.ToArray());
            }
        }
    }
}