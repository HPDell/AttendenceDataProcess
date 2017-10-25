using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AttendenceDataProcess
{
	/// <summary>
	/// SetSchoolStartDate.xaml 的交互逻辑
	/// </summary>
	public partial class SetSchoolStartDate : Window
	{
		DateTime _TermStartDate = new DateTime(2016,2,21);
		int _SchoolWeeks = 18;
		
		public DateTime TermStartDate
		{
			set { _TermStartDate = value; }
			get { return _TermStartDate; }
		}

		public int WeekNum
		{
			set { _SchoolWeeks = value; }
			get { return _SchoolWeeks; }
		}

		public SetSchoolStartDate()
		{
			InitializeComponent();
			TermStartDatePicker.SelectedDate = TermStartDatePicker.DisplayDate = _TermStartDate;
		}

		private void Button_OK_Click(object sender, RoutedEventArgs e)
		{
			_TermStartDate = TermStartDatePicker.SelectedDate.GetValueOrDefault();
			try
			{
				_SchoolWeeks = int.Parse(TermWeekNum.Text);
				DialogResult = true;
			}
			catch (FormatException)
			{
				MessageBox.Show("周数输入不正确！");
			}
		}
	}
}
