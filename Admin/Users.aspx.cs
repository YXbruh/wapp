using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace CSA.Admin
{
    public partial class Users : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Admin")            //Bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadUsers();
        }

        private void LoadUsers()
        {
            // TODO:
            // var users = UserService.Search(tbSearch.Text.Trim(), ddlRole.SelectedValue, ddlStatus.SelectedValue);
            // litTotal.Text       = users.Count.ToString();
            // litStudents.Text    = users.Count(u => u.Role == "Student").ToString();
            // litInstructors.Text = users.Count(u => u.Role == "Instructor").ToString();
            // litActiveToday.Text = UserService.GetActiveTodayCount().ToString();
            // rptUsers.DataSource = users;
            // rptUsers.DataBind();
            // pnlEmpty.Visible = users.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void Search_Changed(object sender, EventArgs e) => LoadUsers();

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "Edit":
                    Response.Redirect($"~/Admin/EditUser.aspx?id={id}");
                    break;
                case "ToggleStatus":
                    // TODO: UserService.ToggleActive(id);
                    ShowSuccess("User status updated.");
                    LoadUsers();
                    break;
                case "Delete":
                    // TODO: UserService.Delete(id);
                    ShowSuccess("User deleted.");
                    LoadUsers();
                    break;
            }
        }

        protected void lbExport_Click(object sender, EventArgs e)
        {
            // TODO: generate CSV
            // Response.ContentType = "text/csv";
            // Response.AddHeader("Content-Disposition","attachment;filename=users.csv");
            // Response.Write(UserService.ExportCsv());
            // Response.End();
        }

        private void ShowSuccess(string msg)
        { pnlSuccess.Visible = true; litSuccess.Text = msg; pnlError.Visible = false; }

        public string GetRoleBadge(string role) =>
            role == "Admin" ? "badge-red" : role == "Instructor" ? "badge-green" : "badge-blue";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}