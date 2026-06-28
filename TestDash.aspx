<%@ Page Title="Admin Dashboard – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="TestDash.aspx.cs" Inherits="CSA.TestDash" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <!-- ===== SIDEBAR ===== -->
    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link active"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users
            <span class="sidebar-badge"><asp:Literal ID="litPendingUsers" runat="server" Text="0" /></span>
        </a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="ContentReview.aspx"  class="sidebar-link"><i class="ti ti-file-check"></i>Content Review</a>

        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx"   class="sidebar-link"><i class="ti ti-activity"></i>Activity Logs</a>
        <a href="Announcements.aspx"  class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx"         class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <a href="SecurityAlerts.aspx" class="sidebar-link"><i class="ti ti-alert-triangle"></i>Security Alerts
            <span class="sidebar-badge"><asp:Literal ID="litAlertCount" runat="server" Text="0" /></span>
        </a>

        <div class="sidebar-section">Account</div>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link"
                        OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <!-- ===== MAIN ===== -->
    <main class="dash-content">

        <div class="dash-header">
            <h2>Admin Dashboard</h2>
            <p>Platform overview &middot; Last updated: <asp:Literal ID="litLastUpdated" runat="server" /></p>
        </div>

        <!-- Metrics -->
        <div class="metrics" role="region" aria-label="Platform stats">
            <div class="metric">
                <div class="metric-label">Total Users</div>
                <div class="metric-val"><asp:Literal ID="litTotalUsers" runat="server" Text="0" /></div>
                <div class="metric-sub">registered accounts</div>
            </div>
            <div class="metric">
                <div class="metric-label">Active Courses</div>
                <div class="metric-val"><asp:Literal ID="litActiveCourses" runat="server" Text="0" /></div>
                <div class="metric-sub">published</div>
            </div>
            <div class="metric">
                <div class="metric-label">Labs Online</div>
                <div class="metric-val"><asp:Literal ID="litLabsOnline" runat="server" Text="0" /></div>
                <div class="metric-sub">available</div>
            </div>
            <div class="metric">
                <div class="metric-label">Alerts</div>
                <div class="metric-val" style="color:var(--danger)">
                    <asp:Literal ID="litAlerts" runat="server" Text="0" />
                </div>
                <div class="metric-sub" style="color:var(--danger)">
                    <asp:Literal ID="litAlertStatus" runat="server" Text="All clear" />
                </div>
            </div>
        </div>

        <!-- User Management card -->
        <div class="card mb-24">
            <div class="card-header">User Management</div>

            <!-- Toolbar: search + buttons -->
            <div class="toolbar">
                <div class="search-wrap">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                        placeholder="Search users by name or email..." AutoPostBack="true"
                        OnTextChanged="tbSearch_TextChanged" />
                    <i class="ti ti-search" aria-hidden="true"></i>
                </div>
                <a href="CreateUser.aspx" class="btn-sm">
                    <i class="ti ti-user-plus" aria-hidden="true"></i>New User
                </a>
                <asp:LinkButton ID="lbExport" runat="server" CssClass="btn-sm secondary"
                                OnClick="lbExport_Click">
                    <i class="ti ti-download" aria-hidden="true"></i>Export
                </asp:LinkButton>
            </div>

            <!-- User table -->
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">User</th>
                            <th scope="col">Role</th>
                            <th scope="col">Status</th>
                            <th scope="col">Enrolled</th>
                            <th scope="col">Joined</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptUsers" runat="server"
                                      OnItemCommand="rptUsers_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="fw-bold" style="color:var(--text)"><%# Eval("FullName") %></div>
                                        <div class="text-small text-muted"><%# Eval("Email") %></div>
                                    </td>
                                    <td>
                                        <span class="badge <%# GetRoleBadge(Eval("Role").ToString()) %>">
                                            <%# Eval("Role") %>
                                        </span>
                                    </td>
                                    <td>
                                        <span class="badge <%# GetStatusBadge(Eval("IsActive").ToString()) %>">
                                            <%# Eval("IsActive").ToString() == "True" ? "Active" : "Inactive" %>
                                        </span>
                                    </td>
                                    <td class="text-muted"><%# Eval("EnrolledCount") %> courses</td>
                                    <td class="text-muted"><%# Eval("JoinedDisplay") %></td>
                                    <td>
                                        <div class="action-btns">
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="Edit"
                                                CommandArgument='<%# Eval("UserID") %>'>
                                                <i class="ti ti-edit"></i> Edit
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-danger"
                                                CommandName="Delete"
                                                CommandArgument='<%# Eval("UserID") %>'
                                                OnClientClick="return confirm('Delete this user?');">
                                                <i class="ti ti-trash"></i> Del
                                            </asp:LinkButton>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>

            <asp:Panel ID="pnlNoUsers" runat="server" Visible="false">
                <p class="text-muted text-small mt-16">No users found.</p>
            </asp:Panel>
        </div>

    </main>
</div>
</asp:Content>