using Xamarin.Forms.Utils.Validation.Core;

namespace Xamarin.Forms.Utils.Validation
{
    public class IsNotNullOrEmptyRule : IValidationRule<string>
    {
        public string ValidationMessage { get; set; }

        public bool Check(string value) => !(value == null || string.IsNullOrWhiteSpace(value.ToString()));
    }
}
