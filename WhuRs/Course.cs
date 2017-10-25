using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhuRs
{
	public enum AttendantStatus
	{
		Absent, Leave, Lated, Attendant
	}

	public class Course :INotifyPropertyChanged
	{
		
		string _courseID;
		string _courseName;
		string _courseTeacherName;
		DateTime _courseStartTime;
		DateTime _courseEndTime;
		int _courseStartWeek;
		int _courseEndWeek;
		int _courseDirection;
		int _courseStartClassIndex;
		int _courseEndClassIndex;
		DayOfWeek _courseWeekIndex;
		AttendantStatus _courseAttendantStatus;

		static TimeSpan LateThreshold = TimeSpan.FromMinutes(4);
		static TimeSpan AttendantThreshold = TimeSpan.FromMinutes(11);
		public static string[] WeekString = { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
		public static string[] ChnNum = { "全年级", "一方向", "二方向", "三方向", "地理国情监测" };
		static DateTime[] StartTimeIndex =
		{
			DateTime.Parse("00:00"),
			DateTime.Parse("08:00"),
			DateTime.Parse("08:50"),
			DateTime.Parse("09:50"),
			DateTime.Parse("10:40"),
			DateTime.Parse("11:30"),
			DateTime.Parse("14:05"),
			DateTime.Parse("14:55"),
			DateTime.Parse("15:45"),
			DateTime.Parse("16:40"),
			DateTime.Parse("17:30"),
			DateTime.Parse("18:30"),
			DateTime.Parse("19:25"),
			DateTime.Parse("20:10")
		};
		static DateTime[] EndTimeIndex =
		{
			DateTime.Parse("00:00"),
			DateTime.Parse("08:45"),
			DateTime.Parse("09:35"),
			DateTime.Parse("10:35"),
			DateTime.Parse("11:25"),
			DateTime.Parse("12:15"),
			DateTime.Parse("14:50"),
			DateTime.Parse("15:40"),
			DateTime.Parse("16:30"),
			DateTime.Parse("17:25"),
			DateTime.Parse("18:15"),
			DateTime.Parse("19:15"),
			DateTime.Parse("20:05"),
			DateTime.Parse("20:55")
		};

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		public string CourseID
		{
			set { _courseID = value; OnPropertyChanged(new PropertyChangedEventArgs("CourseID")); }
			get { return _courseID; }
		}
		public string CourseName
		{
			set { _courseName = value; OnPropertyChanged(new PropertyChangedEventArgs("CourseName")); }
			get { return _courseName; }
		}
		public string TeacherName
		{
			set { _courseTeacherName = value; OnPropertyChanged(new PropertyChangedEventArgs("TeacherName")); }
			get { return _courseTeacherName; }
		}
		public DateTime StartTime
		{
			set { _courseStartTime = value; OnPropertyChanged(new PropertyChangedEventArgs("StartTime")); }
			get { return _courseStartTime; }
		}
		public DateTime EndTime
		{
			set { _courseEndTime = value; OnPropertyChanged(new PropertyChangedEventArgs("EndTime")); }
			get { return _courseEndTime; }
		}
		public int StartWeek
		{
			set { _courseStartWeek = value; OnPropertyChanged(new PropertyChangedEventArgs("StartWeek")); }
			get { return _courseStartWeek; }
		}
		public int EndWeek
		{
			set { _courseEndWeek = value; OnPropertyChanged(new PropertyChangedEventArgs("EndWeek")); }
			get { return _courseEndWeek; }
		}
		public int StartClassIndex
		{
			set
			{
				_courseStartClassIndex = value;
				_courseStartTime = StartTimeIndex[_courseStartClassIndex];
				OnPropertyChanged(new PropertyChangedEventArgs("StartClassIndex"));
			}
			get { return _courseStartClassIndex; }
		}
		public int EndClassIndex
		{
			set
			{
				_courseEndClassIndex = value;
				_courseEndTime = EndTimeIndex[_courseEndClassIndex];
				OnPropertyChanged(new PropertyChangedEventArgs("EndClassIndex"));
			}
			get { return _courseEndClassIndex; }
		}
		public string ClassIndex
		{
			get { return string.Format("第{0}~{1}节", _courseStartClassIndex, _courseEndClassIndex); }
		}
		public DayOfWeek WeekIndex
		{
			set { _courseWeekIndex = value; OnPropertyChanged(new PropertyChangedEventArgs("WeekIndex")); }
			get { return _courseWeekIndex; }
		}
		public AttendantStatus AbsenceStatus
		{
			set { _courseAttendantStatus = value; OnPropertyChanged(new PropertyChangedEventArgs("AbsenceStatus")); }
			get { return _courseAttendantStatus; }
		}
		public string CourseDirection
		{
			set
			{
				_courseDirection = 0;
				for (int i = 0; i < 5; i++)
				{
					if (ChnNum[i] == value) _courseDirection = i;
				}
				OnPropertyChanged(new PropertyChangedEventArgs("CourseID"));
			}
			get { return ChnNum[_courseDirection]; }
		}

		public string ToWeekString()
		{
			return WeekString[(int)WeekIndex];
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2}~{3}节,{4}", CourseName, WeekString[(int)_courseWeekIndex], _courseStartClassIndex, _courseEndClassIndex, CourseDirection);
		}

		public Course(string id, string name, string teacher, int direction, DayOfWeek week, int startindex, int endindex, int startweek, int endweek)
		{
			_courseID = id;
			_courseName = name;
			_courseTeacherName = teacher;
			_courseWeekIndex = week;
			_courseStartClassIndex = startindex;
			_courseEndClassIndex = endindex;
			_courseStartTime = StartTimeIndex[startindex];
			_courseEndTime = EndTimeIndex[endindex];
			_courseStartWeek = startweek;
			_courseEndWeek = endweek;
			_courseDirection = direction;
		}

		public Course(string id, string name, string teacher, string direction, DayOfWeek week, int startindex, int endindex, int startweek, int endweek)
		{
			_courseID = id;
			_courseName = name;
			_courseTeacherName = teacher;
			_courseWeekIndex = week;
			_courseStartClassIndex = startindex;
			_courseEndClassIndex = endindex;
			_courseStartTime = StartTimeIndex[startindex];
			_courseEndTime = EndTimeIndex[endindex];
			_courseStartWeek = startweek;
			_courseEndWeek = endweek;
			for (int i = 0; i < 5; i++)
			{
				if (ChnNum[i] == direction) _courseDirection = i;
			}
		}

		public Course(Course course)
		{
			_courseID = course.CourseID;
			_courseName = course.CourseName;
			_courseTeacherName = course.TeacherName;
			_courseWeekIndex = course.WeekIndex;
			_courseStartClassIndex = course.StartClassIndex;
			_courseEndClassIndex = course.EndClassIndex;
			_courseStartTime = StartTimeIndex[_courseStartClassIndex];
			_courseEndTime = EndTimeIndex[_courseEndClassIndex];
			_courseStartWeek = course.StartWeek;
			_courseEndWeek = course.EndWeek;
			for (int i = 0; i < ChnNum.Count(); i++)
			{
				if (ChnNum[i] == course.CourseDirection) _courseDirection = i;
			}
		}

		public static void SetThreshold(int attendant_threshold, int lated_threshold)
		{
			AttendantThreshold = TimeSpan.FromMinutes(attendant_threshold);
			LateThreshold = TimeSpan.FromMinutes(lated_threshold);
		}

		public AttendantStatus CheckAttentStatus(DateTime time)
		{
			if ((time.TimeOfDay >= (_courseStartTime.TimeOfDay - AttendantThreshold)) && (time.TimeOfDay <= (_courseStartTime.TimeOfDay + LateThreshold)) )
			{
				_courseAttendantStatus = AttendantStatus.Attendant;
			}
			else if ((time.TimeOfDay > (_courseStartTime.TimeOfDay + LateThreshold)) && (time.TimeOfDay <= _courseEndTime.TimeOfDay))
			{
				_courseAttendantStatus = AttendantStatus.Lated;
			}
			else
			{
				//_courseAttendantStatus = AttendantStatus.Absent;
			}
			return _courseAttendantStatus;
		}

		
	}
}
