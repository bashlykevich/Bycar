using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Xml;

namespace bycar3.Helpers.ValueConversion
{
    public class WarehouseToColumnConverter : IValueConverter
    {

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "Ошибка!";
            int WarehouseIndex = (Int32)parameter;
            WarehouseIndex = WarehouseIndex < 0 ? 0 : WarehouseIndex;
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(value as string);
            string str = "";
            XmlNodeList xnl = xml.SelectNodes("/r/w");
            if (xnl.Count > 0)
                str = xnl[WarehouseIndex].Attributes["q"].Value;
            else
                str = "###";
            return str;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
