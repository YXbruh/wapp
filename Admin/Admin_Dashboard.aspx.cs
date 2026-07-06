using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Admin
{
    public partial class Admin_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Auth + role guard
            //if (Session["UserID"] == null || Session["Role"] as string != "Admin")     //Bypass login for testing
            //{
            //    Response.Redirect("~/Login.aspx");
            //    return;
            //}

            if (!IsPostBack)
                LoadDashboard();
        }

        private void LoadDashboard()
        {
            litLastUpdated.Text = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");

            // TODO: load real stats from DB
            // var stats = AdminService.GetPlatformStats();
            // litTotalUsers.Text   = stats.UserCount.ToString("N0");
            // litActiveCourses.Text= stats.CourseCount.ToString();
            // litLabsOnline.Text   = stats.LabCount.ToString();
            // litAlerts.Text       = stats.AlertCount.ToString();
            // litAlertCount.Text   = stats.AlertCount.ToString();

            // TODO: bind user table
            // rptUsers.DataSource = AdminService.GetUsers(tbSearch.Text.Trim());
            // rptUsers.DataBind();
            pnlNoUsers.Visible = true;
        }

        protected void tbSearch_TextChanged(object sender, EventArgs e)
        {
            // TODO: re-bind with search filter
            // rptUsers.DataSource = AdminService.GetUsers(tbSearch.Text.Trim());
            // rptUsers.DataBind();
        }

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userId = Convert.ToInt32(e.CommandArgument);                      //Bypass login for testing
            if (e.CommandName == "Edit")
                Response.Redirect($"~/Admin/EditUser.aspx?id={userId}");          //Bypass login for testing
            //else if (e.CommandName == "Delete")
            //{
            //TODO: AdminService.DeleteUser(userId);
            //LoadDashboard();
            //}
        }

        protected void lbExport_Click(object sender, EventArgs e)
        {
            // TODO: generate CSV export
        }

        // Helper methods called from markup
        //public string GetRoleBadge(string role) =>                                                                //Bypass login for testing
        //role == "Admin" ? "badge-red" : role == "Instructor" ? "badge-green" : "badge-blue";            //Bypass login for testing

        //public string GetStatusBadge(string isActive) =>                                      //Bypass login for testing
        //isActive == "True" ? "badge-green" : "badge-amber";                                       //Bypass login for testing

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}