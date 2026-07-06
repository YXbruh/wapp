using System;
using System.Data.SqlClient;
using System.Reflection.Emit;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebGrease.Activities;

namespace CSA.Admin
{
    
    public partial class Backup : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null || Session["Role"] as string != "Admin")        //bypass login for testing
            //{ Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadBackups();
        }

        private void LoadBackups()
        {
            // TODO:
            // var backups = BackupService.GetAll();
            // litLastBackup.Text  = backups.Count > 0 ? backups[0].CreatedAt.ToString("dd MMM yyyy HH:mm") : "Never";
            // litBackupCount.Text = backups.Count.ToString();
            // rptBackups.DataSource = backups; rptBackups.DataBind();
            // pnlEmpty.Visible = backups.Count == 0;
            // litDbStatus.Text = BackupService.TestConnection() ? "Online" : "Offline";
            pnlEmpty.Visible = true;
        }

        protected void btnBackup_Click(object sender, EventArgs e)
        {
            int adminId = (int)Session["UserID"];
            // TODO:
            // bool ok = BackupService.Execute(tbLabel.Text.Trim(), ddlType.SelectedValue, adminId, out string err);
            bool ok = false;
            string err = "Backup service not yet connected.";

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
                litError.Text = $"Backup failed: {Server.HtmlEncode(err)}";
                pnlSuccess.Visible = false;
            }
        }

        protected void rptBackups_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Download")
            {
                // TODO: BackupService.StreamDownload(id, Response);
            }
            else if (e.CommandName == "Delete")
            {
                // TODO: BackupService.Delete(id);
                pnlSuccess.Visible = true; litSuccess.Text = "Backup deleted.";
                LoadBackups();
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx"); }
    }

}