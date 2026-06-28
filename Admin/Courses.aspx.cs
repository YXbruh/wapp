using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Admin
{

    public partial class Courses : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Admin")            //Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadCourses();
        }

        private void LoadCourses()
        {
            // TODO:
            // var list = CourseService.AdminSearch(tbSearch.Text.Trim(), ddlStatus.SelectedValue, ddlLevel.SelectedValue);
            // litTotal.Text     = list.Count.ToString();
            // litPublished.Text = list.Count(c => c.Status == "Published").ToString();
            // litDraft.Text     = list.Count(c => c.Status == "Draft").ToString();
            // litPending.Text   = list.Count(c => c.Status == "Pending").ToString();
            // rptCourses.DataSource = list; rptCourses.DataBind();
            // pnlEmpty.Visible = list.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void Search_Changed(object sender, EventArgs e) => LoadCourses();

        protected void rptCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "Edit": Response.Redirect($"~/Admin/EditCourse.aspx?id={id}"); break;
                case "TogglePublish":
                    // TODO: CourseService.TogglePublish(id);
                    pnlSuccess.Visible = true; litSuccess.Text = "Course status updated.";
                    LoadCourses(); break;
                case "Delete":
                    // TODO: CourseService.Delete(id);
                    pnlSuccess.Visible = true; litSuccess.Text = "Course deleted.";
                    LoadCourses(); break;
            }
        }

        public string GetLevelBadge(string l) =>
            l == "Beginner" ? "badge-blue" : l == "Intermediate" ? "badge-amber" : "badge-red";

        public string GetStatusBadge(string s) =>
            s == "Published" ? "badge-green" : s == "Pending" ? "badge-blue" : "badge-amber";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}