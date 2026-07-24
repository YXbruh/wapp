<%@ Page Title="Preview Content – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="PreviewContent.aspx.cs" Inherits="CSA.Admin.PreviewContent" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Admin menu">
        <div class="sidebar-section">Admin Panel</div>
        <a href="Admin_Dashboard.aspx" class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Overview</a>
        <a href="Users.aspx" class="sidebar-link"><i class="ti ti-users"></i>Users</a>
        <a href="Courses.aspx" class="sidebar-link"><i class="ti ti-books"></i>Courses</a>
        <a href="Categories.aspx" class="sidebar-link"><i class="ti ti-category"></i>Categories</a>
        <a href="ContentReview.aspx" class="sidebar-link active"><i class="ti ti-file-check"></i>Content Review</a>
        <div class="sidebar-section">System</div>
        <a href="ActivityLogs.aspx" class="sidebar-link"><i class="ti ti-activity"></i>Activity Logs</a>
        <a href="Announcements.aspx" class="sidebar-link"><i class="ti ti-bell"></i>Announcements</a>
        <a href="Backup.aspx" class="sidebar-link"><i class="ti ti-database"></i>DB Backup</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user-circle"></i>My Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Submission Details</h2>
            <p><a href="ContentReview.aspx" style="color:var(--accent2)">&larr; Back to Content Review</a></p>
        </div>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="validation-summary-errors mb-16"><asp:Literal ID="litError" runat="server" /></div>
        </asp:Panel>

        <div class="card">
            <!-- Common metadata -->
            <div style="display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:12px 24px">
                <div class="form-group">
                    <label class="form-label">Content ID</label>
                    <asp:Literal ID="litContentID" runat="server" />
                </div>
                <div class="form-group">
                    <label class="form-label">Content Type</label>
                    <asp:Literal ID="litContentType" runat="server" />
                </div>
                <div class="form-group">
                    <label class="form-label">Status</label>
                    <asp:Literal ID="litStatus" runat="server" />
                </div>
                <div class="form-group">
                    <label class="form-label">Author</label>
                    <asp:Literal ID="litSubmittedBy" runat="server" />
                </div>
                <div class="form-group">
                    <label class="form-label">Course</label>
                    <asp:Literal ID="litCourse" runat="server" />
                </div>
                <div class="form-group">
                    <label class="form-label">Last Updated</label>
                    <asp:Literal ID="litSubmittedAt" runat="server" />
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">Title</label>
                <asp:Literal ID="litTitle" runat="server" />
            </div>

            <!-- Chapter-specific -->
            <asp:Panel ID="pnlChapterMeta" runat="server" Visible="false" CssClass="form-group">
                <label class="form-label">Chapter Order</label>
                <asp:Literal ID="litChSortOrder" runat="server" />
            </asp:Panel>

            <!-- Quiz-specific -->
            <asp:Panel ID="pnlQuizMeta" runat="server" Visible="false">
                <label class="form-label">Quiz Settings</label>
                <div style="display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:10px 24px;background:var(--bg2);padding:14px;border-radius:8px">
                    <div><span class="text-small text-muted">Questions</span><br /><strong><asp:Literal ID="litQQuestionCount" runat="server" /></strong></div>
                    <div><span class="text-small text-muted">Total Marks</span><br /><strong><asp:Literal ID="litQTotalMarks" runat="server" /></strong></div>
                    <div><span class="text-small text-muted">Pass Mark</span><br /><strong><asp:Literal ID="litQPassMark" runat="server" /></strong></div>
                    <div><span class="text-small text-muted">Max Attempts</span><br /><strong><asp:Literal ID="litQMaxAttempts" runat="server" /></strong></div>
                    <div><span class="text-small text-muted">Duration</span><br /><strong><asp:Literal ID="litQDuration" runat="server" /></strong></div>
                </div>
            </asp:Panel>

            <!-- Lab-specific -->
            <asp:Panel ID="pnlLabMeta" runat="server" Visible="false">
                <label class="form-label">Lab Settings</label>
                <div style="display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:10px 24px;background:var(--bg2);padding:14px;border-radius:8px">
                    <div><span class="text-small text-muted">Difficulty</span><br /><strong><asp:Literal ID="litLDifficulty" runat="server" /></strong></div>
                    <div><span class="text-small text-muted">Skill Tag</span><br /><strong><asp:Literal ID="litLSkillTag" runat="server" /></strong></div>
                    <div><span class="text-small text-muted">Points Reward</span><br /><strong><asp:Literal ID="litLPoints" runat="server" /></strong></div>
                    <div><span class="text-small text-muted">Time Limit</span><br /><strong><asp:Literal ID="litLTimeLimit" runat="server" /></strong></div>
                    <div><span class="text-small text-muted">Validation</span><br /><strong><asp:Literal ID="litLValidation" runat="server" /></strong></div>
                </div>
                <div class="form-group" style="margin-top:12px">
                    <label class="form-label">Expected Command / Answer</label>
                    <div style="background:var(--bg2);padding:12px;border-radius:8px;font-family:monospace;white-space:pre-wrap"><asp:Literal ID="litLExpected" runat="server" /></div>
                </div>
                <asp:Panel ID="pnlLabHint" runat="server" Visible="false" CssClass="form-group">
                    <label class="form-label">Hint</label>
                    <div style="background:var(--bg2);padding:12px;border-radius:8px;white-space:pre-wrap"><asp:Literal ID="litLHint" runat="server" /></div>
                </asp:Panel>
            </asp:Panel>

            <div class="form-group">
                <label class="form-label"><asp:Literal ID="litBodyLabel" runat="server" Text="Content" /></label>
                <div style="background:var(--bg2);padding:16px;border-radius:8px;white-space:pre-wrap;max-height:420px;overflow:auto"><asp:Literal ID="litPreview" runat="server" Mode="Encode" /></div>
            </div>

            <!-- Quiz questions -->
            <asp:Panel ID="pnlQuestions" runat="server" Visible="false" CssClass="form-group">
                <label class="form-label">Questions (<asp:Literal ID="litQuestionsHeading" runat="server" />)</label>
                <asp:Repeater ID="rptQuestions" runat="server">
                    <ItemTemplate>
                        <div style="border:1px solid var(--border);border-radius:8px;padding:12px;margin-bottom:10px;background:var(--bg2)">
                            <div style="display:flex;justify-content:space-between;gap:8px;margin-bottom:6px">
                                <strong>Q<%# Container.ItemIndex + 1 %>. <%#: Eval("QuestionText") %></strong>
                                <span class="badge" style="flex-shrink:0"><%#: Eval("QuestionType") %> &middot; <%# Eval("Points") %> pts</span>
                            </div>
                            <div class="text-small" style="white-space:pre-wrap;margin-bottom:6px"><%# RenderOptions(Eval("OptionA"), Eval("OptionB"), Eval("OptionC"), Eval("OptionD")) %></div>
                            <div class="text-small"><strong>Correct answer:</strong> <%#: Eval("CorrectAnswer") %></div>
                            <asp:Panel runat="server" Visible='<%# !string.IsNullOrWhiteSpace(Convert.ToString(Eval("Explanation"))) %>'>
                                <div class="text-small text-muted" style="margin-top:4px"><strong>Explanation:</strong> <%#: Eval("Explanation") %></div>
                            </asp:Panel>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>

            <!-- Attachments -->
            <asp:Panel ID="pnlAttachments" runat="server" Visible="false" CssClass="form-group">
                <label class="form-label">Attachments</label>
                <asp:Repeater ID="rptAttachments" runat="server">
                    <ItemTemplate>
                        <div style="display:flex;align-items:center;gap:8px;padding:6px 0;border-bottom:1px solid var(--border)">
                            <i class="ti <%# GetAttachmentIcon(Eval("AttachmentType")) %>" aria-hidden="true"></i>
                            <a href='<%# GetAttachmentUrl(Eval("AttachmentType"), Eval("FilePath"), Eval("LinkUrl")) %>' target="_blank" rel="noopener"><%#: Eval("Title") %></a>
                            <span class="text-small text-muted"><%#: Eval("AttachmentType") %></span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>

<div class="form-group" style="display:flex;gap:8px">
                <asp:LinkButton ID="btnApprove" runat="server" CssClass="btn-primary" OnClick="btnApprove_Click"
                    OnClientClick="return showConfirmAction(this, 'Publish this content so enrolled students can see it?', 'Publish', 'var(--success)');">
                    <i class="ti ti-circle-check" aria-hidden="true"></i> Approve &amp; Publish
                </asp:LinkButton>
                <asp:LinkButton ID="btnReject" runat="server" CssClass="btn-sm" OnClick="btnReject_Click"
                    OnClientClick="return showConfirmAction(this, 'Reject this submission and leave it as a draft?', 'Reject', 'var(--danger)');">
                    <i class="ti ti-circle-x" aria-hidden="true"></i> Reject (Keep as Draft)
                </asp:LinkButton>
            </div>
        </div>
    </main>
</div>
</asp:Content>
