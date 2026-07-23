using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.DataAccess;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Backup : Page
    {
        protected Literal litPageInfo;
        protected LinkButton btnPrev;
        protected LinkButton btnNext;

        private Pager _pager;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadBackups();
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
                        _pager = new Pager { PageSize = 10 };
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

        private void LoadBackups()
        {
            var p = PagerState;
            DataTable backups = AdminService.GetBackups(p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            if (backups.Rows.Count > 0)
                litLastBackup.Text = backups.Rows[0]["CreatedDisplay"].ToString();
            else
                litLastBackup.Text = "Never";

            litBackupCount.Text = backups.Rows.Count.ToString();
            rptBackups.DataSource = backups;
            rptBackups.DataBind();
            pnlEmpty.Visible = backups.Rows.Count == 0;
            litDbStatus.Text = AdminService.TestConnection() ? "Online" : "Offline";
            UpdatePagerUI();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            PagerState.Page--;
            LoadBackups();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            PagerState.Page++;
            LoadBackups();
        }

        protected void btnBackup_Click(object sender, EventArgs e)
        {
            string adminId = Session["UserID"].ToString();

            bool ok = AdminService.ExecuteBackup(
                tbLabel.Text.Trim(), ddlType.SelectedValue, adminId, out string err);

            if (ok)
            {
                pnlSuccess.Visible = true;
                litSuccess.Text = $"Backup completed successfully at {DateTime.Now:HH:mm:ss}.";
                pnlError.Visible = false;
                tbLabel.Text = "";
                LoadBackups();
            }
            else
            {
                pnlError.Visible = true;
                litError.Text = $"Backup failed: {err}";
                pnlSuccess.Visible = false;
            }
        }

        protected void rptBackups_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int id)) return;
            if (e.CommandName == "Download")
            {
                try
                {
                    DataTable dt = DBHelper.ExecuteQuery(
                        "SELECT FilePath FROM DatabaseBackups WHERE BackupID = @ID",
                        new System.Data.SqlClient.SqlParameter("@ID", id));
                    if (dt.Rows.Count > 0)
                    {
                        string path = dt.Rows[0]["FilePath"].ToString();
                        if (System.IO.File.Exists(path))
                        {
                            Response.ContentType = "application/octet-stream";
                            Response.AddHeader("Content-Disposition",
                                $"attachment;filename={System.IO.Path.GetFileName(path)}");
                            Response.WriteFile(path);
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
                catch (Exception ex)
                {
                    pnlError.Visible = true;
                    litError.Text = "Error downloading backup: " + ex.Message;
                }
            }
            else if (e.CommandName == "Delete")
            {
                try
                {
                    DataTable dt = DBHelper.ExecuteQuery(
                        "SELECT FilePath FROM DatabaseBackups WHERE BackupID = @ID",
                        new System.Data.SqlClient.SqlParameter("@ID", id));
                    if (dt.Rows.Count > 0)
                    {
                        string path = dt.Rows[0]["FilePath"].ToString();
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);
                    }
                    DBHelper.ExecuteNonQuery(
                        "DELETE FROM DatabaseBackups WHERE BackupID = @ID",
                        new System.Data.SqlClient.SqlParameter("@ID", id));
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Backup deleted.";
                    LoadBackups();
                }
                catch (Exception ex)
                {
                    pnlError.Visible = true;
                    litError.Text = "Error deleting backup: " + ex.Message;
                    pnlSuccess.Visible = false;
                }
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
