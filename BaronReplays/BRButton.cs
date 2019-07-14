using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace BaronReplays
{
    public class BRButton : Button
    {
        public static readonly DependencyProperty IsSelectedProperty;
        static BRButton()
        {
            IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(Boolean), typeof(Control));
        }

        public Boolean IsSelected
        {
            set { SetValue(IsSelectedProperty, value); }
            get { return (Boolean)GetValue(IsSelectedProperty); }
        }

    }
}
