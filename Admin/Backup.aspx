<%@ Page Title="Database Backup – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Backup.aspx.cs" Inherits="CSA.Admin.Backup" %>

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
        <a href="Backup.aspx"         class="sidebar-link active"><i class="ti ti-database"></i>DB Backup</a>
        <a href="SecurityAlerts.aspx" class="sidebar-link"><i class="ti ti-alert-triangle"></i>Security Alerts</a>
        <div class="sidebar-section">Account</div>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Database Backup</h2>
            <p>Create and manage database backups to protect platform data.</p>
        </div>

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

        <!-- DB status + last backup -->
        <div class="metrics" style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">DB Status</div>
                <div class="metric-val" style="color:var(--success);font-size:16px">
                    <i class="ti ti-circle-filled" style="font-size:12px" aria-hidden="true"></i>
                    <asp:Literal ID="litDbStatus" runat="server" Text="Online" />
                </div>
                <div class="metric-sub">connection OK</div>
            </div>
            <div class="metric">
                <div class="metric-label">Last Backup</div>
                <div class="metric-val" style="font-size:16px">
                    <asp:Literal ID="litLastBackup" runat="server" Text="—" />
                </div>
                <div class="metric-sub">most recent</div>
            </div>
            <div class="metric">
                <div class="metric-label">Total Backups</div>
                <div class="metric-val"><asp:Literal ID="litBackupCount" runat="server" Text="0" /></div>
                <div class="metric-sub">stored</div>
            </div>
        </div>

        <div class="cards-row" style="align-items:start">

            <!-- Create backup card -->
            <div class="card">
                <div class="card-header">Create New Backup</div>

                <div class="form-group">
                    <label class="form-label">
                        <i class="ti ti-tag" aria-hidden="true"></i>Backup Label
                        <span class="text-muted" style="font-weight:400">(optional)</span>
                    </label>
                    <asp:TextBox ID="tbLabel" runat="server" CssClass="form-input"
                        placeholder="e.g. Pre-deployment backup" MaxLength="100" />
                </div>

                <div class="form-group">
                    <label class="form-label">
                        <i class="ti ti-list" aria-hidden="true"></i>Backup Type
                    </label>
                    <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Full">Full Backup</asp:ListItem>
                        <asp:ListItem Value="Schema">Schema Only</asp:ListItem>
                        <asp:ListItem Value="Data">Data Only</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div style="background:var(--bg2);border:1px solid var(--border);border-radius:8px;padding:14px;margin-bottom:18px;font-size:12px;color:var(--text2)">
                    <i class="ti ti-info-circle" style="color:var(--info);margin-right:6px" aria-hidden="true"></i>
                    Full backups may take a few minutes depending on database size.
                    Do not close the browser during backup.
                </div>

                <asp:Button ID="btnBackup" runat="server" CssClass="btn-primary"
                    OnClick="btnBackup_Click"
                    OnClientClick="return confirm('Start a new database backup now?');"
                    Text="Start Backup" />
            </div>

            <!-- Backup history -->
            <div class="card">
                <div class="card-header">Backup History</div>
                <div style="overflow-x:auto">
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th scope="col">Label</th>
                                <th scope="col">Type</th>
                                <th scope="col">Size</th>
                                <th scope="col">Status</th>
                                <th scope="col">Created</th>
                                <th scope="col">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptBackups" runat="server"
                                          OnItemCommand="rptBackups_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="fw-bold" style="color:var(--text)"><%# Eval("Label") %></td>
                                        <td><span class="badge badge-blue"><%# Eval("BackupType") %></span></td>
                                        <td class="text-muted"><%# Eval("SizeDisplay") %></td>
                                        <td><span class="badge <%# Eval("Status").ToString()=="Success" ? "badge-green" : "badge-red" %>"><%# Eval("Status") %></span></td>
                                        <td class="text-muted text-small"><%# Eval("CreatedDisplay") %></td>
                                        <td>
                                            <div class="action-btns">
                                                <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                    CommandName="Download" CommandArgument='<%# Eval("BackupID") %>'>
                                                    <i class="ti ti-download"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton runat="server" CssClass="btn-danger"
                                                    CommandName="Delete" CommandArgument='<%# Eval("BackupID") %>'
                                                    OnClientClick="return confirm('Delete this backup file?');">
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
                    <p class="text-muted text-small mt-16" style="text-align:center;padding:16px 0">No backups yet.</p>
                </asp:Panel>
            </div>

        </div>
    </main>
</div>
</asp:Content>
<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}</style>
</asp:Content>
