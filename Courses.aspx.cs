using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA
{
    public partial class Courses : Page
    {
        private string _role = "";

        private string ActiveCategory
        {
            get { return ViewState["ActiveCategory"]?.ToString() ?? "All"; }
            set { ViewState["ActiveCategory"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _role = Session["Role"]?.ToString() ?? "";
            if (!IsPostBack)
            {
                // Set default category
                ActiveCategory = "All";
                BindCategories();
                BindCourses();
            }
        }

        private void BindCategories()
        {
            DataTable dt = CourseService.GetCategories();
            // Insert "All" at the top
            DataRow allRow = dt.NewRow();
            allRow["CategoryName"] = "All";
            allRow["Description"] = "";
            allRow["CourseCount"] = 0;
            dt.Rows.InsertAt(allRow, 0);

            rptCategories.DataSource = dt;
            rptCategories.DataBind();
        }

        private void BindCourses()
        {
            string userId = Session["UserID"]?.ToString() ?? "";
            DataTable dt = CourseService.GetPublishedCourses(
                tbSearch.Text.Trim(), ActiveCategory, userId);

            rptCourses.DataSource = dt;
            rptCourses.DataBind();
            pnlNoCourses.Visible = dt.Rows.Count == 0;
        }

        protected void tbSearch_TextChanged(object sender, EventArgs e) => BindCourses();

        protected void rptCategories_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Filter")
            {
                ActiveCategory = e.CommandArgument.ToString();
                // Re‑bind categories so the active class is updated
                BindCategories();
                BindCourses();
            }
        }

        protected void rptCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string courseId = e.CommandArgument.ToString();
            if (_role == "Admin")
            {
                Response.Redirect($"~/Admin/EditCourse.aspx?id={courseId}");
            }
            else if (_role == "Lecturer")
            {
                Response.Redirect("~/Lecturer/ManageContent.aspx");
            }
            else if (_role == "Student")
            {
                Response.Redirect("~/Student/MyCourses.aspx");
            }
            else
            {
                Response.Redirect("~/Register.aspx");
            }
        }

        // ----- Helper methods for UI -----

        public string GetChipClass(string cat) =>
            cat == ActiveCategory ? "filter-chip active" : "filter-chip";

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
            if (_role == "Student") return isEnrolled ? "Open Course" : "Enroll Now";
            return "Enroll Now";
        }

        public string GetActionCss(bool isEnrolled)
        {
            if (_role == "Admin" || _role == "Lecturer") return "enroll-btn";
            return isEnrolled ? "enroll-btn enrolled" : "enroll-btn";
        }

        public bool GetActionEnabled(bool isEnrolled)
        {
            // Everyone can click – admins/lecturers manage, students jump to their course area, guests go to register.
            return true;
        }
    }
}