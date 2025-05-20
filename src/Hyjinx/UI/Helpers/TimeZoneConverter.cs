using Avalonia.Data.Converters;
using System;
using System.Globalization;
using TimeZone = Hyjinx.Ava.UI.Models.TimeZone;

namespace Hyjinx.Ava.UI.Helpers
{
    internal class TimeZoneConverter : IValueConverter
    {
        public static TimeZoneConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var timeZone = (TimeZone)value;
            return string.Format("{0}  {1}   {2}", timeZone.UtcDifference, timeZone.Location, timeZone.Abbreviation);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}