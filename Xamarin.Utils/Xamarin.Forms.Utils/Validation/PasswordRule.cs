using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms.Utils.Validation.Core;

namespace Xamarin.Forms.Utils.Validation
{
    public class PasswordRule : IValidationRule<string>
    {
        private const string LOWER_REG = "(?=.*[a-z])";
        private const string UPPER_REG = "(?=.*[A-Z])";
        private const string NUMERIC_REG = @"(?=.*\d)";
        private const string SPECIAL_REG = "(?=.*[#$^+=!*()@%&])";

        private readonly Regex _regex;

        public PasswordRule(bool lower, bool upper, bool numeric, bool special, int length)
        {
            StringBuilder sb = new StringBuilder("^");
            if (lower) sb.Append(LOWER_REG);
            if (upper) sb.Append(UPPER_REG);
            if (numeric) sb.Append(NUMERIC_REG);
            if (special) sb.Append(SPECIAL_REG);
            sb.Append(".{").Append(length).Append(",}$");
            _regex = new Regex(sb.ToString());
        }

        public string ValidationMessage { get; set; }

        public bool Check(string value)
        {
            if (value == null)
            {
                return false;
            }
            Match match = _regex.Match(value);
            return match.Success;
        }
    }
}
