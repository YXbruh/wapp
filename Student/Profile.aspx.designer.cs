namespace CSA.Student
{
    public partial class Student_Profile
    {
        protected global::System.Web.UI.WebControls.LinkButton lbLogout;
        protected global::System.Web.UI.WebControls.Panel pnlSuccess;
        protected global::System.Web.UI.WebControls.Literal litSuccess;
        protected global::System.Web.UI.WebControls.Panel pnlError;
        protected global::System.Web.UI.WebControls.Literal litError;
        protected global::System.Web.UI.WebControls.Literal litAvatarInitials;
        protected global::System.Web.UI.WebControls.Literal litDisplayName;
        protected global::System.Web.UI.WebControls.Literal litJoined;
        protected global::System.Web.UI.WebControls.ValidationSummary valSummaryInfo;
        protected global::System.Web.UI.WebControls.TextBox tbFullName;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvName;
        protected global::System.Web.UI.WebControls.TextBox tbEmail;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvEmail;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator revEmail;
        protected global::System.Web.UI.WebControls.TextBox tbBio;
        protected global::System.Web.UI.WebControls.Button btnSaveInfo;
        protected global::System.Web.UI.WebControls.ValidationSummary valSummaryPwd;
        protected global::System.Web.UI.WebControls.TextBox tbCurrentPwd;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvCurrent;
        protected global::System.Web.UI.WebControls.TextBox tbNewPwd;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvNew;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator revNew;
        protected global::System.Web.UI.WebControls.TextBox tbConfirmPwd;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvConfirm;
        protected global::System.Web.UI.WebControls.CompareValidator cvPwd;
        protected global::System.Web.UI.WebControls.Button btnChangePwd;
    }
}