using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhuRs;

namespace AttendantData
{
    public class Attendant
    {
		string _attendantStudentID;
		Course _attendantCourse;

		public string StudentID
		{
			set { _attendantStudentID = value; }
			get { return _attendantStudentID; }
		}

		public Course Course
		{
			set { _attendantCourse = value; }
			get { return Course; }
		}

		public Attendant(string id, Course course)
		{
			_attendantStudentID = id;
			_attendantCourse = course;
		}
    }
}
