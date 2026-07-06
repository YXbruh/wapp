<%@ Page Title="My Courses – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="MyCourses.aspx.cs" Inherits="CSA.Student.Student_MyCourses" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Student menu">
        <div class="sidebar-section">Main</div>
        <a href="Student_Dashboard.aspx"    class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="MyCourses.aspx"    class="sidebar-link active"><i class="ti ti-books"></i>My Courses</a>
        <a href="Labs.aspx"         class="sidebar-link"><i class="ti ti-terminal-2"></i>Virtual Labs</a>
        <a href="Challenges.aspx"   class="sidebar-link"><i class="ti ti-trophy"></i>Challenges</a>
        <div class="sidebar-section">Progress</div>
        <a href="Analytics.aspx"    class="sidebar-link"><i class="ti ti-chart-bar"></i>Analytics</a>
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
            <h2>My Courses</h2>
            <p>Track your enrolled courses and continue where you left off.</p>
        </div>

        <!-- Filter bar -->
        <div class="filter-bar" role="group" aria-label="Filter by status">
            <button type="button" class="filter-chip active" onclick="filterCourses('all', this)">All</button>
            <button type="button" class="filter-chip" onclick="filterCourses('inprogress', this)">In Progress</button>
            <button type="button" class="filter-chip" onclick="filterCourses('completed', this)">Completed</button>
            <button type="button" class="filter-chip" onclick="filterCourses('notstarted', this)">Not Started</button>
        </div>

        <!-- Course cards grid -->
        <div class="courses-grid" role="list" id="courseGrid">
            <asp:Repeater ID="rptCourses" runat="server">
                <ItemTemplate>
                    <div class="course-card" role="listitem"
                         data-status='<%# Eval("StatusKey") %>'>
                        <div class="course-thumb">
                            <i class="ti <%# Eval("IconClass") %>" aria-hidden="true"></i>
                            <span class="course-level badge <%# Eval("LevelBadgeClass") %>">
                                <%# Eval("Level") %>
                            </span>
                        </div>
                        <div class="course-body">
                            <h3><%# Eval("CourseName") %></h3>
                            <p><%# Eval("Description") %></p>

                            <!-- Progress bar inside card -->
                            <div style="margin-bottom:10px">
                                <div style="display:flex;justify-content:space-between;font-size:11px;color:var(--text3);margin-bottom:4px">
                                    <span>Progress</span>
                                    <span><%# Eval("Progress") %>%</span>
                                </div>
                                <div class="progress-bar">
                                    <div class="progress-fill" style="width:<%# Eval("Progress") %>%"></div>
                                </div>
                            </div>

                            <div class="course-meta">
                                <span><i class="ti ti-clock" aria-hidden="true"></i> <%# Eval("DurationHours") %>h</span>
                                <span><i class="ti ti-terminal-2" aria-hidden="true"></i> <%# Eval("LabCount") %> labs</span>
                                <span>
                                    <span class="badge <%# Eval("StatusBadgeClass") %>"><%# Eval("StatusLabel") %></span>
                                </span>
                            </div>
                        </div>
                        <div class="course-footer">
                            <span class="text-muted text-small"><%# Eval("InstructorName") %></span>
                            <a href='CourseDetail.aspx?id=<%# Eval("CourseID") %>' class="enroll-btn">
                                <%# Eval("Progress").ToString() == "0" ? "Start" : "Continue" %>
                            </a>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
            <div style="text-align:center;padding:60px 20px">
                <i class="ti ti-books" style="font-size:48px;color:var(--text3)" aria-hidden="true"></i>
                <p class="text-muted mt-16">You haven't enrolled in any courses yet.</p>
                <a href="../Courses.aspx" class="btn-primary mt-16" style="display:inline-flex;margin-top:16px">
                    <i class="ti ti-search" aria-hidden="true"></i>Browse Courses
                </a>
            </div>
        </asp:Panel>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<script>
function filterCourses(status, btn) {
    document.querySelectorAll('.filter-chip').forEach(c => c.classList.remove('active'));
    btn.classList.add('active');
    document.querySelectorAll('#courseGrid .course-card').forEach(card => {
        card.style.display = (status === 'all' || card.dataset.status === status) ? '' : 'none';
    });
}
</script>
</asp:Content>
