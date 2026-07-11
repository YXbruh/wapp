<%@ Page Title="Student Dashboard – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Student_Dashboard.aspx.cs" Inherits="CSA.Student.Student_Dashboard" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <!-- ===== SIDEBAR ===== -->
    <aside class="sidebar" role="navigation" aria-label="Student menu">
        <div class="sidebar-section">Main</div>
        <a href="Dashboard.aspx"   class="sidebar-link active"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="MyCourses.aspx"   class="sidebar-link"><i class="ti ti-books"></i>My Courses
            <span class="sidebar-badge"><asp:Literal ID="litCourseCount" runat="server" Text="0" /></span>
        </a>
        <a href="Labs.aspx"        class="sidebar-link"><i class="ti ti-terminal-2"></i>Virtual Labs</a>
        <a href="Challenges.aspx"  class="sidebar-link"><i class="ti ti-trophy"></i>Challenges
            <span class="sidebar-badge"><asp:Literal ID="litChallengeCount" runat="server" Text="0" /></span>
        </a>

        <div class="sidebar-section">Progress</div>
        <a href="Analytics.aspx"   class="sidebar-link"><i class="ti ti-chart-bar"></i>Analytics</a>
        <a href="Certificates.aspx"class="sidebar-link"><i class="ti ti-certificate"></i>Certificates</a>
        <a href="Achievements.aspx"class="sidebar-link"><i class="ti ti-star"></i>Achievements</a>

        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx"     class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <a href="Settings.aspx"    class="sidebar-link"><i class="ti ti-settings"></i>Settings</a>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link"
                        OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <!-- ===== MAIN CONTENT ===== -->
    <main class="dash-content">

        <!-- Header -->
        <div class="dash-header">
            <h2>Welcome back, <asp:Literal ID="litName" runat="server" Text="Student" /> &#128075;</h2>
            <p><asp:Literal ID="litSubtitle" runat="server" Text="Loading your activity..." /></p>
        </div>

        <!-- Metrics -->
        <div class="metrics" role="region" aria-label="Your stats">
            <div class="metric">
                <div class="metric-label">Courses</div>
                <div class="metric-val"><asp:Literal ID="litMetricCourses" runat="server" Text="0" /></div>
                <div class="metric-sub">enrolled</div>
            </div>
            <div class="metric">
                <div class="metric-label">Labs Done</div>
                <div class="metric-val"><asp:Literal ID="litMetricLabs" runat="server" Text="0" /></div>
                <div class="metric-sub">completed</div>
            </div>
            <div class="metric">
                <div class="metric-label">Quiz Score</div>
                <div class="metric-val"><asp:Literal ID="litMetricQuiz" runat="server" Text="—" /></div>
                <div class="metric-sub">average</div>
            </div>
            <div class="metric">
                <div class="metric-label">Badges</div>
                <div class="metric-val"><asp:Literal ID="litMetricBadges" runat="server" Text="0" /></div>
                <div class="metric-sub">earned</div>
            </div>
        </div>

        <!-- Two-col cards -->
        <div class="cards-row">

            <!-- Active Courses card -->
            <div class="card">
                <div class="card-header">
                    Active Courses
                    <a href="MyCourses.aspx">View all &rarr;</a>
                </div>
                <asp:Repeater ID="rptCourses" runat="server">
                    <ItemTemplate>
                        <div class="course-item">
                            <div class="course-icon">
                                <i class="ti ti-books" aria-hidden="true"></i>
                            </div>
                            <div class="course-info">
                                <div class="course-name"><%# Eval("CourseName") %></div>
                                <div class="course-prog"><%# Eval("Progress") %>% complete</div>
                                <div class="progress-bar">
                                    <div class="progress-fill" style="width:<%# Eval("Progress") %>%"></div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoCourses" runat="server" Visible="false">
                    <p class="text-muted text-small mt-8">
                        No courses yet. <a href="../Courses.aspx">Browse courses</a>
                    </p>
                </asp:Panel>
            </div>

            <!-- Recent Activity card -->
            <div class="card">
                <div class="card-header">
                    Recent Activity
                    <a href="ActivityLog.aspx">View all &rarr;</a>
                </div>
                <asp:Repeater ID="rptActivity" runat="server">
                    <ItemTemplate>
                        <div class="activity-item">
                            <div class="activity-dot" aria-hidden="true"></div>
                            <div>
                                <div class="activity-text"><%# Eval("Description") %></div>
                                <div class="activity-time"><%# Eval("TimeAgo") %></div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoActivity" runat="server" Visible="false">
                    <p class="text-muted text-small mt-8">No recent activity yet.</p>
                </asp:Panel>
            </div>

        </div>
    </main>
</div>
</asp:Content>