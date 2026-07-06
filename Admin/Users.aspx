<%@ Page Title="Manage Users – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Users.aspx.cs" Inherits="CSA.Admin.Users" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link active"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="ContentReview.aspx"  class="sidebar-link"><i class="ti ti-file-check"></i>Content Review</a>
        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx"   class="sidebar-link"><i class="ti ti-activity"></i>Activity Logs</a>
        <a href="Announcements.aspx"  class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx"         class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <a href="SecurityAlerts.aspx" class="sidebar-link"><i class="ti ti-alert-triangle"></i>Security Alerts</a>
        <div class="sidebar-section">Account</div>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Manage Users</h2>
            <p>Search, filter, and manage all registered accounts.</p>
        </div>

        <!-- Success / Error -->
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litSuccess" runat="server" />
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="validation-summary-errors mb-16">
                <asp:Literal ID="litError" runat="server" />
            </div>
        </asp:Panel>

        <!-- Metrics -->
        <div class="metrics" style="grid-template-columns:repeat(4,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Total Users</div>
                <div class="metric-val"><asp:Literal ID="litTotal" runat="server" Text="0"/></div>
                <div class="metric-sub">registered</div>
            </div>
            <div class="metric">
                <div class="metric-label">Students</div>
                <div class="metric-val"><asp:Literal ID="litStudents" runat="server" Text="0"/></div>
                <div class="metric-sub">accounts</div>
            </div>
            <div class="metric">
                <div class="metric-label">Instructors</div>
                <div class="metric-val"><asp:Literal ID="litInstructors" runat="server" Text="0"/></div>
                <div class="metric-sub">accounts</div>
            </div>
            <div class="metric">
                <div class="metric-label">Active Today</div>
                <div class="metric-val" style="color:var(--success)">
                    <asp:Literal ID="litActiveToday" runat="server" Text="0"/>
                </div>
                <div class="metric-sub">logged in</div>
            </div>
        </div>

        <div class="card">
            <!-- Toolbar: search + role filter + status filter + new user btn -->
            <div class="toolbar" style="flex-wrap:wrap;gap:10px">
                <div class="search-wrap" style="min-width:200px;flex:1">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                        placeholder="Search by name or email..."
                        AutoPostBack="true" OnTextChanged="Search_Changed" />
                    <i class="ti ti-search" aria-hidden="true"></i>
                </div>

                <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select"
                    style="width:140px" AutoPostBack="true" OnSelectedIndexChanged="Search_Changed">
                    <asp:ListItem Value="">All Roles</asp:ListItem>
                    <asp:ListItem Value="Student">Student</asp:ListItem>
                    <asp:ListItem Value="Instructor">Instructor</asp:ListItem>
                    <asp:ListItem Value="Admin">Admin</asp:ListItem>
                </asp:DropDownList>

                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"
                    style="width:140px" AutoPostBack="true" OnSelectedIndexChanged="Search_Changed">
                    <asp:ListItem Value="">All Status</asp:ListItem>
                    <asp:ListItem Value="1">Active</asp:ListItem>
                    <asp:ListItem Value="0">Inactive</asp:ListItem>
                </asp:DropDownList>

                <a href="CreateUser.aspx" class="btn-sm">
                    <i class="ti ti-user-plus" aria-hidden="true"></i>New User
                </a>
                <asp:LinkButton ID="lbExport" runat="server" CssClass="btn-sm secondary"
                    OnClick="lbExport_Click">
                    <i class="ti ti-download" aria-hidden="true"></i>Export CSV
                </asp:LinkButton>
            </div>

            <!-- Users table -->
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">User</th>
                            <th scope="col">Role</th>
                            <th scope="col">Status</th>
                            <th scope="col">Enrolled</th>
                            <th scope="col">Last Login</th>
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
                                    <td><span class="badge <%# GetRoleBadge(Eval("Role").ToString()) %>"><%# Eval("Role") %></span></td>
                                    <td><span class="badge <%# (bool)Eval("IsActive") ? "badge-green" : "badge-amber" %>"><%# (bool)Eval("IsActive") ? "Active" : "Inactive" %></span></td>
                                    <td class="text-muted"><%# Eval("EnrolledCount") %></td>
                                    <td class="text-muted"><%# Eval("LastLoginDisplay") %></td>
                                    <td class="text-muted"><%# Eval("JoinedDisplay") %></td>
                                    <td>
                                        <div class="action-btns">
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="Edit" CommandArgument='<%# Eval("UserID") %>'>
                                                <i class="ti ti-edit"></i> Edit
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="ToggleStatus" CommandArgument='<%# Eval("UserID") %>'>
                                                <i class="ti <%# (bool)Eval("IsActive") ? "ti-user-off" : "ti-user-check" %>"></i>
                                                <%# (bool)Eval("IsActive") ? "Deactivate" : "Activate" %>
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-danger"
                                                CommandName="Delete" CommandArgument='<%# Eval("UserID") %>'
                                                OnClientClick="return confirm('Permanently delete this user?');">
                                                <i class="ti ti-trash"></i>
                                            </asp:LinkButton>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <p class="text-muted text-small mt-16" style="text-align:center;padding:20px 0">
                    No users found matching your search.
                </p>
            </asp:Panel>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
</style>
</asp:Content>
