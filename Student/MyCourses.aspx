<%@ Page Title="My Courses – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="MyCourses.aspx.cs"
    Inherits="CSA.Student.Student_MyCourses" %>

<asp:Content ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .section-title {
        display:flex;
        justify-content:space-between;
        align-items:center;
        margin:28px 0 14px;
    }

    .chapter-card {
        border:1px solid var(--border);
        border-radius:10px;
        padding:16px;
        margin-bottom:12px;
        background:var(--surface);
    }

    .chapter-top {
        display:flex;
        justify-content:space-between;
        align-items:center;
        gap:12px;
    }

    .chapter-content {
        display:none;
        border-top:1px solid var(--border);
        margin-top:14px;
        padding-top:14px;
        white-space:pre-line;
        line-height:1.7;
        color:var(--text2);
    }

    .message-success {
        padding:12px 15px;
        border:1px solid var(--success);
        color:var(--success);
        border-radius:8px;
        margin-bottom:16px;
    }
</style>

<div class="dash-layout">

    <aside class="sidebar">

        <div class="sidebar-section">Main</div>

        <a href="Student_Dashboard.aspx"
            class="sidebar-link">
            <i class="ti ti-layout-dashboard"></i>
            Dashboard
        </a>

        <a href="MyCourses.aspx"
            class="sidebar-link active">
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

        <asp:Panel
            ID="pnlCoursePage"
            runat="server">

            <div class="dash-header">
                <h2>My Courses</h2>
                <p>
                    Continue enrolled courses or enrol
                    in another available course.
                </p>
            </div>

            <asp:Panel
                ID="pnlMessage"
                runat="server"
                Visible="false"
                CssClass="message-success">

                <asp:Literal
                    ID="litMessage"
                    runat="server" />

            </asp:Panel>

            <div class="section-title">
                <h3>Enrolled Courses</h3>
            </div>

            <div class="courses-grid">

                <asp:Repeater
                    ID="rptCourses"
                    runat="server"
                    OnItemCommand="rptCourses_ItemCommand">

                    <ItemTemplate>

                        <div class="course-card">

                            <div class="course-thumb">

                                <i class="ti ti-shield-lock"></i>

                                <span class="course-level badge <%# Eval("LevelClass") %>">
                                    <%# Eval("Level") %>
                                </span>

                            </div>

                            <div class="course-body">

                                <h3>
                                    <%# Eval("CourseName") %>
                                </h3>

                                <p>
                                    <%# Eval("Description") %>
                                </p>

                                <div style="margin-bottom:10px">

                                    <div style="display:flex;justify-content:space-between;font-size:11px;margin-bottom:4px">

                                        <span>Progress</span>

                                        <span>
                                            <%# Eval("Progress", "{0:0}") %>%
                                        </span>

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

                                <div class="course-meta">

                                    <span>
                                        <%# Eval("ChapterCount") %>
                                        chapters
                                    </span>

                                    <span>
                                        <%# Eval("LabCount") %>
                                        labs
                                    </span>

                                    <span class="badge <%# Eval("StatusClass") %>">
                                        <%# Eval("Status") %>
                                    </span>

                                </div>

                            </div>

                            <div class="course-footer">

                                <span class="text-muted text-small">
                                    <%# Eval("InstructorName") %>
                                </span>

                                <asp:LinkButton
                                    ID="btnOpen"
                                    runat="server"
                                    CssClass="enroll-btn"
                                    CausesValidation="false"
                                    CommandName="OpenCourse"
                                    CommandArgument='<%# Eval("CourseID") %>'>

                                    Open Course

                                </asp:LinkButton>

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

            </div>

            <asp:Panel
                ID="pnlNoEnrolled"
                runat="server"
                Visible="false">

                <p class="text-muted">
                    You have not enrolled in any course.
                </p>

            </asp:Panel>

            <div class="section-title">
                <h3>Available Courses</h3>
            </div>

            <div class="courses-grid">

                <asp:Repeater
                    ID="rptAvailable"
                    runat="server"
                    OnItemCommand="rptAvailable_ItemCommand">

                    <ItemTemplate>

                        <div class="course-card">

                            <div class="course-thumb">

                                <i class="ti ti-book"></i>

                                <span class="course-level badge <%# Eval("LevelClass") %>">
                                    <%# Eval("Level") %>
                                </span>

                            </div>

                            <div class="course-body">

                                <h3>
                                    <%# Eval("CourseName") %>
                                </h3>

                                <p>
                                    <%# Eval("Description") %>
                                </p>

                                <div class="course-meta">

                                    <span>
                                        <%# Eval("ChapterCount") %>
                                        chapters
                                    </span>

                                    <span>
                                        <%# Eval("LabCount") %>
                                        labs
                                    </span>

                                </div>

                            </div>

                            <div class="course-footer">

                                <span class="text-muted text-small">
                                    <%# Eval("InstructorName") %>
                                </span>

                                <asp:LinkButton
                                    ID="btnEnroll"
                                    runat="server"
                                    CssClass="enroll-btn"
                                    CausesValidation="false"
                                    CommandName="Enroll"
                                    CommandArgument='<%# Eval("CourseID") %>'>

                                    Enrol Now

                                </asp:LinkButton>

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

            </div>

            <asp:Panel
                ID="pnlNoAvailable"
                runat="server"
                Visible="false">

                <p class="text-muted">
                    There are no other published courses available.
                </p>

            </asp:Panel>

        </asp:Panel>

        <asp:Panel
            ID="pnlCourseDetails"
            runat="server"
            Visible="false">

            <asp:LinkButton
                ID="btnBack"
                runat="server"
                CssClass="btn-sm secondary"
                CausesValidation="false"
                OnClick="btnBack_Click">

                <i class="ti ti-arrow-left"></i>
                Back to Courses

            </asp:LinkButton>

            <div class="card"
                style="margin-top:16px">

                <h2>
                    <asp:Literal
                        ID="litCourseName"
                        runat="server" />
                </h2>

                <p class="text-muted">
                    <asp:Literal
                        ID="litCourseDescription"
                        runat="server" />
                </p>

                <div style="margin:20px 0">

                    <div style="display:flex;justify-content:space-between">

                        <span>Course Progress</span>

                        <strong>
                            <asp:Literal
                                ID="litProgress"
                                runat="server" />%
                        </strong>

                    </div>

                    <div class="progress-bar"
                        style="margin-top:6px">

                        <div
                            ID="progressFill"
                            runat="server"
                            class="progress-fill">
                        </div>

                    </div>

                </div>

                <h3>Chapters</h3>

                <asp:Repeater
                    ID="rptChapters"
                    runat="server"
                    OnItemCommand="rptChapters_ItemCommand">

                    <ItemTemplate>

                        <div class="chapter-card">

                            <div class="chapter-top">

                                <div>

                                    <strong>
                                        Chapter
                                        <%# Eval("SortOrder") %>:
                                        <%# Eval("ChapterTitle") %>
                                    </strong>

                                    <div class="text-muted text-small">
                                        <%# GetChapterStatus(
                                            Eval("IsCompleted")) %>
                                    </div>

                                </div>

                                <div>

                                    <button
                                        type="button"
                                        class="btn-sm secondary"
                                        data-chapter='<%# Eval("ChapterID") %>'
                                        onclick="toggleChapter(this)">

                                        View

                                    </button>

                                    <asp:LinkButton
                                        ID="btnComplete"
                                        runat="server"
                                        CssClass="btn-sm"
                                        CausesValidation="false"
                                        CommandName="Complete"
                                        CommandArgument='<%# Eval("ChapterID") %>'
                                        Visible='<%# ShowCompleteButton(
                                            Eval("IsCompleted")) %>'>

                                        Mark Complete

                                    </asp:LinkButton>

                                    <asp:Label
                                        ID="lblDone"
                                        runat="server"
                                        CssClass="badge badge-green"
                                        Text="Completed"
                                        Visible='<%# IsCompleted(
                                            Eval("IsCompleted")) %>' />

                                </div>

                            </div>

                            <div
                                id='chapter_<%# Eval("ChapterID") %>'
                                class="chapter-content">

                                <%# Eval("Content") %>

                                <asp:HyperLink
                                    ID="hlAttachment"
                                    runat="server"
                                    NavigateUrl='<%# Eval("FilePath") %>'
                                    Text="Open attachment"
                                    Target="_blank"
                                    Visible='<%# HasAttachment(
                                        Eval("FilePath")) %>' />

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

                <asp:Panel
                    ID="pnlNoChapters"
                    runat="server"
                    Visible="false">

                    <p class="text-muted">
                        No published chapters are available.
                    </p>

                </asp:Panel>

            </div>

        </asp:Panel>

    </main>

</div>

</asp:Content>

<asp:Content
    ID="cScripts"
    ContentPlaceHolderID="Scripts"
    runat="server">

<script>
    function toggleChapter(button) {
        var id =
            button.getAttribute('data-chapter');

        var content =
            document.getElementById(
                'chapter_' + id
            );

        if (!content) {
            return;
        }

        if (content.style.display === 'block') {
            content.style.display = 'none';
            button.innerText = 'View';
        }
        else {
            content.style.display = 'block';
            button.innerText = 'Hide';
        }
    }
</script>

</asp:Content>