<%@ Page Title="Challenges – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Challenges.aspx.cs"
    Inherits="CSA.Student.Student_Challenges" %>

<asp:Content
    ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .question-card {
        margin-bottom:14px;
        padding:16px;
        border:1px solid var(--border);
        border-radius:9px;
    }

    .question-text {
        margin-bottom:12px;
        color:var(--text);
        font-weight:600;
        line-height:1.6;
    }

    .answer-option {
        display:block;
        margin:7px 0;
        padding:9px 11px;
        background:var(--bg3);
        border-radius:7px;
    }

    .answer-box {
        width:100%;
        min-height:85px;
        padding:10px;
        color:var(--text);
        background:var(--bg3);
        border:1px solid var(--border);
        border-radius:7px;
        box-sizing:border-box;
    }

    .answer-review {
        margin-top:12px;
        padding:11px;
        color:var(--text2);
        background:var(--bg3);
        border-left:3px solid var(--accent2);
        border-radius:6px;
        line-height:1.7;
    }

    .result-success,
    .result-error,
    .notice-box {
        margin-bottom:15px;
        padding:12px;
        border-radius:8px;
    }

    .result-success {
        color:var(--success);
        border:1px solid var(--success);
    }

    .result-error {
        color:var(--danger);
        border:1px solid var(--danger);
    }

    .notice-box {
        color:var(--warning);
        border:1px solid var(--warning);
    }

    .attachment-list {
        margin-top:12px;
        display:flex;
        flex-direction:column;
        gap:8px;
    }

    .attachment-item {
        display:flex;
        align-items:center;
        gap:8px;
        width:fit-content;
        max-width:100%;
        padding:8px 11px;
        color:var(--accent3);
        background:var(--bg3);
        border:1px solid var(--border);
        border-radius:7px;
        text-decoration:none;
    }

    .attachment-item:hover {
        border-color:var(--accent2);
    }

    .attachment-image {
        display:block;
        max-width:100%;
        max-height:320px;
        margin-top:8px;
        border:1px solid var(--border);
        border-radius:8px;
    }

    .quiz-description {
        color:var(--text2);
    }

    .quiz-actions {
        display:flex;
        align-items:center;
        flex-wrap:wrap;
        gap:10px;
        margin-top:18px;
    }

    .quiz-timer {
        display:inline-flex;
        align-items:center;
        gap:7px;
        padding:8px 12px;
        color:var(--text);
        background:var(--surface);
        border:1px solid var(--border);
        border-radius:7px;
        font-weight:800;
    }

    .quiz-timer.timer-warning {
        color:var(--warning);
        border-color:var(--warning);
    }

    .quiz-timer.timer-danger {
        color:var(--danger);
        border-color:var(--danger);
    }

    .quiz-timer-label {
        color:var(--text2);
        font-size:11px;
        font-weight:600;
    }

    .quiz-timer-value {
        min-width:48px;
        font-variant-numeric:tabular-nums;
    }

    @media (max-width:700px) {
        .quiz-actions {
            align-items:stretch;
            flex-direction:column;
        }

        .quiz-actions .btn-primary,
        .quiz-actions .btn-sm {
            text-align:center;
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

        <a href="Challenges.aspx" class="sidebar-link active">
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

        <a href="viewFeedback.aspx" class="sidebar-link">
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

        <asp:Panel
            ID="pnlChallengeList"
            runat="server">

            <div class="dash-header">

                <h2>Challenges</h2>

                <p>
                    Complete quizzes from your enrolled courses.
                </p>

            </div>

            <div
                class="metrics"
                style="grid-template-columns:repeat(3,1fr);
                       margin-bottom:24px">

                <div class="metric">

                    <div class="metric-label">
                        Available
                    </div>

                    <div class="metric-val">

                        <asp:Literal
                            ID="litTotal"
                            runat="server"
                            Text="0" />

                    </div>

                    <div class="metric-sub">
                        challenges
                    </div>

                </div>

                <div class="metric">

                    <div class="metric-label">
                        Passed
                    </div>

                    <div
                        class="metric-val"
                        style="color:var(--success)">

                        <asp:Literal
                            ID="litPassed"
                            runat="server"
                            Text="0" />

                    </div>

                    <div class="metric-sub">
                        completed
                    </div>

                </div>

                <div class="metric">

                    <div class="metric-label">
                        Attempts
                    </div>

                    <div class="metric-val">

                        <asp:Literal
                            ID="litAttempts"
                            runat="server"
                            Text="0" />

                    </div>

                    <div class="metric-sub">
                        submitted
                    </div>

                </div>

            </div>

            <div class="courses-grid">

                <asp:Repeater
                    ID="rptChallenges"
                    runat="server"
                    OnItemCommand="rptChallenges_ItemCommand">

                    <ItemTemplate>

                        <div class="course-card">

                            <div class="course-body">

                                <h3>
                                    <%# Eval("Title") %>
                                </h3>

                                <p>
                                    <%# Eval("CourseName") %>
                                </p>

                                <div class="course-meta">

                                    <span>
                                        <%# Eval("QuestionCount") %>
                                        questions
                                    </span>

                                    <span>
                                        Pass:
                                        <%# Eval("PassMark", "{0:0.##}") %>%
                                    </span>

                                    <span>
                                        Attempts:
                                        <%# Eval("AttemptCount") %>
                                        /
                                        <%# (Container.DataItem as System.Data.DataRowView)?.Row.Table.Columns.Contains("MaxAttempts") == true ? Eval("MaxAttempts") : 3 %>
                                    </span>

                                    <span class='badge <%#
                                        Convert.ToBoolean(Eval("HasPassed"))
                                            ? "badge-green"
                                            : "badge-amber"
                                    %>'>

                                        <%#
                                            Convert.ToBoolean(Eval("HasPassed"))
                                                ? "Passed"
                                                : "Available"
                                        %>

                                    </span>

                                </div>

                            </div>

                            <div class="course-footer">

                                <span class="text-muted text-small">

                                    <%#
                                        Eval("DurationMinutes") == DBNull.Value
                                            ? "No time limit"
                                            : Eval("DurationMinutes") + " minutes"
                                    %>

                                </span>

                                <asp:LinkButton
                                    ID="btnOpen"
                                    runat="server"
                                    CssClass="btn-sm"
                                    CausesValidation="false"
                                    CommandName="Open"
                                    CommandArgument='<%# Eval("QuizID") %>'>

                                    <%#
                                        Convert.ToInt32(Eval("AttemptCount")) >=
                                        Convert.ToInt32(Eval("MaxAttempts"))
                                            ? "View Answers"
                                            : Convert.ToInt32(Eval("AttemptCount")) > 0
                                                ? "Reattempt"
                                                : "Start"
                                    %>

                                </asp:LinkButton>

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

            </div>

            <asp:Panel
                ID="pnlEmpty"
                runat="server"
                Visible="false">

                <div class="card">

                    <p class="text-muted">
                        No published challenges are available.
                    </p>

                </div>

            </asp:Panel>

        </asp:Panel>

        <asp:Panel
            ID="pnlWorkspace"
            runat="server"
            Visible="false">

            <asp:LinkButton
                ID="btnBack"
                runat="server"
                CssClass="btn-sm secondary"
                CausesValidation="false"
                OnClick="btnBack_Click">

                <i class="ti ti-arrow-left"></i>
                Back

            </asp:LinkButton>

            <div class="dash-header mt-16">

                <h2>

                    <asp:Literal
                        ID="litQuizTitle"
                        runat="server" />

                </h2>

                <div class="quiz-description">

                    <asp:Literal
                        ID="litQuizDescription"
                        runat="server" />

                </div>

            </div>

            <div class="filter-bar">

                <span class="badge badge-blue">

                    <asp:Literal
                        ID="litCourseName"
                        runat="server" />

                </span>

                <span class="badge badge-amber">

                    <asp:Literal
                        ID="litAttemptUsage"
                        runat="server" />

                </span>

                <span class="badge badge-green">

                    Pass mark:

                    <asp:Literal
                        ID="litPassMark"
                        runat="server" />%

                </span>

                <asp:Panel
                    ID="pnlTimer"
                    runat="server"
                    CssClass="quiz-timer"
                    Visible="false">

                    <i class="ti ti-clock"></i>

                    <span class="quiz-timer-label">
                        Time left
                    </span>

                    <span
                        id="quizTimerText"
                        class="quiz-timer-value">

                        00:00

                    </span>

                </asp:Panel>

                <asp:HiddenField
                    ID="hfRemainingSeconds"
                    runat="server" />

            </div>

            <asp:Panel
                ID="pnlNotice"
                runat="server"
                CssClass="notice-box"
                Visible="false">

                <asp:Literal
                    ID="litNotice"
                    runat="server" />

            </asp:Panel>

            <asp:Panel
                ID="pnlResult"
                runat="server"
                Visible="false">

                <asp:Literal
                    ID="litResult"
                    runat="server" />

            </asp:Panel>

            <asp:Repeater
                ID="rptQuestions"
                runat="server"
                OnItemDataBound="rptQuestions_ItemDataBound">

                <ItemTemplate>

                    <div class="question-card">

                        <asp:HiddenField
                            ID="hfQuestionID"
                            runat="server"
                            Value='<%# Eval("QuestionID") %>' />

                        <asp:HiddenField
                            ID="hfQuestionType"
                            runat="server"
                            Value='<%# Eval("QuestionType") %>' />

                        <div class="question-text">

                            <%# Container.ItemIndex + 1 %>.

                            <%#
                                Server.HtmlEncode(
                                    Convert.ToString(
                                        Eval("QuestionText")))
                            %>

                            <span class="badge badge-blue">

                                <%# Eval("Points") %>
                                marks

                            </span>

                        </div>

                        <asp:Literal
                            ID="litQuestionAttachments"
                            runat="server" />

                        <asp:Panel
                            ID="pnlMCQ"
                            runat="server"
                            Visible="false">

                            <label class="answer-option">

                                <asp:CheckBox
                                    ID="cbA"
                                    runat="server" />

                                <asp:Label
                                    ID="lblA"
                                    runat="server"
                                    AssociatedControlID="cbA" />

                            </label>

                            <label class="answer-option">

                                <asp:CheckBox
                                    ID="cbB"
                                    runat="server" />

                                <asp:Label
                                    ID="lblB"
                                    runat="server"
                                    AssociatedControlID="cbB" />

                            </label>

                            <label class="answer-option">

                                <asp:CheckBox
                                    ID="cbC"
                                    runat="server" />

                                <asp:Label
                                    ID="lblC"
                                    runat="server"
                                    AssociatedControlID="cbC" />

                            </label>

                            <label class="answer-option">

                                <asp:CheckBox
                                    ID="cbD"
                                    runat="server" />

                                <asp:Label
                                    ID="lblD"
                                    runat="server"
                                    AssociatedControlID="cbD" />

                            </label>

                        </asp:Panel>

                        <asp:RadioButtonList
                            ID="rblTrueFalse"
                            runat="server"
                            Visible="false">

                            <asp:ListItem
                                Text="True"
                                Value="True" />

                            <asp:ListItem
                                Text="False"
                                Value="False" />

                        </asp:RadioButtonList>

                        <asp:TextBox
                            ID="tbStructure"
                            runat="server"
                            CssClass="answer-box"
                            TextMode="MultiLine"
                            MaxLength="500"
                            Visible="false"
                            placeholder="Enter your answer..." />

                        <asp:Panel
                            ID="pnlAnswerReview"
                            runat="server"
                            CssClass="answer-review"
                            Visible="false">

                            <asp:Label
                                ID="lblAnswerResult"
                                runat="server"
                                CssClass="badge" />

                            <asp:Panel
                                ID="pnlStudentAnswer"
                                runat="server"
                                style="margin-top:8px">

                                <strong>Your answer:</strong>

                                <asp:Literal
                                    ID="litStudentAnswer"
                                    runat="server" />

                            </asp:Panel>

                            <div>
                                <strong>Correct answer:</strong>

                                <asp:Literal
                                    ID="litCorrectAnswer"
                                    runat="server" />
                            </div>

                            <asp:Panel
                                ID="pnlExplanation"
                                runat="server"
                                Visible="false">

                                <strong>Explanation:</strong>

                                <asp:Literal
                                    ID="litExplanation"
                                    runat="server" />

                            </asp:Panel>

                        </asp:Panel>

                    </div>

                </ItemTemplate>

            </asp:Repeater>

            <div class="quiz-actions">

                <asp:Button
                    ID="btnSubmit"
                    runat="server"
                    CssClass="btn-primary"
                    Text="Submit Challenge"
                    UseSubmitBehavior="false"
                    OnClientClick="
                        if (this.getAttribute('data-submitting') === '1') {
                            return false;
                        }

                        this.setAttribute('data-submitting', '1');
                        this.disabled = true;
                        this.value = 'Submitting...';"
                    OnClick="btnSubmit_Click" />

                <asp:HyperLink
                    ID="hlQuizFeedback"
                    runat="server"
                    CssClass="btn-sm secondary"
                    Visible="false">

                    <i class="ti ti-message-star"></i>
                    Give Quiz Feedback

                </asp:HyperLink>

            </div>

            <asp:Panel
                ID="pnlAttempts"
                runat="server"
                CssClass="card mt-24"
                Visible="false">

                <div class="card-header">
                    Previous Attempts
                </div>

                <asp:GridView
                    ID="gvAttempts"
                    runat="server"
                    AutoGenerateColumns="false"
                    GridLines="None"
                    CssClass="data-table">

                    <Columns>

                        <asp:BoundField
                            DataField="ObtainedMarks"
                            HeaderText="Marks" />

                        <asp:BoundField
                            DataField="TotalMarks"
                            HeaderText="Total" />

                        <asp:BoundField
                            DataField="Score"
                            HeaderText="Score"
                            DataFormatString="{0:0.##}%" />

                        <asp:CheckBoxField
                            DataField="IsPassed"
                            HeaderText="Passed" />

                        <asp:BoundField
                            DataField="AttemptedAt"
                            HeaderText="Submitted"
                            DataFormatString="{0:dd MMM yyyy, hh:mm tt}" />

                    </Columns>

                </asp:GridView>

            </asp:Panel>

        </asp:Panel>

    </main>

</div>

<script>
    (function () {
        const timerPanel =
            document.getElementById(
                "<%= pnlTimer.ClientID %>");

        const remainingField =
            document.getElementById(
                "<%= hfRemainingSeconds.ClientID %>");

        const submitButton =
            document.getElementById(
                "<%= btnSubmit.ClientID %>");

        const timerText =
            document.getElementById(
                "quizTimerText");

        if (!timerPanel ||
            !remainingField ||
            !submitButton ||
            !timerText) {
            return;
        }

        const panelStyle =
            window.getComputedStyle(timerPanel);

        if (panelStyle.display === "none" ||
            submitButton.disabled) {
            return;
        }

        let remainingSeconds =
            parseInt(
                remainingField.value || "0",
                10);

        let autoSubmitted = false;

        function displayTimer() {
            const minutes =
                Math.floor(
                    remainingSeconds / 60);

            const seconds =
                remainingSeconds % 60;

            timerText.textContent =
                String(minutes).padStart(2, "0") +
                ":" +
                String(seconds).padStart(2, "0");

            timerPanel.classList.remove(
                "timer-warning",
                "timer-danger");

            if (remainingSeconds <= 60) {
                timerPanel.classList.add(
                    "timer-danger");
            }
            else if (remainingSeconds <= 300) {
                timerPanel.classList.add(
                    "timer-warning");
            }
        }

        function submitExpiredQuiz() {
            if (autoSubmitted) {
                return;
            }

            autoSubmitted = true;
            remainingField.value = "0";

            if (typeof submitButton.click === "function") {
                submitButton.click();
            }
            else {
                __doPostBack(
                    "<%= btnSubmit.UniqueID %>",
                    "");
            }
        }

        displayTimer();

        if (remainingSeconds <= 0) {
            submitExpiredQuiz();
            return;
        }

        window.setInterval(function () {
            if (autoSubmitted) {
                return;
            }

            remainingSeconds--;

            remainingField.value =
                String(remainingSeconds);

            displayTimer();

            if (remainingSeconds <= 0) {
                submitExpiredQuiz();
            }
        }, 1000);
    })();
</script>

</asp:Content>