<%@ Page Title="Edit User – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="EditUser.aspx.cs" Inherits="CSA.Admin.EditUser" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx" class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx" class="sidebar-link active"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx" class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="Categories.aspx" class="sidebar-link"><i class="ti ti-category"></i>Categories</a>
        <a href="ContentReview.aspx" class="sidebar-link"><i class="ti ti-file-check"></i>Content Review</a>
        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx" class="sidebar-link"><i class="ti ti-activity"></i>Activity Logs</a>
        <a href="ErrorLogs.aspx"      class="sidebar-link"><i class="ti ti-bug"></i>Error Logs</a>
        <a href="Announcements.aspx" class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx" class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <a href="SecurityAlerts.aspx" class="sidebar-link"><i class="ti ti-alert-triangle"></i>Security Alerts</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user-circle"></i>My Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Edit User</h2>
            <p>Update user details and account status.</p>
        </div>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="validation-summary-errors mb-16"><asp:Literal ID="litError" runat="server" /></div>
        </asp:Panel>
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16"><asp:Literal ID="litSuccess" runat="server" /></div>
        </asp:Panel>

        <asp:ValidationSummary ID="valSummary" runat="server"
            ValidationGroup="EditUser" CssClass="validation-summary-errors" HeaderText="Please fix:" DisplayMode="BulletList" />

        <div class="card" style="max-width:560px">
            <asp:HiddenField ID="hfUserID" runat="server" />

            <div class="form-group">
                <label class="form-label">Full Name</label>
                <asp:TextBox ID="tbFullName" runat="server" CssClass="form-input" MaxLength="150" />
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="tbFullName"
                    ValidationGroup="EditUser" Display="Dynamic" CssClass="val-error"
                    Text="<i class='ti ti-alert-circle'></i> Required." />
            </div>
            <div class="form-group">
                <label class="form-label">Email</label>
                <asp:TextBox ID="tbEmail" runat="server" CssClass="form-input" TextMode="Email" MaxLength="255" />
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="tbEmail"
                    ValidationGroup="EditUser" Display="Dynamic" CssClass="val-error"
                    Text="<i class='ti ti-alert-circle'></i> Required." />
            </div>
            <div class="form-group">
                <label class="form-label">Phone Number</label>
                <asp:TextBox ID="tbPhone" runat="server" CssClass="form-input" MaxLength="50" />
            </div>
            <div class="form-group">
                <label class="form-label">Department</label>
                <asp:TextBox ID="tbDepartment" runat="server" CssClass="form-input" MaxLength="100" />
            </div>
            <div class="form-group">
                <label class="form-label">Student/Staff ID <span class="text-muted">(optional)</span></label>
                <asp:TextBox ID="tbStudentID" runat="server" CssClass="form-input" MaxLength="20" />
            </div>
            <div class="form-group">
                <label class="form-label">Role</label>
                <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select" />
            </div>
            <div class="form-group">
                <label class="form-label">Account Status</label>
                <asp:CheckBox ID="cbActive" runat="server" Text=" Active" style="display:flex;align-items:center;gap:8px;font-size:13px;color:var(--text2)" />
            </div>
            <div class="form-group" style="border-top:1px solid var(--border);padding-top:16px;margin-top:8px">
                <label class="form-label" style="font-weight:600">Reset Password <span class="text-muted">(leave blank to keep current)</span></label>
                <div style="display:flex;gap:8px;align-items:center">
                    <asp:TextBox ID="tbNewPassword" runat="server" CssClass="form-input"
                        TextMode="Password" MaxLength="128" placeholder="New password"
                        style="flex:1;max-width:260px" />
                    <asp:Button ID="btnGeneratePw" runat="server" CssClass="btn-sm secondary"
                        Text="Generate" OnClick="btnGeneratePw_Click"
                        OnClientClick="return confirm('Generate a new random password?');"
                        style="white-space:nowrap" />
                </div>
                <asp:RegularExpressionValidator ID="revPassword" runat="server"
                    ControlToValidate="tbNewPassword" ValidationGroup="EditUser"
                    Display="Dynamic" CssClass="val-error"
                    ValidationExpression="^.{0,128}$"
                    Text="<i class='ti ti-alert-circle'></i> Max 128 characters."
                    style="margin-top:4px" />
                <asp:Literal ID="litGeneratedPw" runat="server" />
            </div>
            <div class="form-group">
                <asp:Button ID="btnSave" runat="server" CssClass="btn-primary"
                    ValidationGroup="EditUser" OnClick="btnSave_Click" Text="Save Changes" />
                <a href="Users.aspx" class="btn-sm secondary" style="margin-left:8px">Cancel</a>
            </div>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
</style>
</asp:Content>
