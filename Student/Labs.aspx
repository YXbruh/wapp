<%@ Page Title="Virtual Labs – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Labs.aspx.cs"
    Inherits="CSA.Student.Student_Labs" %>

<asp:Content
    ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .lab-row {
        display:flex;
        align-items:center;
        gap:14px;
        padding:12px 0;
        border-bottom:1px solid var(--bg3);
    }

    .lab-row:last-child {
        border-bottom:none;
    }

    .lab-icon {
        display:flex;
        align-items:center;
        justify-content:center;
        flex-shrink:0;
        width:40px;
        height:40px;
        color:var(--accent3);
        background:var(--accent1);
        border-radius:8px;
        font-size:18px;
    }

    .lab-info {
        flex:1;
        min-width:0;
    }

    .lab-name {
        margin-bottom:4px;
        color:var(--text);
        font-size:14px;
        font-weight:600;
    }

    .lab-meta {
        display:flex;
        flex-wrap:wrap;
        gap:14px;
        color:var(--text3);
        font-size:11px;
    }

    .lab-meta i {
        font-size:12px;
        vertical-align:-1px;
    }

    .lab-actions {
        display:flex;
        align-items:center;
        flex-shrink:0;
        gap:10px;
    }

    .workspace-header {
        display:flex;
        align-items:center;
        justify-content:space-between;
        gap:12px;
        margin-bottom:18px;
    }

    .workspace-title {
        margin:0;
        color:var(--text);
        font-size:24px;
    }

    .lab-workspace-grid {
        display:grid;
        grid-template-columns:minmax(0, 1fr) minmax(320px, 420px);
        gap:18px;
    }

    .lab-briefing {
        white-space:pre-wrap;
        line-height:1.7;
    }

    .lab-details {
        display:flex;
        flex-wrap:wrap;
        gap:8px;
        margin:12px 0 18px;
    }

    .hint-box {
        margin-top:16px;
        padding:14px;
        color:var(--text2);
        background:rgba(239, 159, 39, 0.08);
        border:1px solid var(--warning);
        border-radius:8px;
    }

    .submission-card {
        position:sticky;
        top:20px;
        align-self:start;
    }

    .submission-note {
        margin-bottom:14px;
        color:var(--text3);
        font-size:12px;
        line-height:1.6;
    }

    .terminal-link {
        display:flex;
        align-items:center;
        justify-content:center;
        gap:7px;
        width:100%;
        margin-bottom:16px;
        padding:10px 14px;
        color:var(--text);
        text-decoration:none;
        background:var(--bg3);
        border:1px solid var(--border);
        border-radius:8px;
    }

    .terminal-link:hover {
        color:var(--accent3);
        border-color:var(--accent2);
    }

    .command-input {
        width:100%;
        min-height:105px;
        padding:12px;
        color:#b9f6d9;
        background:#071712;
        border:1px solid var(--border);
        border-radius:8px;
        font-family:Consolas, Monaco, monospace;
        font-size:13px;
        resize:vertical;
        box-sizing:border-box;
    }

    .command-input:focus {
        outline:none;
        border-color:var(--accent2);
    }

    .submit-lab-button {
        width:100%;
        margin-top:12px;
    }

    .result-success,
    .result-error {
        margin-top:14px;
        padding:12px 14px;
        border-radius:8px;
        font-size:13px;
        font-weight:600;
    }

    .result-success {
        color:var(--success);
        background:rgba(111, 207, 151, 0.08);
        border:1px solid var(--success);
    }

    .result-error {
        color:var(--danger);
        background:rgba(226, 75, 74, 0.08);
        border:1px solid var(--danger);
    }

    .attempts-table {
        width:100%;
        border-collapse:collapse;
    }

    .attempts-table th,
    .attempts-table td {
        padding:10px 12px;
        text-align:left;
        border-bottom:1px solid var(--border);
        font-size:12px;
    }

    .attempts-table th {
        color:var(--text3);
        font-weight:700;
        text-transform:uppercase;
        letter-spacing:.4px;
    }

    .attempt-command {
        max-width:420px;
        color:var(--text2);
        font-family:Consolas, Monaco, monospace;
        overflow-wrap:anywhere;
    }

    @media (max-width:900px) {
        .lab-workspace-grid {
            grid-template-columns:1fr;
        }

        .submission-card {
            position:static;
        }
    }

    @media (max-width:650px) {
        .lab-row {
            align-items:flex-start;
            flex-wrap:wrap;
        }

        .lab-actions {
            width:100%;
            justify-content:flex-end;
        }

        .workspace-header {
            align-items:flex-start;
            flex-direction:column;
        }
    }
</style>

<div class="dash-layout">

    <aside
        class="sidebar"
        role="navigation"
        aria-label="Student menu">

        <div class="sidebar-section">Main</div>

        <a href="Student_Dashboard.aspx"
            class="sidebar-link">

            <i class="ti ti-layout-dashboard"></i>
            Dashboard

        </a>

        <a href="MyCourses.aspx"
            class="sidebar-link">

            <i class="ti ti-books"></i>
            My Courses

        </a>

        <a href="Labs.aspx"
            class="sidebar-link active">

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
            OnClientClick="return showLogoutConfirm(this);"
            OnClick="lbLogout_Click">

            <i class="ti ti-logout"></i>
            Sign Out

        </asp:LinkButton>

    </aside>

    <main class="dash-content">

        <!-- =====================================================
             LAB LIST
             ===================================================== -->

        <asp:Panel
            ID="pnlLabList"
            runat="server">

            <div class="dash-header">

                <h2>Virtual Labs</h2>

                <p>
                    Complete practical labs assigned through
                    your enrolled courses.
                </p>

            </div>

            <div class="attachments-cta mb-16">

                <i class="ti ti-terminal-2"></i>

                <span>
                    The browser terminal runs locally using WebAssembly.
                    It cannot access the internet or install online packages.
                    Use it to test available Linux commands, then submit your
                    final command for validation.
                </span>

            </div>

            <div class="metrics"
                style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">

                <div class="metric">

                    <div class="metric-label">
                        Total Labs
                    </div>

                    <div class="metric-val">

                        <asp:Literal
                            ID="litTotal"
                            runat="server"
                            Text="0" />

                    </div>

                    <div class="metric-sub">
                        assigned
                    </div>

                </div>

                <div class="metric">

                    <div class="metric-label">
                        Completed
                    </div>

                    <div class="metric-val"
                        style="color:var(--success)">

                        <asp:Literal
                            ID="litDone"
                            runat="server"
                            Text="0" />

                    </div>

                    <div class="metric-sub">
                        finished
                    </div>

                </div>

                <div class="metric">

                    <div class="metric-label">
                        Remaining
                    </div>

                    <div class="metric-val"
                        style="color:var(--warning)">

                        <asp:Literal
                            ID="litRemaining"
                            runat="server"
                            Text="0" />

                    </div>

                    <div class="metric-sub">
                        to complete
                    </div>

                </div>

            </div>

            <div
                class="filter-bar"
                role="group"
                aria-label="Filter labs">

                <button
                    type="button"
                    class="filter-chip active"
                    onclick="filterItems('all', this, 'lab-row')">

                    All

                </button>

                <button
                    type="button"
                    class="filter-chip"
                    onclick="filterItems('not-started', this, 'lab-row')">

                    Not Started

                </button>

                <button
                    type="button"
                    class="filter-chip"
                    onclick="filterItems('in-progress', this, 'lab-row')">

                    In Progress

                </button>

                <button
                    type="button"
                    class="filter-chip"
                    onclick="filterItems('done', this, 'lab-row')">

                    Done

                </button>

            </div>

            <div class="card">

                <asp:Repeater
                    ID="rptLabs"
                    runat="server"
                    OnItemCommand="rptLabs_ItemCommand">

                    <ItemTemplate>

                        <div
                            class="lab-row"
                            data-status='<%# Eval("StatusKey") %>'>

                            <div class="lab-icon">

                                <i class="ti ti-terminal-2"></i>

                            </div>

                            <div class="lab-info">

                                <div class="lab-name">
                                    <%# Eval("LabName") %>
                                </div>

                                <div class="lab-meta">

                                    <span>

                                        <i class="ti ti-books"></i>
                                        <%# Eval("CourseName") %>

                                    </span>

                                    <span>

                                        <i class="ti ti-clock"></i>
                                        ~<%# Eval("EstimatedMinutes") %> min

                                    </span>

                                    <span>

                                        <i class="ti ti-tool"></i>
                                        <%# Eval("Difficulty") %>

                                    </span>

                                    <span>

                                        <i class="ti ti-award"></i>
                                        <%# Eval("PointsReward") %> XP

                                    </span>

                                </div>

                            </div>

                            <div class="lab-actions">

                                <span class="badge <%# Eval("StatusBadgeClass") %>">
                                    <%# Eval("StatusLabel") %>
                                </span>

                                <asp:LinkButton
                                    ID="btnOpenLab"
                                    runat="server"
                                    CssClass='<%#
                                        Eval("StatusKey").ToString() == "done"
                                            ? "btn-sm secondary"
                                            : "btn-sm"
                                    %>'
                                    CausesValidation="false"
                                    CommandName="Open"
                                    CommandArgument='<%# Eval("LabID") %>'>

                                    <i class='ti <%#
                                        Eval("StatusKey").ToString() == "done"
                                            ? "ti-eye"
                                            : "ti-player-play"
                                    %>'></i>

                                    <%#
                                        Eval("StatusKey").ToString() == "done"
                                            ? "Review"
                                            : Eval("StatusKey").ToString() == "in-progress"
                                                ? "Continue"
                                                : "Start Lab"
                                    %>

                                </asp:LinkButton>

                                <asp:HyperLink
                                    ID="hlLabFeedback"
                                    runat="server"
                                    CssClass="btn-sm secondary"
                                    NavigateUrl='<%#
                                        "Feedback.aspx?type=lab&id=" +
                                        Server.UrlEncode(
                                            Convert.ToString(Eval("LabID")))
                                    %>'
                                    Visible='<%#
                                        Convert.ToString(Eval("StatusKey")) == "done"
                                    %>'>

                                    <i class="ti ti-message-star"></i>
                                    Feedback

                                </asp:HyperLink>

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

                <asp:Panel
                    ID="pnlEmpty"
                    runat="server"
                    Visible="false">

                    <div style="text-align:center;padding:40px 20px">

                        <i class="ti ti-terminal-2"
                            style="font-size:40px;color:var(--text3)">
                        </i>

                        <p class="text-muted mt-16">
                            No labs are available. Enrol in a course
                            to unlock labs.
                        </p>

                    </div>

                </asp:Panel>

            </div>

        </asp:Panel>


        <!-- =====================================================
             LAB SUBMISSION WORKSPACE
             ===================================================== -->

        <asp:Panel
            ID="pnlLabWorkspace"
            runat="server"
            Visible="false">

            <div class="workspace-header">

                <div>

                    <h2 class="workspace-title">

                        <asp:Literal
                            ID="litLabTitle"
                            runat="server" />

                    </h2>

                    <div class="text-muted text-small mt-4">

                        <asp:Literal
                            ID="litCourseName"
                            runat="server" />

                    </div>

                </div>

                <asp:LinkButton
                    ID="btnBackToLabs"
                    runat="server"
                    CssClass="btn-sm secondary"
                    CausesValidation="false"
                    OnClick="btnBackToLabs_Click">

                    <i class="ti ti-arrow-left"></i>
                    Back to Labs

                </asp:LinkButton>

            </div>

            <div class="lab-workspace-grid">

                <div>

                    <div class="card">

                        <div class="card-header">

                            <span>

                                <i class="ti ti-file-description"></i>
                                Lab Briefing

                            </span>

                        </div>

                        <div class="lab-details">

                            <span class="badge badge-blue">

                                <i class="ti ti-tool"></i>

                                <asp:Literal
                                    ID="litDifficulty"
                                    runat="server" />

                            </span>

                            <span class="badge badge-amber">

                                <i class="ti ti-clock"></i>

                                <asp:Literal
                                    ID="litTimeLimit"
                                    runat="server" />

                            </span>

                            <span class="badge badge-green">

                                <i class="ti ti-award"></i>

                                <asp:Literal
                                    ID="litPointsReward"
                                    runat="server" />
                                XP

                            </span>

                        </div>

                        <div class="lab-briefing">

                            <asp:Literal
                                ID="litScenario"
                                runat="server" />

                        </div>

                        <asp:Panel
                            ID="pnlHint"
                            runat="server"
                            CssClass="hint-box"
                            Visible="false">

                            <strong>

                                <i class="ti ti-bulb"></i>
                                Hint

                            </strong>

                            <div class="mt-8">

                                <asp:Literal
                                    ID="litHint"
                                    runat="server" />

                            </div>

                        </asp:Panel>

                    </div>

                    <asp:Panel
                        ID="pnlAttempts"
                        runat="server"
                        CssClass="card mt-16"
                        Visible="false">

                        <div class="card-header">

                            <span>

                                <i class="ti ti-history"></i>
                                Submission History

                            </span>

                        </div>

                        <asp:GridView
                            ID="gvAttempts"
                            runat="server"
                            AutoGenerateColumns="false"
                            CssClass="attempts-table"
                            GridLines="None">

                            <Columns>

                                <asp:BoundField
                                    DataField="CommandSubmitted"
                                    HeaderText="Command"
                                    ItemStyle-CssClass="attempt-command" />

                                <asp:TemplateField
                                    HeaderText="Result">

                                    <ItemTemplate>

                                        <span class='badge <%#
                                            Eval("Result").ToString() == "Passed"
                                                ? "badge-green"
                                                : "badge-amber"
                                        %>'>

                                            <%# Eval("Result") %>

                                        </span>

                                    </ItemTemplate>

                                </asp:TemplateField>

                                <asp:BoundField
                                    DataField="PointsEarned"
                                    HeaderText="XP" />

                                <asp:BoundField
                                    DataField="SubmittedAt"
                                    HeaderText="Submitted"
                                    DataFormatString="{0:dd MMM yyyy, hh:mm tt}" />

                            </Columns>

                        </asp:GridView>

                    </asp:Panel>

                </div>

                <div class="card submission-card">

                    <div class="card-header">

                        <span>

                            <i class="ti ti-send"></i>
                            Final Lab Submission

                        </span>

                    </div>

                    <div class="submission-note">
                        Use the browser terminal to test commands. When you
                        are ready, enter the final command below and submit it
                        for validation.
                    </div>

                    <asp:HyperLink
                        ID="hlOpenTerminal"
                        runat="server"
                        CssClass="terminal-link"
                        Target="_blank">

                        <i class="ti ti-terminal-2"></i>
                        Open Browser Terminal

                    </asp:HyperLink>

                    <div class="form-group">

                        <label class="form-label"
                            for="<%= tbFinalCommand.ClientID %>">

                            Final command

                        </label>

                        <asp:TextBox
                            ID="tbFinalCommand"
                            runat="server"
                            CssClass="command-input"
                            TextMode="MultiLine"
                            MaxLength="2000"
                            ValidationGroup="LabSubmission"
                            placeholder="Enter the final command used to solve this lab..." />

                        <asp:RequiredFieldValidator
                            ID="rfvFinalCommand"
                            runat="server"
                            ControlToValidate="tbFinalCommand"
                            ValidationGroup="LabSubmission"
                            CssClass="field-error"
                            Display="Dynamic"
                            ErrorMessage="Enter your final command before submitting." />

                    </div>

                    <asp:Button
                        ID="btnSubmitLab"
                        runat="server"
                        CssClass="btn-primary submit-lab-button"
                        ValidationGroup="LabSubmission"
                        Text="Submit Final Command"
                        OnClick="btnSubmitLab_Click" />

                    <asp:Panel
                        ID="pnlSubmissionResult"
                        runat="server"
                        Visible="false">

                        <asp:Literal
                            ID="litSubmissionResult"
                            runat="server" />

                    </asp:Panel>

                </div>

            </div>

        </asp:Panel>

    </main>

</div>

<script>
    function filterItems(status, button, className) {
        document
            .querySelectorAll(".filter-chip")
            .forEach(function (chip) {
                chip.classList.remove("active");
            });

        button.classList.add("active");

        document
            .querySelectorAll("." + className)
            .forEach(function (row) {
                row.style.display =
                    status === "all" ||
                        row.dataset.status === status
                        ? ""
                        : "none";
            });
    }
</script>

</asp:Content>