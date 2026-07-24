using System;
using System.Data;
using System.Web.UI;
using CSA.Services;

namespace CSA
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            try
            {
                DataRow stats = AdminService.GetHomepageStats();

                litStudents.Text = Convert.ToInt32(stats["StudentCount"]).ToString("N0");
                litCourses.Text = Convert.ToInt32(stats["CourseCount"]).ToString("N0");
                litLabs.Text = Convert.ToInt32(stats["LabCount"]).ToString("N0");

                // Satisfaction: real average star rating (1–5) expressed as a percentage.
                // Shown as "—" until at least one rating has been submitted.
                if (stats["AvgRating"] != DBNull.Value)
                {
                    double pct = Convert.ToDouble(stats["AvgRating"]) / 5.0 * 100.0;
                    litSatisfaction.Text = Math.Round(pct) + "%";
                }
            }
            catch
            {
                // Landing page must still render if the database is unreachable;
                // the literals keep their neutral defaults.
            }
        }
    }
}