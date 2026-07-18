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
    .result-success {
        padding:12px;
        color:var(--success);
        border:1px solid var(--success);
        border-radius:8px;
    }

    .result-error {
        padding:12px;
        color:var(--danger);
        border:1px solid var(--danger);
        border-radius:8px;
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
            class="sidebar-link active">
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
            ID="pnlChallengeList"
            runat="server">

            <div class="dash-header">
                <h2>Challenges</h2>
                <p>
                    Attempt practical command challenges
                    from your enrolled courses.
                </p>
            </div>

            <div class="metrics"
                style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">

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

                </div>

                <div class="metric">

                    <div class="metric-label">
                        Completed
                    </div>

                    <div class="metric-val">
                        <asp:Literal
                            ID="litDone"
                            runat="server"
                            Text="0" />
                    </div>

                </div>

                <div class="metric">

                    <div class="metric-label">
                        XP Earned
                    </div>

                    <div class="metric-val">
                        <asp:Literal
                            ID="litXP"
                            runat="server"
                            Text="0" />
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

                            <div class="course-thumb">
                                <i class="ti ti-trophy"></i>
                            </div>

                            <div class="course-body">

                                <h3>
                                    <%# Eval("ChallengeName") %>
                                </h3>

                                <p>
                                    <%# Eval("CourseName") %>
                                </p>

                                <div class="course-meta">

                                    <span>
                                        <%# Eval("Difficulty") %>
                                    </span>

                                    <span>
                                        <%# Eval("XPReward") %>
                                        XP
                                    </span>

                                    <span class="badge <%# Eval("StatusClass") %>">
                                        <%# Eval("StatusLabel") %>
                                    </span>

                                </div>

                            </div>

                            <div class="course-footer">

                                <span class="text-muted text-small">
                                    Attempts:
                                    <%# Eval("AttemptCount") %>
                                </span>

                                <asp:LinkButton
                                    ID="btnOpen"
                                    runat="server"
                                    CssClass="btn-sm"
                                    CausesValidation="false"
                                    CommandName="Open"
                                    CommandArgument='<%# Eval("ChallengeID") %>'>

                                    <%# Eval("ActionText") %>

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

                <p class="text-muted">
                    There are no published challenges
                    in your enrolled courses.
                </p>

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
                Back to Challenges

            </asp:LinkButton>

            <div class="card"
                style="margin-top:16px">

                <h2>
                    <asp:Literal
                        ID="litChallengeTitle"
                        runat="server" />
                </h2>

                <p class="text-muted">
                    Difficulty:
                    <asp:Literal
                        ID="litDifficulty"
                        runat="server" />

                    &nbsp; | &nbsp;

                    Reward:
                    <asp:Literal
                        ID="litReward"
                        runat="server" />
                    XP
                </p>

                <h3>Scenario</h3>

                <p style="white-space:pre-line">
                    <asp:Literal
                        ID="litScenario"
                        runat="server" />
                </p>

                <asp:Panel
                    ID="pnlHint"
                    runat="server"
                    Visible="false">

                    <h3>Hint</h3>

                    <p>
                        <asp:Literal
                            ID="litHint"
                            runat="server" />
                    </p>

                </asp:Panel>

                <div class="form-group">

                    <label class="form-label">
                        Enter your command
                    </label>

                    <asp:TextBox
                        ID="tbCommand"
                        runat="server"
                        CssClass="form-input"
                        TextMode="MultiLine"
                        Rows="3" />

                    <asp:RequiredFieldValidator
                        ID="rfvCommand"
                        runat="server"
                        ControlToValidate="tbCommand"
                        ValidationGroup="ChallengeGroup"
                        CssClass="val-error"
                        ErrorMessage="Enter a command." />

                </div>

                <asp:Button
                    ID="btnSubmit"
                    runat="server"
                    Text="Submit Attempt"
                    CssClass="btn-primary"
                    ValidationGroup="ChallengeGroup"
                    OnClick="btnSubmit_Click" />

                <asp:Panel
                    ID="pnlResult"
                    runat="server"
                    Visible="false"
                    style="margin-top:16px">

                    <asp:Literal
                        ID="litResult"
                        runat="server" />

                </asp:Panel>

                <asp:Panel
                    ID="pnlAttempts"
                    runat="server"
                    Visible="false"
                    style="margin-top:24px">

                    <h3>Previous Attempts</h3>

                    <asp:GridView
                        ID="gvAttempts"
                        runat="server"
                        AutoGenerateColumns="false"
                        CssClass="data-table">

                        <Columns>

                            <asp:BoundField
                                DataField="CommandSubmitted"
                                HeaderText="Command" />

                            <asp:BoundField
                                DataField="Result"
                                HeaderText="Result" />

                            <asp:BoundField
                                DataField="PointsEarned"
                                HeaderText="XP" />

                            <asp:BoundField
                                DataField="SubmittedAt"
                                HeaderText="Attempted At"
                                DataFormatString="{0:dd MMM yyyy hh:mm tt}" />

                        </Columns>

                    </asp:GridView>

                </asp:Panel>

            </div>

        </asp:Panel>

    </main>

</div>

</asp:Content>