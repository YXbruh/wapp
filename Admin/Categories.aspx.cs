using System;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Categories : System.Web.UI.Page
    {
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

            string editID = hfEditID.Value;
            bool isEdit = !string.IsNullOrEmpty(editID) && editID != "0";
            string name = tbName.Text.Trim();
            string description = tbDescription.Text.Trim();

            if (name.Length > 100)
            { SetMessage("Category name cannot exceed 100 characters.", false); return; }
            if (description.Length > 500)
            { SetMessage("Description cannot exceed 500 characters.", false); return; }
            if (CourseService.CategoryNameExists(name, isEdit ? editID : null))
            { SetMessage("A category with that name already exists.", false); return; }

            try
            {
                if (isEdit)
                {
                    CourseService.UpdateCategory(editID, name, description);
                    AdminService.LogAudit(Session["UserID"].ToString(), "UPDATE_CATEGORY", "Categories", editID, "", name);
                    SetMessage("Category updated.", true);
                }
                else
                {
                    // Log the generated id rather than a placeholder "0".
                    string newId = CourseService.CreateCategory(name, description);
                    AdminService.LogAudit(Session["UserID"].ToString(), "CREATE_CATEGORY", "Categories", newId, "", name);
                    SetMessage("Category created.", true);
                }

                tbName.Text = "";
                tbDescription.Text = "";
                hfEditID.Value = "0";
                litFormTitle.Text = "New Category";
                lbCancelEdit.Visible = false;
                LoadCategories();
            }
            catch (SqlException ex)
            {
                SetMessage(ex.Number == 2601 || ex.Number == 2627
                    ? "A category with that name already exists."
                    : "Could not save the category. Please try again.", false);
            }
            catch (Exception)
            {
                SetMessage("Could not save the category. Please try again.", false);
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
                catch (Exception)
                {
                    SetMessage("Cannot delete this category because courses are still assigned to it. Reassign those courses first.", false);
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
