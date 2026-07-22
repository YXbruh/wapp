<%@ Page Title="Student Dashboard – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Student_Dashboard.aspx.cs"
    Inherits="CSA.Student.Student_Dashboard" %>

<asp:Content ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<div class="dash-layout">

    <aside class="sidebar"
        role="navigation"
        aria-label="Student menu">

        <div class="sidebar-section">Main</div>

        <a href="Student_Dashboard.aspx"
            class="sidebar-link active">
            <i class="ti ti-layout-dashboard"></i>
            Dashboard
        </a>

        <a href="MyCourses.aspx"
            class="sidebar-link">
            <i class="ti ti-books"></i>
            My Courses
        </a>

        <a href="Labs.aspx"
            class="sidebar-link">
            <i class="ti ti-terminal-2"></i>
            Virtual Labs
        </a>

        <a href="Challenges.aspx"
            class="sidebar-link">
            <i class="ti ti-trophy"></i>
            Challenges
        </a>

        <div class="sidebar-section">Progress</div>

        <a href="Analytics.aspx"
            class="sidebar-link">
            <i class="ti ti-chart-bar"></i>
            Analytics
        </a>

        <a href="Certificates.aspx"
            class="sidebar-link">
            <i class="ti ti-certificate"></i>
            Certificates
        </a>

        <a href="Achievements.aspx"
            class="sidebar-link">
            <i class="ti ti-star"></i>
            Achievements
        </a>

        <div class="sidebar-section">Account</div>

        <a href="Profile.aspx"
            class="sidebar-link">
            <i class="ti ti-user"></i>
            Profile
        </a>

        <asp:LinkButton
            ID="lbLogout"
            runat="server"
            CssClass="sidebar-link"
            CausesValidation="false"
            OnClick="lbLogout_Click">

            <i class="ti ti-logout"></i>
            Sign Out

        </asp:LinkButton>

    </aside>

    <main class="dash-content">

        <div class="dash-header">
            <h2>
                Welcome back,
                <asp:Literal
                    ID="litName"
                    runat="server" />!
            </h2>

            <p>
                <asp:Literal
                    ID="litSubtitle"
                    runat="server" />
            </p>
        </div>

        <div class="metrics">

            <div class="metric">
                <div class="metric-label">
                    Enrolled Courses
                </div>

                <div class="metric-val">
                    <asp:Literal
                        ID="litMetricCourses"
                        runat="server"
                        Text="0" />
                </div>

                <div class="metric-sub">
                    total enrolments
                </div>
            </div>

            <div class="metric">
                <div class="metric-label">
                    Labs Completed
                </div>

                <div class="metric-val">
                    <asp:Literal
                        ID="litMetricLabs"
                        runat="server"
                        Text="0" />
                </div>

                <div class="metric-sub">
                    passed labs
                </div>
            </div>

            <div class="metric">
                <div class="metric-label">
                    Quiz Score
                </div>

                <div class="metric-val">
                    <asp:Literal
                        ID="litMetricQuiz"
                        runat="server"
                        Text="—" />
                </div>

                <div class="metric-sub">
                    average
                </div>
            </div>

            <div class="metric">
                <div class="metric-label">
                    Badges
                </div>

                <div class="metric-val">
                    <asp:Literal
                        ID="litMetricBadges"
                        runat="server"
                        Text="0" />
                </div>

                <div class="metric-sub">
                    earned
                </div>
            </div>

        </div>

        <div class="cards-row">

            <div class="card">

                <div class="card-header">
                    My Courses

                    <a href="MyCourses.aspx">
                        View all &rarr;
                    </a>
                </div>

                <asp:Repeater
                    ID="rptCourses"
                    runat="server">

                    <ItemTemplate>

                        <div class="course-item">

                            <div class="course-icon">
                                <i class="ti ti-books"></i>
                            </div>

                            <div class="course-info">

                                <div class="course-name">
                                    <%# Eval("CourseName") %>
                                </div>

                                <div class="course-prog">
                                    <%# Eval("Progress", "{0:0}") %>%
                                    complete —
                                    <%# Eval("Status") %>
                                </div>

                                <div class="progress-bar">

                                    <div class="progress-fill"
                                        style='<%#
                                            "width:" +
                                            Eval("Progress", "{0:0}") +
                                            "%"
                                        %>'>
                                    </div>

                                </div>

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

                <asp:Panel
                    ID="pnlNoCourses"
                    runat="server"
                    Visible="false">

                    <p class="text-muted text-small mt-8">
                        You have not enrolled in any course.
                        Open
                        <a href="MyCourses.aspx">
                            My Courses
                        </a>
                        to browse available courses.
                    </p>

                </asp:Panel>

            </div>

            <div class="card">

                <div class="card-header">
                    Recent Activity

                    <span class="text-muted text-small">
                        Latest updates
                    </span>
                </div>

                <asp:Repeater
                    ID="rptActivity"
                    runat="server">

                    <ItemTemplate>

                        <div class="activity-item">

                            <div class="activity-dot"></div>

                            <div>

                                <div class="activity-text">
                                    <%# Eval("Description") %>
                                </div>

                                <div class="activity-time">
                                    <%# Eval("TimeAgo") %>
                                </div>

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

                <asp:Panel
                    ID="pnlNoActivity"
                    runat="server"
                    Visible="false">

                    <p class="text-muted text-small mt-8">
                        No recent activity yet.
                    </p>

                </asp:Panel>

            </div>

        </div>

    </main>

</div>

</asp:Content>