using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Users : Page
    {
        private Pager _pager;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadUsers();
        }

        private Pager PagerState
        {
            get
            {
                if (_pager == null)
                {
                    if (ViewState["Pager"] is Pager p)
                        _pager = p;
                    else
                    {
                        _pager = new Pager { PageSize = 20 };
                        ViewState["Pager"] = _pager;
                    }
                }
                return _pager;
            }
        }

        private void UpdatePagerUI()
        {
            var p = PagerState;
            litPageInfo.Text = "Page " + p.Page + " of " + Math.Max(1, p.TotalPages) + " (" + p.Total + " total)";
            btnPrev.Visible = p.HasPrevious;
            btnNext.Visible = p.HasNext;
        }

        private void LoadUsers()
        {
            var p = PagerState;
            DataTable users = UserService.Search(tbSearch.Text.Trim(),
                ddlRole.SelectedValue, ddlStatus.SelectedValue,
                p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            litTotal.Text = total.ToString();
            UserService.GetCountsByRole(out int totalStudents, out int totalLecturers);
            litStudents.Text = totalStudents.ToString();
            litInstructors.Text = totalLecturers.ToString();
            litActiveToday.Text = UserService.GetActiveTodayCount().ToString();

            rptUsers.DataSource = users;
            rptUsers.DataBind();
            pnlEmpty.Visible = users.Rows.Count == 0;
            UpdatePagerUI();
        }

        protected void Search_Changed(object sender, EventArgs e)
        {
            PagerState.Page = 1;
            LoadUsers();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            PagerState.Page--;
            LoadUsers();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            PagerState.Page++;
            LoadUsers();
        }

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();
            string adminId = Session["UserID"].ToString();
            switch (e.CommandName)
            {
                case "Edit":
                    Response.Redirect($"~/Admin/EditUser.aspx?id={id}");
                    break;
                case "ToggleStatus":
                    try
                    {
                        UserService.ToggleActive(id);
                        AdminService.LogAudit(adminId, "TOGGLE_USER_STATUS", "Users", id, "", "");
                        ShowSuccess("User status updated.");
                        LoadUsers();
                    }
                    catch (Exception)
                    {
                        pnlError.Visible = true;
                        litError.Text = "Error updating user status.";
                    }
                    break;
                case "Delete":
                    try
                    {
                        UserService.Delete(id);
                        AdminService.LogAudit(adminId, "DELETE_USER", "Users", id, "", "");
                        ShowSuccess("User deleted.");
                        LoadUsers();
                    }
                    catch (Exception)
                    {
                        pnlError.Visible = true;
                        litError.Text = "Cannot delete this user. They may have existing enrollments, activity logs, or other linked records. Remove those first.";
                    }
                    break;
            }
        }

        protected void lbExport_Click(object sender, EventArgs e)
        {
            string csv = UserService.ExportCsv(tbSearch.Text.Trim(),
                ddlRole.SelectedValue, ddlStatus.SelectedValue);
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=users.csv");
            Response.Write(csv);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowSuccess(string msg)
        { pnlSuccess.Visible = true; litSuccess.Text = msg; pnlError.Visible = false; }

        public string GetRoleBadge(string role) =>
            role == "Admin" ? "badge-red" : role == "Lecturer" ? "badge-green" : "badge-blue";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
