using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ViewModels.Converter
{
    // get number sequence from index of element in a list
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            if (ItemsControl.ItemsControlFromItemContainer(item) is ListView listView)
            {
                int index = listView.ItemContainerGenerator.IndexFromContainer(item) + 1;

                return index.ToString();
            }
            return -1; //null error
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
