using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Aesalon
{
    public static class SelectorHelper
    {
        public static readonly DependencyProperty SelectIndexInDesignModeProperty = DependencyProperty.RegisterAttached("SelectIndexInDesignMode", typeof(int?), typeof(SelectorHelper), new UIPropertyMetadata(null, SelectIndexInDesignModePropertyChanged));

        private static void SelectIndexInDesignModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Selector selector = sender as Selector;
            if (selector == null)
                return;

            int? index = (int?)e.NewValue;

            if (index.HasValue)
            {
                bool designMode = DesignerProperties.GetIsInDesignMode(selector);
                if (designMode)
                    selector.SelectedIndex = index.Value;
            }
        }

        public static int? GetSelectIndexInDesignMode(DependencyObject obj) => (int?)obj.GetValue(SelectIndexInDesignModeProperty);

        public static void SetSelectIndexInDesignMode(DependencyObject obj, int? value) => obj.SetValue(SelectIndexInDesignModeProperty, value);
    }
}
