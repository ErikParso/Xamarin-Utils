using System.Text.RegularExpressions;
using Xamarin.Forms.Utils.Validation.Core;

namespace Xamarin.Forms.Utils.Validation
{
    public class EmailRule : IValidationRule<string>
    {
        public string ValidationMessage { get; set; }

        public bool Check(string value)
        {
            if (value == null)
            {
                return false;
            }

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(value);

            return match.Success;
        }
    }
}
