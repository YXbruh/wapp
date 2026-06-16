using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSA
{
    public partial class Courses : Page
    {
        private string _activeCategory = "All";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindCategories();
                BindCourses();
            }
        }

        private void BindCategories()
        {
            // TODO: load from DB — CategoryService.GetAll()
            var cats = new List<object>
        {
            new { CategoryName = "All" },
            new { CategoryName = "Network Security" },
            new { CategoryName = "Web Security" },
            new { CategoryName = "Forensics" },
            new { CategoryName = "Cryptography" },
            new { CategoryName = "CTF" },
        };
            rptCategories.DataSource = cats;
            rptCategories.DataBind();
        }

        private void BindCourses()
        {
            // TODO: replace with real DB query filtered by tbSearch.Text + _activeCategory
            // var courses = CourseService.GetCourses(tbSearch.Text.Trim(), _activeCategory, Session["UserID"]);
            // rptCourses.DataSource = courses;
            // rptCourses.DataBind();
            // pnlNoCourses.Visible = courses.Count == 0;
            pnlNoCourses.Visible = true;
        }

        protected void tbSearch_TextChanged(object sender, EventArgs e) => BindCourses();

        protected void rptCategories_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Filter")
            {
                _activeCategory = e.CommandArgument.ToString();
                BindCourses();
            }
        }

        // Called from markup to set active chip class
        public string GetChipClass(string cat) =>
            cat == _activeCategory ? "filter-chip active" : "filter-chip";
    }

}