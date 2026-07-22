<%@ Page Title="My Courses – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="MyCourses.aspx.cs"
    Inherits="CSA.Student.Student_MyCourses" %>

<asp:Content
    ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .section-title {
        display:flex;
        align-items:center;
        justify-content:space-between;
        margin:28px 0 14px;
    }

    .chapter-card {
        margin-bottom:12px;
        padding:16px;
        background:var(--surface);
        border:1px solid var(--border);
        border-radius:10px;
    }

    .chapter-top {
        display:flex;
        align-items:center;
        justify-content:space-between;
        gap:12px;
    }

    .chapter-actions {
        display:flex;
        align-items:center;
        gap:8px;
    }

    .chapter-content {
        display:none;
        margin-top:14px;
        padding-top:14px;
        color:var(--text2);
        border-top:1px solid var(--border);
        line-height:1.7;
    }

    .chapter-text {
        margin:0;
    }

    .resource-list {
        margin-top:12px;
        padding-top:12px;
        border-top:1px solid var(--border);
    }

    .resource-item {
        display:flex;
        align-items:center;
        gap:9px;
        margin-top:8px;
        padding:9px 11px;
        color:var(--text2);
        text-decoration:none;
        background:var(--bg3);
        border:1px solid var(--border);
        border-radius:7px;
    }

    .resource-item:hover {
        color:var(--accent3);
        border-color:var(--accent2);
    }

    .message-success {
        margin-bottom:16px;
        padding:12px 15px;
        color:var(--success);
        border:1px solid var(--success);
        border-radius:8px;
    }

    @media (max-width:650px) {
        .chapter-top {
            align-items:flex-start;
            flex-direction:column;
        }

        .chapter-actions {
            width:100%;
            flex-wrap:wrap;
        }
    }
</style>

<div class="dash-layout">

    <aside class="sidebar">

        <div class="sidebar-section">Main</div>

        <a href="Student_Dashboard.aspx" class="sidebar-link">
            <i class="ti ti-layout-dashboard"></i>
            Dashboard
        </a>

        <a href="MyCourses.aspx" class="sidebar-link active">
            <i class="ti ti-books"></i>
            My Courses
        </a>

        <a href="Labs.aspx" class="sidebar-link">
            <i class="ti ti-terminal-2"></i>
            Virtual Labs
        </a>

        <a href="Challenges.aspx" class="sidebar-link">
            <i class="ti ti-trophy"></i>
            Challenges
        </a>

        <div class="sidebar-section">Progress</div>

        <a href="Analytics.aspx" class="sidebar-link">
            <i class="ti ti-chart-bar"></i>
            Analytics
        </a>

        <a href="Certificates.aspx" class="sidebar-link">
            <i class="ti ti-certificate"></i>
            Certificates
        </a>

        <a href="Achievements.aspx" class="sidebar-link">
            <i class="ti ti-star"></i>
            Achievements
        </a>

        <div class="sidebar-section">Account</div>

        <a href="Profile.aspx" class="sidebar-link">
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
                    View your enrolled courses and discover available
                    learning content.
                </p>

            </div>

            <asp:Panel
                ID="pnlMessage"
                runat="server"
                CssClass="message-success"
                Visible="false">

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
                                        <%# Eval("ChapterCount") %> chapters
                                    </span>

                                    <span>
                                        <%# Eval("LabCount") %> labs
                                    </span>

                                </div>

                                <div style="margin-top:14px">

                                    <div style="display:flex;justify-content:space-between">

                                        <span class="text-muted text-small">
                                            Progress
                                        </span>

                                        <span class="text-muted text-small">
                                            <%# Eval("Progress", "{0:0}") %>%
                                        </span>

                                    </div>

                                    <div class="progress-bar" style="margin-top:5px">

                                        <div
                                            class="progress-fill"
                                            style='<%#
                                                "width:" +
                                                Convert.ToDecimal(Eval("Progress"))
                                                    .ToString("0") +
                                                "%"
                                            %>'>
                                        </div>

                                    </div>

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
                                        <%# Eval("ChapterCount") %> chapters
                                    </span>

                                    <span>
                                        <%# Eval("LabCount") %> labs
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

            <div class="card" style="margin-top:16px">

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

                    <div class="progress-bar" style="margin-top:6px">

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
                    OnItemCommand="rptChapters_ItemCommand"
                    OnItemDataBound="rptChapters_ItemDataBound">

                    <ItemTemplate>

                        <div class="chapter-card">

                            <div class="chapter-top">

                                <div>

                                    <strong>
                                        Chapter <%# Eval("SortOrder") %>:
                                        <%# Eval("ChapterTitle") %>
                                    </strong>

                                    <div class="text-muted text-small">
                                        <%# GetChapterStatus(Eval("IsCompleted")) %>
                                    </div>

                                </div>

                                <div class="chapter-actions">

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

                                <div class="chapter-text">
                                    <%# FormatChapterContent(Eval("Content")) %>
                                </div>

                                <asp:Panel
                                    ID="pnlResources"
                                    runat="server"
                                    CssClass="resource-list"
                                    Visible="false">

                                    <strong>
                                        <i class="ti ti-paperclip"></i>
                                        Resources
                                    </strong>

                                    <asp:Repeater
                                        ID="rptResources"
                                        runat="server">

                                        <ItemTemplate>

                                            <a
                                                class="resource-item"
                                                href='<%# GetResourceUrl(
                                                    Eval("AttachmentType"),
                                                    Eval("FilePath"),
                                                    Eval("LinkUrl")) %>'
                                                target="_blank"
                                                rel="noopener noreferrer">

                                                <i class='ti <%# GetResourceIcon(
                                                    Eval("AttachmentType")) %>'>
                                                </i>

                                                <span>
                                                    <%# Server.HtmlEncode(
                                                        Convert.ToString(
                                                            Eval("Title"))) %>
                                                </span>

                                            </a>

                                        </ItemTemplate>

                                    </asp:Repeater>

                                </asp:Panel>

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
        var id = button.getAttribute("data-chapter");
        var content = document.getElementById("chapter_" + id);

        if (!content) {
            return;
        }

        var isOpen = content.style.display === "block";

        content.style.display = isOpen
            ? "none"
            : "block";

        button.innerText = isOpen
            ? "View"
            : "Hide";
    }
</script>

</asp:Content>