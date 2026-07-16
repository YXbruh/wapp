<%@ Page Title="Mentorship & Feedback – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Mentorship.aspx.cs" Inherits="CSA.Lecturer.Mentorship" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Lecturer menu">
        <div class="sidebar-section">Lecturer</div>
        <a href="Lecturer_Dashboard.aspx"       class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="ManageContent.aspx"   class="sidebar-link"><i class="ti ti-files"></i>Manage Content</a>
        <a href="TerminalSandbox.aspx" class="sidebar-link"><i class="ti ti-terminal-2"></i>Terminal Sandbox</a>
        <a href="QuizEditor.aspx"      class="sidebar-link"><i class="ti ti-list-check"></i>Quiz Editor</a>
        <div class="sidebar-section">Students</div>
        <a href="ClassAnalytics.aspx"  class="sidebar-link"><i class="ti ti-chart-bar"></i>Class Analytics</a>
        <a href="Mentorship.aspx"      class="sidebar-link active"><i class="ti ti-messages"></i>Mentorship</a>
        <div class="sidebar-section">Account</div>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Mentorship &amp; Feedback</h2>
            <p>Review student feedback and send personalised performance remarks.</p>
        </div>

        <!-- Metrics -->
        <div class="metrics" style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Unread Feedback</div>
                <div class="metric-val" style="color:var(--warning)">
                    <asp:Literal ID="litUnread" runat="server" Text="0"/>
                </div>
                <div class="metric-sub">awaiting response</div>
            </div>
            <div class="metric">
                <div class="metric-label">Replied</div>
                <div class="metric-val" style="color:var(--success)">
                    <asp:Literal ID="litReplied" runat="server" Text="0"/>
                </div>
                <div class="metric-sub">this month</div>
            </div>
            <div class="metric">
                <div class="metric-label">Avg Rating</div>
                <div class="metric-val" style="color:var(--accent2)">
                    <asp:Literal ID="litAvgRating" runat="server" Text="—"/>
                </div>
                <div class="metric-sub">&#9733; out of 5</div>
            </div>
        </div>

        <!-- Inbox layout: feedback list LEFT, reply panel RIGHT -->
        <div class="inbox-layout">

            <!-- LEFT: Feedback list -->
            <div class="inbox-list-col">

                <!-- Filter + search -->
                <div style="margin-bottom:12px;display:flex;flex-direction:column;gap:8px">
                    <div class="search-wrap">
                        <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                            placeholder="Search by student name..."
                            AutoPostBack="true" OnTextChanged="tbSearch_Changed" />
                        <i class="ti ti-search" aria-hidden="true"></i>
                    </div>
                    <div style="display:flex;gap:6px;flex-wrap:wrap">
                        <asp:DropDownList ID="ddlFilter" runat="server" CssClass="form-select"
                            style="flex:1" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlFilter_Changed">
                            <asp:ListItem Value="">All Feedback</asp:ListItem>
                            <asp:ListItem Value="Unread">Unread</asp:ListItem>
                            <asp:ListItem Value="Replied">Replied</asp:ListItem>
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select"
                            style="flex:1" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlFilter_Changed" />
                    </div>
                </div>

                <!-- Feedback items -->
                <div class="feedback-list" id="feedbackList">
                    <asp:Repeater ID="rptFeedback" runat="server"
                                  OnItemCommand="rptFeedback_ItemCommand">
                        <ItemTemplate>
                            <div class="feedback-item <%# Eval("IsRead").ToString()=="False" ? "unread":"" %>"
                                 id='fb-<%# Eval("FeedbackID") %>'
                                 onclick="selectFeedback(this)">
                                <div class="fb-top">
                                    <div class="fb-avatar"><%# GetInitials(Eval("StudentName").ToString()) %></div>
                                    <div class="fb-meta">
                                        <div class="fb-name"><%# Eval("StudentName") %></div>
                                        <div class="fb-sub"><%# Eval("QuizName") %> &middot; <%# Eval("CourseName") %></div>
                                    </div>
                                    <div class="fb-right">
                                        <div class="fb-time"><%# Eval("TimeAgo") %></div>
                                        <div class="fb-stars"><%# BuildStars(Convert.ToInt32(Eval("StarRating"))) %></div>
                                    </div>
                                </div>
                                <div class="fb-preview"><%# Eval("CommentPreview") %></div>
                                <div style="margin-top:6px;display:flex;gap:6px">
                                    <span class="badge <%# Eval("IsRead").ToString()=="False" ? "badge-amber":"badge-green" %>">
                                        <%# Eval("IsRead").ToString()=="False" ? "Unread":"Read" %>
                                    </span>
                                    <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                        CommandName="Open" CommandArgument='<%# Eval("FeedbackID") %>'
                                        style="padding:2px 8px;font-size:11px">
                                        Open &rarr;
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <div style="text-align:center;padding:40px 16px">
                        <i class="ti ti-message-off" style="font-size:36px;color:var(--text3)" aria-hidden="true"></i>
                        <p class="text-muted mt-16">No feedback yet.</p>
                    </div>
                </asp:Panel>
            </div>

            <!-- RIGHT: Reply panel -->
            <div class="reply-panel card" id="replyPanel">

                <!-- Empty state -->
                <asp:Panel ID="pnlNoSelection" runat="server">
                    <div style="text-align:center;padding:60px 20px">
                        <i class="ti ti-message-circle" style="font-size:44px;color:var(--text3)" aria-hidden="true"></i>
                        <p class="text-muted mt-16">Select a feedback item to read</p>
                    </div>
                </asp:Panel>

                <!-- Active feedback detail -->
                <asp:Panel ID="pnlDetail" runat="server" Visible="false">

                    <asp:HiddenField ID="hfFeedbackID" runat="server" Value="0" />

                    <!-- Student info -->
                    <div style="display:flex;align-items:center;gap:12px;margin-bottom:20px;padding-bottom:16px;border-bottom:1px solid var(--border)">
                        <div class="fb-avatar" style="width:44px;height:44px;font-size:16px;flex-shrink:0">
                            <asp:Literal ID="litDetailInitials" runat="server" />
                        </div>
                        <div style="flex:1">
                            <div style="font-size:15px;font-weight:700;color:var(--text)">
                                <asp:Literal ID="litDetailName" runat="server" />
                            </div>
                            <div class="text-small text-muted">
                                <asp:Literal ID="litDetailCourse" runat="server" />
                                &middot; <asp:Literal ID="litDetailQuiz" runat="server" />
                            </div>
                        </div>
                        <a href='StudentDetail.aspx?id=<asp:Literal ID="litDetailStudentID" runat="server" />'
                           class="btn-sm secondary">
                            <i class="ti ti-chart-bar" aria-hidden="true"></i> Analytics
                        </a>
                    </div>

                    <!-- Star rating -->
                    <div style="margin-bottom:16px">
                        <div class="form-label" style="margin-bottom:6px">Student Rating</div>
                        <div style="font-size:22px;color:#EF9F27">
                            <asp:Literal ID="litDetailStars" runat="server" />
                        </div>
                        <div class="text-small text-muted mt-4">
                            <asp:Literal ID="litDetailRatingNum" runat="server" /> / 5 &middot;
                            <asp:Literal ID="litDetailDate" runat="server" />
                        </div>
                    </div>

                    <!-- Student comment -->
                    <div style="background:var(--bg2);border-left:3px solid var(--accent2);border-radius:0 8px 8px 0;padding:14px;margin-bottom:20px;font-size:13px;color:var(--text2);line-height:1.7">
                        <div class="form-label" style="margin-bottom:8px;color:var(--text3)">
                            <i class="ti ti-quote" aria-hidden="true"></i> Student Comment
                        </div>
                        <asp:Literal ID="litDetailComment" runat="server" />
                    </div>

                    <!-- Quiz score context -->
                    <div style="display:flex;gap:12px;margin-bottom:20px">
                        <div class="metric" style="flex:1;padding:12px">
                            <div class="metric-label">Quiz Score</div>
                            <div class="metric-val" style="font-size:20px">
                                <asp:Literal ID="litDetailScore" runat="server" Text="—"/>
                            </div>
                        </div>
                        <div class="metric" style="flex:1;padding:12px">
                            <div class="metric-label">Labs Done</div>
                            <div class="metric-val" style="font-size:20px">
                                <asp:Literal ID="litDetailLabs" runat="server" Text="—"/>
                            </div>
                        </div>
                    </div>

                    <!-- Previous reply (if any) -->
                    <asp:Panel ID="pnlPrevReply" runat="server" Visible="false">
                        <div style="background:var(--bg3);border-radius:8px;padding:14px;margin-bottom:16px;font-size:13px;color:var(--text2)">
                            <div class="form-label" style="margin-bottom:6px;color:var(--text3)">
                                <i class="ti ti-corner-down-right" aria-hidden="true"></i> Your Previous Reply
                            </div>
                            <asp:Literal ID="litPrevReply" runat="server" />
                            <div class="text-small text-muted mt-4">
                                Sent: <asp:Literal ID="litReplyDate" runat="server" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- Reply compose -->
                    <asp:ValidationSummary ID="valSummary" runat="server"
                        ValidationGroup="ReplyGroup"
                        CssClass="validation-summary-errors" HeaderText="Please fix:"
                        DisplayMode="BulletList" />

                    <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                        <div class="alert-success mb-16">
                            <i class="ti ti-circle-check" aria-hidden="true"></i>
                            <asp:Literal ID="litSuccess" runat="server" />
                        </div>
                    </asp:Panel>

                    <div class="form-group">
                        <label class="form-label" for="<%= tbReply.ClientID %>">
                            <i class="ti ti-pencil" aria-hidden="true"></i>
                            Your Response / Remediation Guidance
                        </label>
                        <asp:TextBox ID="tbReply" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="6"
                            placeholder="Write personalised feedback, guidance, or remediation advice for this student..." />
                        <asp:RequiredFieldValidator ID="rfvReply" runat="server"
                            ControlToValidate="tbReply" ValidationGroup="ReplyGroup"
                            Display="Dynamic" CssClass="val-error"
                            ErrorMessage="Reply cannot be empty."
                            Text="<i class='ti ti-alert-circle'></i> Reply cannot be empty." />
                        <div class="val-hint">
                            <i class="ti ti-info-circle" aria-hidden="true"></i>
                            This reply will appear on the student's dashboard under their quiz results.
                        </div>
                    </div>

                    <div style="display:flex;gap:10px;margin-top:4px">
                        <asp:Button ID="btnSendReply" runat="server" CssClass="btn-primary"
                            ValidationGroup="ReplyGroup" OnClick="btnSendReply_Click"
                            Text="Send Reply" />
                        <asp:Button ID="btnClear" runat="server" CssClass="btn-outline"
                            CausesValidation="false" OnClick="btnClear_Click"
                            Text="Clear" />
                    </div>

                </asp:Panel>
            </div><!-- /.reply-panel -->

        </div><!-- /.inbox-layout -->
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}

/* Inbox split layout */
.inbox-layout{display:grid;grid-template-columns:340px 1fr;gap:16px;align-items:start}

/* Left column */
.inbox-list-col{}
.feedback-list{display:flex;flex-direction:column;gap:8px;max-height:calc(100vh - 320px);overflow-y:auto;padding-right:4px}
.feedback-list::-webkit-scrollbar{width:4px}
.feedback-list::-webkit-scrollbar-thumb{background:var(--accent1);border-radius:2px}

.feedback-item{background:var(--surface);border:1px solid var(--border);border-radius:10px;padding:14px;cursor:pointer;transition:border-color .2s,background .2s}
.feedback-item:hover{border-color:var(--accent2)}
.feedback-item.unread{border-left:3px solid var(--warning)}
.feedback-item.selected{border-color:var(--accent2);background:var(--bg2)}

.fb-top{display:flex;align-items:flex-start;gap:10px;margin-bottom:8px}
.fb-avatar{width:36px;height:36px;border-radius:50%;background:var(--accent1);border:2px solid var(--accent2);display:flex;align-items:center;justify-content:center;font-size:12px;font-weight:700;color:var(--accent3);flex-shrink:0}
.fb-meta{flex:1;min-width:0}
.fb-name{font-size:13px;font-weight:700;color:var(--text);white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.fb-sub{font-size:11px;color:var(--text3);margin-top:2px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.fb-right{text-align:right;flex-shrink:0}
.fb-time{font-size:11px;color:var(--text3)}
.fb-stars{font-size:12px;color:#EF9F27;margin-top:2px}
.fb-preview{font-size:12px;color:var(--text2);line-height:1.5;display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden}

/* Right panel */
.reply-panel{min-height:400px}

@media(max-width:900px){
    .inbox-layout{grid-template-columns:1fr}
    .feedback-list{max-height:300px}
}
</style>
<script>
function selectFeedback(el) {
    document.querySelectorAll('.feedback-item').forEach(f => f.classList.remove('selected'));
    el.classList.add('selected');
}
</script>
</asp:Content>
