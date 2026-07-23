<%@ Page Title="Feedback - CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Feedback.aspx.cs"
    Inherits="CSA.Student.Student_Feedback" %>

<asp:Content
    ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .feedback-container {
        max-width:720px;
    }

    .feedback-card {
        padding:22px;
        background:var(--surface);
        border:1px solid var(--border);
        border-radius:11px;
    }

    .feedback-title {
        margin:0 0 6px;
        color:var(--text);
    }

    .feedback-description {
        margin:0 0 20px;
        color:var(--text2);
        line-height:1.6;
    }

    .feedback-field {
        margin-bottom:16px;
    }

    .feedback-field label {
        display:block;
        margin-bottom:6px;
        color:var(--text);
        font-size:12px;
        font-weight:700;
    }

    .feedback-input {
        width:100%;
        padding:10px;
        color:var(--text);
        background:var(--bg3);
        border:1px solid var(--border);
        border-radius:7px;
        box-sizing:border-box;
        font-family:inherit;
    }

    .feedback-comment {
        min-height:110px;
        resize:vertical;
    }

    .feedback-message {
        margin-bottom:16px;
        padding:12px;
        border-radius:8px;
    }

    .feedback-success {
        color:var(--success);
        border:1px solid var(--success);
    }

    .feedback-error {
        color:var(--danger);
        border:1px solid var(--danger);
    }

    .feedback-saved {
        padding:15px;
        color:var(--text2);
        background:var(--bg3);
        border-radius:8px;
        line-height:1.8;
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

        <a href="viewFeedback.aspx" class="sidebar-link active">
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
            OnClick="lbLogout_Click">

            <i class="ti ti-logout"></i>
            Sign Out

        </asp:LinkButton>

    </aside>

    <main class="dash-content">

        <div class="feedback-container">

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

                <h2>Learning Feedback</h2>

                <p>
                    Rate the completed learning activity and share
                    your experience with the lecturer.
                </p>

            </div>

            <asp:Panel
                ID="pnlMessage"
                runat="server"
                Visible="false">

                <asp:Literal
                    ID="litMessage"
                    runat="server" />

            </asp:Panel>

            <asp:Panel
                ID="pnlFeedback"
                runat="server"
                CssClass="feedback-card"
                Visible="false">

                <h3 class="feedback-title">

                    <asp:Literal
                        ID="litItemName"
                        runat="server" />

                </h3>

                <p class="feedback-description">

                    <asp:Literal
                        ID="litItemType"
                        runat="server" />

                </p>

                <asp:Panel
                    ID="pnlForm"
                    runat="server">

                    <div class="feedback-field">

                        <label for="<%= ddlRating.ClientID %>">
                            Rating
                        </label>

                        <asp:DropDownList
                            ID="ddlRating"
                            runat="server"
                            CssClass="feedback-input">

                            <asp:ListItem
                                Text="Select a rating"
                                Value="" />

                            <asp:ListItem
                                Text="1 - Very Poor"
                                Value="1" />

                            <asp:ListItem
                                Text="2 - Poor"
                                Value="2" />

                            <asp:ListItem
                                Text="3 - Average"
                                Value="3" />

                            <asp:ListItem
                                Text="4 - Good"
                                Value="4" />

                            <asp:ListItem
                                Text="5 - Excellent"
                                Value="5" />

                        </asp:DropDownList>

                        <asp:RequiredFieldValidator
                            ID="rfvRating"
                            runat="server"
                            ControlToValidate="ddlRating"
                            InitialValue=""
                            ValidationGroup="FeedbackGroup"
                            ErrorMessage="Please select a rating."
                            CssClass="text-danger text-small"
                            Display="Dynamic" />

                    </div>

                    <div class="feedback-field">

                        <label for="<%= tbComment.ClientID %>">
                            Comment
                        </label>

                        <asp:TextBox
                            ID="tbComment"
                            runat="server"
                            CssClass="feedback-input feedback-comment"
                            TextMode="MultiLine"
                            MaxLength="2000"
                            placeholder="Share your thoughts about this learning activity..." />

                    </div>

                    <asp:Button
                        ID="btnSubmit"
                        runat="server"
                        CssClass="btn-primary"
                        Text="Submit Feedback"
                        ValidationGroup="FeedbackGroup"
                        OnClick="btnSubmit_Click" />

                </asp:Panel>

                <asp:Panel
                    ID="pnlSaved"
                    runat="server"
                    CssClass="feedback-saved"
                    Visible="false">

                    <div>

                        <strong>Your rating:</strong>

                        <asp:Literal
                            ID="litRating"
                            runat="server" />

                        / 5

                    </div>

                    <asp:Panel
                        ID="pnlComment"
                        runat="server"
                        Visible="false">

                        <strong>Your comment:</strong>

                        <asp:Literal
                            ID="litComment"
                            runat="server" />

                    </asp:Panel>

                    <div class="text-muted text-small">

                        Submitted on

                        <asp:Literal
                            ID="litSubmittedAt"
                            runat="server" />

                    </div>

                </asp:Panel>

            </asp:Panel>

        </div>

    </main>

</div>

</asp:Content>