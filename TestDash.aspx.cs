using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA
{
    public partial class Admin_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Auth + role guard
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

            // Hardcoded Test Metrics (Bypassing database calls)
            litTotalUsers.Text = "42";
            litActiveCourses.Text = "7";
            litLabsOnline.Text = "5";
            litAlerts.Text = "0";
            litAlertCount.Text = "0";

            // Bind Fake Mock Data to the Repeater to prevent errors and test UI
            rptUsers.DataSource = GetMockUserData();
            rptUsers.DataBind();

            pnlNoUsers.Visible = true;
        }

        // Helper function to build dummy table rows instantly for testing
        private DataTable GetMockUserData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("UserID", typeof(int));
            dt.Columns.Add("FullName", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Role", typeof(string));
            dt.Columns.Add("IsActive", typeof(string));
            dt.Columns.Add("EnrolledCount", typeof(int));
            dt.Columns.Add("JoinedDisplay", typeof(string));

            // Populate mock rows to see how your CSS styles look
            dt.Rows.Add(1, "Alex Mercer", "alex.mercer@cybershield.edu", "Admin", "True", 0, "Jun 2026");
            dt.Rows.Add(2, "Sarah Connor", "s.connor@cybershield.edu", "Instructor", "True", 4, "May 2026");
            dt.Rows.Add(3, "John Doe", "johndoe@student.com", "Student", "False", 2, "Just Now");

            return dt;
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
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect($"~/Admin/EditUser.aspx?id={userId}");          //Bypass login for testing
            }
            else if (e.CommandName == "Delete")
            {
                //TODO: AdminService.DeleteUser(userId);
                LoadDashboard();
            }
        }

        protected void lbExport_Click(object sender, EventArgs e)
        {
            // TODO: generate CSV export
        }

        // Helper methods called from markup
        public string GetRoleBadge(string role) =>                                                                //Bypass login for testing
            role == "Admin" ? "badge-red" : role == "Instructor" ? "badge-green" : "badge-blue";            //Bypass login for testing

        public string GetStatusBadge(string isActive) =>                                      //Bypass login for testing
            isActive == "True" ? "badge-green" : "badge-amber";                                       //Bypass login for testing

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}