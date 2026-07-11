using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebGrease.Activities;

namespace CSA.Lecturer
{
    public partial class QuizEditor : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] as string != "Lecturer")
            { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) { LoadQuizDropdown(); LoadQuestions(); }
        }

        private void LoadQuizDropdown()
        {
            //string userId = Session["UserID"].ToString();                                                  //Bypass login for testing
            ddlQuiz.Items.Clear();
            ddlQuiz.Items.Add(new ListItem("� Select Quiz �", ""));
            // TODO: foreach (var q in QuizService.GetByInstructor(userId))
            //           ddlQuiz.Items.Add(new ListItem(q.QuizName, q.QuizID.ToString()));
        }

        protected void ddlQType_Changed(object sender, EventArgs e)
        {
            string t = ddlQType.SelectedValue;
            pnlMCQ.Visible = t == "MCQ";
            pnlStringMatch.Visible = t == "StringMatch";
            pnlTrueFalse.Visible = t == "TrueFalse";
        }

        private void LoadQuestions()
        {
            //string userId = Session["UserID"].ToString();                                              //Bypass login for testing
            // TODO:
            // var list = QuizService.GetQuestions(userId, tbSearch.Text.Trim(), ddlFilterType.SelectedValue);
            // litCount.Text          = list.Count.ToString();
            // rptQuestions.DataSource = list; rptQuestions.DataBind();
            // pnlEmpty.Visible = list.Count == 0;
            litCount.Text = "0";
            pnlEmpty.Visible = true;
        }

        protected void btnSaveQuestion_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // Validate question text: 10�1000 chars
            if (tbQuestion.Text.Trim().Length < 10)
            { pnlError.Visible = true; litError.Text = "Question must be at least 10 characters."; return; }

            string qType = ddlQType.SelectedValue;
            string qId = hfQuestionID.Value;
            //string userId = Session["UserID"].ToString();                                              //Bypass login for testing

            // Determine correct answer key
            string correctKey = "";
            if (qType == "MCQ")
            {
                if (rbA.Checked) correctKey = "A";
                else if (rbB.Checked) correctKey = "B";
                else if (rbC.Checked) correctKey = "C";
                else if (rbD.Checked) correctKey = "D";
                else { pnlError.Visible = true; litError.Text = "Please select the correct answer option."; return; }
            }
            else if (qType == "StringMatch")
            {
                correctKey = tbCorrectString.Text.Trim();
                if (string.IsNullOrEmpty(correctKey))
                { pnlError.Visible = true; litError.Text = "Please enter the correct answer string."; return; }
            }
            else if (qType == "TrueFalse")
            {
                if (rbTrue.Checked) correctKey = "True";
                else if (rbFalse.Checked) correctKey = "False";
                else { pnlError.Visible = true; litError.Text = "Please select True or False."; return; }
            }

            int points = 5;
            if (!string.IsNullOrWhiteSpace(tbPoints.Text)) int.TryParse(tbPoints.Text, out points);

            // TODO: QuizService.SaveQuestion(qId, userId,
            //   Convert.ToInt32(ddlQuiz.SelectedValue), qType,
            //   tbQuestion.Text.Trim(), tbOptA.Text.Trim(), tbOptB.Text.Trim(),
            //   tbOptC.Text.Trim(), tbOptD.Text.Trim(), correctKey,
            //   ddlMatchStrategy.SelectedValue, tbExplanation.Text.Trim(), points);

            pnlSuccess.Visible = true;
            litSuccess.Text = string.IsNullOrEmpty(qId) ? "Question saved to bank." : "Question updated.";
            pnlError.Visible = false;
            ResetForm();
            LoadQuestions();
        }

        protected void rptQuestions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string id = e.CommandArgument.ToString();
            if (e.CommandName == "Edit")
            {
                // TODO: var q = QuizService.GetQuestionById(id); populate fields
                hfQuestionID.Value = id;
                litFormTitle.Text = "Edit Question";
                lbCancel.Visible = true;
                btnSaveQuestion.Text = "Update Question";
            }
            else if (e.CommandName == "Delete")
            {
                // TODO: QuizService.DeleteQuestion(id);
                pnlSuccess.Visible = true; litSuccess.Text = "Question deleted.";
                LoadQuestions();
            }
        }

        protected void lbCancel_Click(object sender, EventArgs e) => ResetForm();
        protected void tbSearch_TextChanged(object sender, EventArgs e) => LoadQuestions();
        protected void ddlFilterType_Changed(object sender, EventArgs e) => LoadQuestions();

        private void ResetForm()
        {
            tbQuestion.Text = tbOptA.Text = tbOptB.Text = tbOptC.Text = tbOptD.Text = "";
            tbCorrectString.Text = tbExplanation.Text = tbPoints.Text = "";
            rbA.Checked = rbB.Checked = rbC.Checked = rbD.Checked = false;
            rbTrue.Checked = rbFalse.Checked = false;
            hfQuestionID.Value = "0";
            litFormTitle.Text = "New Question";
            lbCancel.Visible = false;
            btnSaveQuestion.Text = "Save Question";
        }

        public string GetTypeBadge(string t) =>
            t == "MCQ" ? "badge-blue" : t == "StringMatch" ? "badge-amber" : "badge-green";

        protected void lbLogout_Click(object sender, EventArgs e)
        { Session.Clear(); Session.Abandon(); Response.Redirect("~/Login.aspx?msg=loggedout"); }
    }

}