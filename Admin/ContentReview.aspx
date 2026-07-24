<%@ Page Title="Content Review – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ContentReview.aspx.cs" Inherits="CSA.Admin.ContentReview" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx"      class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx"          class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx"        class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="Categories.aspx"     class="sidebar-link"><i class="ti ti-category"></i>Categories</a>
        <a href="ContentReview.aspx"  class="sidebar-link active"><i class="ti ti-file-check"></i>Content Review</a>
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
            <h2>Content Review</h2>
            <p>Chapters, quizzes and labs submitted by lecturers. Nothing reaches students until you publish it here.</p>
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

        <!-- Metrics -->
        <div class="metrics" style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Pending Review</div>
                <div class="metric-val" style="color:var(--warning)"><asp:Literal ID="litPending" runat="server" Text="0"/></div>
                <div class="metric-sub">drafts awaiting a decision</div>
            </div>
            <div class="metric">
                <div class="metric-label">Published</div>
                <div class="metric-val" style="color:var(--success)"><asp:Literal ID="litPublished" runat="server" Text="0"/></div>
                <div class="metric-sub">live for students</div>
            </div>
            <div class="metric">
                <div class="metric-label">Reviewed Today</div>
                <div class="metric-val"><asp:Literal ID="litReviewedToday" runat="server" Text="0"/></div>
                <div class="metric-sub">decisions recorded</div>
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
            </asp:DropDownList>
        </div>

        <div class="card mt-16">
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">Content</th>
                            <th scope="col">Type</th>
                            <th scope="col">Author</th>
                            <th scope="col">Course</th>
                            <th scope="col">Last Updated</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptContent" runat="server"
                                      OnItemCommand="rptContent_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="fw-bold" style="color:var(--text)"><%#: Eval("Title") %></div>
                                        <div class="text-small text-muted" style="max-width:260px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                                            <%#: Eval("Preview") %>
                                        </div>
                                    </td>
                                    <td><span class="badge badge-blue"><%#: Eval("ContentType") %></span></td>
                                    <td class="text-muted"><%#: Eval("SubmittedBy") %></td>
                                    <td class="text-muted"><%#: Eval("CourseName") %></td>
                                    <td class="text-muted"><%#: Eval("SubmittedAt", "{0:dd MMM yyyy}") %></td>
                                    <td>
                                        <div class="action-btns">
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="Preview" CommandArgument='<%# Eval("ContentType") + "|" + Eval("ContentID") %>'>
                                                <i class="ti ti-eye"></i> Preview
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-sm"
                                                CommandName="Approve" CommandArgument='<%# Eval("ContentType") + "|" + Eval("ContentID") %>'
                                                OnClientClick="return showConfirmAction(this, 'Publish this content so enrolled students can see it?', 'Publish', 'var(--success)');">
                                                <i class="ti ti-circle-check"></i> Approve
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-sm"
                                                CommandName="Reject" CommandArgument='<%# Eval("ContentType") + "|" + Eval("ContentID") %>'
                                                OnClientClick="return showConfirmAction(this, 'Reject this submission and leave it as a draft?', 'Reject', 'var(--danger)');">
                                                <i class="ti ti-circle-x"></i> Reject
                                            </asp:LinkButton>
                                            <button type="button" class="btn-sm secondary"
                                                onclick='openRevisionModal("<%# Eval("ContentType") %>|<%# Eval("ContentID") %>", "<%# System.Web.HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("Title"))) %>")'>
                                                <i class="ti ti-message-2"></i> Request Revision
                                            </button>
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
                <div style="text-align:center;padding:40px 20px">
                    <i class="ti ti-circle-check" style="font-size:40px;color:var(--success)" aria-hidden="true"></i>
                    <p class="text-muted mt-16">All caught up — no submissions waiting to be published.</p>
                </div>
            </asp:Panel>
        </div>

        <!-- Request Revision modal -->
        <asp:HiddenField ID="hfRevisionRef" runat="server" />
        <div id="revisionModal" class="modal-overlay" style="display:none">
            <div class="modal-box" style="text-align:left;max-width:520px">
                <div class="modal-icon" style="color:var(--accent2);text-align:center">
                    <i class="ti ti-message-2"></i>
                </div>
                <p class="modal-text" style="text-align:center">Request changes from the lecturer</p>
                <p class="text-small text-muted" style="margin-bottom:8px">
                    Reviewing: <strong id="revisionTitle"></strong>. The item stays a draft and the
                    lecturer is emailed the changes you describe below.
                </p>
                <asp:TextBox ID="tbRevisionMessage" runat="server" TextMode="MultiLine" Rows="5"
                    CssClass="form-input" placeholder="Describe what the lecturer needs to change before this can be published…"
                    style="width:100%;resize:vertical" />
                <div style="display:flex;justify-content:center;gap:12px;margin-top:16px">
                    <button type="button" class="form-submit" onclick="closeRevisionModal()"
                        style="width:auto;min-width:120px;background:var(--bg2);color:var(--text)">
                        Cancel
                    </button>
                    <asp:Button ID="btnSendRevision" runat="server" CssClass="form-submit"
                        Text="Send to Lecturer" OnClick="btnSendRevision_Click"
                        OnClientClick="return validateRevision();"
                        style="width:auto;min-width:120px;background:var(--accent2);color:#fff" />
                </div>
            </div>
        </div>
    </main>
</div>
</asp:Content>
<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
.pager-bar{display:flex;align-items:center;gap:8px;padding:10px 0}
.pager-info{flex:1;text-align:center;font-size:12px;color:var(--text3)}</style>
<script>
    function openRevisionModal(ref, title) {
        document.getElementById('<%= hfRevisionRef.ClientID %>').value = ref;
        document.getElementById('<%= tbRevisionMessage.ClientID %>').value = '';
        document.getElementById('revisionTitle').textContent = title || 'this item';
        document.getElementById('revisionModal').style.display = 'flex';
        return false;
    }
    function closeRevisionModal() {
        document.getElementById('revisionModal').style.display = 'none';
    }
    function validateRevision() {
        var box = document.getElementById('<%= tbRevisionMessage.ClientID %>');
        if (!box.value.trim()) {
            box.focus();
            return false;
        }
        return true;
    }
</script>
</asp:Content>
