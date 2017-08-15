using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace hcc
{
    class HccTag
    {
        public static readonly BindableProperty TagProperty = BindableProperty.Create("Tag", typeof(string), typeof(HccTag), null);

        public static string GetTag(BindableObject bindable)
        {
            return (string)bindable.GetValue(TagProperty);
        }

        public static void SetTag(BindableObject bindable, string value)
        {
            bindable.SetValue(TagProperty, value);
        }
    }
}
