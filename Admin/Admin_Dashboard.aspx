<%@ Page Title="Admin Dashboard – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Admin_Dashboard.aspx.cs" Inherits="CSA.Admin.Admin_Dashboard" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <!-- ===== SIDEBAR ===== -->
    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx" class="sidebar-link active"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="Categories.aspx"     class="sidebar-link"><i class="ti ti-category"></i>Categories</a>
        <a href="ContentReview.aspx"  class="sidebar-link"><i class="ti ti-file-check"></i>Content Review</a>

        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx"   class="sidebar-link"><i class="ti ti-activity"></i>Activity Logs</a>
        <a href="ErrorLogs.aspx"      class="sidebar-link"><i class="ti ti-bug"></i>Error Logs</a>
        <a href="Announcements.aspx"  class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx"         class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <a href="SecurityAlerts.aspx" class="sidebar-link"><i class="ti ti-alert-triangle"></i>Security Alerts</a>

        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user-circle"></i>My Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
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

        <!-- Charts -->
        <div class="cards-row" style="gap:16px;margin-bottom:20px">
            <div class="card" style="flex:1">
                <div class="card-header">Users by Role</div>
                <asp:Repeater ID="rptUserChart" runat="server">
                    <ItemTemplate>
                        <div style="margin-bottom:8px">
                            <div style="display:flex;justify-content:space-between;font-size:12px;color:var(--text2);margin-bottom:2px">
                                <span><%# Eval("Label") %></span>
                                <span><%# Eval("Value") %></span>
                            </div>
                            <div class="chart-bar-bg">
                                <div class="chart-bar-fill" style="width:<%# Eval("Percent") %>%"></div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="card" style="flex:1">
                <div class="card-header">Courses by Level</div>
                <asp:Repeater ID="rptCourseChart" runat="server">
                    <ItemTemplate>
                        <div style="margin-bottom:8px">
                            <div style="display:flex;justify-content:space-between;font-size:12px;color:var(--text2);margin-bottom:2px">
                                <span><%# Eval("Label") %></span>
                                <span><%# Eval("Value") %></span>
                            </div>
                            <div class="chart-bar-bg">
                                <div class="chart-bar-fill" style="width:<%# Eval("Percent") %>%"></div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="card" style="flex:1">
                <div class="card-header">Labs</div>
                <asp:Repeater ID="rptLabChart" runat="server">
                    <ItemTemplate>
                        <div style="margin-bottom:8px">
                            <div style="display:flex;justify-content:space-between;font-size:12px;color:var(--text2);margin-bottom:2px">
                                <span><%# Eval("Label") %></span>
                                <span><%# Eval("Value") %></span>
                            </div>
                            <div class="chart-bar-bg">
                                <div class="chart-bar-fill" style="width:<%# Eval("Percent") %>%"></div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
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
                                    <td class="text-muted"><%# Eval("CreatedAt", "{0:dd MMM yyyy}") %></td>
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
                                                OnClientClick="return showDeleteConfirm(this);">
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
<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.chart-bar-bg{background:var(--bg2);border-radius:4px;height:8px;overflow:hidden}
.chart-bar-fill{background:var(--primary);height:100%;border-radius:4px;transition:width .3s ease;min-width:2px}
.cards-row{display:flex;gap:16px}
@media(max-width:768px){.cards-row{flex-direction:column}}
</style>
</asp:Content>