using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

using WhuRs;

namespace DataTransfer
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void buttonOpenOldFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "班级数据(*.wrc)|*.wrc";

			if (ofd.ShowDialog() == true)
			{
				textboxOldFilePath.Text = ofd.FileName;
			}
		}

		private void buttonTransfer_Click(object sender, RoutedEventArgs e)
		{
			FileStream ifs = new FileStream(textboxOldFilePath.Text, FileMode.Open);
			StreamReader sr = new StreamReader(ifs);

			string sClassName = sr.ReadLine().Substring("CLASS_INDEX : ".Length);
			Class cClass = new Class(sClassName);

			for (int i = 0; i < 7; i++)
			{
				sr.ReadLine();
				sr.ReadLine();
			}

			
			while (!sr.EndOfStream)
			{
				sr.ReadLine();
				string sLine = sr.ReadLine();
				//sLine = Encoding.Unicode.GetString(Encoding.Convert(Encoding.GetEncoding("GB2312"), Encoding.Unicode, Encoding.GetEncoding("GB2312").GetBytes(sLine)));
				string[] sStudentInfo = new string[3];
				try
				{
					sStudentInfo = sLine.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
				}
				catch (Exception)
				{
					break;
				}

				string sStudentID = sStudentInfo[0].Substring("ID : ".Length);
				string sStudentName = sStudentInfo[1].Substring("NAME : ".Length);
				try
				{
					cClass.AddMember(new Student(sStudentID, sStudentName, sClassName));
				}
				catch (ItemExistException)
				{
					continue;
				}

				for (int i = 0; i < 5; i++)
				{
					sr.ReadLine();
					sr.ReadLine();
				}
			}

			sr.Close();
			ifs.Close();

			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "班级XML数据文件(*.xml)|*.xml";
			sfd.FileName = sClassName + ".xml";
			if (sfd.ShowDialog() == false)
			{
				return;
			}

			XmlTextWriter writer = new XmlTextWriter(sfd.FileName, Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();
			
			writer.WriteStartElement("Class");      //Start Class
			writer.WriteAttributeString("ClassName", cClass.ClassName);
			writer.WriteAttributeString("MemberNum", cClass.MemberNum.ToString());

			writer.WriteStartElement("MemberList");       //Start MemberList
			foreach (Student student in cClass.MemberList)
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
	}
}
