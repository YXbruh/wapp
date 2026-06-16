using System;
using System.Web.UI;

namespace CSA
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // TODO: load live counts from DB
            // litStudents.Text = CourseService.GetStudentCount().ToString("N0") + "+";
            // litCourses.Text  = CourseService.GetActiveCourseCount().ToString();
        }
    }
}