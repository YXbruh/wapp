<%@ Page Title="Create User – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="CreateUser.aspx.cs" Inherits="CSA.Admin.CreateUser" %>

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
        <a href="Announcements.aspx" class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx" class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user-circle"></i>My Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Create New User</h2>
            <p>Add a new student, instructor, or administrator account.</p>
        </div>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="validation-summary-errors mb-16"><asp:Literal ID="litError" runat="server" /></div>
        </asp:Panel>
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16"><asp:Literal ID="litSuccess" runat="server" /></div>
        </asp:Panel>

        <asp:ValidationSummary ID="valSummary" runat="server"
            ValidationGroup="CreateUser" CssClass="validation-summary-errors" HeaderText="Please fix:" DisplayMode="BulletList" />

        <div class="card" style="max-width:560px">
            <div class="form-group">
                <label class="form-label">Full Name</label>
                <asp:TextBox ID="tbFullName" runat="server" CssClass="form-input" MaxLength="150" />
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="tbFullName"
                    ValidationGroup="CreateUser" Display="Dynamic" CssClass="val-error"
                    Text="<i class='ti ti-alert-circle'></i> Required." />
            </div>
            <div class="form-group">
                <label class="form-label">Email</label>
                <asp:TextBox ID="tbEmail" runat="server" CssClass="form-input" TextMode="Email" MaxLength="255" />
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="tbEmail"
                    ValidationGroup="CreateUser" Display="Dynamic" CssClass="val-error"
                    Text="<i class='ti ti-alert-circle'></i> Required." />
                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="tbEmail"
                    ValidationGroup="CreateUser" Display="Dynamic" CssClass="val-error"
                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                    Text="<i class='ti ti-alert-circle'></i> Invalid email." />
            </div>
            <div class="form-group">
                <label class="form-label">Password</label>
                <asp:TextBox ID="tbPassword" runat="server" CssClass="form-input" TextMode="Password" MaxLength="100" />
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="tbPassword"
                    ValidationGroup="CreateUser" Display="Dynamic" CssClass="val-error"
                    Text="<i class='ti ti-alert-circle'></i> Required." />
                <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="tbPassword"
                    ValidationGroup="CreateUser" Display="Dynamic" CssClass="val-error"
                    ValidationExpression="^(?=.*[A-Z])(?=.*\d).{8,}$"
                    Text="<i class='ti ti-alert-circle'></i> Min 8 chars, 1 uppercase, 1 number." />
            </div>
            <div class="form-group">
                <label class="form-label">Role</label>
                <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select" />
            </div>
            <div class="form-group">
                <label class="form-label">Phone Number <span class="text-muted">(optional)</span></label>
                <asp:TextBox ID="tbPhone" runat="server" CssClass="form-input" MaxLength="50" />
            </div>
            <div class="form-group">
                <label class="form-label">Department <span class="text-muted">(optional)</span></label>
                <asp:TextBox ID="tbDepartment" runat="server" CssClass="form-input" MaxLength="100" />
            </div>
            <div class="form-group">
                <label class="form-label">Student/Staff ID <span class="text-muted">(optional)</span></label>
                <asp:TextBox ID="tbStudentID" runat="server" CssClass="form-input" MaxLength="20" />
            </div>
            <div class="form-group">
                <asp:Button ID="btnCreate" runat="server" CssClass="btn-primary"
                    ValidationGroup="CreateUser" OnClick="btnCreate_Click" Text="Create User" />
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
