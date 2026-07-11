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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadBackups();
        }

        private void LoadBackups()
        {
            DataTable backups = AdminService.GetBackups();

            if (backups.Rows.Count > 0)
                litLastBackup.Text = backups.Rows[0]["CreatedDisplay"].ToString();
            else
                litLastBackup.Text = "Never";

            litBackupCount.Text = backups.Rows.Count.ToString();
            rptBackups.DataSource = backups;
            rptBackups.DataBind();
            pnlEmpty.Visible = backups.Rows.Count == 0;
            litDbStatus.Text = AdminService.TestConnection() ? "Online" : "Offline";
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
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Download")
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
                        Response.End();
                    }
                }
            }
            else if (e.CommandName == "Delete")
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
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
