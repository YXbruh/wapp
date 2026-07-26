<%@ Page Title="My Feedback – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="ViewFeedback.aspx.cs"
    Inherits="CSA.Student.Student_ViewFeedback" %>

<asp:Content
    ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .feedback-list {
        display:flex;
        flex-direction:column;
        gap:16px;
    }

    .feedback-card {
        overflow:hidden;
        color:var(--text);
        background:var(--surface);
        border:1px solid var(--border);
        border-radius:11px;
    }

    .feedback-card-header {
        display:flex;
        align-items:flex-start;
        justify-content:space-between;
        gap:16px;
        padding:16px 18px;
        background:var(--bg2);
        border-bottom:1px solid var(--border);
    }

    .feedback-card-title {
        margin:0 0 5px;
        color:var(--text);
        font-size:15px;
        font-weight:800;
    }

    .feedback-card-date {
        color:var(--text2);
        font-size:11px;
    }

    .feedback-rating {
        white-space:nowrap;
        color:var(--warning);
        font-weight:800;
    }

    .feedback-card-body {
        padding:17px 18px;
    }

    .feedback-section {
        margin-bottom:14px;
    }

    .feedback-section:last-child {
        margin-bottom:0;
    }

    .feedback-label {
        display:block;
        margin-bottom:5px;
        color:var(--text2);
        font-size:11px;
        font-weight:800;
        letter-spacing:.5px;
        text-transform:uppercase;
    }

    .feedback-text {
        color:var(--text);
        line-height:1.65;
        white-space:pre-wrap;
        overflow-wrap:anywhere;
    }

    .lecturer-reply {
        margin-top:15px;
        padding:13px;
        background:var(--bg3);
        border-left:3px solid var(--accent2);
        border-radius:7px;
    }

    .reply-meta {
        margin-top:7px;
        color:var(--text2);
        font-size:11px;
    }

    .awaiting-reply {
        display:inline-flex;
        align-items:center;
        gap:5px;
        margin-top:11px;
        padding:6px 9px;
        color:var(--warning);
        background:var(--bg3);
        border:1px solid var(--border);
        border-radius:6px;
        font-size:11px;
        font-weight:700;
    }

    .empty-feedback {
        padding:50px 20px;
        text-align:center;
        color:var(--text2);
        background:var(--surface);
        border:1px solid var(--border);
        border-radius:11px;
    }

    @media (max-width:650px) {
        .feedback-card-header {
            flex-direction:column;
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

        <a href="MyCourses.aspx" class="sidebar-link">
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

        <a href="ViewFeedback.aspx" class="sidebar-link active">
            <i class="ti ti-message-star"></i>
            My Feedback
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
            OnClientClick="return showLogoutConfirm(this);"
            OnClick="lbLogout_Click">

            <i class="ti ti-logout"></i>
            Sign Out

        </asp:LinkButton>

    </aside>

    <main class="dash-content">

        <div class="dash-header">

            <h2>My Feedback</h2>

            <p>
                View your submitted feedback and replies from lecturers.
            </p>

        </div>

        <div class="metrics"
            style="grid-template-columns:repeat(3,1fr);
                   margin-bottom:24px">

            <div class="metric">

                <div class="metric-label">Submitted</div>

                <div class="metric-val">
                    <asp:Literal
                        ID="litTotal"
                        runat="server"
                        Text="0" />
                </div>

                <div class="metric-sub">
                    feedback entries
                </div>

            </div>

            <div class="metric">

                <div class="metric-label">Replied</div>

                <div class="metric-val"
                    style="color:var(--success)">

                    <asp:Literal
                        ID="litReplied"
                        runat="server"
                        Text="0" />

                </div>

                <div class="metric-sub">
                    lecturer replies
                </div>

            </div>

            <div class="metric">

                <div class="metric-label">Awaiting Reply</div>

                <div class="metric-val"
                    style="color:var(--warning)">

                    <asp:Literal
                        ID="litPending"
                        runat="server"
                        Text="0" />

                </div>

                <div class="metric-sub">
                    pending entries
                </div>

            </div>

        </div>

        <div class="feedback-list">

            <asp:Repeater
                ID="rptFeedback"
                runat="server">

                <ItemTemplate>

                    <div class="feedback-card">

                        <div class="feedback-card-header">

                            <div>

                                <div class="feedback-card-title">

                                    <%#
                                        Server.HtmlEncode(
                                            Convert.ToString(
                                                Eval("ItemName")))
                                    %>

                                </div>

                                <div class="feedback-card-date">

                                    <%# Eval("FeedbackType") %>

                                    · Submitted

                                    <%#
                                        Convert.ToDateTime(
                                            Eval("SubmittedAt"))
                                            .ToString(
                                                "dd MMM yyyy, hh:mm tt")
                                    %>

                                </div>

                            </div>

                            <div class="feedback-rating">

                                <%# Eval("StarRating") %> / 5

                                <i class="ti ti-star-filled"></i>

                            </div>

                        </div>

                        <div class="feedback-card-body">

                            <div class="feedback-section">

                                <span class="feedback-label">
                                    Your comment
                                </span>

                                <div class="feedback-text">

                                    <%#
                                        string.IsNullOrWhiteSpace(
                                            Convert.ToString(
                                                Eval("Comment")))
                                            ? "No written comment was provided."
                                            : Server.HtmlEncode(
                                                Convert.ToString(
                                                    Eval("Comment")))
                                    %>

                                </div>

                            </div>

                            <asp:Panel
                                ID="pnlReply"
                                runat="server"
                                CssClass="lecturer-reply"
                                Visible='<%#
                                    !string.IsNullOrWhiteSpace(
                                        Convert.ToString(
                                            Eval("RepText")))
                                %>'>

                                <span class="feedback-label">
                                    Lecturer reply
                                </span>

                                <div class="feedback-text">

                                    <%#
                                        Server.HtmlEncode(
                                            Convert.ToString(
                                                Eval("RepText")))
                                    %>

                                </div>

                                <div class="reply-meta">

                                    <%#
                                        string.IsNullOrWhiteSpace(
                                            Convert.ToString(
                                                Eval("LecturerName")))
                                            ? "Lecturer"
                                            : Server.HtmlEncode(
                                                Convert.ToString(
                                                    Eval("LecturerName")))
                                    %>

                                    <%#
                                        Eval("RepAt") == DBNull.Value
                                            ? ""
                                            : " · " +
                                              Convert.ToDateTime(
                                                  Eval("RepAt"))
                                                  .ToString(
                                                      "dd MMM yyyy, hh:mm tt")
                                    %>

                                </div>

                            </asp:Panel>

                            <asp:Panel
                                ID="pnlAwaiting"
                                runat="server"
                                CssClass="awaiting-reply"
                                Visible='<%#
                                    string.IsNullOrWhiteSpace(
                                        Convert.ToString(
                                            Eval("RepText")))
                                %>'>

                                <i class="ti ti-clock"></i>
                                Awaiting lecturer reply

                            </asp:Panel>

                        </div>

                    </div>

                </ItemTemplate>

            </asp:Repeater>

        </div>

        <asp:Panel
            ID="pnlEmpty"
            runat="server"
            CssClass="empty-feedback"
            Visible="false">

            <i class="ti ti-message-off"
                style="font-size:34px"></i>

            <p>
                You have not submitted any feedback yet.
            </p>

        </asp:Panel>

    </main>

</div>

</asp:Content>