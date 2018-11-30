using System;
using Xamarin.Forms.Utils.Validation.Core;

namespace Xamarin.Forms.Utils.Validation
{
    public class ConfirmPasswordRule : IValidationRule<string>
    {
        private readonly Func<string> _originalPassword;

        public ConfirmPasswordRule(Func<string> originalPassword)
        {
            _originalPassword = originalPassword;
        }

        public string ValidationMessage { get; set; }

        public bool Check(string value) => _originalPassword() == value;
    }
}
