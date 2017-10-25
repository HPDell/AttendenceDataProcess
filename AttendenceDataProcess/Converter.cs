using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;

namespace AttendenceDataProcess
{
	class DayOfWeekToWeekString : IValueConverter
	{
		static string[] WeekString = { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			DayOfWeek weekindex = (DayOfWeek)value;
			return WeekString[(int)weekindex];
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string weekstring = (string)value;
			for (int i = 0; i < WeekString.Count(); i++)
			{
				if (weekstring == WeekString[i])
				{
					return (DayOfWeek)i;
				}
			}
			return null;
		}
	}

	class DayOfWeekToInt : IValueConverter
	{
		
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			DayOfWeek weekname = (DayOfWeek)value;
			return (int)weekname;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int weekindex = (int)value;
			return (DayOfWeek)weekindex;
		}
	}

	class DirectionStringToInt : IValueConverter
	{
		public static string[] ChnNum = { "全年级", "一方向", "二方向", "三方向", "地理国情监测" };

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string DirectionString = (string)value;
			for (int i = 0; i < 5; i++)
			{
				if (ChnNum[i] == DirectionString) return i;
			}
			return 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ChnNum[(int)value];
		}
	}

	class SelectionIndexToEnable : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((int)value > -1) return true;
			else return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	class FloatToPercentString : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Format("{0:F2}",(double)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
