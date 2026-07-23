<%@ Page Title="Error Logs – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ErrorLogs.aspx.cs" Inherits="CSA.Admin.ErrorLogs" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="Categories.aspx"     class="sidebar-link"><i class="ti ti-category"></i>Categories</a>
        <a href="ContentReview.aspx"  class="sidebar-link"><i class="ti ti-file-check"></i>Content Review</a>
        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx"   class="sidebar-link"><i class="ti ti-activity"></i>Activity Logs</a>
        <a href="ErrorLogs.aspx"      class="sidebar-link active"><i class="ti ti-bug"></i>Error Logs</a>
        <a href="Announcements.aspx"  class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx"         class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <a href="SecurityAlerts.aspx" class="sidebar-link"><i class="ti ti-alert-triangle"></i>Security Alerts</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user-circle"></i>My Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Error Logs</h2>
            <p>View and manage application-level errors and exceptions.</p>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litSuccess" runat="server" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="alert-danger mb-16">
                <i class="ti ti-alert-circle" aria-hidden="true"></i>
                <asp:Literal ID="litError" runat="server" />
            </div>
        </asp:Panel>

        <!-- Severity summary -->
        <div class="metrics" style="grid-template-columns:repeat(4,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Errors Today</div>
                <div class="metric-val" style="color:var(--danger)"><asp:Literal ID="litErrors" runat="server" Text="0"/></div>
                <div class="metric-sub">critical/error</div>
            </div>
            <div class="metric">
                <div class="metric-label">Warnings Today</div>
                <div class="metric-val" style="color:var(--warning)"><asp:Literal ID="litWarnings" runat="server" Text="0"/></div>
                <div class="metric-sub">warnings</div>
            </div>
            <div class="metric">
                <div class="metric-label">Info Today</div>
                <div class="metric-val" style="color:var(--info)"><asp:Literal ID="litInfo" runat="server" Text="0"/></div>
                <div class="metric-sub">info</div>
            </div>
            <div class="metric">
                <div class="metric-label">Unresolved</div>
                <div class="metric-val"><asp:Literal ID="litUnresolved" runat="server" Text="0"/></div>
                <div class="metric-sub">need attention</div>
            </div>
        </div>

        <div class="card">
            <!-- Toolbar -->
            <div class="toolbar">
                <div class="search-wrap" style="flex:1">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                        placeholder="Search error message, type, or URL..."
                        AutoPostBack="true" OnTextChanged="Filter_Changed" />
                    <i class="ti ti-search" aria-hidden="true"></i>
                </div>

                <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-select"
                    style="width:150px" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                    <asp:ListItem Value="">All Severity</asp:ListItem>
                    <asp:ListItem Value="Info">Info</asp:ListItem>
                    <asp:ListItem Value="Warning">Warning</asp:ListItem>
                    <asp:ListItem Value="Error">Error</asp:ListItem>
                    <asp:ListItem Value="Critical">Critical</asp:ListItem>
                </asp:DropDownList>

                <asp:TextBox ID="tbDateFrom" runat="server" CssClass="form-input"
                    TextMode="Date" style="width:140px"
                    AutoPostBack="true" OnTextChanged="Filter_Changed" />
                <asp:TextBox ID="tbDateTo" runat="server" CssClass="form-input"
                    TextMode="Date" style="width:140px"
                    AutoPostBack="true" OnTextChanged="Filter_Changed" />
            </div>

            <!-- Logs table -->
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">Severity</th>
                            <th scope="col">Error Type</th>
                            <th scope="col">Message</th>
                            <th scope="col">User</th>
                            <th scope="col">URL</th>
                            <th scope="col">Timestamp</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptErrors" runat="server"
                                      OnItemCommand="rptErrors_ItemCommand">
                            <ItemTemplate>
                                <tr class="<%# GetRowClass(Eval("Severity").ToString()) %>">
                                    <td>
                                        <span class="badge <%# GetSeverityBadge(Eval("Severity").ToString()) %>">
                                            <i class="ti <%# GetSeverityIcon(Eval("Severity").ToString()) %>"></i>
                                            <%# Eval("Severity") %>
                                        </span>
                                    </td>
                                    <td class="fw-bold" style="color:var(--text)"><%# Eval("ErrorType") %></td>
                                    <td class="text-muted" style="max-width:280px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis" title="<%# Eval("Message") %>">
                                        <%# Eval("MessagePreview") %>
                                    </td>
                                    <td class="text-muted"><%# Eval("UserName") %></td>
                                    <td class="text-muted" style="max-width:150px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis"><%# Eval("PageURL") %></td>
                                    <td class="text-muted text-small"><%# Eval("OccurredAt", "{0:dd MMM yyyy HH:mm}") %></td>
                                    <td>
                                        <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                            CommandName="Resolve" CommandArgument='<%# Eval("ErrorID") %>'
                                            Visible='<%# !Convert.ToBoolean(Eval("IsResolved")) %>'
                                            OnClientClick="return showConfirmAction(this, 'Mark this error as resolved?', 'Resolve');">
                                            <i class="ti ti-circle-check"></i> Resolve
                                        </asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="btn-sm"
                                            style="background:#1e7e34; border-color:#1e7e34; color:#fff; pointer-events:none; opacity:0.85;"
                                            Enabled="false"
                                            Visible='<%# Convert.ToBoolean(Eval("IsResolved")) %>'>
                                            <i class="ti ti-circle-check"></i> Resolved
                                        </asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <p class="text-muted text-small mt-16" style="text-align:center;padding:20px 0">No error logs found.</p>
            </asp:Panel>

            <!-- Pagination -->
            <div style="display:flex;justify-content:space-between;align-items:center;margin-top:16px;font-size:12px;color:var(--text3)">
                <span>Showing <asp:Literal ID="litShowing" runat="server" Text="0" /> entries</span>
                <div style="display:flex;gap:6px">
                    <asp:LinkButton ID="lbPrev" runat="server" CssClass="btn-sm secondary" OnClick="lbPrev_Click">
                        <i class="ti ti-chevron-left"></i> Prev
                    </asp:LinkButton>
                    <asp:LinkButton ID="lbNext" runat="server" CssClass="btn-sm secondary" OnClick="lbNext_Click">
                        Next <i class="ti ti-chevron-right"></i>
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
.error-row-critical td{border-left:3px solid var(--danger)}
.error-row-error td{border-left:3px solid var(--danger)}
.error-row-warning td{border-left:3px solid var(--warning)}
.error-row-info td{border-left:3px solid var(--info)}
</style>
</asp:Content>