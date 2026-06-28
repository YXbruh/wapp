<%@ Page Title="Announcements – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Announcements.aspx.cs" Inherits="CSA.Admin.Announcements" %>

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
        <a href="Announcements.aspx"  class="sidebar-link active"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx"         class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <a href="SecurityAlerts.aspx" class="sidebar-link"><i class="ti ti-alert-triangle"></i>Security Alerts</a>
        <div class="sidebar-section">Account</div>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Announcements</h2>
            <p>Broadcast messages to all students and instructors.</p>
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

        <div class="cards-row" style="align-items:start">

            <!-- Compose form -->
            <div class="card">
                <div class="card-header">
                    <span><i class="ti ti-pencil" style="margin-right:6px" aria-hidden="true"></i>
                        <asp:Literal ID="litFormTitle" runat="server" Text="New Announcement" />
                    </span>
                    <asp:LinkButton ID="lbCancelEdit" runat="server" Visible="false"
                        OnClick="lbCancelEdit_Click" style="font-size:12px;color:var(--text3)">
                        Cancel edit
                    </asp:LinkButton>
                </div>

                <asp:HiddenField ID="hfEditID" runat="server" Value="0" />

                <asp:ValidationSummary ID="valSummary" runat="server"
                    ValidationGroup="AnnGroup"
                    CssClass="validation-summary-errors" HeaderText="Please fix:"
                    DisplayMode="BulletList" />

                <div class="form-group">
                    <label class="form-label" for="<%= tbTitle.ClientID %>">
                        <i class="ti ti-heading" aria-hidden="true"></i>Title
                    </label>
                    <asp:TextBox ID="tbTitle" runat="server" CssClass="form-input"
                        placeholder="Announcement title" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server"
                        ControlToValidate="tbTitle" ValidationGroup="AnnGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Title is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= ddlAudience.ClientID %>">
                        <i class="ti ti-users" aria-hidden="true"></i>Audience
                    </label>
                    <asp:DropDownList ID="ddlAudience" runat="server" CssClass="form-select">
                        <asp:ListItem Value="All">All Users</asp:ListItem>
                        <asp:ListItem Value="Student">Students Only</asp:ListItem>
                        <asp:ListItem Value="Instructor">Instructors Only</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= ddlPriority.ClientID %>">
                        <i class="ti ti-flag" aria-hidden="true"></i>Priority
                    </label>
                    <asp:DropDownList ID="ddlPriority" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Normal">Normal</asp:ListItem>
                        <asp:ListItem Value="Important">Important</asp:ListItem>
                        <asp:ListItem Value="Urgent">Urgent</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= tbMessage.ClientID %>">
                        <i class="ti ti-message" aria-hidden="true"></i>Message
                    </label>
                    <asp:TextBox ID="tbMessage" runat="server" CssClass="form-input"
                        TextMode="MultiLine" Rows="5"
                        placeholder="Write your announcement here..." MaxLength="2000" />
                    <asp:RequiredFieldValidator ID="rfvMessage" runat="server"
                        ControlToValidate="tbMessage" ValidationGroup="AnnGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Message body is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= tbExpiry.ClientID %>">
                        <i class="ti ti-calendar" aria-hidden="true"></i>Expiry Date
                        <span class="text-muted" style="font-weight:400">(optional)</span>
                    </label>
                    <asp:TextBox ID="tbExpiry" runat="server" CssClass="form-input"
                        TextMode="Date" />
                </div>

                <asp:Button ID="btnPublish" runat="server" CssClass="btn-primary"
                    ValidationGroup="AnnGroup" OnClick="btnPublish_Click"
                    Text="Publish Announcement" />
            </div>

            <!-- Past announcements table -->
            <div class="card">
                <div class="card-header">Past Announcements</div>

                <div class="toolbar" style="margin-bottom:14px">
                    <div class="search-wrap" style="flex:1">
                        <asp:TextBox ID="tbSearchAnn" runat="server" CssClass="search-input"
                            placeholder="Search announcements..."
                            AutoPostBack="true" OnTextChanged="tbSearchAnn_TextChanged" />
                        <i class="ti ti-search" aria-hidden="true"></i>
                    </div>
                </div>

                <div style="overflow-x:auto">
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th scope="col">Title</th>
                                <th scope="col">Audience</th>
                                <th scope="col">Priority</th>
                                <th scope="col">Published</th>
                                <th scope="col">Expires</th>
                                <th scope="col">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptAnnouncements" runat="server"
                                          OnItemCommand="rptAnnouncements_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <div class="fw-bold" style="color:var(--text)"><%# Eval("Title") %></div>
                                            <div class="text-small text-muted" style="max-width:180px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                                                <%# Eval("MessagePreview") %>
                                            </div>
                                        </td>
                                        <td><span class="badge badge-blue"><%# Eval("Audience") %></span></td>
                                        <td><span class="badge <%# GetPriorityBadge(Eval("Priority").ToString()) %>"><%# Eval("Priority") %></span></td>
                                        <td class="text-muted text-small"><%# Eval("PublishedDisplay") %></td>
                                        <td class="text-muted text-small"><%# Eval("ExpiryDisplay") %></td>
                                        <td>
                                            <div class="action-btns">
                                                <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                    CommandName="Edit" CommandArgument='<%# Eval("AnnouncementID") %>'>
                                                    <i class="ti ti-edit"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton runat="server" CssClass="btn-danger"
                                                    CommandName="Delete" CommandArgument='<%# Eval("AnnouncementID") %>'
                                                    OnClientClick="return confirm('Delete this announcement?');">
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
                    <p class="text-muted text-small mt-16" style="text-align:center;padding:16px 0">No announcements yet.</p>
                </asp:Panel>
            </div>

        </div>
    </main>
</div>
</asp:Content>
<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}</style>
</asp:Content>
