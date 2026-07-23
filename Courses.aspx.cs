using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA
{
    public partial class Courses : Page
    {
        private string _activeCategory = "All";
        private string _role = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            _role = Session["Role"]?.ToString() ?? "";
            if (!IsPostBack)
            {
                BindCategories();
                BindCourses();
            }
        }

        private void BindCategories()
        {
            DataTable dt = CourseService.GetCategories();
            var rows = dt.NewRow();
            rows["CategoryName"] = "All";
            rows["Description"] = "";
            rows["CourseCount"] = 0;
            dt.Rows.InsertAt(rows, 0);

            rptCategories.DataSource = dt;
            rptCategories.DataBind();
        }

        private void BindCourses()
        {
            string userId = Session["UserID"]?.ToString() ?? "";
            DataTable dt = CourseService.GetPublishedCourses(
                tbSearch.Text.Trim(), _activeCategory, userId);

            rptCourses.DataSource = dt;
            rptCourses.DataBind();
            pnlNoCourses.Visible = dt.Rows.Count == 0;
        }

        protected void tbSearch_TextChanged(object sender, EventArgs e) => BindCourses();

        protected void rptCategories_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Filter")
            {
                _activeCategory = e.CommandArgument.ToString();
                BindCourses();
            }
        }

        protected void rptCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string courseId = e.CommandArgument.ToString();
            if (_role == "Admin" || _role == "Lecturer")
            {
                Response.Redirect($"~/Admin/EditCourse.aspx?id={courseId}");
            }
            else if (_role == "Student")
            {
                // TODO: enroll student in course
                BindCourses();
            }
        }

        public string GetChipClass(string cat) =>
            cat == _activeCategory ? "filter-chip active" : "filter-chip";

        public string GetIconClass(string category)
        {
            switch (category)
            {
                case "Fundamentals": return "ti ti-shield";
                case "Network Security": return "ti ti-network";
                case "Ethical Hacking": return "ti ti-hacker";
                case "Web Security": return "ti ti-world-www";
                default: return "ti ti-book";
            }
        }

        public string GetLevelBadgeClass(string level)
        {
            switch (level)
            {
                case "Beginner": return "badge-green";
                case "Intermediate": return "badge-amber";
                case "Advanced": return "badge-red";
                default: return "badge-blue";
            }
        }

        public string GetActionText(bool isEnrolled)
        {
            if (_role == "Admin" || _role == "Lecturer") return "Manage";
            return isEnrolled ? "Enrolled" : "Enroll Now";
        }

        public string GetActionCss(bool isEnrolled)
        {
            if (_role == "Admin" || _role == "Lecturer") return "enroll-btn";
            return isEnrolled ? "enroll-btn enrolled" : "enroll-btn";
        }

        public bool GetActionEnabled(bool isEnrolled)
        {
            if (_role == "Admin" || _role == "Lecturer") return true;
            return !isEnrolled;
        }
    }
}