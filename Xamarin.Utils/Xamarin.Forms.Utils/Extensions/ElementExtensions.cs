using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Utils.Extensions
{
    public static class ElementExtensions
    {
        public static Page GetPage(this Element element)
        {
            Element current = element;
            while (!(current is Page))
            {
                current = current.Parent;
            }
            return current as Page;
        }
    }
}
