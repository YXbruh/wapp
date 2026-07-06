<%@ Page Title="Manage Courses – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Courses.aspx.cs" Inherits="CSA.Admin.Courses" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link active"><i class="ti ti-books"></i>Courses</a>
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
            <h2>Manage Courses</h2>
            <p>Create, publish, and manage all courses on the platform.</p>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litSuccess" runat="server" />
            </div>
        </asp:Panel>

        <!-- Metrics -->
        <div class="metrics" style="grid-template-columns:repeat(4,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Total</div>
                <div class="metric-val"><asp:Literal ID="litTotal" runat="server" Text="0"/></div>
                <div class="metric-sub">courses</div>
            </div>
            <div class="metric">
                <div class="metric-label">Published</div>
                <div class="metric-val" style="color:var(--success)"><asp:Literal ID="litPublished" runat="server" Text="0"/></div>
                <div class="metric-sub">live</div>
            </div>
            <div class="metric">
                <div class="metric-label">Draft</div>
                <div class="metric-val" style="color:var(--warning)"><asp:Literal ID="litDraft" runat="server" Text="0"/></div>
                <div class="metric-sub">in progress</div>
            </div>
            <div class="metric">
                <div class="metric-label">Pending</div>
                <div class="metric-val" style="color:var(--info)"><asp:Literal ID="litPending" runat="server" Text="0"/></div>
                <div class="metric-sub">awaiting review</div>
            </div>
        </div>

        <div class="card">
            <!-- Toolbar -->
            <div class="toolbar">
                <div class="search-wrap" style="flex:1">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                        placeholder="Search courses..."
                        AutoPostBack="true" OnTextChanged="Search_Changed" />
                    <i class="ti ti-search" aria-hidden="true"></i>
                </div>
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"
                    style="width:150px" AutoPostBack="true" OnSelectedIndexChanged="Search_Changed">
                    <asp:ListItem Value="">All Status</asp:ListItem>
                    <asp:ListItem Value="Published">Published</asp:ListItem>
                    <asp:ListItem Value="Draft">Draft</asp:ListItem>
                    <asp:ListItem Value="Pending">Pending</asp:ListItem>
                </asp:DropDownList>
                <asp:DropDownList ID="ddlLevel" runat="server" CssClass="form-select"
                    style="width:140px" AutoPostBack="true" OnSelectedIndexChanged="Search_Changed">
                    <asp:ListItem Value="">All Levels</asp:ListItem>
                    <asp:ListItem Value="Beginner">Beginner</asp:ListItem>
                    <asp:ListItem Value="Intermediate">Intermediate</asp:ListItem>
                    <asp:ListItem Value="Advanced">Advanced</asp:ListItem>
                </asp:DropDownList>
                <a href="CreateCourse.aspx" class="btn-sm">
                    <i class="ti ti-plus" aria-hidden="true"></i>New Course
                </a>
            </div>

            <!-- Courses table -->
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">Course</th>
                            <th scope="col">Instructor</th>
                            <th scope="col">Level</th>
                            <th scope="col">Status</th>
                            <th scope="col">Enrolled</th>
                            <th scope="col">Labs</th>
                            <th scope="col">Created</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptCourses" runat="server"
                                      OnItemCommand="rptCourses_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="fw-bold" style="color:var(--text)"><%# Eval("CourseName") %></div>
                                        <div class="text-small text-muted"><%# Eval("Category") %></div>
                                    </td>
                                    <td class="text-muted"><%# Eval("InstructorName") %></td>
                                    <td><span class="badge <%# GetLevelBadge(Eval("Level").ToString()) %>"><%# Eval("Level") %></span></td>
                                    <td><span class="badge <%# GetStatusBadge(Eval("Status").ToString()) %>"><%# Eval("Status") %></span></td>
                                    <td class="text-muted"><%# Eval("EnrolledCount") %></td>
                                    <td class="text-muted"><%# Eval("LabCount") %></td>
                                    <td class="text-muted"><%# Eval("CreatedDisplay") %></td>
                                    <td>
                                        <div class="action-btns">
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="Edit" CommandArgument='<%# Eval("CourseID") %>'>
                                                <i class="ti ti-edit"></i> Edit
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="TogglePublish" CommandArgument='<%# Eval("CourseID") %>'>
                                                <i class="ti <%# Eval("Status").ToString()=="Published" ? "ti-eye-off" : "ti-eye" %>"></i>
                                                <%# Eval("Status").ToString()=="Published" ? "Unpublish" : "Publish" %>
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-danger"
                                                CommandName="Delete" CommandArgument='<%# Eval("CourseID") %>'
                                                OnClientClick="return confirm('Delete this course? This cannot be undone.');">
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
                <p class="text-muted text-small mt-16" style="text-align:center;padding:20px 0">No courses found.</p>
            </asp:Panel>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}</style>
</asp:Content>
