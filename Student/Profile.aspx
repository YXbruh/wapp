<%@ Page Title="My Profile – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Profile.aspx.cs" Inherits="CSA.Student.Student_Profile" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Student menu">
        <div class="sidebar-section">Main</div>
        <a href="Student_Dashboard.aspx"    class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="MyCourses.aspx"    class="sidebar-link"><i class="ti ti-books"></i>My Courses</a>
        <a href="Labs.aspx"         class="sidebar-link"><i class="ti ti-terminal-2"></i>Virtual Labs</a>
        <a href="Challenges.aspx"   class="sidebar-link"><i class="ti ti-trophy"></i>Challenges</a>
        <div class="sidebar-section">Progress</div>
        <a href="Analytics.aspx"    class="sidebar-link"><i class="ti ti-chart-bar"></i>Analytics</a>
        <a href="Certificates.aspx" class="sidebar-link"><i class="ti ti-certificate"></i>Certificates</a>
        <a href="Achievements.aspx" class="sidebar-link"><i class="ti ti-star"></i>Achievements</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx"      class="sidebar-link active"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>My Profile</h2>
            <p>Update your personal information and change your password.</p>
        </div>

        <!-- Success / Error messages -->
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litSuccess" runat="server" />
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="validation-summary-errors">
                <asp:Literal ID="litError" runat="server" />
            </div>
        </asp:Panel>

        <div class="cards-row">

            <!-- Personal Info -->
            <div class="card">
                <div class="card-header">Personal Information</div>

                <!-- Avatar -->
                <div style="display:flex;align-items:center;gap:16px;margin-bottom:24px">
                    <div class="profile-avatar">
                        <asp:Literal ID="litAvatarInitials" runat="server" Text="CS" />
                    </div>
                    <div>
                        <div style="font-size:15px;font-weight:700;color:var(--text)">
                            <asp:Literal ID="litDisplayName" runat="server" />
                        </div>
                        <div class="text-muted text-small mt-4">
                            Member since <asp:Literal ID="litJoined" runat="server" />
                        </div>
                    </div>
                </div>

                <asp:ValidationSummary ID="valSummaryInfo" runat="server"
                    ValidationGroup="InfoGroup"
                    CssClass="validation-summary-errors" HeaderText="Please fix:"
                    DisplayMode="BulletList" />

                <div class="form-group">
                    <label class="form-label" for="<%= tbFullName.ClientID %>">
                        <i class="ti ti-user" aria-hidden="true"></i>Full Name
                    </label>
                    <asp:TextBox ID="tbFullName" runat="server" CssClass="form-input"
                        MaxLength="100" placeholder="Your full name" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server"
                        ControlToValidate="tbFullName" ValidationGroup="InfoGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Full name is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= tbEmail.ClientID %>">
                        <i class="ti ti-mail" aria-hidden="true"></i>Email Address
                    </label>
                    <asp:TextBox ID="tbEmail" runat="server" CssClass="form-input"
                        TextMode="Email" MaxLength="150" placeholder="you@example.com" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                        ControlToValidate="tbEmail" ValidationGroup="InfoGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Email is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server"
                        ControlToValidate="tbEmail" ValidationGroup="InfoGroup"
                        ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Enter a valid email."
                        Text="<i class='ti ti-alert-circle'></i> Enter a valid email." />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= tbBio.ClientID %>">
                        <i class="ti ti-align-left" aria-hidden="true"></i>Bio
                        <span class="text-muted" style="font-weight:400">(optional)</span>
                    </label>
                    <asp:TextBox ID="tbBio" runat="server" CssClass="form-input"
                        TextMode="MultiLine" Rows="3" MaxLength="300"
                        placeholder="Tell us a bit about yourself..." />
                </div>

                <asp:Button ID="btnSaveInfo" runat="server" CssClass="btn-primary"
                    ValidationGroup="InfoGroup" OnClick="btnSaveInfo_Click"
                    Text="Save Changes" />
            </div>

            <!-- Change Password -->
            <div class="card">
                <div class="card-header">Change Password</div>

                <asp:ValidationSummary ID="valSummaryPwd" runat="server"
                    ValidationGroup="PwdGroup"
                    CssClass="validation-summary-errors" HeaderText="Please fix:"
                    DisplayMode="BulletList" />

                <div class="form-group">
                    <label class="form-label" for="<%= tbCurrentPwd.ClientID %>">
                        <i class="ti ti-lock" aria-hidden="true"></i>Current Password
                    </label>
                    <div class="input-icon-wrap">
                        <i class="ti ti-lock" aria-hidden="true"></i>
                        <asp:TextBox ID="tbCurrentPwd" runat="server" CssClass="form-input"
                            TextMode="Password" placeholder="••••••••" MaxLength="100" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvCurrent" runat="server"
                        ControlToValidate="tbCurrentPwd" ValidationGroup="PwdGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Current password is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= tbNewPwd.ClientID %>">
                        <i class="ti ti-lock-open" aria-hidden="true"></i>New Password
                    </label>
                    <div class="input-icon-wrap">
                        <i class="ti ti-lock-open" aria-hidden="true"></i>
                        <asp:TextBox ID="tbNewPwd" runat="server" CssClass="form-input"
                            TextMode="Password" placeholder="Min. 8 characters" MaxLength="100" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvNew" runat="server"
                        ControlToValidate="tbNewPwd" ValidationGroup="PwdGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="New password is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                    <asp:RegularExpressionValidator ID="revNew" runat="server"
                        ControlToValidate="tbNewPwd" ValidationGroup="PwdGroup"
                        ValidationExpression="^(?=.*[A-Z])(?=.*\d).{8,}$"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Min 8 chars, 1 uppercase, 1 number."
                        Text="<i class='ti ti-alert-circle'></i> Min 8 chars, 1 uppercase, 1 number." />
                    <div class="val-hint">
                        <i class="ti ti-info-circle" aria-hidden="true"></i>
                        At least 8 characters, 1 uppercase, 1 number
                    </div>
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= tbConfirmPwd.ClientID %>">
                        <i class="ti ti-lock-check" aria-hidden="true"></i>Confirm New Password
                    </label>
                    <div class="input-icon-wrap">
                        <i class="ti ti-lock-check" aria-hidden="true"></i>
                        <asp:TextBox ID="tbConfirmPwd" runat="server" CssClass="form-input"
                            TextMode="Password" placeholder="Repeat new password" MaxLength="100" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvConfirm" runat="server"
                        ControlToValidate="tbConfirmPwd" ValidationGroup="PwdGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Please confirm new password."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                    <asp:CompareValidator ID="cvPwd" runat="server"
                        ControlToValidate="tbConfirmPwd" ControlToCompare="tbNewPwd"
                        ValidationGroup="PwdGroup" Operator="Equal"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Passwords do not match."
                        Text="<i class='ti ti-alert-circle'></i> Passwords do not match." />
                </div>

                <asp:Button ID="btnChangePwd" runat="server" CssClass="btn-primary"
                    ValidationGroup="PwdGroup" OnClick="btnChangePwd_Click"
                    Text="Update Password" />
            </div>

        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.profile-avatar{width:56px;height:56px;border-radius:50%;background:var(--accent1);border:2px solid var(--accent2);display:flex;align-items:center;justify-content:center;font-size:18px;font-weight:700;color:var(--accent3);flex-shrink:0}
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;margin-bottom:16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
</style>
</asp:Content>
