using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendenceDataProcess.Data
{
    public class StatisticsFileModel
    {
        string studentID;
        string gradeName;
        string className;
        string studentName;
        string weekIndex;
        string courseIndex;
        string courseName;
        string status;

        public string StudentID { get => studentID; set => studentID = value; }
        public string GradeName { get => gradeName; set => gradeName = value; }
        public string ClassName { get => className; set => className = value; }
        public string StudentName { get => studentName; set => studentName = value; }
        public string WeekIndex { get => weekIndex; set => weekIndex = value; }
        public string CourseIndex { get => courseIndex; set => courseIndex = value; }
        public string CourseName { get => courseName; set => courseName = value; }
        public string Status { get => status; set => status = value; }
    }
}
