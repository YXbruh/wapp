using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Announcements : Page
    {
        private Pager _pager;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) 
            {
                LoadAnnouncements();
                LoadRecipients();
            }
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

        private void LoadAnnouncements()
        {
            var p = PagerState;
            DataTable list = AdminService.SearchAnnouncements(tbSearchAnn.Text.Trim(),
                p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            rptAnnouncements.DataSource = list;
            rptAnnouncements.DataBind();
            pnlEmpty.Visible = list.Rows.Count == 0;
            UpdatePagerUI();
        }

        protected void tbSearchAnn_TextChanged(object sender, EventArgs e)
        {
            PagerState.Page = 1;
            LoadAnnouncements();
        }

        protected void ddlAudience_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecipients();
            pnlCustomRecipients.Visible = ddlAudience.SelectedValue == "Custom";
        }

        private void LoadRecipients()
        {
            if (ddlAudience.SelectedValue == "Custom")
            {
                var dt = AdminService.GetAllUsers();
                rptRecipients.DataSource = dt;
                rptRecipients.DataBind();
            }
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            PagerState.Page--;
            LoadAnnouncements();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            PagerState.Page++;
            LoadAnnouncements();
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            string adminId = Session["UserID"].ToString();
            if (!int.TryParse(hfEditID.Value, out int editId)) editId = 0;

            DateTime? expiry = null;
            if (!string.IsNullOrWhiteSpace(tbExpiry.Text))
            {
                if (DateTime.TryParseExact(tbExpiry.Text, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime exp))
                    expiry = exp;
            }

            try
            {
                if (editId == 0)
                {
                    AdminService.CreateAnnouncement(
                        tbTitle.Text.Trim(), tbMessage.Text.Trim(),
                        ddlAudience.SelectedValue, ddlPriority.SelectedValue,
                        expiry, adminId);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Announcement published successfully.";
                    
                    // Send email notification
                    if (ddlAudience.SelectedValue == "Custom")
                    {
                        var selectedEmails = GetSelectedEmails();
                        if (selectedEmails.Count > 0)
                            SendAnnouncementEmail(selectedEmails, tbTitle.Text.Trim(), tbMessage.Text.Trim());
                    }
                    else
                    {
                        var emailAudience = MapAudienceForEmail(ddlAudience.SelectedValue);
                        SendAnnouncementEmail(emailAudience, tbTitle.Text.Trim(), tbMessage.Text.Trim());
                    }
                }
                else
                {
                    AdminService.UpdateAnnouncement(editId,
                        tbTitle.Text.Trim(), tbMessage.Text.Trim(),
                        ddlAudience.SelectedValue, ddlPriority.SelectedValue, expiry);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Announcement updated.";
                    ResetForm();
                }
                LoadAnnouncements();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                    litError.Text = "Error saving announcement: " + Server.HtmlEncode(ex.Message);
                pnlSuccess.Visible = false;
            }
        }

        protected void rptAnnouncements_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument.ToString(), out int id)) return;
            if (e.CommandName == "Edit")
            {
                DataTable dt = AdminService.GetAnnouncementById(id);
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    tbTitle.Text = row["Title"].ToString();
                    tbMessage.Text = row["Body"].ToString();
                    tbExpiry.Text = row["ExpiresAt"] != DBNull.Value
                        ? Convert.ToDateTime(row["ExpiresAt"]).ToString("yyyy-MM-dd") : "";
                    hfEditID.Value = id.ToString();
                    litFormTitle.Text = "Edit Announcement";
                    lbCancelEdit.Visible = true;
                    btnPublish.Text = "Update Announcement";
                }
            }
            else if (e.CommandName == "Delete")
            {
                try
                {
                    AdminService.DeleteAnnouncement(id);
                    pnlSuccess.Visible = true;
                    litSuccess.Text = "Announcement deleted.";
                    LoadAnnouncements();
                }
                catch (Exception ex)
                {
                    pnlError.Visible = true;
                    litError.Text = "Error deleting announcement: " + Server.HtmlEncode(ex.Message);
                    pnlSuccess.Visible = false;
                }
            }
        }

        protected void lbCancelEdit_Click(object sender, EventArgs e) => ResetForm();

        private void ResetForm()
        {
            tbTitle.Text = tbMessage.Text = tbExpiry.Text = "";
            ddlAudience.SelectedIndex = 0;
            ddlPriority.SelectedIndex = 0;
            hfEditID.Value = "0";
            litFormTitle.Text = "New Announcement";
            lbCancelEdit.Visible = false;
            btnPublish.Text = "Publish Announcement";
        }

        public string GetPriorityBadge(string p) =>
            p == "Urgent" ? "badge-red" : p == "Important" ? "badge-amber" : "badge-blue";

        private void SendAnnouncementEmail(List<string> recipients, string title, string message)
        {
            try
            {
                var smtpHost = System.Configuration.ConfigurationManager.AppSettings["SmtpHost"];
                var smtpPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                var smtpUser = System.Configuration.ConfigurationManager.AppSettings["SmtpUser"];
                var smtpPass = System.Configuration.ConfigurationManager.AppSettings["SmtpPass"];
                var fromEmail = System.Configuration.ConfigurationManager.AppSettings["FromEmail"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail))
                    return; // Email not configured

                if (recipients.Count == 0) return;

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    // Only enable SSL for non-localhost (Gmail, etc.)
                    client.EnableSsl = !smtpHost.Equals("localhost", StringComparison.OrdinalIgnoreCase) 
                        && !smtpHost.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
                    
                    if (!string.IsNullOrEmpty(smtpUser))
                        client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);

                    foreach (var email in recipients)
                    {
                        var mail = new MailMessage
                        {
                            From = new MailAddress(fromEmail, "CyberShield Academy"),
                            Subject = title,
                            Body = $"<h2>{System.Net.WebUtility.HtmlEncode(title)}</h2><p>{System.Net.WebUtility.HtmlEncode(message)}</p><hr><p><small>Sent from CyberShield Academy</small></p>",
                            IsBodyHtml = true
                        };
                        mail.To.Add(email);
                        client.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the announcement
                System.Diagnostics.Debug.WriteLine($"Email send failed: {ex.Message}");
            }
        }

        private void SendAnnouncementEmail(string audience, string title, string message)
        {
            try
            {
                var smtpHost = System.Configuration.ConfigurationManager.AppSettings["SmtpHost"];
                var smtpPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                var smtpUser = System.Configuration.ConfigurationManager.AppSettings["SmtpUser"];
                var smtpPass = System.Configuration.ConfigurationManager.AppSettings["SmtpPass"];
                var fromEmail = System.Configuration.ConfigurationManager.AppSettings["FromEmail"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail))
                    return; // Email not configured

                var recipients = GetRecipientEmails(audience);
                if (recipients.Count == 0) return;

                SendAnnouncementEmail(recipients, title, message);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the announcement
                System.Diagnostics.Debug.WriteLine($"Email send failed: {ex.Message}");
            }
        }

private List<string> GetSelectedEmails()
        {
            var emails = new List<string>();
            foreach (RepeaterItem item in rptRecipients.Items)
            {
                var cb = item.FindControl("cbRecipient") as CheckBox;
var hf = item.FindControl("hfEmail") as HiddenField;
                if (cb != null && cb.Checked && hf != null && !string.IsNullOrWhiteSpace(hf.Value))
                    emails.Add(hf.Value);
            }
            return emails;
        }

        private List<string> GetRecipientEmails(string audience)
        {
            var emails = new List<string>();
            var dt = AdminService.GetEmailsByAudience(audience);
            foreach (DataRow row in dt.Rows)
            {
                var email = row["Email"].ToString();
                if (!string.IsNullOrWhiteSpace(email))
                    emails.Add(email);
            }
            return emails;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }

        private string MapAudienceForEmail(string audience)
        {
            switch (audience)
            {
                case "Student": return "Students";
                case "Lecturer": return "Lecturers";
                case "All": return "All";
                default: return audience;
            }
        }
    }
}
