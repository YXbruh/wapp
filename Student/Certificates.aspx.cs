using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Student
{
    public partial class Student_Certificates : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }     //remove to bypass login for testing
            if (!IsPostBack) LoadCertificates();
        }

        private void LoadCertificates()
        {
            //int userId = (int)Session["UserID"];                                          //remove to bypass login for testing
            // TODO: replace with DB call
            // var certs = CertificateService.GetByStudent(userId);
            // litCount.Text  = certs.Count.ToString();
            // litLatest.Text = certs.Count > 0 ? certs[0].IssuedDate.ToString("dd MMM yyyy") : "—";
            // rptCerts.DataSource = certs;
            // rptCerts.DataBind();
            // pnlEmpty.Visible = certs.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }

}