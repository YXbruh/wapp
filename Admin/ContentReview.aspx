<%@ Page Title="Content Review – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="ContentReview.aspx.cs" Inherits="CSA.Admin.ContentReview" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="ContentReview.aspx"  class="sidebar-link active"><i class="ti ti-file-check"></i>Content Review</a>
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
            <h2>Content Review</h2>
            <p>Approve or reject instructor-submitted content before it goes live.</p>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litSuccess" runat="server" />
            </div>
        </asp:Panel>

        <!-- Metrics -->
        <div class="metrics" style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Pending Review</div>
                <div class="metric-val" style="color:var(--warning)"><asp:Literal ID="litPending" runat="server" Text="0"/></div>
                <div class="metric-sub">awaiting action</div>
            </div>
            <div class="metric">
                <div class="metric-label">Approved Today</div>
                <div class="metric-val" style="color:var(--success)"><asp:Literal ID="litApproved" runat="server" Text="0"/></div>
                <div class="metric-sub">published</div>
            </div>
            <div class="metric">
                <div class="metric-label">Rejected Today</div>
                <div class="metric-val" style="color:var(--danger)"><asp:Literal ID="litRejected" runat="server" Text="0"/></div>
                <div class="metric-sub">sent back</div>
            </div>
        </div>

        <!-- Filter chips -->
        <div class="filter-bar" role="group">
            <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select"
                style="width:160px" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                <asp:ListItem Value="">All Types</asp:ListItem>
                <asp:ListItem Value="Chapter">Chapter</asp:ListItem>
                <asp:ListItem Value="Quiz">Quiz</asp:ListItem>
                <asp:ListItem Value="Lab">Lab</asp:ListItem>
                <asp:ListItem Value="Announcement">Announcement</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="card mt-16">
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">Content</th>
                            <th scope="col">Type</th>
                            <th scope="col">Submitted By</th>
                            <th scope="col">Course</th>
                            <th scope="col">Submitted</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptContent" runat="server"
                                      OnItemCommand="rptContent_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="fw-bold" style="color:var(--text)"><%# Eval("Title") %></div>
                                        <div class="text-small text-muted" style="max-width:260px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                                            <%# Eval("Preview") %>
                                        </div>
                                    </td>
                                    <td><span class="badge badge-blue"><%# Eval("ContentType") %></span></td>
                                    <td class="text-muted"><%# Eval("SubmittedBy") %></td>
                                    <td class="text-muted"><%# Eval("CourseName") %></td>
                                    <td class="text-muted"><%# Eval("SubmittedDisplay") %></td>
                                    <td>
                                        <div class="action-btns">
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="Preview" CommandArgument='<%# Eval("ContentID") %>'>
                                                <i class="ti ti-eye"></i> Preview
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-sm"
                                                CommandName="Approve" CommandArgument='<%# Eval("ContentID") %>'>
                                                <i class="ti ti-circle-check"></i> Approve
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-danger"
                                                CommandName="Reject" CommandArgument='<%# Eval("ContentID") %>'
                                                OnClientClick="return confirm('Reject and notify the instructor?');">
                                                <i class="ti ti-circle-x"></i> Reject
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
                    <i class="ti ti-circle-check" style="font-size:40px;color:var(--success)" aria-hidden="true"></i>
                    <p class="text-muted mt-16">All clear — no content pending review.</p>
                </div>
            </asp:Panel>
        </div>
    </main>
</div>
</asp:Content>
<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}</style>
</asp:Content>
