using System;
using System.Web.UI.WebControls;
using CSA.Services;

namespace CSA.Admin
{
    public partial class Categories : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                Response.Redirect("~/Login.aspx");

            if (!IsPostBack)
                LoadCategories();
        }

        private void LoadCategories()
        {
            var dt = CourseService.GetCategories();
            rptCategories.DataSource = dt;
            rptCategories.DataBind();
            pnlEmpty.Visible = dt.Rows.Count == 0;
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
                    SetMessage("Category updated.", true);
                }
                else
                {
                    CourseService.CreateCategory(tbName.Text.Trim(), tbDescription.Text.Trim());
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
