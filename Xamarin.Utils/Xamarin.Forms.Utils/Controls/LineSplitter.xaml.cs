using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Utils.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LineSplitter : ContentView
	{
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(
            nameof(Color), typeof(Color), typeof(LineSplitter), Color.Black);
        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text), typeof(string), typeof(LineSplitter), "Text");

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public LineSplitter ()
		{
			InitializeComponent ();
            BindingContext = this;
		}
	}
}