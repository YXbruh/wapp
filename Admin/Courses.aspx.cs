using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Courses : Page
    {
        private Pager _pager;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) LoadCourses();
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

        private void LoadCourses()
        {
            var p = PagerState;
            DataTable list = CourseService.AdminSearch(tbSearch.Text.Trim(),
                ddlStatus.SelectedValue, ddlLevel.SelectedValue,
                p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            CourseService.GetCounts(out int totalAll, out int pubAll, out int draftAll);
            litTotal.Text = totalAll.ToString();
            litPublished.Text = pubAll.ToString();
            litDraft.Text = draftAll.ToString();

            rptCourses.DataSource = list;
            rptCourses.DataBind();
            pnlEmpty.Visible = list.Rows.Count == 0;
            UpdatePagerUI();
        }

        protected void Search_Changed(object sender, EventArgs e)
        {
            PagerState.Page = 1;
            LoadCourses();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            PagerState.Page--;
            LoadCourses();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            PagerState.Page++;
            LoadCourses();
        }

        protected void rptCourses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();
            string adminId = Session["UserID"].ToString();
            switch (e.CommandName)
            {
                case "Edit":
                    Response.Redirect($"~/Admin/EditCourse.aspx?id={id}");
                    break;
                case "TogglePublish":
                    try
                    {
                        CourseService.TogglePublish(id);
                        AdminService.LogAudit(adminId, "TOGGLE_COURSE_PUBLISH", "Courses", id, "", "");
                        pnlSuccess.Visible = true;
                        litSuccess.Text = "Course status updated.";
                        LoadCourses();
                    }
                    catch (Exception)
                    {
                        pnlError.Visible = true;
                        litError.Text = "Error updating course status.";
                    }
                    break;
                case "Delete":
                    try
                    {
                        CourseService.Delete(id);
                        AdminService.LogAudit(adminId, "DELETE_COURSE", "Courses", id, "", "");
                        pnlSuccess.Visible = true;
                        litSuccess.Text = "Course deleted.";
                        LoadCourses();
                    }
                    catch (Exception)
                    {
                        pnlError.Visible = true;
                        litError.Text = "Cannot delete this course. It may have existing chapters, enrollments, labs, or quizzes. Remove those first.";
                    }
                    break;
            }
        }

        public string GetLevelBadge(string l) =>
            l == "Beginner" ? "badge-blue" : l == "Intermediate" ? "badge-amber" : "badge-red";

        public string GetStatusBadge(string s) =>
            s == "Published" ? "badge-green" : "badge-amber";

        protected void lbExport_Click(object sender, EventArgs e)
        {
            string csv = CourseService.ExportCsv(tbSearch.Text.Trim(),
                ddlStatus.SelectedValue, ddlLevel.SelectedValue);
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=courses.csv");
            Response.Write(csv);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }
}
