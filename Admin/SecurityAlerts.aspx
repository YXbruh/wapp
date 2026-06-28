<%@ Page Title="Security Alerts – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="SecurityAlerts.aspx.cs" Inherits="CSA.Admin.SecurityAlerts" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="ContentReview.aspx"  class="sidebar-link"><i class="ti ti-file-check"></i>Content Review</a>
        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx"   class="sidebar-link"><i class="ti ti-activity"></i>Activity Logs</a>
        <a href="Announcements.aspx"  class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx"         class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <a href="SecurityAlerts.aspx" class="sidebar-link active"><i class="ti ti-alert-triangle"></i>Security Alerts</a>
        <div class="sidebar-section">Account</div>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Security Alerts</h2>
            <p>Monitor and investigate suspicious activity on the platform.</p>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litSuccess" runat="server" />
            </div>
        </asp:Panel>

        <!-- Alert summary -->
        <div class="metrics" style="grid-template-columns:repeat(4,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Open Alerts</div>
                <div class="metric-val" style="color:var(--danger)">
                    <asp:Literal ID="litOpen" runat="server" Text="0"/>
                </div>
                <div class="metric-sub">need attention</div>
            </div>
            <div class="metric">
                <div class="metric-label">High Severity</div>
                <div class="metric-val" style="color:var(--danger)">
                    <asp:Literal ID="litHigh" runat="server" Text="0"/>
                </div>
                <div class="metric-sub">critical</div>
            </div>
            <div class="metric">
                <div class="metric-label">Investigating</div>
                <div class="metric-val" style="color:var(--warning)">
                    <asp:Literal ID="litInvestigating" runat="server" Text="0"/>
                </div>
                <div class="metric-sub">in progress</div>
            </div>
            <div class="metric">
                <div class="metric-label">Resolved Today</div>
                <div class="metric-val" style="color:var(--success)">
                    <asp:Literal ID="litResolved" runat="server" Text="0"/>
                </div>
                <div class="metric-sub">closed</div>
            </div>
        </div>

        <div class="card">
            <!-- Toolbar -->
            <div class="toolbar">
                <div class="search-wrap" style="flex:1">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                        placeholder="Search alerts..."
                        AutoPostBack="true" OnTextChanged="Filter_Changed" />
                    <i class="ti ti-search" aria-hidden="true"></i>
                </div>
                <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-select"
                    style="width:150px" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                    <asp:ListItem Value="">All Severity</asp:ListItem>
                    <asp:ListItem Value="High">High</asp:ListItem>
                    <asp:ListItem Value="Medium">Medium</asp:ListItem>
                    <asp:ListItem Value="Low">Low</asp:ListItem>
                </asp:DropDownList>
                <asp:DropDownList ID="ddlAlertStatus" runat="server" CssClass="form-select"
                    style="width:160px" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                    <asp:ListItem Value="">All Status</asp:ListItem>
                    <asp:ListItem Value="Open">Open</asp:ListItem>
                    <asp:ListItem Value="Investigating">Investigating</asp:ListItem>
                    <asp:ListItem Value="Resolved">Resolved</asp:ListItem>
                </asp:DropDownList>
            </div>

            <!-- Alerts table -->
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">Alert</th>
                            <th scope="col">Severity</th>
                            <th scope="col">Affected User</th>
                            <th scope="col">IP Address</th>
                            <th scope="col">Status</th>
                            <th scope="col">Detected</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptAlerts" runat="server"
                                      OnItemCommand="rptAlerts_ItemCommand">
                            <ItemTemplate>
                                <tr class="alert-row-<%# Eval("Severity").ToString().ToLower() %>">
                                    <td>
                                        <div class="fw-bold" style="color:var(--text)"><%# Eval("AlertType") %></div>
                                        <div class="text-small text-muted"><%# Eval("Description") %></div>
                                    </td>
                                    <td><span class="badge <%# GetSeverityBadge(Eval("Severity").ToString()) %>"><%# Eval("Severity") %></span></td>
                                    <td>
                                        <div style="color:var(--text)"><%# Eval("AffectedUser") %></div>
                                        <div class="text-small text-muted"><%# Eval("AffectedEmail") %></div>
                                    </td>
                                    <td class="text-muted" style="font-family:monospace;font-size:12px"><%# Eval("IPAddress") %></td>
                                    <td><span class="badge <%# GetAlertStatusBadge(Eval("AlertStatus").ToString()) %>"><%# Eval("AlertStatus") %></span></td>
                                    <td class="text-muted text-small"><%# Eval("DetectedDisplay") %></td>
                                    <td>
                                        <div class="action-btns">
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="Investigate" CommandArgument='<%# Eval("AlertID") %>'
                                                Visible='<%# Eval("AlertStatus").ToString() == "Open" %>'>
                                                <i class="ti ti-search"></i> Investigate
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-sm"
                                                CommandName="Resolve" CommandArgument='<%# Eval("AlertID") %>'
                                                Visible='<%# Eval("AlertStatus").ToString() != "Resolved" %>'
                                                OnClientClick="return confirm('Mark this alert as resolved?');">
                                                <i class="ti ti-circle-check"></i> Resolve
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="BlockUser" CommandArgument='<%# Eval("AlertID") %>'
                                                OnClientClick="return confirm('Block the affected user account?');">
                                                <i class="ti ti-user-off"></i> Block
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
                <div style="text-align:center;padding:40px 20px">
                    <i class="ti ti-shield-check" style="font-size:40px;color:var(--success)" aria-hidden="true"></i>
                    <p class="text-muted mt-16">No security alerts — platform is secure.</p>
                </div>
            </asp:Panel>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
.alert-row-high   td{border-left:3px solid var(--danger)}
.alert-row-medium td{border-left:3px solid var(--warning)}
.alert-row-low    td{border-left:3px solid var(--info)}
</style>
</asp:Content>
