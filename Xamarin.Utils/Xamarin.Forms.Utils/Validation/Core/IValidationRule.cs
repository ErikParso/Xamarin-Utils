namespace Xamarin.Forms.Utils.Validation.Core
{
    public interface IValidationRule<T>
    {
        string ValidationMessage { get; set; }

        bool Check(T value);
    }
}
