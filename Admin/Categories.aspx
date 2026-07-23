<%@ Page Title="Course Categories – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Categories.aspx.cs" Inherits="CSA.Admin.Categories" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="Categories.aspx"     class="sidebar-link active"><i class="ti ti-category"></i>Categories</a>
        <a href="ContentReview.aspx"  class="sidebar-link"><i class="ti ti-file-check"></i>Content Review</a>
        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx"   class="sidebar-link"><i class="ti ti-activity"></i>Activity Logs</a>
        <a href="Announcements.aspx"  class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx"         class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user-circle"></i>My Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Course Categories</h2>
            <p>Manage the subject taxonomy hierarchy for courses.</p>
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

            <!-- Add/Edit form -->
            <div class="card">
                <div class="card-header">
                    <span><i class="ti ti-pencil" style="margin-right:6px" aria-hidden="true"></i>
                        <asp:Literal ID="litFormTitle" runat="server" Text="New Category" />
                    </span>
                    <asp:LinkButton ID="lbCancelEdit" runat="server" Visible="false"
                        OnClick="lbCancelEdit_Click" style="font-size:12px;color:var(--text3)">
                        Cancel edit
                    </asp:LinkButton>
                </div>

                <asp:HiddenField ID="hfEditID" runat="server" Value="0" />

                <div class="form-group">
                    <label class="form-label">Category Name</label>
                    <asp:TextBox ID="tbName" runat="server" CssClass="form-input" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server"
                        ControlToValidate="tbName" ValidationGroup="CatGroup"
                        Display="Dynamic" CssClass="val-error"
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>
                <div class="form-group">
                    <label class="form-label">Description <span class="text-muted">(optional)</span></label>
                    <asp:TextBox ID="tbDescription" runat="server" CssClass="form-input"
                        TextMode="MultiLine" Rows="3" MaxLength="500" />
                </div>

                <asp:Button ID="btnSave" runat="server" CssClass="btn-primary"
                    ValidationGroup="CatGroup" OnClick="btnSave_Click"
                    Text="Save Category" />
            </div>

            <!-- Category list -->
            <div class="card">
                <div class="card-header">All Categories</div>
                <div style="overflow-x:auto">
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th scope="col">Name</th>
                                <th scope="col">Description</th>
                                <th scope="col">Courses</th>
                                <th scope="col">Created</th>
                                <th scope="col">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptCategories" runat="server"
                                          OnItemCommand="rptCategories_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="fw-bold" style="color:var(--text)"><%# Eval("CategoryName") %></td>
                                        <td class="text-muted"><%# Eval("Description") %></td>
                                        <td class="text-muted"><%# Eval("CourseCount") %></td>
                                        <td class="text-muted text-small"><%# Eval("CreatedDisplay") %></td>
                                        <td>
                                            <div class="action-btns">
                                                <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                    CommandName="Edit" CommandArgument='<%# Eval("CategoryID") %>'>
                                                    <i class="ti ti-edit"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton runat="server" CssClass="btn-danger"
                                                    CommandName="Delete" CommandArgument='<%# Eval("CategoryID") %>'
                                                    OnClientClick="return showDeleteConfirm(this);">
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
                <div class="pager-bar">
                    <asp:LinkButton ID="btnPrev" runat="server" CssClass="btn-sm secondary" OnClick="btnPrev_Click">
                        <i class="ti ti-chevron-left"></i> Previous
                    </asp:LinkButton>
                    <span class="pager-info"><asp:Literal ID="litPageInfo" runat="server" /></span>
                    <asp:LinkButton ID="btnNext" runat="server" CssClass="btn-sm secondary" OnClick="btnNext_Click">
                        Next <i class="ti ti-chevron-right"></i>
                    </asp:LinkButton>
                </div>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <p class="text-muted text-small mt-16" style="text-align:center;padding:16px 0">No categories yet.</p>
                </asp:Panel>
            </div>

        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
.pager-bar{display:flex;align-items:center;gap:8px;padding:10px 0}
.pager-info{flex:1;text-align:center;font-size:12px;color:var(--text3)}
</style>
</asp:Content>