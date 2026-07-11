<%@ Page Title="Preview Content – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="PreviewContent.aspx.cs" Inherits="CSA.Admin.PreviewContent" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx" class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx" class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx" class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="Categories.aspx" class="sidebar-link"><i class="ti ti-category"></i>Categories</a>
        <a href="ContentReview.aspx" class="sidebar-link active"><i class="ti ti-file-check"></i>Content Review</a>
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
            <h2>Content Flag Details</h2>
            <p><a href="ContentReview.aspx" style="color:var(--accent2)">&larr; Back to Content Review</a></p>
        </div>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="validation-summary-errors mb-16"><asp:Literal ID="litError" runat="server" /></div>
        </asp:Panel>

        <div class="card">
            <div class="form-group">
                <label class="form-label">Flag ID</label>
                <asp:Literal ID="litFlagID" runat="server" />
            </div>
            <div class="form-group">
                <label class="form-label">Content Type</label>
                <asp:Literal ID="litContentType" runat="server" />
            </div>
            <div class="form-group">
                <label class="form-label">Title</label>
                <asp:Literal ID="litTitle" runat="server" />
            </div>
            <div class="form-group">
                <label class="form-label">Reason</label>
                <asp:Literal ID="litReason" runat="server" />
            </div>
            <div class="form-group">
                <label class="form-label">Reported By</label>
                <asp:Literal ID="litReportedBy" runat="server" />
            </div>
            <div class="form-group">
                <label class="form-label">Course</label>
                <asp:Literal ID="litCourse" runat="server" />
            </div>
            <div class="form-group">
                <label class="form-label">Preview</label>
                <div style="background:var(--bg2);padding:16px;border-radius:8px;white-space:pre-wrap"><asp:Literal ID="litPreview" runat="server" /></div>
            </div>
            <div class="form-group" style="display:flex;gap:8px">
                <asp:Button ID="btnApprove" runat="server" CssClass="btn-primary" OnClick="btnApprove_Click" Text="Approve (Remove Content)" />
                <asp:Button ID="btnReject" runat="server" CssClass="btn-sm" OnClick="btnReject_Click" Text="Dismiss Flag" />
            </div>
        </div>
    </main>
</div>
</asp:Content>
