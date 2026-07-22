<%@ Page Title="Challenges – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Challenges.aspx.cs"
    Inherits="CSA.Student.Student_Challenges" %>

<asp:Content ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .question-card {
        margin-bottom:16px;
        padding:18px;
        border:1px solid var(--border);
        border-radius:10px;
    }

    .question-text {
        margin-bottom:14px;
        color:var(--text);
        font-weight:600;
        line-height:1.6;
    }

    .answer-option {
        display:block;
        margin:8px 0;
        padding:9px 12px;
        background:var(--bg3);
        border-radius:7px;
    }

    .answer-box {
        width:100%;
        min-height:90px;
        padding:10px;
        color:var(--text);
        background:var(--bg3);
        border:1px solid var(--border);
        border-radius:7px;
        box-sizing:border-box;
    }

    .result-success,
    .result-error,
    .notice-box {
        margin-bottom:16px;
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

        <!-- CHALLENGE LIST -->

        <asp:Panel
            ID="pnlChallengeList"
            runat="server">

            <div class="dash-header">
                <h2>Challenges</h2>
                <p>Complete quizzes from your enrolled courses.</p>
            </div>

            <div class="metrics"
                style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">

                <div class="metric">
                    <div class="metric-label">Available</div>
                    <div class="metric-val">
                        <asp:Literal ID="litTotal" runat="server" Text="0" />
                    </div>
                    <div class="metric-sub">challenges</div>
                </div>

                <div class="metric">
                    <div class="metric-label">Passed</div>
                    <div class="metric-val" style="color:var(--success)">
                        <asp:Literal ID="litPassed" runat="server" Text="0" />
                    </div>
                    <div class="metric-sub">completed</div>
                </div>

                <div class="metric">
                    <div class="metric-label">Attempts</div>
                    <div class="metric-val">
                        <asp:Literal ID="litAttempts" runat="server" Text="0" />
                    </div>
                    <div class="metric-sub">submitted</div>
                </div>

            </div>

            <div class="course-grid">

                <asp:Repeater
                    ID="rptChallenges"
                    runat="server"
                    OnItemCommand="rptChallenges_ItemCommand">

                    <ItemTemplate>

                        <div class="course-card">

                            <div class="course-body">

                                <h3><%# Eval("Title") %></h3>

                                <p><%# Eval("CourseName") %></p>

                                <div class="course-meta">

                                    <span>
                                        <%# Eval("QuestionCount") %> questions
                                    </span>

                                    <span>
                                        Pass: <%# Eval("PassMark", "{0:0.##}") %>%
                                    </span>

                                    <span>
                                        Attempts:
                                        <%# Eval("AttemptCount") %> /
                                        <%# Eval("MaxAttempts") %>
                                    </span>

                                    <span class="badge <%#
                                        Convert.ToBoolean(Eval("HasPassed"))
                                            ? "badge-green"
                                            : "badge-amber"
                                    %>">

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
                                        Convert.ToBoolean(Eval("HasPassed"))
                                            ? "Review"
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


        <!-- QUIZ WORKSPACE -->

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

                <p>
                    <asp:Literal
                        ID="litQuizDescription"
                        runat="server" />
                </p>

            </div>

            <div class="filter-bar">

                <span class="badge badge-blue">
                    <asp:Literal ID="litCourseName" runat="server" />
                </span>

                <span class="badge badge-amber">
                    <asp:Literal ID="litAttemptUsage" runat="server" />
                </span>

                <span class="badge badge-green">
                    Pass mark:
                    <asp:Literal ID="litPassMark" runat="server" />%
                </span>

            </div>

            <asp:Panel
                ID="pnlNotice"
                runat="server"
                CssClass="notice-box"
                Visible="false">

                <asp:Literal ID="litNotice" runat="server" />

            </asp:Panel>

            <asp:Panel
                ID="pnlResult"
                runat="server"
                Visible="false">

                <asp:Literal ID="litResult" runat="server" />

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
                            <%# Server.HtmlEncode(
                                Convert.ToString(Eval("QuestionText")))
                            %>

                            <span class="badge badge-blue">
                                <%# Eval("Points") %> marks
                            </span>

                        </div>

                        <asp:Panel
                            ID="pnlMCQ"
                            runat="server"
                            Visible="false">

                            <label class="answer-option">
                                <asp:CheckBox ID="cbA" runat="server" />
                                <asp:Label
                                    ID="lblA"
                                    runat="server"
                                    AssociatedControlID="cbA" />
                            </label>

                            <label class="answer-option">
                                <asp:CheckBox ID="cbB" runat="server" />
                                <asp:Label
                                    ID="lblB"
                                    runat="server"
                                    AssociatedControlID="cbB" />
                            </label>

                            <label class="answer-option">
                                <asp:CheckBox ID="cbC" runat="server" />
                                <asp:Label
                                    ID="lblC"
                                    runat="server"
                                    AssociatedControlID="cbC" />
                            </label>

                            <label class="answer-option">
                                <asp:CheckBox ID="cbD" runat="server" />
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

                            <asp:ListItem Text="True" Value="True" />
                            <asp:ListItem Text="False" Value="False" />

                        </asp:RadioButtonList>

                        <asp:TextBox
                            ID="tbStructure"
                            runat="server"
                            CssClass="answer-box"
                            TextMode="MultiLine"
                            MaxLength="500"
                            Visible="false"
                            placeholder="Enter your answer..." />

                    </div>

                </ItemTemplate>

            </asp:Repeater>

            <asp:Button
                ID="btnSubmit"
                runat="server"
                CssClass="btn-primary"
                Text="Submit Challenge"
                OnClick="btnSubmit_Click" />

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

</asp:Content>