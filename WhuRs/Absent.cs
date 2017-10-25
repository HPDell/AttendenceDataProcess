using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhuRs;

namespace AttendantData
{
	public class Absent
	{
		Student _absentStudent;
		Course _absentCourse;

		static string[] AbsentString = { "旷课", "请假", "迟到", "出勤" };

		public Student Student
		{
			set { _absentStudent = value; }
			get { return _absentStudent; }
		}

		public Course Course
		{
			set { _absentCourse = value; }
			get { return _absentCourse; }
		}

		public string StudentID
		{
			get { return _absentStudent.StudentID; }
		}

		public string StudentName
		{
			get { return _absentStudent.StudentName; }
		}

		public string AbsenceWeekIndex
		{
			get { return _absentCourse.ToWeekString(); }
		}

		public string AbsenceClassIndex
		{
			get { return string.Format("第{0}~{1}节", _absentCourse.StartClassIndex, _absentCourse.EndClassIndex); }
		}

		public string AbsenceClassName
		{
			get { return _absentCourse.CourseName; }
		}

		public int AbsenceStatus
		{
			set { _absentCourse.AbsenceStatus = (AttendantStatus)value; }
			get { return (int)(_absentCourse.AbsenceStatus); }
		}
		public string AbsentStatusString
		{
			get { return AbsentString[(int)(_absentCourse.AbsenceStatus)]; }
		}

		public Absent(Student student, Course course)
		{
			_absentStudent = student;
			_absentCourse = course;
		}
	}
}
