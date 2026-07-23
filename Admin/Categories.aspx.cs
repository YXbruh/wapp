using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Categories : System.Web.UI.Page
    {
        protected Literal litPageInfo;
        protected LinkButton btnPrev;
        protected LinkButton btnNext;

        private Pager _pager;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Admin")
            { Response.Redirect("~/Login.aspx"); return; }

            if (!IsPostBack) LoadCategories();
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

        private void LoadCategories()
        {
            var p = PagerState;
            var dt = CourseService.GetCategories(p.Page, p.PageSize, out int total);
            p.Total = total;
            ViewState["Pager"] = _pager;

            rptCategories.DataSource = dt;
            rptCategories.DataBind();
            pnlEmpty.Visible = dt.Rows.Count == 0;
            UpdatePagerUI();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            PagerState.Page--;
            LoadCategories();
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            PagerState.Page++;
            LoadCategories();
        }

        private void SetMessage(string msg, bool success)
        {
            if (success)
            {
                litSuccess.Text = msg;
                pnlSuccess.Visible = true;
                pnlError.Visible = false;
            }
            else
            {
                litError.Text = msg;
                pnlError.Visible = true;
                pnlSuccess.Visible = false;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                string editID = hfEditID.Value;
                bool isEdit = !string.IsNullOrEmpty(editID) && editID != "0";

                if (isEdit)
                {
                    CourseService.UpdateCategory(editID, tbName.Text.Trim(), tbDescription.Text.Trim());
                    AdminService.LogAudit(Session["UserID"].ToString(), "UPDATE_CATEGORY", "Categories", editID, "", tbName.Text.Trim());
                    SetMessage("Category updated.", true);
                }
                else
                {
                    CourseService.CreateCategory(tbName.Text.Trim(), tbDescription.Text.Trim());
                    AdminService.LogAudit(Session["UserID"].ToString(), "CREATE_CATEGORY", "Categories", "0", "", tbName.Text.Trim());
                    SetMessage("Category created.", true);
                }

                tbName.Text = "";
                tbDescription.Text = "";
                hfEditID.Value = "0";
                litFormTitle.Text = "New Category";
                lbCancelEdit.Visible = false;
                LoadCategories();
            }
            catch (Exception ex)
            {
                SetMessage("Error: " + ex.Message, false);
            }
        }

        protected void rptCategories_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();

            if (e.CommandName == "Edit")
            {
                var dt = CourseService.GetCategoryById(id);
                if (dt.Rows.Count > 0)
                {
                    tbName.Text = dt.Rows[0]["CategoryName"].ToString();
                    tbDescription.Text = dt.Rows[0]["Description"]?.ToString();
                    hfEditID.Value = id;
                    litFormTitle.Text = "Edit Category";
                    lbCancelEdit.Visible = true;
                }
            }
            else if (e.CommandName == "Delete")
            {
                try
                {
                    CourseService.DeleteCategory(id);
                    AdminService.LogAudit(Session["UserID"].ToString(), "DELETE_CATEGORY", "Categories", id, "", "");
                    SetMessage("Category deleted.", true);
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    SetMessage("Error: " + ex.Message, false);
                }
            }
        }

        protected void lbCancelEdit_Click(object sender, EventArgs e)
        {
            tbName.Text = "";
            tbDescription.Text = "";
            hfEditID.Value = "0";
            litFormTitle.Text = "New Category";
            lbCancelEdit.Visible = false;
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout");
        }
    }
}
