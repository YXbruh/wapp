<%@ Page Title="Student Detail – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="StudentDetail.aspx.cs" Inherits="CSA.Lecturer.StudentDetail" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Lecturer menu">
        <div class="sidebar-section">Lecturer</div>
        <a href="Lecturer_Dashboard.aspx"       class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="ManageContent.aspx"   class="sidebar-link"><i class="ti ti-files"></i>Manage Content</a>
        <a href="TerminalSandbox.aspx" class="sidebar-link"><i class="ti ti-terminal-2"></i>Terminal Sandbox</a>
        <a href="QuizEditor.aspx"      class="sidebar-link"><i class="ti ti-list-check"></i>Quiz Editor</a>
        <div class="sidebar-section">Students</div>
        <a href="ClassAnalytics.aspx"  class="sidebar-link active"><i class="ti ti-chart-bar"></i>Class Analytics</a>
        <a href="Mentorship.aspx"      class="sidebar-link"><i class="ti ti-messages"></i>Mentorship</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">

        <!-- Breadcrumb -->
        <div style="font-size:12px;color:var(--text3);margin-bottom:16px">
            <a href="ClassAnalytics.aspx" style="color:var(--accent2)">
                <i class="ti ti-arrow-left" aria-hidden="true"></i> Back to Class Analytics
            </a>
        </div>

        <!-- Student header card -->
        <div class="card mb-24">
            <div style="display:flex;align-items:center;gap:16px">
                <div class="profile-avatar" style="width:52px;height:52px;font-size:18px">
                    <asp:Literal ID="litInitials" runat="server" Text="S" />
                </div>
                <div style="flex:1">
                    <div style="font-size:18px;font-weight:700;color:var(--text)">
                        <asp:Literal ID="litName" runat="server" />
                    </div>
                    <div class="text-muted text-small mt-4">
                        <asp:Literal ID="litEmail" runat="server" />
                        &middot; Enrolled: <asp:Literal ID="litEnrolled" runat="server" />
                        &middot; Last active: <asp:Literal ID="litLastActive" runat="server" />
                    </div>
                </div>
                <button type="button" class="btn-sm" onclick="showFeedbackModal()">
                    <i class="ti ti-message" aria-hidden="true"></i> Send Feedback
                </button>
            </div>
        </div>

        <!-- Send Feedback modal -->
        <div id="sendFeedbackModal" class="modal-overlay" style="display:none">
            <div class="modal-box" style="max-width:480px;text-align:left">
                <div class="modal-icon"><i class="ti ti-message"></i></div>
                <p class="modal-text" style="margin-bottom:14px">
                    Send feedback to <asp:Literal ID="litModalStudentName" runat="server" />
                </p>

                <asp:Panel ID="pnlFeedbackSuccess" runat="server" Visible="false">
                    <div class="alert-success mb-16">
                        <i class="ti ti-circle-check" aria-hidden="true"></i>
                        <asp:Literal ID="litFeedbackSuccess" runat="server" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlFeedbackError" runat="server" Visible="false">
                    <div class="validation-summary-errors mb-16">
                        <asp:Literal ID="litFeedbackError" runat="server" />
                    </div>
                </asp:Panel>

                <asp:TextBox ID="tbFeedbackMessage" runat="server" CssClass="form-input"
                    TextMode="MultiLine" Rows="5" MaxLength="2000" style="width:100%;margin-bottom:14px"
                    placeholder="Write feedback, guidance, or remediation advice for this student..." />

                <div style="display:flex;justify-content:center;gap:12px">
                    <button type="button" class="form-submit" onclick="closeFeedbackModal()"
                        style="width:auto;min-width:120px;background:var(--bg2);color:var(--text)">Cancel</button>
                    <asp:Button ID="btnSendFeedback" runat="server" CssClass="form-submit"
                        OnClick="btnSendFeedback_Click" Text="Send" style="width:auto;min-width:120px" />
                </div>
            </div>
        </div>

        <!-- Individual metrics -->
        <div class="metrics" style="margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Quiz Average</div>
                <div class="metric-val" style="color:var(--accent2)">
                    <asp:Literal ID="litQuizAvg" runat="server" Text="—"/>
                </div>
                <div class="metric-sub">across all quizzes</div>
            </div>
            <div class="metric">
                <div class="metric-label">Labs Completed</div>
                <div class="metric-val"><asp:Literal ID="litLabsDone" runat="server" Text="0"/></div>
                <div class="metric-sub">out of <asp:Literal ID="litLabsTotal" runat="server" Text="0"/></div>
            </div>
            <div class="metric">
                <div class="metric-label">Sandbox Status</div>
                <div class="metric-val" style="font-size:14px">
                    <asp:Literal ID="litSandbox" runat="server" Text="Pending"/>
                </div>
                <div class="metric-sub">terminal labs</div>
            </div>
        </div>

        <div class="cards-row">

            <!-- Quiz attempt detail -->
            <div class="card">
                <div class="card-header">Quiz Attempts</div>
                <div style="overflow-x:auto">
                    <table class="admin-table">
                        <thead>
                            <tr><th>Quiz</th><th>Score</th><th>Attempts</th><th>Status</th><th>Date</th></tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptQuizAttempts" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td class="fw-bold" style="color:var(--text)"><%#: Eval("QuizName") %></td>
                                        <td>
                                            <span style='<%# "font-weight:700;color:" + GetScoreColor(Convert.ToInt32(Eval("Score"))) %>'>
                                                <%# Eval("Score") %>%
                                            </span>
                                        </td>
                                        <td class="text-muted"><%#: Eval("AttemptCount") %></td>
                                        <td><span class="badge <%# Convert.ToInt32(Eval("Score")) >= 50 ? "badge-green":"badge-red" %>">
                                            <%# Convert.ToInt32(Eval("Score")) >= 50 ? "Pass":"Fail" %>
                                        </span></td>
                                        <td class="text-muted text-small"><%#: Eval("LastAttemptDisplay") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <asp:Panel ID="pnlNoQuiz" runat="server" Visible="false">
                    <p class="text-muted text-small mt-8 text-center">No quiz attempts yet.</p>
                </asp:Panel>
            </div>

            <!-- Lab completion status -->
            <div class="card">
                <div class="card-header">Lab Completion</div>
                <asp:Repeater ID="rptLabs" runat="server">
                    <ItemTemplate>
                        <div style="display:flex;align-items:center;gap:12px;padding:9px 0;border-bottom:1px solid var(--bg3)">
                            <div style="width:32px;height:32px;background:var(--accent1);border-radius:7px;display:flex;align-items:center;justify-content:center;color:var(--accent3);font-size:15px;flex-shrink:0">
                                <i class="ti ti-terminal-2" aria-hidden="true"></i>
                            </div>
                            <div style="flex:1;min-width:0">
                                <div style="font-size:13px;font-weight:600;color:var(--text)"><%#: Eval("LabName") %></div>
                                <div class="text-small text-muted"><%#: Eval("CompletedDisplay") %></div>
                            </div>
                            <span class="badge <%# GetLabStatusBadge(Eval("Status").ToString()) %>">
                                <i class="ti <%# GetLabStatusIcon(Eval("Status").ToString()) %>"></i>
                                <%# Eval("Status") %>
                            </span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoLabs" runat="server" Visible="false">
                    <p class="text-muted text-small mt-8" style="text-align:center">No labs assigned yet.</p>
                </asp:Panel>
            </div>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.profile-avatar{width:52px;height:52px;border-radius:50%;background:var(--accent1);border:2px solid var(--accent2);display:flex;align-items:center;justify-content:center;font-size:18px;font-weight:700;color:var(--accent3);flex-shrink:0}
.text-center{text-align:center}
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px;text-align:left}
</style>
<script>
function showFeedbackModal() {
    document.getElementById('sendFeedbackModal').style.display = 'flex';
}
function closeFeedbackModal() {
    document.getElementById('sendFeedbackModal').style.display = 'none';
}
</script>
</asp:Content>