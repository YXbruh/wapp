using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA.Student
{
        public partial class Student_Achievements : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["UserID"] == null) { Response.Redirect("~/Login.aspx"); return; }         //remove to bypass login for testing
            if (!IsPostBack) LoadAchievements();
        }

        private void LoadAchievements()
        {
            //int userId = (int)Session["UserID"];                                      //remove to bypass login for testing
            // TODO: replace with DB call
            // var list = AchievementService.GetAll(userId);  // includes locked ones
            // litEarned.Text = list.Count(a => a.IsEarned).ToString();
            // litTotal.Text  = list.Count.ToString();
            // litXP.Text     = list.Where(a => a.IsEarned).Sum(a => a.XPValue).ToString();
            // var latest = list.Where(a => a.IsEarned).OrderByDescending(a => a.EarnedDate).FirstOrDefault();
            // litLatest.Text = latest?.BadgeName ?? "—";
            // rptAchievements.DataSource = list;
            // rptAchievements.DataBind();
            // pnlEmpty.Visible = list.Count == 0;
            pnlEmpty.Visible = true;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }

}