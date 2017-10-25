using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AttendantData;

namespace WhuRs
{
	public class Class : INotifyPropertyChanged
	{
		string _className;
		int _classMemberNum;
		int _classAbsentNum;
		int _classLeaveNum;
		int _classLatedNum;
		int _classAttendantNum;
		double _classAttendentRate;
		ObservableCollection<Student> _classMemberList;
		ObservableCollection<Absent> _classAbsentList;

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		//属性
		public string ClassName
		{
			set
			{
				if (_className != value)
				{
					_className = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ClassName"));
				}
			}
			get { return _className; }
		}
		public int MemberNum
		{
			private set
			{
				if (_classMemberNum != value)
				{
					_classMemberNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("MemberNum"));
				}
			}
			get { return _classMemberNum; }
		}
		public int AbsentNum
		{
			private set
			{
				if (_classAbsentNum != value)
				{
					_classAbsentNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("AbsentNum"));
				}
			}
			get { RefreshAttendData(); return _classAbsentNum; }
		}
		public int LeaveNum
		{
			private set
			{
				if (_classLeaveNum != value)
				{
					_classLeaveNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("LeaveNum"));
				}
			}
			get {  return _classLeaveNum; }
		}
		public double AttendentRate
		{
			private set
			{
				if (_classAttendentRate != value)
				{
					_classAttendentRate = value;
					OnPropertyChanged(new PropertyChangedEventArgs("AttendentRate"));
				}
			}
			get {  return _classAttendentRate; }
		}
		public int AttendantNum
		{
			private set
			{
				if (_classAttendantNum != value)
				{
					_classAttendantNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("AttendantNum"));
				}
			}
			get {  return _classAttendantNum; }
		}
		public int LatedNum
		{
			private set
			{
				if (_classLatedNum != value)
				{
					_classLatedNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("LatedNum"));
				}
			}
			get {  return _classLatedNum; }
		}
		public ObservableCollection<Student> MemberList
		{
			get { return _classMemberList; }
		}
		public ObservableCollection<Absent> AbsentList
		{
			get { return _classAbsentList; }
		}

		//方法
		public Class(string name)
		{
			_className = name;
			_classMemberNum = 0;
			_classAbsentNum = 0;
			_classLeaveNum = 0;
			_classAttendentRate = 0;
			_classMemberList = new ObservableCollection<Student>();
			_classAbsentList = new ObservableCollection<Absent>();
		}

		public void AddMember(Student member)
		{
			foreach (Student item in _classMemberList)
			{
				if (item.StudentID == member.StudentID)
				{
					throw new ItemExistException(ClassName + "班已有该成员");
				}
			}
			_classMemberList.Add(member);
			_classMemberNum++;
		}

		public void DeleteMember(Student member)
		{
			_classMemberList.Remove(member);
			_classMemberNum--;
		}

		public void DeleteMember(int nIndex)
		{
			try
			{
				_classMemberList.RemoveAt(nIndex);
				_classMemberNum--;
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new ArgumentOutOfRangeException("索引超出范围！");
			}
		}

		public void ChangeMember(Student oldMember, Student newMember)
		{
			int nIndex = _classMemberList.IndexOf(oldMember);
			try
			{
				_classMemberList[nIndex] = newMember;
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new ArgumentOutOfRangeException("此人！");
			}
		}

		public void RefreshAttendData()
		{
			int nClassNum = _classMemberList.Sum(x => x.CourseList.Count);
			int nAttendentNum = _classMemberList.Sum(x => x.AttendentNum);
			int nAbsentNum = _classMemberList.Sum(x => x.AbsentNum);
			int nLeaveNum = _classMemberList.Sum(x => x.LeaveNum);
			int nLatedNum = _classMemberList.Sum(x => x.LatedNum);
			
			
			AbsentNum = nAbsentNum;
			LeaveNum = nLeaveNum;
			LatedNum = nLatedNum;
			AttendantNum = nAttendentNum;
			if (nClassNum != 0) AttendentRate = ((double)(nLeaveNum + nLatedNum + nAttendentNum) / (double)nClassNum);
			else AttendentRate = 0;
		}

		public void AddAbsentItem(Absent item)
		{
			foreach (Absent absent in _classAbsentList)
			{
				if ((absent.StudentID == item.StudentID) && (absent.AbsenceWeekIndex == item.AbsenceWeekIndex) && (absent.AbsenceClassIndex == item.AbsenceClassIndex))
					throw new ItemExistException("该记录已存在！");
			}
		}

		public void GetAbsentList(DayOfWeek WeekIndexStart, DayOfWeek WeekIndexEnd, int WeekIndex)
		{
			foreach (Student student in _classMemberList)
			{
				foreach (Course course in student.CourseList)
				{
					if ((course.WeekIndex.CompareTo(WeekIndexStart) >= 0) && (course.WeekIndex.CompareTo(WeekIndexEnd) <= 0) &&
						(WeekIndex >= course.StartWeek) && (WeekIndex <= course.EndWeek))
					{
						switch (course.AbsenceStatus)
						{
							case AttendantStatus.Absent:
							case AttendantStatus.Leave:
							case AttendantStatus.Lated:
								_classAbsentList.Add(new Absent(student, course));
								break;
							case AttendantStatus.Attendant:
								break;
							default:
								break;
						}
					}
					else
					{
						course.AbsenceStatus = AttendantStatus.Attendant;
					}
				}
			}
		}

		//重写
		public override string ToString()
		{
			return string.Format("{0}班", _className);
		}
	}
}
