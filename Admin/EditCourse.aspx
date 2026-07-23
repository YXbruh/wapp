<%@ Page Title="Edit Course – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="EditCourse.aspx.cs" Inherits="CSA.Admin.EditCourse" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx" class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx" class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx" class="sidebar-link active"><i class="ti ti-books"></i>Courses</a>
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
            <h2>Edit Course</h2>
            <p>Modify course details.</p>
        </div>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="validation-summary-errors mb-16"><asp:Literal ID="litError" runat="server" /></div>
        </asp:Panel>
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16"><asp:Literal ID="litSuccess" runat="server" /></div>
        </asp:Panel>

        <div class="card" style="max-width:640px">
            <div class="form-group">
                <label class="form-label">Course Name</label>
                <asp:TextBox ID="tbName" runat="server" CssClass="form-input" MaxLength="200" />
            </div>
            <div class="form-group">
                <label class="form-label">Description</label>
                <asp:TextBox ID="tbDescription" runat="server" CssClass="form-input" TextMode="MultiLine" Rows="4" MaxLength="2000" />
            </div>
            <div class="form-group">
                <label class="form-label">Category</label>
                <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
            </div>
            <div class="form-group">
                <label class="form-label">Instructor</label>
                <asp:DropDownList ID="ddlInstructor" runat="server" CssClass="form-select" />
            </div>
            <div class="form-group">
                <label class="form-label">Level</label>
                <asp:DropDownList ID="ddlLevel" runat="server" CssClass="form-select">
                    <asp:ListItem Value="Beginner">Beginner</asp:ListItem>
                    <asp:ListItem Value="Intermediate">Intermediate</asp:ListItem>
                    <asp:ListItem Value="Advanced">Advanced</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="form-group">
                <label class="form-label">Duration (hours)</label>
                <asp:TextBox ID="tbDuration" runat="server" CssClass="form-input" TextMode="Number" />
            </div>
            <div class="form-group">
                <asp:Button ID="btnSave" runat="server" CssClass="btn-primary" OnClick="btnSave_Click" Text="Save Changes" />
                <a href="Courses.aspx" class="btn-sm secondary" style="margin-left:8px">Cancel</a>
            </div>
        </div>
    </main>
</div>
</asp:Content>
