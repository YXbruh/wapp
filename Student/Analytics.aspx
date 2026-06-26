<%@ Page Title="Analytics – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Analytics.aspx.cs" Inherits="CSA.Student.Student_Analytics" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Student menu">
        <div class="sidebar-section">Main</div>
        <a href="Student_Dashboard.aspx"    class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="MyCourses.aspx"    class="sidebar-link"><i class="ti ti-books"></i>My Courses</a>
        <a href="Labs.aspx"         class="sidebar-link"><i class="ti ti-terminal-2"></i>Virtual Labs</a>
        <a href="Challenges.aspx"   class="sidebar-link"><i class="ti ti-trophy"></i>Challenges</a>
        <div class="sidebar-section">Progress</div>
        <a href="Analytics.aspx"    class="sidebar-link active"><i class="ti ti-chart-bar"></i>Analytics</a>
        <a href="Certificates.aspx" class="sidebar-link"><i class="ti ti-certificate"></i>Certificates</a>
        <a href="Achievements.aspx" class="sidebar-link"><i class="ti ti-star"></i>Achievements</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx"      class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>My Analytics</h2>
            <p>A detailed view of your learning progress and performance.</p>
        </div>

        <!-- Top stat blocks -->
        <div class="metrics" style="margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Overall Progress</div>
                <div class="metric-val"><asp:Literal ID="litOverall" runat="server" Text="0%" /></div>
                <div class="metric-sub">across all courses</div>
            </div>
            <div class="metric">
                <div class="metric-label">Avg Quiz Score</div>
                <div class="metric-val"><asp:Literal ID="litAvgQuiz" runat="server" Text="—" /></div>
                <div class="metric-sub">all attempts</div>
            </div>
            <div class="metric">
                <div class="metric-label">Labs Completed</div>
                <div class="metric-val"><asp:Literal ID="litLabsDone" runat="server" Text="0" /></div>
                <div class="metric-sub">out of <asp:Literal ID="litLabsTotal" runat="server" Text="0" /></div>
            </div>
            <div class="metric">
                <div class="metric-label">Total Study Time</div>
                <div class="metric-val"><asp:Literal ID="litStudyTime" runat="server" Text="0h" /></div>
                <div class="metric-sub">logged sessions</div>
            </div>
        </div>

        <div class="cards-row">

            <!-- Per-course progress bars -->
            <div class="card">
                <div class="card-header">Course Progress</div>
                <asp:Repeater ID="rptCourseProgress" runat="server">
                    <ItemTemplate>
                        <div style="margin-bottom:16px">
                            <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:6px">
                                <span style="font-size:13px;font-weight:600;color:var(--text)"><%# Eval("CourseName") %></span>
                                <span style="font-size:12px;color:var(--accent2);font-weight:700"><%# Eval("Progress") %>%</span>
                            </div>
                            <div class="progress-bar" style="height:6px">
                                <div class="progress-fill" style="width:<%# Eval("Progress") %>%"></div>
                            </div>
                            <div style="display:flex;justify-content:space-between;font-size:11px;color:var(--text3);margin-top:4px">
                                <span><%# Eval("CompletedModules") %> / <%# Eval("TotalModules") %> modules</span>
                                <span class="badge <%# Eval("StatusBadgeClass") %>"><%# Eval("StatusLabel") %></span>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoCourses" runat="server" Visible="false">
                    <p class="text-muted text-small">No courses enrolled yet.</p>
                </asp:Panel>
            </div>

            <!-- Quiz performance progress bars -->
            <div class="card">
                <div class="card-header">Quiz Performance</div>
                <asp:Repeater ID="rptQuizScores" runat="server">
                    <ItemTemplate>
                        <div style="margin-bottom:16px">
                            <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:6px">
                                <span style="font-size:13px;font-weight:600;color:var(--text)"><%# Eval("QuizName") %></span>
                                <span style="font-size:12px;color:var(--accent2);font-weight:700"><%# Eval("Score") %>%</span>
                            </div>
                            <div class="progress-bar" style="height:6px">
                                <div class="progress-fill" style="width:<%# Eval("Score") %>%;background:<%# GetScoreColor(Convert.ToInt32(Eval("Score"))) %>"></div>
                            </div>
                            <div style="font-size:11px;color:var(--text3);margin-top:4px">
                                Attempted: <%# Eval("AttemptedOn") %>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoQuizzes" runat="server" Visible="false">
                    <p class="text-muted text-small">No quiz attempts yet.</p>
                </asp:Panel>
            </div>

        </div>

        <!-- Lab completion progress -->
        <div class="card mt-16">
            <div class="card-header">Lab Completion by Course</div>
            <asp:Repeater ID="rptLabProgress" runat="server">
                <ItemTemplate>
                    <div style="display:flex;align-items:center;gap:14px;padding:10px 0;border-bottom:1px solid var(--bg3)">
                        <div style="width:36px;height:36px;background:var(--accent1);border-radius:8px;display:flex;align-items:center;justify-content:center;color:var(--accent3);flex-shrink:0">
                            <i class="ti ti-terminal-2" aria-hidden="true"></i>
                        </div>
                        <div style="flex:1;min-width:0">
                            <div style="font-size:13px;font-weight:600;color:var(--text);margin-bottom:4px"><%# Eval("CourseName") %></div>
                            <div class="progress-bar" style="height:5px">
                                <div class="progress-fill" style="width:<%# Eval("LabProgressPct") %>%"></div>
                            </div>
                        </div>
                        <div style="font-size:12px;color:var(--text3);flex-shrink:0;text-align:right">
                            <%# Eval("LabsDone") %> / <%# Eval("LabsTotal") %> labs
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <asp:Panel ID="pnlNoLabs" runat="server" Visible="false">
                <p class="text-muted text-small mt-8">No lab data available yet.</p>
            </asp:Panel>
        </div>

    </main>
</div>
</asp:Content>
