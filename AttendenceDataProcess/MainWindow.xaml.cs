using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
//using Microsoft.Office.Core;
//using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;

using WhuRs;
using AttendantData;
using System.Threading;
using Microsoft.Win32;
using System.IO;

namespace AttendenceDataProcess
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
	{
		ObservableCollection<int> WeekIndexCollection = new ObservableCollection<int>();
		ObservableCollection<Grade> GradeList = new ObservableCollection<Grade>();
		ObservableCollection<Absent> AbsentList = new ObservableCollection<Absent>();
		ObservableCollection<Course> ClassCheckingInCourse = new ObservableCollection<Course>();

		int _SchoolTermCentury = 20;
		DateTime _SchoolTermStartDate = new DateTime();
		int _WeekCheckInNum = 18;
		DateTime _WeekDateStart = new DateTime();
		DateTime _WeekDateEnd = new DateTime();
		DateTime _CheckingDateStart = new DateTime();
		DateTime _CheckingDateEnd = new DateTime();

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (!(PropertyChanged == null))
			{
				PropertyChanged(this, e);
			}
		}

		//属性
		public DateTime CheckingInDateStart
		{
			set { _CheckingDateStart = value; OnPropertyChanged(new PropertyChangedEventArgs("CheckingInDateStart")); }
			get { return _CheckingDateStart; }
		}

		public DateTime CheckingInDateEnd
		{
			set { _CheckingDateEnd = value; OnPropertyChanged(new PropertyChangedEventArgs("CheckingInDateEnd")); }
			get { return _CheckingDateEnd; }
		}

		//方法
		public MainWindow()
		{
			InitializeComponent();

			InitConfig();
			InitUIControls();

			SchoolWeek.ItemsSource = WeekIndexCollection;
			for (int i = 0; i < _WeekCheckInNum; i++)
			{
				WeekIndexCollection.Add(i + 1);
			}
			Course.SetThreshold(11, 4);

			Main_Grade.ItemsSource = GradeList;
			Setting_ClassCheckingInCourse.DataContext = ClassCheckingInCourse;
			
		}

		//后台函数
		/// <summary>
		/// 读取本地年级、班级数据
		/// </summary>
		private void InitGrades()
		{
			XmlDocument xml = new XmlDocument();
			try
			{
				xml.Load("Member.xml");
			}
			catch (XmlException)
			{
				MessageBox.Show("无法打开文件 Member.xml，请检查文件是否存在！");
				return;
			}
			catch (NotSupportedException)
			{
				MessageBox.Show("Member.xml 的文件格式无效！");
				return;
			}

			XmlElement rootElem = xml.DocumentElement;

			XmlNodeList gradeNodes = rootElem.GetElementsByTagName("Grade");
			foreach (XmlElement grade in gradeNodes)
			{
				string gradename = grade.GetAttribute("GradeName");
				Grade newGrade = new Grade(gradename);

				foreach (XmlNode course in (grade["CheckingInCourseList"].GetElementsByTagName("Course")))
				{
					string CourseID = ((XmlElement)course).GetAttribute("CourseID");
					string CourseName = ((XmlElement)course).GetAttribute("CourseName");
					string TeacherName = ((XmlElement)course).GetAttribute("TeacherName");
					string CourseDirection = ((XmlElement)course).GetAttribute("CourseDirection");
					DayOfWeek WeekIndex = (DayOfWeek)(int.Parse(((XmlElement)course).GetAttribute("WeekIndex")));
					int StartWeek = int.Parse(((XmlElement)course).GetAttribute("StartWeek"));
					int EndWeek = int.Parse(((XmlElement)course).GetAttribute("EndWeek"));
					int StartClassIndex = int.Parse(((XmlElement)course).GetAttribute("StartClassIndex"));
					int EndClassIndex = int.Parse(((XmlElement)course).GetAttribute("EndClassIndex"));

					Course newCourse = new Course(CourseID, CourseName, TeacherName, CourseDirection, WeekIndex, StartClassIndex, EndClassIndex, StartWeek, EndWeek);
					try
					{
						newGrade.AddCheckingInCourse(newCourse);
					}
					catch (ItemExistException ex)
					{
						MessageBox.Show(ex.Message);
						return;
					}
				}


				try
				{
					foreach (XmlElement classitem in grade["ClassList"].GetElementsByTagName("Class"))
					{
						string ClassName = classitem.GetAttribute("ClassName");
						Class newClass = new Class(ClassName);

						foreach (XmlElement student in classitem["MemberList"].GetElementsByTagName("Student"))
						{
							string StudentID = student.GetAttribute("StudentID");
							string StudentName = student.GetAttribute("StudentName");
							string ClassID = student.GetAttribute("ClassID");

							Student newStudent = new Student(StudentID, StudentName, ClassID);

							foreach (XmlElement course in student["CourseList"].GetElementsByTagName("Course"))
							{
								string CourseID = course.GetAttribute("CourseID");

								foreach (Course item in newGrade.CheckingInCourseList)
								{
									try { if (item.CourseID == CourseID) newStudent.AddClass(new Course(item)); }
									catch (ItemExistException) { continue; }
								}
							}

							try
							{
								newClass.AddMember(newStudent);
							}
							catch (ItemExistException ex)
							{
								MessageBox.Show(ex.Message);
								return;
							}
						}

						try
						{
							newGrade.AddClass(newClass);
						}
						catch (ItemExistException ex)
						{
							MessageBox.Show(ex.Message);
							return;
						}
					}
				}
				catch (Exception)
				{
					
				}
				
				try
				{
					GradeList.Add(newGrade);
				}
				catch (ItemExistException ex)
				{
					MessageBox.Show(ex.Message);
					return;
				}
			}

		}

		private void InitConfig()
		{
			XmlDocument configxml = new XmlDocument();
			try
			{
				configxml.Load("config.xml");
				XmlElement configs = configxml.DocumentElement;
				_SchoolTermStartDate = DateTime.Parse(configs["TermStartDate"].FirstChild.Value);
				_SchoolTermCentury = int.Parse(configs["SchoolCentury"].FirstChild.Value);
				_WeekCheckInNum = int.Parse(configs["WeekCheckInNum"].FirstChild.Value);
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("无法打开 Config.xml。将自动创建！");

				XmlTextWriter xml = new XmlTextWriter("config.xml", Encoding.UTF8);
				xml.WriteStartDocument();
				xml.WriteStartElement("Cofing");
				xml.WriteElementString("TermStartDate", "2016-2-21");
				xml.WriteElementString("SchoolCentury", "20");
				xml.WriteElementString("WeekCheckInNum", "18");
				xml.WriteEndElement();
				xml.Close();
			}
			catch (XmlException)
			{
				MessageBox.Show("Config.xml 的文件格式无效！");

				XmlTextWriter xml = new XmlTextWriter("config.xml", Encoding.UTF8);
				xml.WriteStartDocument();
				xml.WriteStartElement("Cofing");
				xml.WriteElementString("TermStartDate", "2016-2-21");
				xml.WriteElementString("SchoolCentury", "20");
				xml.WriteElementString("WeekCheckInNum", "18");
				xml.WriteEndElement();
				xml.Close();
			}
			
		}

		private void InitUIControls()
		{

			WeekStart.DataContext = this;
			WeekEnd.DataContext = this;

			/*WeekStart.SelectedDate = WeekStart.DisplayDate = */CheckingInDateStart = _WeekDateStart = _SchoolTermStartDate;
			/*WeekEnd.SelectedDate = WeekStart.DisplayDate = */CheckingInDateEnd = _WeekDateEnd = _SchoolTermStartDate;
		}

		//Main标签页 事件响应函数
		//private void comboGrade_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//	listboxClassList.ItemsSource = ((Grade)Main_Grade.SelectedItem).ClassList;

		//}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			foreach (TextBlock removeItems in e.RemovedItems)
			{
				if ((removeItems.Text == "旷课" || removeItems.Text == "请假"))
				{
					foreach (TextBlock addItems in e.AddedItems)
					{
						if (addItems.Text == "迟到")
						{
							MessageBox.Show("无法将状态更改为“迟到”！");
							((ComboBox)sender).SelectedItem = removeItems;
						}
					}
				}
			}
			Main_Refresh_Click(Main_Refresh, new RoutedEventArgs());
		}


		private void WeekStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			
		}

		private void WeekEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{

		}


		private void Main_SetTermStartDate_Click(object sender, RoutedEventArgs e)
		{
			SetSchoolStartDate dialog = new SetSchoolStartDate { TermStartDate = _SchoolTermStartDate };
			if (dialog.ShowDialog() == true)
			{
				_SchoolTermStartDate = dialog.TermStartDate;
				_WeekCheckInNum = dialog.WeekNum;

				CheckingInDateStart = CheckingInDateEnd = _SchoolTermStartDate;

				WeekIndexCollection.Clear();
				for (int i = 0; i < _WeekCheckInNum; i++)
				{
					WeekIndexCollection.Add(i + 1);
				}

				XmlDocument configxml = new XmlDocument();
				configxml.Load("config.xml");

				XmlElement configs = configxml.DocumentElement;
				//configs["TermStartDate"].Value = _SchoolTermStartDate.ToShortDateString();
				//configs["SchoolCentury"].Value = (_SchoolTermStartDate.Year / 100).ToString();
				//configs["WeekCheckInNum"].Value = 18.ToString();

				configs.GetElementsByTagName("TermStartDate")[0].InnerText = _SchoolTermStartDate.ToShortDateString();
				configs.GetElementsByTagName("SchoolCentury")[0].InnerText = (_SchoolTermStartDate.Year / 100).ToString();
				configs.GetElementsByTagName("WeekCheckInNum")[0].InnerText = _WeekCheckInNum.ToString();

				configxml.Save("config.xml");
				MessageBox.Show(this,"OK!", "设置开学日期");
			}
			else
			{
				MessageBox.Show(this,"Canceled!");
			}
		}

		private void SchoolWeek_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SchoolWeek.SelectedIndex <= -1)
			{
				return;
			}

			_WeekDateStart = _SchoolTermStartDate.AddDays(SchoolWeek.SelectedIndex * 7);
			_WeekDateEnd = _WeekDateStart.AddDays(7);

			/*WeekStart.SelectedDate = */CheckingInDateStart = _WeekDateStart.AddDays(1);
			/*WeekEnd.SelectedDate = */CheckingInDateEnd = _WeekDateStart.AddDays(5);
		}

		private void Main_OpenAttendantData_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Multiselect = true;
			ofd.Filter = "考勤数据(*.dat)|*.dat";
			if (ofd.ShowDialog() != true)
			{
				MessageBox.Show(this, "未打开考勤数据！", "考勤数据", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			List<object[]> FileContent = new List<object[]>();
			foreach (string FileName in ofd.FileNames)
			{
				FileStream ifs = new FileStream(FileName, FileMode.Open);
				StreamReader sr = new StreamReader(ifs);

				while (!sr.EndOfStream)
				{
					string[] content = sr.ReadLine().Split('\t');
					string StudentID = _SchoolTermCentury.ToString() + content[2].Substring(0,2) + content[0];
					DateTime AttendantTime = DateTime.Parse(content[1]);
					string ClassName = content[2].Substring(0, 2) + string.Format("{0,2:D2}", int.Parse(content[2].Substring(2)));

					if ((AttendantTime.Date >= _CheckingDateStart) && (AttendantTime.Date <= _CheckingDateEnd))
					{
						FileContent.Add(new object[] { StudentID, AttendantTime, ClassName });
					}
				}
			}

			List<Student> AllStudent = new List<Student>();
			foreach (Grade gGrade in GradeList)
			{
				foreach (Class cClass in gGrade.ClassList)
				{
					AllStudent = AllStudent.Concat(cClass.MemberList).ToList();
				}
			}

			foreach (object[] item in FileContent)
			{
				var targetList = AllStudent.Where(x => x.StudentID == ((string)item[0])).Select(x => x).ToList();
				Student target = null;
				try
				{
					target = targetList[0];
				}
				catch (ArgumentOutOfRangeException)
				{
					continue;
				}

				for (int g = 0; g < GradeList.Count; g++)
					if (GradeList[g].GradeName == (target.StudentID).Substring(0,4))
					{
						for (int c = 0; c < GradeList[g].ClassList.Count; c++)
							if (GradeList[g].ClassList[c].ClassName == target.ClassID)
							{
								int nIndex = GradeList[g].ClassList[c].MemberList.IndexOf(target);
								GradeList[g].ClassList[c].MemberList[nIndex].SetClassAttendStatus((DateTime)item[1]);
							}
					}
			}

			foreach (Grade gGrade in GradeList)
			{
				foreach (Class cClass in gGrade.ClassList)
				{
					cClass.GetAbsentList(_CheckingDateStart.DayOfWeek, _CheckingDateEnd.DayOfWeek, SchoolWeek.SelectedIndex + 1);
				}
			}

			foreach (Grade gGrade in GradeList)
			{
				foreach (Class cClass in gGrade.ClassList)
				{
					foreach (Student sStudent in cClass.MemberList)
					{
						sStudent.RefreshAttendData();
					}
					cClass.RefreshAttendData();
				}
				gGrade.RefreshAttendData();
			}

			Main_CreateExcel.IsEnabled = true;

			MessageBox.Show("完成！");
		}

		private void Main_Refresh_Click(object sender, RoutedEventArgs e)
		{
			foreach (Grade gGrade in GradeList)
			{
				foreach (Class cClass in gGrade.ClassList)
				{
					foreach (Student sStudent in cClass.MemberList)
					{
						sStudent.RefreshAttendData();
					}
					cClass.RefreshAttendData();
				}
				gGrade.RefreshAttendData();
			}
		}

		
		private void Main_CreateExcel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Excel.Application appx = new Excel.Application();
			}
			catch (Exception)
			{

				throw;
			}
			Excel.Application app = new Excel.Application();
			Excel.Workbooks workBooks = app.Workbooks;
			Excel.Workbook workBook = workBooks.Add(true);
			Excel.Worksheet workSheet = workBook.Sheets.Item["Sheet1"];

			app.Visible = true;

			workSheet.Name = string.Format("第{0}周", SchoolWeek.SelectedIndex + 1);
			Excel.Range TitleRange = workSheet.Range[workSheet.Cells[1][1], workSheet.Cells[9][1]];
			TitleRange.Merge();
			TitleRange.Value = string.Format("遥感信息工程学院第{0}周考勤统计", SchoolWeek.SelectedIndex + 1);
			TitleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

			workSheet.Cells[1][2] = "年级";
			workSheet.Cells[2][2] = "班级";
			workSheet.Cells[3][2] = "姓名";
			workSheet.Cells[4][2] = "缺课周次";
			workSheet.Cells[5][2] = "缺课时段";
			workSheet.Cells[6][2] = "缺课课程";
			workSheet.Cells[7][2] = "状态";
			workSheet.Cells[8][2] = "班级出勤率";
			workSheet.Cells[9][2] = "年级出勤率";

			TitleRange = workSheet.Range[workSheet.Cells[1][2], workSheet.Cells[9][2]];
			TitleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
			TitleRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
			
			((Excel.Range)workSheet.Columns["H:I", Type.Missing]).AutoFit();

			int GradeRowStart = 3, GradeRowEnd = 3, ClassRowStart = 3, ClassRowEnd = 3;
			foreach (Grade gGrade in GradeList)
			{
				GradeRowEnd = GradeRowStart;
				//workSheet.Cells[GradeRowStart][1] = gGrade.GradeName;
				foreach (Class cClass in gGrade.ClassList)
				{
					//workSheet.Cells[ClassRowStart][2] = cClass.ClassName;
					ClassRowEnd = ClassRowStart;
					//if (cClass.AbsentList.Count > 0 )
					//{
					if (cClass.AbsentList.Count > 0)
					{
						foreach (Absent absent in cClass.AbsentList)
						{
							workSheet.Cells[3][ClassRowEnd] = absent.StudentName;           //填写缺勤者 姓名
							workSheet.Cells[4][ClassRowEnd] = absent.AbsenceWeekIndex;      //填写缺勤者 缺勤周次
							workSheet.Cells[5][ClassRowEnd] = absent.AbsenceClassIndex;     //填写缺勤者 缺勤课次
							workSheet.Cells[6][ClassRowEnd] = absent.Course.CourseName;     //填写缺勤者 缺勤课程
							workSheet.Cells[7][ClassRowEnd] = absent.AbsentStatusString;    //填写缺勤者 缺勤状态，旷课、请假、迟到
							ClassRowEnd++;
						}
						ClassRowEnd--;
					}
					//填写班级号
					Excel.Range range = workSheet.Range[workSheet.Cells[2][ClassRowStart], workSheet.Cells[2][ClassRowEnd]];
					range.ClearContents();
					range.Merge();
					range.Value = cClass.ClassName;
					range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
					range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

					//填写班级出勤率
					range = workSheet.Range[workSheet.Cells[8][ClassRowStart], workSheet.Cells[8][ClassRowEnd]];
					range.ClearContents();
					range.Merge();
					range.Value = string.Format("{0:P2}", cClass.AttendentRate);
					range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
					range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
					//}
					//else
					//{
					//	workSheet.Cells[ClassRowStart][8] = string.Format("{0:P2}", cClass.AttendentRate);
					//}
					ClassRowStart = ClassRowEnd + 1;
				}
				GradeRowEnd = ClassRowEnd;
				
				//填写年级号
				Excel.Range GradeRange = workSheet.Range[workSheet.Cells[1][GradeRowStart], workSheet.Cells[1][GradeRowEnd]];
				GradeRange.ClearContents();
				GradeRange.Select();
				GradeRange.Merge();
				GradeRange.Value = gGrade.GradeName;
				GradeRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
				GradeRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
				//填写年级出勤率
				GradeRange = workSheet.Range[workSheet.Cells[9][GradeRowStart], workSheet.Cells[9][GradeRowEnd]];
				GradeRange.ClearContents();
				GradeRange.Merge();
				GradeRange.Value = string.Format("{0:P2}", gGrade.AttendentRate);
				//GradeRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
				//GradeRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

				GradeRowStart = GradeRowEnd + 1;

			}

			((Excel.Range)workSheet.Columns["E:F", Type.Missing]).AutoFit();
			//workBook.Close(null, null, null);
			//workBooks.Close();
			//app.Quit();
			System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
			app = null;
		}

		//Setting标签页 事件相应函数
		private void Setting_NewGrade_Click(object sender, RoutedEventArgs e)
		{
			//更改UI
			Setting_NewGradeInformation.Visibility = Visibility.Visible;
			Setting_GradeGroup.Visibility = Visibility.Hidden;

			Grade addGrade = new Grade("");
			GradeList.Add(addGrade);
			Setting_Grade.SelectedItem = addGrade;
		}

		private void Setting_NewGrade_Accept_Click(object sender, RoutedEventArgs e)
		{
			Setting_NewGradeInformation.Visibility = Visibility.Collapsed;
			Setting_GradeGroup.Visibility = Visibility.Visible;
		}

		private void Setting_NewGrade_Cancel_Click(object sender, RoutedEventArgs e)
		{
			Setting_NewGradeInformation.Visibility = Visibility.Collapsed;
			Setting_GradeGroup.Visibility = Visibility.Visible;

			GradeList.Remove((Grade)Setting_Grade.SelectedItem);
			Setting_Grade.SelectedIndex = -1;
		}

		private void Setting_Grade_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//Setting_CheckingInCourse.ItemsSource = ((Grade)((ComboBox)sender).SelectedItem).CheckingInCourseList;
			//Setting_ClassList.ItemsSource = ((Grade)((ComboBox)sender).SelectedItem).ClassList;
		}

		private void Setting_ClassList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (((System.Windows.Controls.ListBox)sender).SelectedIndex > -1)
			{
				Setting_MemberList.ItemsSource = ((Class)((System.Windows.Controls.ListBox)sender).SelectedItem).MemberList;
			}
			else Setting_MemberList.ItemsSource = null;
		}

		private void Setting_DeleteGrade_Click(object sender, RoutedEventArgs e)
		{
			GradeList.Remove(Setting_Grade.SelectedItem as Grade);
			Setting_Grade.SelectedIndex = -1;
		}

		private void Setting_AddCheckingInCourse_Click(object sender, RoutedEventArgs e)
		{
			//更改UI
			Setting_GradeGroup.Visibility = Visibility.Hidden;
			Setting_Class.Visibility = Visibility.Hidden;
			Setting_NewCheckingInCourse.Visibility = Visibility.Visible;
			Setting_NewCheckingIn_Cancel.Visibility = Visibility.Visible;

			//添加空课程
			Course coursenew = new Course("", "", "", 0, DayOfWeek.Sunday, 0, 0, 0, 0);
			((Grade)Setting_Grade.SelectedItem).AddCheckingInCourse(coursenew);
			Setting_CheckingInCourse.SelectedItem = coursenew;
		}

		private void Setting_DeleteCheckingInCourse_Click(object sender, RoutedEventArgs e)
		{
			((Grade)Setting_Grade.SelectedItem).DeleteCheckingInCourse((Course)Setting_CheckingInCourse.SelectedItem);
		}

		private void Setting_NewCheckingIn_Accept_Click(object sender, RoutedEventArgs e)
		{
			//更改UI
			Setting_GradeGroup.Visibility = Visibility.Visible;
			Setting_Class.Visibility = Visibility.Visible;
			Setting_NewCheckingInCourse.Visibility = Visibility.Collapsed;
		}

		private void Setting_EditCheckingInCourse_Click(object sender, RoutedEventArgs e)
		{
			//更改UI
			if (Setting_CheckingInCourse.SelectedIndex > -1)
			{
				Setting_GradeGroup.Visibility = Visibility.Collapsed;
				Setting_Class.Visibility = Visibility.Collapsed;
				Setting_NewCheckingInCourse.Visibility = Visibility.Visible;
				Setting_NewCheckingIn_Cancel.Visibility = Visibility.Collapsed;
			}
		}

		private void Setting_NewCheckingIn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			((Grade)Setting_Grade.SelectedItem).DeleteCheckingInCourse((Course)Setting_CheckingInCourse.SelectedItem);
			Setting_CheckingInCourse.SelectedIndex = -1;

			Setting_GradeGroup.Visibility = Visibility.Visible;
			Setting_Class.Visibility = Visibility.Visible;
			Setting_NewCheckingInCourse.Visibility = Visibility.Collapsed;
		}

		private void Setting_AddClass_Click(object sender, RoutedEventArgs e)
		{
			Class addclass = new Class("");
			((Grade)Setting_Grade.SelectedItem).AddClass(addclass);
			Setting_ClassList.SelectedItem = addclass;
		}

		private void Setting_DeleteClass_Click(object sender, RoutedEventArgs e)
		{
			((Grade)Setting_Grade.SelectedItem).DeleteClass((Class)Setting_ClassList.SelectedItem);
		}

		private void Setting_AddClassMember_Click(object sender, RoutedEventArgs e)
		{
			//更改UI
			Setting_ClassSettingGrid.Visibility = Visibility.Visible;
			Setting_ClassNewMemberGroup.Visibility = Visibility.Visible;

			Student student = new Student("", "", ((Class)Setting_ClassList.SelectedItem).ClassName);
			((Class)Setting_ClassList.SelectedItem).AddMember(student);
			Setting_MemberList.SelectedItem = student;
		}

		private void Setting_DeleteClassMember_Click(object sender, RoutedEventArgs e)
		{
			((Class)Setting_ClassList.SelectedItem).DeleteMember(((Student)Setting_MemberList.SelectedItem));
			}

		private void Setting_SetAllMember_Click(object sender, RoutedEventArgs e)
		{
			//更改UI
			Setting_ClassSettingGrid.Visibility = Visibility.Visible;
			Setting_ClassSchedule.Visibility = Visibility.Visible;
		}

		private void Setting_SaveClass_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "班级XML数据文件(*.xml)|*.xml";
			sfd.FileName = ((Class)Setting_ClassList.SelectedItem).ClassName + ".xml";
			if (sfd.ShowDialog() == false)
			{
				return;
			}

			XmlTextWriter writer = new XmlTextWriter(sfd.FileName, Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();

			Class item = Setting_ClassList.SelectedItem as Class;
			writer.WriteStartElement("Class");      //Start Class
			writer.WriteAttributeString("ClassName", item.ClassName);
			writer.WriteAttributeString("MemberNum", item.MemberNum.ToString());

			writer.WriteStartElement("MemberList");       //Start MemberList
			foreach (Student student in item.MemberList)
			{
				writer.WriteStartElement("Student");
				writer.WriteAttributeString("StudentID", student.StudentID);
				writer.WriteAttributeString("StudentName", student.StudentName);
				writer.WriteAttributeString("ClassID", student.ClassID);

				writer.WriteStartElement("CourseList");
				foreach (Course course in student.CourseList)
				{
					writer.WriteStartElement("Course");     //Start Course
					writer.WriteAttributeString("CourseID", course.CourseID);
					writer.WriteEndElement();               //Close Course
				}
				writer.WriteEndElement();

				writer.WriteEndElement();

			}
			writer.WriteEndElement();                   //Start MemberList

			writer.Close();
			
		}

		private void Setting_SaveAllClass_Click(object sender, RoutedEventArgs e)
		{
			XmlTextWriter writer = new XmlTextWriter("Member.xml", Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();

			writer.WriteStartElement("Whurs");
			foreach (Grade grade in GradeList)
			{
				writer.WriteStartElement("Grade");
				writer.WriteAttributeString("GradeName", grade.GradeName);

				writer.WriteStartElement("CheckingInCourseList");
				foreach (Course course in grade.CheckingInCourseList)
				{
					writer.WriteStartElement("Course");		//Start Course
					writer.WriteAttributeString("CourseID", course.CourseID);
					writer.WriteAttributeString("CourseName", course.CourseName);
					writer.WriteAttributeString("TeacherName", course.TeacherName);
					writer.WriteAttributeString("CourseDirection", course.CourseDirection);
					writer.WriteAttributeString("WeekIndex", ((int)course.WeekIndex).ToString());
					writer.WriteAttributeString("StartWeek", course.StartWeek.ToString());
					writer.WriteAttributeString("EndWeek", course.EndWeek.ToString());
					writer.WriteAttributeString("StartClassIndex", course.StartClassIndex.ToString());
					writer.WriteAttributeString("EndClassIndex", course.EndClassIndex.ToString());
					writer.WriteEndElement();				//Close Course
				}
				writer.WriteEndElement();		//Close CheckingInCourse

				writer.WriteStartElement("ClassList");
				foreach (Class item in grade.ClassList)
				{
					writer.WriteStartElement("Class");		//Start Class
					writer.WriteAttributeString("ClassName", item.ClassName);
					writer.WriteAttributeString("MemberNum", item.MemberNum.ToString());

					writer.WriteStartElement("MemberList");		//Start MemberList
					foreach (Student student in item.MemberList)
					{
						writer.WriteStartElement("Student");
						writer.WriteAttributeString("StudentID", student.StudentID);
						writer.WriteAttributeString("StudentName", student.StudentName);
						writer.WriteAttributeString("ClassID", student.ClassID);

						writer.WriteStartElement("CourseList");
						foreach (Course course in student.CourseList)
						{
							writer.WriteStartElement("Course");     //Start Course
							writer.WriteAttributeString("CourseID", course.CourseID);
							writer.WriteEndElement();               //Close Course
						}
						writer.WriteEndElement();

						writer.WriteEndElement();

					}
					writer.WriteEndElement();					//Start MemberList

					writer.WriteEndElement();				//Close Class
				}
				writer.WriteEndElement();		//Close ClassList
				writer.WriteEndElement();		//Close Grade
			}

			writer.Close();
		}

		private void Setting_NewMember_AddAnother_Click(object sender, RoutedEventArgs e)
		{
			Student student = new Student("", "", ((Class)Setting_ClassList.SelectedItem).ClassName);
			((Class)Setting_ClassList.SelectedItem).AddMember(student);
			Setting_MemberList.SelectedItem = student;
		}

		private void Setting_NewMember_Accept_Click(object sender, RoutedEventArgs e)
		{
			//更改UI
			Setting_ClassSettingGrid.Visibility = Visibility.Collapsed;
			Setting_ClassNewMemberGroup.Visibility = Visibility.Collapsed;
		}

		private void Setting_NewMember_Cancel_Click(object sender, RoutedEventArgs e)
		{
			//更改UI
			Setting_ClassSettingGrid.Visibility = Visibility.Collapsed;
			Setting_ClassNewMemberGroup.Visibility = Visibility.Collapsed;

			((Class)Setting_ClassList.SelectedItem).DeleteMember(((Student)Setting_MemberList.SelectedItem));
		}
		
		private void Setting_ClassSchedule_AddCourse_Click(object sender, RoutedEventArgs e)
		{
			Course Selected = Setting_ClassSchedule_CourseForChoose.SelectedItem as Course;
			foreach (Course item in ClassCheckingInCourse)
			{
				if ((item.CourseID == Selected.CourseID) && (item.WeekIndex == Selected.WeekIndex)
					&& (item.StartClassIndex == Selected.StartClassIndex) && (item.EndClassIndex == Selected.EndClassIndex))
				{
					MessageBox.Show("课程已存在！");
					return;
				}
			}
			ClassCheckingInCourse.Add((Course)Setting_ClassSchedule_CourseForChoose.SelectedItem);
		}

		private void Setting_ClassSchedule_DeleteCourse_Click(object sender, RoutedEventArgs e)
		{
			ClassCheckingInCourse.Remove((Course)Setting_ClassCheckingInCourse.SelectedItem);
			Setting_ClassCheckingInCourse.SelectedIndex = -1;
		}

		private void Setting_ClassSchedule_SetAll_Click(object sender, RoutedEventArgs e)
		{
			foreach (Student student in ((Class)Setting_ClassList.SelectedItem).MemberList)
			{
				student.CourseList.Clear();
				foreach (Course course in ClassCheckingInCourse)
				{
					student.CourseList.Add(course);
				}
			}
			Setting_ClassSchedule.Visibility = Visibility.Collapsed;
		}

		private void Setting_ClassSchedule_Cancel_Click(object sender, RoutedEventArgs e)
		{
			ClassCheckingInCourse.Clear();
			Setting_ClassSchedule.Visibility = Visibility.Collapsed;
		}

		private void Setting_MemberList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			for (int i = 40; i < Setting_MemberSchedule.Children.Count;)
			{
				Setting_MemberSchedule.Children.RemoveAt(i);
			}

			foreach (Student student in e.AddedItems)
			{
				foreach (Course item in student.CourseList)
				{
					Border border = new Border();
					border.Background = new SolidColorBrush(Colors.AliceBlue);

					StackPanel sp = new StackPanel();
					sp.HorizontalAlignment = HorizontalAlignment.Center;
					sp.VerticalAlignment = VerticalAlignment.Center;
					sp.Children.Add(new TextBlock { Text = item.CourseName, TextWrapping = TextWrapping.Wrap, FontSize = 12, TextAlignment = TextAlignment.Center });
					sp.Margin = new Thickness(3);
					border.Child = sp;

					Setting_MemberSchedule.Children.Add(border);
					Grid.SetRow(border, item.StartClassIndex * 2 + 1);
					Grid.SetRowSpan(border, (item.EndClassIndex - item.StartClassIndex + 1) * 2 - 1);
					Grid.SetColumn(border, ((int)item.WeekIndex) * 2 + 1);
				}
			}
		}

		private void Setting_SetClassMemberSchedule_Click(object sender, RoutedEventArgs e)
		{
			Setting_ClassSettingGrid.Visibility = Visibility.Visible;
			Setting_ClassMemberSchedule.Visibility = Visibility.Visible;
		}

		private void Setting_MemberScheduleCheckingInCours_Accept_Click(object sender, RoutedEventArgs e)
		{
			Setting_ClassSettingGrid.Visibility = Visibility.Collapsed;
			Setting_ClassMemberSchedule.Visibility = Visibility.Collapsed;
		}

		private void Setting_MemberSchedule_EditCourse_Click(object sender, RoutedEventArgs e)
		{
			int nIndex = Setting_MemberScheduleCheckingInCourse_CourseList.SelectedIndex;
			Course item = Setting_MemberChechingInCourse_ChooseList.SelectedItem as Course;
			foreach (Course course in Setting_MemberScheduleCheckingInCourse_CourseList.Items)
			{
				if ((item.CourseID == course.CourseID) && (item.WeekIndex == course.WeekIndex)
					&& (item.StartClassIndex == course.StartClassIndex) && (item.EndClassIndex == course.EndClassIndex))
				{
					MessageBox.Show("课程已存在！");
					return;
				}
			}
			((Student)Setting_MemberList.SelectedItem).CourseList[nIndex] = (Course)Setting_MemberChechingInCourse_ChooseList.SelectedItem;
		}

		private void Setting_MemberSchedule_DeleteCourse_Click(object sender, RoutedEventArgs e)
		{
			((Student)Setting_MemberList.SelectedItem).DeleteClass((Course)Setting_MemberScheduleCheckingInCourse_CourseList.SelectedItem);
			Setting_MemberScheduleCheckingInCourse_CourseList.SelectedIndex = -1;
		}

		private void Setting_AddClassFromFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "班级XMl文档(*.xml)|*.xml|所有文件(*.*)|*.*";

			if (ofd.ShowDialog() != true)
			{
				MessageBox.Show("打开文件失败！");
				return;
			}

			XmlDocument xml = new XmlDocument();
			xml.Load(ofd.FileName);

			XmlElement newClassNode = xml.DocumentElement;
			string ClassName = newClassNode.GetAttribute("ClassName");
			Class newClass = new Class(ClassName);

			foreach (XmlElement student in newClassNode["MemberList"].GetElementsByTagName("Student"))
			{
				string StudentID = student.GetAttribute("StudentID");
				string StudentName = student.GetAttribute("StudentName");
				string ClassID = student.GetAttribute("ClassID");

				Student newStudent = new Student(StudentID, StudentName, ClassID);

				foreach (XmlElement course in student["CourseList"].GetElementsByTagName("Course"))
				{
					string CourseID = course.GetAttribute("CourseID");

					foreach (Course item in ((Grade)Setting_Grade.SelectedItem).CheckingInCourseList)
					{
						if (item.CourseID == CourseID) newStudent.AddClass(new Course(item));
					}
				}

				try
				{
					newClass.AddMember(newStudent);
				}
				catch (ItemExistException ex)
				{
					MessageBox.Show(ex.Message);
					return;
				}
			}

			try
			{
				((Grade)Setting_Grade.SelectedItem).AddClass(newClass);
			}
			catch (ItemExistException ex)
			{
				for (int i = 0; i < ((Grade)Setting_Grade.SelectedItem).ClassList.Count; i++)
				{
					if (((Grade)Setting_Grade.SelectedItem).ClassList[i].ClassName == newClass.ClassName)
					{
						switch (MessageBox.Show(this,"班级已存在，是否替换？","导入班级", MessageBoxButton.YesNo, MessageBoxImage.Question))
						{
							case MessageBoxResult.Yes:
								((Grade)Setting_Grade.SelectedItem).ClassList[i] = newClass;
								break;
							default:
								break;
						}
					}
				}
			}
		}

		private void Setting_LoadCheckingInCourse_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "考勤课程XMl文档(*.xml)|*.xml|所有文件(*.*)|*.*";

			if (ofd.ShowDialog() != true)
			{
				MessageBox.Show("打开文件失败！");
				return;
			}

			((Grade)Setting_Grade.SelectedItem).CheckingInCourseList.Clear();

			XmlDocument xml = new XmlDocument();
			xml.Load(ofd.FileName);

			XmlElement rootElement = xml.DocumentElement;

			foreach (XmlElement course in rootElement.ChildNodes)
			{
				string CourseID = course.GetAttribute("CourseID");
				string CourseName = course.GetAttribute("CourseName");
				string TeacherName = course.GetAttribute("TeacherName");
				string CourseDirection = course.GetAttribute("CourseDirection");
				DayOfWeek WeekIndex = (DayOfWeek)(int.Parse(course.GetAttribute("WeekIndex")));
				int StartWeek = int.Parse(course.GetAttribute("StartWeek"));
				int EndWeek = int.Parse(course.GetAttribute("EndWeek"));
				int StartClassIndex = int.Parse(course.GetAttribute("StartClassIndex"));
				int EndClassIndex = int.Parse(course.GetAttribute("EndClassIndex"));

				Course newCourse = new Course(CourseID, CourseName, TeacherName, CourseDirection, WeekIndex, StartClassIndex, EndClassIndex, StartWeek, EndWeek);

				((Grade)Setting_Grade.SelectedItem).AddCheckingInCourse(newCourse);
			}
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			InitGrades();
		}

		private void Setting_NewCheckingInCourse_StartClassIndex_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			try
			{
				int number = int.Parse(((TextBox)sender).Text);
				if (number < 0 || number > 13)
				{
					throw new Exception();
				}
			}
			catch (Exception)
			{
				MessageBox.Show("请输入1~13范围内的整数");
				((TextBox)sender).Focus();
				e.Handled = true;
			}
		}

		private void Setting_NewCheckingInCourse_StartWeek_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			try
			{
				int number = int.Parse(((TextBox)sender).Text);
				if (number < 0 || number > _WeekCheckInNum)
				{
					throw new Exception();
				}
			}
			catch (Exception)
			{
				MessageBox.Show("请输入1~" + _WeekCheckInNum + "范围内的整数");
				((TextBox)sender).Focus();
				e.Handled = true;
			}
		}

		private void Setting_MemberSchedule_AddClass_Click(object sender, RoutedEventArgs e)
		{
			Course item = Setting_MemberChechingInCourse_ChooseList.SelectedItem as Course;
			foreach (Course course in Setting_MemberScheduleCheckingInCourse_CourseList.Items)
			{
				if ((item.CourseID == course.CourseID) && (item.WeekIndex == course.WeekIndex)
					&& (item.StartClassIndex == course.StartClassIndex) && (item.EndClassIndex == course.EndClassIndex))
				{
					MessageBox.Show("课程已存在！");
					return;
				}
			}
			((Student)Setting_MemberList.SelectedItem).CourseList.Add((Course)Setting_MemberChechingInCourse_ChooseList.SelectedItem);
		}
	}
}
