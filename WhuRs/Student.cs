using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

using AttendantData;

namespace WhuRs
{

	[Serializable]
	public class ItemExistException : SystemException
	{
		public ItemExistException() { }
		public ItemExistException(string message) : base(message) { }
		public ItemExistException(string message, Exception inner) : base(message, inner) { }
		protected ItemExistException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
	}

    public class Student : INotifyPropertyChanged
    {
		//成员变量
		string _studentID;
		string _studentName;
		string _studentClassID;
		int _studentAbsentNum;
		int _studentLeaveNum;
		int _studentAttendentNum;
		int _studentLatedNum;
		ObservableCollection<Course> _studentCourseList;

		//事件
		public event PropertyChangedEventHandler PropertyChanged;

		//属性
		public string StudentID
		{
			set
			{
				if (_studentID != value)
				{
					_studentID = value;
					OnPropertyChanged(new PropertyChangedEventArgs("StudentID"));
				}
			}
			get { return _studentID; }
		}
		public string StudentName
		{
			set
			{
				if (_studentName != value)
				{
					_studentName = value;
					OnPropertyChanged(new PropertyChangedEventArgs("StudentName"));
				}
			}
			get { return _studentName; }
		}
		public string ClassID
		{
			set
			{
				if (_studentClassID != value)
				{
					_studentClassID = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ClassID"));
				}
			}
			get { return _studentClassID; }
		}
		public ObservableCollection<Course> CourseList
		{
			set
			{
				_studentCourseList = value;
				OnPropertyChanged(new PropertyChangedEventArgs("CourseList"));
			}
			get { return _studentCourseList; }
		}
		public int LeaveNum
		{
			private set
			{
				_studentAbsentNum = value;
			}
			get { return _studentLeaveNum; }
		}
		public int AbsentNum
		{
			private set
			{
				_studentLeaveNum = value;
			}
			get { return _studentAbsentNum; }
		}
		public int AttendentNum
		{
			private set
			{
				_studentAttendentNum = value;
			}
			get { return _studentAttendentNum; }
		}
		public int LatedNum
		{
			private set
			{
				_studentLatedNum = value;
			}
			get { return _studentLatedNum; }
		}

		//方法
		public Student(string id, string name, string classid)
		{
			_studentID = id;
			_studentName = name;
			_studentClassID = classid;
			_studentCourseList = new ObservableCollection<Course>();
		}

		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		public void AddClass(Course item)
		{
			foreach (Course exist in _studentCourseList)
			{
				if ((item.CourseID == exist.CourseID) && (item.WeekIndex == exist.WeekIndex)
					&& (item.StartClassIndex == exist.StartClassIndex) && (item.EndClassIndex == exist.EndClassIndex))
				{
					throw new ItemExistException("课程已存在！");
				}
			}
			_studentCourseList.Add(item);
		}

		public void DeleteClass(int nIndex)
		{
			try
			{
				_studentCourseList.RemoveAt(nIndex);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				throw ex;
			}
		}

		public void DeleteClass(Course item)
		{
			_studentCourseList.Remove(item);
		}

		public void ChangeClass(Course oldItem, Course newItem)
		{
			int nIndex = _studentCourseList.IndexOf(oldItem);
			_studentCourseList[nIndex] = newItem;
		}

		public void SetClassAttendStatus (DateTime time)
		{
			//int attendentNum = 0;
			//int absentNum = 0;
			//int latedNum = 0;

			DayOfWeek weekindex = time.DayOfWeek;
			foreach (Course item in _studentCourseList)
			{
				if (item.WeekIndex == weekindex)
				{
					//switch (item.CheckAttentStatus(time))
					//{
					//	case AttendantStatus.Lated:
					//		LatedNum++;
					//		break;
					//	case AttendantStatus.Attendant:
					//		AttendentNum++;
					//		break;
					//	default:
					//		break;
					//}
					item.CheckAttentStatus(time);
				}
			}

			AbsentNum = _studentCourseList.Count - AttendentNum;

			//AttendentNum = attendentNum;
			//AbsentNum = absentNum;
			//LatedNum = latedNum;
		}

		public void RefreshAttendData()
		{
			int nAttendentNum = _studentCourseList.Where(x=>x.AbsenceStatus == AttendantStatus.Attendant).Count();
			int nAbsentNum = _studentCourseList.Where(x => x.AbsenceStatus == AttendantStatus.Absent).Count();
			int nLeaveNum = _studentCourseList.Where(x => x.AbsenceStatus == AttendantStatus.Leave).Count();
			int nLatedNum = _studentCourseList.Where(x => x.AbsenceStatus == AttendantStatus.Lated).Count();

			//foreach (Course item in _studentCourseList)
			//{
			//	switch (item.AbsenceStatus)
			//	{
			//		case AttendantStatus.Absent:
			//			nAbsentNum++;
			//			break;
			//		case AttendantStatus.Leave:
			//			nLeaveNum++;
			//			break;
			//		case AttendantStatus.Lated:
			//			nLatedNum++;
			//			break;
			//		case AttendantStatus.Attendant:
			//			nAttendentNum++;
			//			break;
			//		default:
			//			break;
			//	}
			//}

			_studentAttendentNum = nAttendentNum;
			_studentAbsentNum = nAbsentNum;
			_studentLatedNum = nLatedNum;
			_studentLeaveNum = nLeaveNum;
		}

		public ObservableCollection<Absent> GetAbsentList(DayOfWeek WeekIndexStart, DayOfWeek WeekIndexEnd)
		{
			ObservableCollection<Absent> _studentAbsentList = new ObservableCollection<Absent>();
			foreach (Course course in _studentCourseList)
			{
				if ((course.WeekIndex.CompareTo(WeekIndexStart) > 0) && (course.WeekIndex.CompareTo(WeekIndexEnd) < 0))
				{
					switch (course.AbsenceStatus)
					{
						case AttendantStatus.Absent:
						case AttendantStatus.Leave:
						case AttendantStatus.Lated:
							_studentAbsentList.Add(new Absent(this, course));
							break;
						case AttendantStatus.Attendant:
							break;
						default:
							break;
					}
				}
			}
			return _studentAbsentList;
		}
    }
}
