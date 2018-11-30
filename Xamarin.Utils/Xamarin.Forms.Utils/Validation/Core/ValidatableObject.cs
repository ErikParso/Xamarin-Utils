using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Utils.Binding;

namespace Xamarin.Forms.Utils.Validation.Core
{
    public class ValidatableObject<T> : ExtendedBindableObject
    {
        private T _value;
        private ValidationResult _validationResult;
        private List<string> _errors;

        private readonly bool _validateOnEdit;
        private readonly List<IValidationRule<T>> _validations;

        public ValidatableObject(bool validateOnEdit)
        {
            _validationResult = ValidationResult.NotValidated;
            _errors = new List<string>();
            _validations = new List<IValidationRule<T>>();
            _validateOnEdit = validateOnEdit;
        }

        public List<string> Errors
        {
            get => _errors;
            set
            {
                _errors = value;
                RaisePropertyChanged(() => Errors);
            }
        }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
                if (_validateOnEdit)
                {
                    Validate();
                }
            }
        }

        public ValidationResult ValidationResult
        {
            get => _validationResult;
            set
            {
                _validationResult = value;
                RaisePropertyChanged(() => ValidationResult);
            }
        }

        public void RegisterValidationRule(IValidationRule<T> validationRule)
        {
            _validations.Add(validationRule);
        }

        public bool Validate()
        {
            Errors.Clear();

            IEnumerable<string> errors = _validations.Where(v => !v.Check(Value))
                .Select(v => v.ValidationMessage);

            Errors = errors.ToList();
            ValidationResult = Errors.Any() ? ValidationResult.Invalid : ValidationResult.Valid;

            return ValidationResult == ValidationResult.Valid;
        }
    }
}
