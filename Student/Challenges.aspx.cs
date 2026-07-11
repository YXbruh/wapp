using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Student
{
    public partial class Student_Challenges : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Student")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadChallenges();
        }

        private void LoadChallenges()
        {
            //string userId = Session["UserID"].ToString();                                              //remove to bypass login for testing
            // TODO: replace with DB call
            // var list = ChallengeService.GetForStudent(userId);
            // litTotal.Text = list.Count.ToString();
            // litDone.Text  = list.Count(c => c.StatusKey == "done").ToString();
            // litXP.Text    = list.Where(c => c.StatusKey == "done").Sum(c => c.XPReward).ToString();
            // rptChallenges.DataSource = list;
            // rptChallenges.DataBind();
            // pnlEmpty.Visible = list.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout");
        }
    }

}