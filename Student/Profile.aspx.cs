using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using WebGrease.Activities;
using System.Security.Cryptography;
using System.Text;

namespace CSA.Student
{
    public partial class Student_Profile : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }             //remove to bypass login for testing
            if (!IsPostBack)
            {
                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("select FullName, Email, CreatedAt from Users where UserID = " + userId, con);
            SqlDataReader dr = cmd.ExecuteReader();

            string name = "";
            string email = "";
            string joined = "—";

            if (dr.Read())
            {
                name = dr["FullName"].ToString();
                email = dr["Email"].ToString();
                joined = Convert.ToDateTime(dr["CreatedAt"]).ToString("MMMM yyyy");
            }
            dr.Close();
            con.Close();

            Session["FullName"] = name;
            Session["Email"] = email;

            tbFullName.Text = name;
            tbEmail.Text = email;
            // NOTE: the Users table has no Bio column yet, so Bio is kept in Session only.
            // Add a "Bio NVARCHAR(300) NULL" column to Users if this needs to be saved for real.
            tbBio.Text = Session["Bio"] as string ?? "";

            litDisplayName.Text = name;
            litJoined.Text = joined;

            string[] parts = name.Split(' ');
            if (parts.Length >= 2)
            {
                litAvatarInitials.Text = parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1);
            }
            else if (name.Length > 0)
            {
                litAvatarInitials.Text = name.Substring(0, Math.Min(2, name.Length));
            }
            else
            {
                litAvatarInitials.Text = "CS";
            }
        }

        protected void btnSaveInfo_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up
            string newName = tbFullName.Text.Trim();
            string newEmail = tbEmail.Text.Trim();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            SqlCommand checkCmd = new SqlCommand("select count(*) from Users where Email = '" + newEmail + "' and UserID <> " + userId, con);
            int emailTaken = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (emailTaken > 0)
            {
                con.Close();
                pnlError.Visible = true;
                pnlSuccess.Visible = false;
                litError.Text = "That email address is already in use by another account.";
                return;
            }

            string updateQuery = "update Users set FullName = '" + newName + "', Email = '" + newEmail + "' where UserID = " + userId;
            SqlCommand cmd = new SqlCommand(updateQuery, con);
            cmd.ExecuteNonQuery();
            con.Close();

            // Bio is session-only for now (see note in LoadProfile).
            Session["Bio"] = tbBio.Text.Trim();
            Session["FullName"] = newName;
            Session["Email"] = newEmail;

            pnlSuccess.Visible = true;
            litSuccess.Text = "Profile updated successfully.";
            pnlError.Visible = false;

            LoadProfile();
        }

        protected void btnChangePwd_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int userId = 1; // TODO: replace with Convert.ToInt32(Session["UserID"]) once Login.aspx is wired up

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("select PasswordHash from Users where UserID = " + userId, con);
            object result = cmd.ExecuteScalar();

            // TODO: swap this block back to PasswordManager/BCrypt.Net-Next once merged with
            // teammate's Login/Register code (App_Code/PasswordManager.cs).
            // Using a plain SHA256 hash here temporarily so Student module compiles standalone.

            bool ok = false;
            string err = "";

            if (result == null)
            {
                err = "Could not find your account. Please log in again.";
            }
            else if (HashPasswordTemp(tbCurrentPwd.Text) != result.ToString())
            {
                err = "Current password is incorrect.";
            }
            else
            {
                string hashedPassword = HashPasswordTemp(tbNewPwd.Text);
                string updateQuery = "update Users set PasswordHash = '" + hashedPassword + "' where UserID = " + userId;
                SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                updateCmd.ExecuteNonQuery();
                ok = true;
            }

            con.Close();

            if (ok)
            {
                pnlSuccess.Visible = true;
                litSuccess.Text = "Password changed successfully.";
                pnlError.Visible = false;
                tbCurrentPwd.Text = "";
                tbNewPwd.Text = "";
                tbConfirmPwd.Text = "";
            }
            else
            {
                pnlError.Visible = true;
                pnlSuccess.Visible = false;
                litError.Text = err;
            }
        }

        // TODO: remove once PasswordManager class is merged in from teammate's code.
        private string HashPasswordTemp(string plainPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
    }