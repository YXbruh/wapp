<%@ Page Title="Activity Logs – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="ActivityLogs.aspx.cs" Inherits="CSA.Admin.Admin_ActivityLogs" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="ContentReview.aspx"  class="sidebar-link"><i class="ti ti-file-check"></i>Content Review</a>
        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx"   class="sidebar-link active"><i class="ti ti-activity"></i>Activity Logs</a>
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
            <h2>Activity Logs</h2>
            <p>Monitor all platform events filtered by severity.</p>
        </div>

        <!-- Severity summary -->
        <div class="metrics" style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Info</div>
                <div class="metric-val" style="color:var(--info)"><asp:Literal ID="litInfo" runat="server" Text="0"/></div>
                <div class="metric-sub">events today</div>
            </div>
            <div class="metric">
                <div class="metric-label">Warning</div>
                <div class="metric-val" style="color:var(--warning)"><asp:Literal ID="litWarning" runat="server" Text="0"/></div>
                <div class="metric-sub">events today</div>
            </div>
            <div class="metric">
                <div class="metric-label">Critical</div>
                <div class="metric-val" style="color:var(--danger)"><asp:Literal ID="litCritical" runat="server" Text="0"/></div>
                <div class="metric-sub">events today</div>
            </div>
        </div>

        <div class="card">
            <!-- Toolbar -->
            <div class="toolbar">
                <div class="search-wrap" style="flex:1">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                        placeholder="Search by user, action, or IP..."
                        AutoPostBack="true" OnTextChanged="Filter_Changed" />
                    <i class="ti ti-search" aria-hidden="true"></i>
                </div>

                <!-- Severity filter chips (PostBack via DropDownList) -->
                <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-select"
                    style="width:150px" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                    <asp:ListItem Value="">All Severity</asp:ListItem>
                    <asp:ListItem Value="Info">Info</asp:ListItem>
                    <asp:ListItem Value="Warning">Warning</asp:ListItem>
                    <asp:ListItem Value="Critical">Critical</asp:ListItem>
                </asp:DropDownList>

                <asp:TextBox ID="tbDateFrom" runat="server" CssClass="form-input"
                    TextMode="Date" style="width:140px"
                    AutoPostBack="true" OnTextChanged="Filter_Changed" />
                <asp:TextBox ID="tbDateTo" runat="server" CssClass="form-input"
                    TextMode="Date" style="width:140px"
                    AutoPostBack="true" OnTextChanged="Filter_Changed" />

                <asp:LinkButton ID="lbExport" runat="server" CssClass="btn-sm secondary"
                    OnClick="lbExport_Click">
                    <i class="ti ti-download" aria-hidden="true"></i>Export
                </asp:LinkButton>
            </div>

            <!-- Logs table -->
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">Severity</th>
                            <th scope="col">User</th>
                            <th scope="col">Action</th>
                            <th scope="col">Details</th>
                            <th scope="col">IP Address</th>
                            <th scope="col">Timestamp</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptLogs" runat="server">
                            <ItemTemplate>
                                <tr class="log-row-<%# Eval("Severity").ToString().ToLower() %>">
                                    <td>
                                        <span class="badge <%# GetSeverityBadge(Eval("Severity").ToString()) %>">
                                            <i class="ti <%# GetSeverityIcon(Eval("Severity").ToString()) %>"></i>
                                            <%# Eval("Severity") %>
                                        </span>
                                    </td>
                                    <td>
                                        <div style="color:var(--text);font-weight:600"><%# Eval("UserName") %></div>
                                        <div class="text-small text-muted"><%# Eval("UserEmail") %></div>
                                    </td>
                                    <td class="fw-bold" style="color:var(--text)"><%# Eval("Action") %></td>
                                    <td class="text-muted" style="max-width:220px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis"><%# Eval("Details") %></td>
                                    <td class="text-muted" style="font-family:monospace;font-size:12px"><%# Eval("IPAddress") %></td>
                                    <td class="text-muted text-small"><%# Eval("TimestampDisplay") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <p class="text-muted text-small mt-16" style="text-align:center;padding:20px 0">No log entries found.</p>
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
.log-row-critical td { border-left: 3px solid var(--danger); }
.log-row-warning  td { border-left: 3px solid var(--warning); }
.log-row-info     td { border-left: 3px solid var(--info); }
</style>
</asp:Content>
