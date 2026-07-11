using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace CSA.Student
{
    public partial class Student_Profile : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadProfile();
            }
        }

        private string CurrentUserId
        {
            get
            {
                return Convert.ToString(
                    Session["UserID"]);
            }
        }

        private void LoadProfile()
        {
            string connectionString =
                ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT
                      FullName,
                      Email,
                      CreatedAt
                  FROM Users
                  WHERE UserID = @UserID", con))
            {
                cmd.Parameters.Add(
                    "@UserID",
                    SqlDbType.NVarChar,
                    10
                ).Value = CurrentUserId;

                con.Open();

                using (SqlDataReader dr =
                       cmd.ExecuteReader())
                {
                    string name = "";
                    string email = "";
                    string joined = "—";

                    if (dr.Read())
                    {
                        name =
                            Convert.ToString(
                                dr["FullName"]);

                        email =
                            Convert.ToString(
                                dr["Email"]);

                        if (dr["CreatedAt"] !=
                            DBNull.Value)
                        {
                            joined =
                                Convert.ToDateTime(
                                    dr["CreatedAt"]
                                ).ToString(
                                    "MMMM yyyy"
                                );
                        }
                    }

                    Session["FullName"] = name;
                    Session["Email"] = email;

                    tbFullName.Text = name;
                    tbEmail.Text = email;

                    tbBio.Text =
                        Convert.ToString(
                            Session["Bio"]);

                    litDisplayName.Text = name;
                    litJoined.Text = joined;

                    litAvatarInitials.Text =
                        MakeInitials(name);
                }
            }
        }

        protected void btnSaveInfo_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string newName =
                tbFullName.Text.Trim();

            string newEmail =
                tbEmail.Text.Trim();

            string connectionString =
                ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlCommand checkCmd =
                       new SqlCommand(
                    @"SELECT COUNT(*)
                      FROM Users
                      WHERE Email = @Email
                        AND UserID <> @UserID",
                    con))
                {
                    checkCmd.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        255
                    ).Value = newEmail;

                    checkCmd.Parameters.Add(
                        "@UserID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = CurrentUserId;

                    int emailCount =
                        Convert.ToInt32(
                            checkCmd.ExecuteScalar()
                        );

                    if (emailCount > 0)
                    {
                        pnlError.Visible = true;
                        pnlSuccess.Visible = false;

                        litError.Text =
                            "That email address is already in use by another account.";

                        return;
                    }
                }

                using (SqlCommand updateCmd =
                       new SqlCommand(
                    @"UPDATE Users
                      SET FullName = @FullName,
                          Email = @Email
                      WHERE UserID = @UserID",
                    con))
                {
                    updateCmd.Parameters.Add(
                        "@FullName",
                        SqlDbType.NVarChar,
                        150
                    ).Value = newName;

                    updateCmd.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        255
                    ).Value = newEmail;

                    updateCmd.Parameters.Add(
                        "@UserID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = CurrentUserId;

                    updateCmd.ExecuteNonQuery();
                }
            }

            Session["Bio"] =
                tbBio.Text.Trim();

            Session["FullName"] = newName;
            Session["Email"] = newEmail;

            pnlSuccess.Visible = true;
            pnlError.Visible = false;

            litSuccess.Text =
                "Profile updated successfully.";

            LoadProfile();
        }

        protected void btnChangePwd_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            bool success = false;
            string errorMessage = "";

            string connectionString =
                ConfigurationManager
                    .ConnectionStrings["CSAConnection"]
                    .ConnectionString;

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                con.Open();

                object storedHash;

                using (SqlCommand cmd =
                       new SqlCommand(
                    @"SELECT PasswordHash
                      FROM Users
                      WHERE UserID = @UserID",
                    con))
                {
                    cmd.Parameters.Add(
                        "@UserID",
                        SqlDbType.NVarChar,
                        10
                    ).Value = CurrentUserId;

                    storedHash =
                        cmd.ExecuteScalar();
                }

                if (storedHash == null ||
                    storedHash == DBNull.Value)
                {
                    errorMessage =
                        "Could not find your account. Please log in again.";
                }
                else if (
                    HashPasswordTemp(
                        tbCurrentPwd.Text
                    ) != storedHash.ToString())
                {
                    errorMessage =
                        "Current password is incorrect.";
                }
                else
                {
                    using (SqlCommand updateCmd =
                           new SqlCommand(
                        @"UPDATE Users
                          SET PasswordHash =
                              @PasswordHash
                          WHERE UserID = @UserID",
                        con))
                    {
                        updateCmd.Parameters.Add(
                            "@PasswordHash",
                            SqlDbType.NVarChar,
                            512
                        ).Value =
                            HashPasswordTemp(
                                tbNewPwd.Text
                            );

                        updateCmd.Parameters.Add(
                            "@UserID",
                            SqlDbType.NVarChar,
                            10
                        ).Value = CurrentUserId;

                        updateCmd.ExecuteNonQuery();
                    }

                    success = true;
                }
            }

            if (success)
            {
                pnlSuccess.Visible = true;
                pnlError.Visible = false;

                litSuccess.Text =
                    "Password changed successfully.";

                tbCurrentPwd.Text = "";
                tbNewPwd.Text = "";
                tbConfirmPwd.Text = "";
            }
            else
            {
                pnlError.Visible = true;
                pnlSuccess.Visible = false;

                litError.Text = errorMessage;
            }
        }

        private static string MakeInitials(
            string name)
        {
            string[] parts =
                (name ?? "").Split(
                    new[] { ' ' },
                    StringSplitOptions
                        .RemoveEmptyEntries
                );

            if (parts.Length >= 2)
            {
                return
                    parts[0].Substring(0, 1) +
                    parts[parts.Length - 1]
                        .Substring(0, 1);
            }

            if (parts.Length == 1)
            {
                return parts[0].Substring(
                    0,
                    Math.Min(
                        2,
                        parts[0].Length
                    )
                );
            }

            return "CS";
        }

        private static string HashPasswordTemp(
            string plainPassword)
        {
            using (SHA256 sha256 =
                   SHA256.Create())
            {
                byte[] bytes =
                    sha256.ComputeHash(
                        Encoding.UTF8.GetBytes(
                            plainPassword ?? ""
                        )
                    );

                StringBuilder result =
                    new StringBuilder();

                foreach (byte b in bytes)
                {
                    result.Append(
                        b.ToString("x2")
                    );
                }

                return result.ToString();
            }
        }

        protected void lbLogout_Click(
            object sender,
            EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect("~/Login.aspx");
        }
    }
}