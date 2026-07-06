using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Student
{

    public partial class Student_MyCourses : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }             //remove to bypass login for testing
            if (!IsPostBack) LoadCourses();
        }

        private void LoadCourses()
        {
            //int userId = (int)Session["UserID"];                              //remove to bypass login for testing
            // TODO: replace with DB call
            // var courses = CourseService.GetEnrolledCourses(userId);
            // rptCourses.DataSource = courses;
            // rptCourses.DataBind();
            // pnlEmpty.Visible = courses.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}