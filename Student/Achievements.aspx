<%@ Page Title="Achievements – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Achievements.aspx.cs"
    Inherits="CSA.Student.Student_Achievements" %>

<asp:Content
    ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .achievement-progress {
        margin-bottom: 26px;
    }

    .achievement-progress-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 7px;
        font-size: 13px;
    }

    .achievement-grid {
        display: grid;
        grid-template-columns:
            repeat(auto-fill, minmax(250px, 1fr));
        gap: 18px;
    }

    .achievement-card {
        position: relative;
        min-height: 230px;
        padding: 22px;
        border: 1px solid var(--border);
        border-radius: 12px;
        background: var(--surface);
        transition:
            transform 0.2s ease,
            border-color 0.2s ease;
    }

    .achievement-card:hover {
        transform: translateY(-3px);
        border-color: var(--accent2);
    }

    .achievement-card.locked {
        opacity: 0.58;
        filter: grayscale(0.7);
    }

    .achievement-icon {
        width: 56px;
        height: 56px;
        display: flex;
        align-items: center;
        justify-content: center;
        margin-bottom: 16px;
        border-radius: 14px;
        background: var(--accent1);
        color: var(--accent3);
        font-size: 28px;
    }

    .achievement-card.locked
    .achievement-icon {
        background: rgba(150, 150, 150, 0.12);
        color: var(--text3);
    }

    .achievement-name {
        margin-bottom: 7px;
        color: var(--text);
        font-size: 16px;
        font-weight: 700;
    }

    .achievement-description {
        min-height: 46px;
        margin-bottom: 15px;
        color: var(--text2);
        font-size: 13px;
        line-height: 1.55;
    }

    .achievement-footer {
        display: flex;
        justify-content: space-between;
        align-items: flex-end;
        gap: 10px;
        margin-top: auto;
    }

    .achievement-xp {
        display: inline-flex;
        align-items: center;
        gap: 5px;
        color: var(--accent3);
        font-size: 12px;
        font-weight: 700;
    }

    .achievement-date {
        margin-top: 7px;
        color: var(--text3);
        font-size: 11px;
    }

    .achievement-status {
        position: absolute;
        top: 16px;
        right: 16px;
    }

    .achievement-empty {
        padding: 50px 20px;
        text-align: center;
        color: var(--text3);
    }

    @media(max-width: 700px) {
        .achievement-grid {
            grid-template-columns: 1fr;
        }

        .metrics {
            grid-template-columns:
                repeat(2, minmax(0, 1fr));
        }
    }
</style>

<div class="dash-layout">

    <aside
        class="sidebar"
        role="navigation"
        aria-label="Student menu">

        <div class="sidebar-section">
            Main
        </div>

        <a
            href="Student_Dashboard.aspx"
            class="sidebar-link">

            <i class="ti ti-layout-dashboard"></i>
            Dashboard

        </a>

        <a
            href="MyCourses.aspx"
            class="sidebar-link">

            <i class="ti ti-books"></i>
            My Courses

        </a>

        <a
            href="Labs.aspx"
            class="sidebar-link">

            <i class="ti ti-terminal-2"></i>
            Virtual Labs

        </a>

        <a
            href="Challenges.aspx"
            class="sidebar-link">

            <i class="ti ti-trophy"></i>
            Challenges

        </a>

        <div class="sidebar-section">
            Progress
        </div>

        <a
            href="Analytics.aspx"
            class="sidebar-link">

            <i class="ti ti-chart-bar"></i>
            Analytics

        </a>

        <a
            href="Certificates.aspx"
            class="sidebar-link">

            <i class="ti ti-certificate"></i>
            Certificates

        </a>

        <a
            href="Achievements.aspx"
            class="sidebar-link active">

            <i class="ti ti-star"></i>
            Achievements

        </a>

        <div class="sidebar-section">
            Account
        </div>

        <a
            href="Profile.aspx"
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

            <h2>Achievements</h2>

            <p>
                Complete courses, quizzes and practical
                challenges to unlock badges and earn XP.
            </p>

        </div>

        <div class="metrics">

            <div class="metric">

                <div class="metric-label">
                    Total XP
                </div>

                <div class="metric-val">

                    <asp:Literal
                        ID="litTotalXP"
                        runat="server"
                        Text="0" />

                </div>

                <div class="metric-sub">
                    experience points
                </div>

            </div>

            <div class="metric">

                <div class="metric-label">
                    Badges Earned
                </div>

                <div class="metric-val">

                    <asp:Literal
                        ID="litEarnedCount"
                        runat="server"
                        Text="0" />

                </div>

                <div class="metric-sub">
                    unlocked badges
                </div>

            </div>

            <div class="metric">

                <div class="metric-label">
                    Badges Locked
                </div>

                <div class="metric-val">

                    <asp:Literal
                        ID="litLockedCount"
                        runat="server"
                        Text="0" />

                </div>

                <div class="metric-sub">
                    remaining badges
                </div>

            </div>

            <div class="metric">

                <div class="metric-label">
                    Current Streak
                </div>

                <div class="metric-val">

                    <asp:Literal
                        ID="litStreak"
                        runat="server"
                        Text="0" />

                </div>

                <div class="metric-sub">
                    consecutive days
                </div>

            </div>

        </div>

        <div class="card achievement-progress">

            <div class="achievement-progress-top">

                <span>
                    Badge collection progress
                </span>

                <strong>

                    <asp:Literal
                        ID="litBadgeProgress"
                        runat="server"
                        Text="0%" />

                </strong>

            </div>

            <div class="progress-bar">

                <div
                    ID="badgeProgressFill"
                    runat="server"
                    class="progress-fill"
                    style="width:0%">
                </div>

            </div>

        </div>

        <div class="achievement-grid">

            <asp:Repeater
                ID="rptAchievements"
                runat="server">

                <ItemTemplate>

                    <div class='achievement-card <%#
                        Eval("CardClass") %>'>

                        <div class="achievement-status">

                            <span class='badge <%#
                                Eval("StatusClass") %>'>

                                <%# Eval("StatusText") %>

                            </span>

                        </div>

                        <div class="achievement-icon">

                            <i class='ti <%#
                                Eval("IconClass") %>'>
                            </i>

                        </div>

                        <div class="achievement-name">

                            <%# Eval("BadgeName") %>

                        </div>

                        <div class="achievement-description">

                            <%# Eval("Description") %>

                        </div>

                        <div class="achievement-footer">

                            <div>

                                <div class="achievement-xp">

                                    <i class="ti ti-bolt"></i>

                                    <%# Eval("PointsGranted") %>
                                    XP

                                </div>

                                <div class="achievement-date">

                                    <%# Eval("EarnedDisplay") %>

                                </div>

                            </div>

                            <i class='ti <%#
                                Eval("CornerIcon") %>'
                                style="font-size:23px">
                            </i>

                        </div>

                    </div>

                </ItemTemplate>

            </asp:Repeater>

        </div>

        <asp:Panel
            ID="pnlEmpty"
            runat="server"
            Visible="false"
            CssClass="achievement-empty">

            <i
                class="ti ti-award-off"
                style="font-size:48px">
            </i>

            <p style="margin-top:12px">
                No achievement definitions are currently
                available.
            </p>

        </asp:Panel>

    </main>

</div>

</asp:Content>