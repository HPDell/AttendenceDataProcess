using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhuRs
{
	public class Grade : INotifyPropertyChanged
	{
		string _gradeName;
		int _gradeClassNum;
		int _gradeAbsentNum;
		int _gradeLeaveNum;
		int _gradeLatedNum;
		int _gradeAttendantNum;
		double _gradeAttendentRate;
		ObservableCollection<Class> _gradeClassList;
		ObservableCollection<Course> _gradeCheckingInCourse;

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		public string GradeName
		{
			set
			{
				if(_gradeName != value)
				{
					_gradeName = value;
					OnPropertyChanged(new PropertyChangedEventArgs("GradeName"));
				}
			}
			get { return _gradeName; }
		}
		public int AbsentNum
		{
			private set
			{
				if (_gradeAbsentNum != value)
				{
					_gradeAbsentNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("AbsentNum"));
				}
			}
			get { RefreshAttendData(); return _gradeAbsentNum; }
		}
		public int LeaveNum
		{
			private set
			{
				if (_gradeLeaveNum != value)
				{
					_gradeLeaveNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("LeaveNum"));
				}
			}
			get { RefreshAttendData(); return _gradeLeaveNum; }
		}
		public double AttendentRate
		{
			private set
			{
				if (_gradeAttendentRate != value)
				{
					_gradeAttendentRate = value;
					OnPropertyChanged(new PropertyChangedEventArgs("AttendenceRate"));
				}
			}
			get { RefreshAttendData(); return _gradeAttendentRate; }
		}
		public int AttendantNum
		{
			private set
			{
				if (_gradeAttendantNum != value)
				{
					_gradeAttendantNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("AttendantNum"));
				}
			}
			get { RefreshAttendData(); return _gradeAttendantNum; }
		}
		public int LatedNum
		{
			private set
			{
				if (_gradeLatedNum != value)
				{
					_gradeLatedNum = value;
					OnPropertyChanged(new PropertyChangedEventArgs("LatedNum"));
				}
			}
			get { RefreshAttendData(); return _gradeLatedNum; }
		}
		public ObservableCollection<Class> ClassList
		{
			get { return _gradeClassList; }
		}
		public ObservableCollection<Course> CheckingInCourseList
		{
			get { return _gradeCheckingInCourse; }
		}

		public Grade(string name)
		{
			_gradeName = name;
			_gradeClassNum = 0;
			_gradeAbsentNum = 0;
			_gradeLeaveNum = 0;
			_gradeAttendentRate = 0;
			_gradeClassList = new ObservableCollection<Class>();
			_gradeCheckingInCourse = new ObservableCollection<Course>();
		}

		public void RefreshAttendData()
		{
			//int nClassNum = 0;
			//int nAttendentNum = 0;
			//int nAbsentNum = 0;
			//int nLeaveNum = 0;
			//int nLatedNum = 0;

			//foreach (Class item in _gradeClassList)
			//{
			//	item.RefreshAttendData();
			//	nAttendentNum += item.AttendantNum;
			//	nAbsentNum += item.AbsentNum;
			//	nLeaveNum += item.AbsentNum;
			//	nLatedNum += item.LatedNum;
			//}

			//_gradeAbsentNum = nAbsentNum;
			//_gradeLeaveNum = nLeaveNum;
			//_gradeLatedNum = nLatedNum;
			//_gradeAttendantNum = nAttendentNum;
			//if (nClassNum != 0) _gradeAttendentRate = (nLeaveNum + nLatedNum + nAttendentNum) / nClassNum;
			//else _gradeAttendentRate = 0;

			AttendentRate = _gradeClassList.Sum(x => x.AttendentRate) / _gradeClassList.Count;
		}

		public void AddClass(Class item)
		{
			//if (_gradeClassList.Count >= _gradeClassNum) throw new ArgumentOutOfRangeException("已无法添加更多班级！");
			foreach (Class existitem in _gradeClassList)
			{
				if (existitem.ClassName == item.ClassName) throw new ItemExistException("该班级已存在！");
			}
			_gradeClassList.Add(item);
			_gradeClassNum++;
		}

		public void DeleteClass(Class item)
		{
			_gradeClassList.Remove(item);
		}

		public void AddCheckingInCourse(Course course)
		{
			foreach (Course item in _gradeCheckingInCourse)
			{
				if ((item.CourseID == course.CourseID) && (item.WeekIndex == course.WeekIndex) 
					&& (item.StartClassIndex == course.StartClassIndex) && (item.EndClassIndex == course.EndClassIndex)
					&& (item.CourseDirection == course.CourseDirection))
					throw new ItemExistException("课程已存在！");
			}
			_gradeCheckingInCourse.Add(course);
		}

		public void DeleteCheckingInCourse(Course course)
		{
			_gradeCheckingInCourse.Remove(course);
		}

		public void DeleteCheckingInCourse(int nIndex)
		{
			try
			{
				_gradeCheckingInCourse.RemoveAt(nIndex);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new ArgumentOutOfRangeException("索引超出范围！");
			}
		}


		//重写
		public override string ToString()
		{
			return string.Format("{0}级", _gradeName);
		}
	}
}
