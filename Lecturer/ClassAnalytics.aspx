<%@ Page Title="Class Analytics – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="ClassAnalytics.aspx.cs" Inherits="CSA.Lecturer.ClassAnalytics" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Lecturer menu">
        <div class="sidebar-section">Lecturer</div>
        <a href="Lecturer_Dashboard.aspx" class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="ManageContent.aspx"   class="sidebar-link"><i class="ti ti-files"></i>Manage Content</a>
        <a href="TerminalSandbox.aspx" class="sidebar-link"><i class="ti ti-terminal-2"></i>Terminal Sandbox</a>
        <a href="QuizEditor.aspx"      class="sidebar-link"><i class="ti ti-list-check"></i>Quiz Editor</a>
        <div class="sidebar-section">Students</div>
        <a href="ClassAnalytics.aspx"  class="sidebar-link active"><i class="ti ti-chart-bar"></i>Class Analytics</a>
        <a href="Mentorship.aspx"      class="sidebar-link"><i class="ti ti-messages"></i>Mentorship</a>
        <div class="sidebar-section">Account</div>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Class Analytics</h2>
            <p>Class-wide performance across quizzes, labs, and challenges.</p>
        </div>

        <!-- Course filter -->
        <div class="toolbar" style="margin-bottom:24px">
            <label class="form-label" style="white-space:nowrap;margin-bottom:0">
                <i class="ti ti-books" aria-hidden="true"></i>Filter by Course:
            </label>
            <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select"
                style="width:260px" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_Changed" />
            <asp:LinkButton ID="lbExport" runat="server" CssClass="btn-sm secondary"
                OnClick="lbExport_Click">
                <i class="ti ti-download" aria-hidden="true"></i>Export CSV
            </asp:LinkButton>
        </div>

        <!-- Class-wide metrics -->
        <div class="metrics" style="margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Total Students</div>
                <div class="metric-val"><asp:Literal ID="litTotal" runat="server" Text="0"/></div>
                <div class="metric-sub">enrolled</div>
            </div>
            <div class="metric">
                <div class="metric-label">Avg Quiz Score</div>
                <div class="metric-val" style="color:var(--accent2)">
                    <asp:Literal ID="litAvgQuiz" runat="server" Text="—"/>
                </div>
                <div class="metric-sub">class average</div>
            </div>
            <div class="metric">
                <div class="metric-label">Lab Completion</div>
                <div class="metric-val" style="color:var(--accent2)">
                    <asp:Literal ID="litLabRate" runat="server" Text="—"/>
                </div>
                <div class="metric-sub">class average</div>
            </div>
            <div class="metric">
                <div class="metric-label">Sandbox Cleared</div>
                <div class="metric-val" style="color:var(--success)">
                    <asp:Literal ID="litSandboxCleared" runat="server" Text="0"/>
                </div>
                <div class="metric-sub">students passed</div>
            </div>
        </div>

        <!-- Student performance table -->
        <div class="card mb-24">
            <div class="card-header">
                Student Score Sheet
                <div class="search-wrap" style="max-width:260px">
                    <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                        placeholder="Search student..." AutoPostBack="true"
                        OnTextChanged="tbSearch_Changed" />
                    <i class="ti ti-search" aria-hidden="true"></i>
                </div>
            </div>
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th>Student</th>
                            <th>Quiz Avg</th>
                            <th>Labs Done</th>
                            <th>Challenges</th>
                            <th>Sandbox</th>
                            <th>Last Active</th>
                            <th>Detail</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptStudents" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="fw-bold" style="color:var(--text)"><%# Eval("FullName") %></div>
                                        <div class="text-small text-muted"><%# Eval("Email") %></div>
                                    </td>
                                    <td>
                                        <span style="font-weight:700;color:<%# GetScoreColor(Convert.ToInt32(Eval("QuizAvg"))) %>">
                                            <%# Eval("QuizAvg") %>%
                                        </span>
                                    </td>
                                    <td class="text-muted"><%# Eval("LabsDone") %> / <%# Eval("LabsTotal") %></td>
                                    <td class="text-muted"><%# Eval("ChallengesDone") %> / <%# Eval("ChallengesTotal") %></td>
                                    <td>
                                        <span class="badge <%# (bool)Eval("SandboxCleared") ? "badge-green":"badge-amber" %>">
                                            <i class="ti <%# (bool)Eval("SandboxCleared") ? "ti-circle-check":"ti-clock" %>"></i>
                                            <%# (bool)Eval("SandboxCleared") ? "Cleared":"Pending" %>
                                        </span>
                                    </td>
                                    <td class="text-muted text-small"><%# Eval("LastActiveDisplay") %></td>
                                    <td>
                                        <a href='StudentDetail.aspx?id=<%# Eval("UserID") %>'
                                           class="btn-sm secondary">
                                            <i class="ti ti-chart-bar" aria-hidden="true"></i> View
                                        </a>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <p class="text-muted text-small mt-16" style="text-align:center;padding:20px 0">
                    No students enrolled yet.
                </p>
            </asp:Panel>
        </div>

        <!-- Quiz attempt breakdown -->
        <div class="card">
            <div class="card-header">Quiz Attempt Breakdown</div>
            <div style="overflow-x:auto">
                <table class="admin-table">
                    <thead>
                        <tr>
                            <th>Quiz</th>
                            <th>Attempts</th>
                            <th>Avg Score</th>
                            <th>Highest</th>
                            <th>Lowest</th>
                            <th>Pass Rate</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptQuizBreakdown" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-bold" style="color:var(--text)"><%# Eval("QuizName") %></td>
                                    <td class="text-muted"><%# Eval("AttemptCount") %></td>
                                    <td>
                                        <span style="font-weight:700;color:<%# GetScoreColor(Convert.ToInt32(Eval("AvgScore"))) %>">
                                            <%# Eval("AvgScore") %>%
                                        </span>
                                    </td>
                                    <td style="color:var(--success)"><%# Eval("HighScore") %>%</td>
                                    <td style="color:var(--danger)"><%# Eval("LowScore") %>%</td>
                                    <td>
                                        <div style="display:flex;align-items:center;gap:8px">
                                            <div class="progress-bar" style="width:80px;height:5px">
                                                <div class="progress-fill" style="width:<%# Eval("PassRate") %>%"></div>
                                            </div>
                                            <span class="text-small text-muted"><%# Eval("PassRate") %>%</span>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
            <asp:Panel ID="pnlNoQuiz" runat="server" Visible="false">
                <p class="text-muted text-small mt-16" style="text-align:center;padding:16px 0">No quiz data yet.</p>
            </asp:Panel>
        </div>

    </main>
</div>
</asp:Content>
