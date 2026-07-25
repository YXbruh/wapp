<%@ Page Title="My Profile – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Profile.aspx.cs" Inherits="CSA.Lecturer.Lecturer_Profile" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Lecturer menu">
        <div class="sidebar-section">Lecturer</div>
        <a href="Lecturer_Dashboard.aspx" class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="ManageContent.aspx"      class="sidebar-link"><i class="ti ti-files"></i>Manage Content</a>
        <a href="TerminalSandbox.aspx"    class="sidebar-link"><i class="ti ti-terminal-2"></i>Terminal Sandbox</a>
        <a href="QuizEditor.aspx"         class="sidebar-link"><i class="ti ti-list-check"></i>Quiz Editor</a>
        <div class="sidebar-section">Students</div>
        <a href="ClassAnalytics.aspx"     class="sidebar-link"><i class="ti ti-chart-bar"></i>Class Analytics</a>
        <a href="Mentorship.aspx"         class="sidebar-link"><i class="ti ti-messages"></i>Mentorship</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx"            class="sidebar-link active"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link"
            OnClientClick="return showLogoutConfirm(this);" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Edit Profile</h2>
            <p>Manage your personal details, account settings and profile picture.</p>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <div class="alert-success mb-16">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litSuccess" runat="server" />
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="validation-summary-errors mb-16">
                <asp:Literal ID="litError" runat="server" />
            </div>
        </asp:Panel>

        <div class="cards-row" style="align-items:start">

            <!-- LEFT: picture + personal details -->
            <div class="card">
                <div class="card-header">Personal Information</div>

                <div style="display:flex;align-items:center;gap:16px;margin-bottom:20px">
                    <asp:Panel ID="pnlPicture" runat="server" Visible="false">
                        <asp:Image ID="imgAvatar" runat="server" CssClass="profile-avatar-img"
                            AlternateText="Profile picture" />
                    </asp:Panel>
                    <asp:Panel ID="pnlInitials" runat="server">
                        <div class="profile-avatar">
                            <asp:Literal ID="litAvatarInitials" runat="server" Text="CS" />
                        </div>
                    </asp:Panel>
                    <div>
                        <div style="font-size:15px;font-weight:700;color:var(--text)">
                            <asp:Literal ID="litDisplayName" runat="server" />
                        </div>
                        <div class="text-muted text-small mt-4">
                            Member since <asp:Literal ID="litJoined" runat="server" />
                        </div>
                    </div>
                </div>

                <!-- Profile picture -->
                <div class="attachment-add-grid" style="margin-bottom:20px">
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label"><i class="ti ti-photo" aria-hidden="true"></i>Profile Picture
                            <span class="text-muted" style="font-weight:400">(PNG, JPG, GIF or WEBP, max 5 MB)</span>
                        </label>
                        <asp:FileUpload ID="fuPicture" runat="server" CssClass="file-input"
                            accept=".png,.jpg,.jpeg,.gif,.webp" />
                    </div>
                    <div style="display:flex;gap:8px">
                        <asp:Button ID="btnUploadPicture" runat="server" CssClass="btn-sm secondary"
                            CausesValidation="false" OnClick="btnUploadPicture_Click" Text="Upload" />
                        <asp:Button ID="btnRemovePicture" runat="server" CssClass="btn-danger"
                            CausesValidation="false" OnClick="btnRemovePicture_Click" Text="Remove"
                            OnClientClick="return confirm('Remove your profile picture?');" />
                    </div>
                </div>

                <asp:ValidationSummary ID="valSummaryInfo" runat="server"
                    ValidationGroup="InfoGroup"
                    CssClass="validation-summary-errors" HeaderText="Please fix:"
                    DisplayMode="BulletList" />

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-user" aria-hidden="true"></i>Full Name</label>
                    <asp:TextBox ID="tbFullName" runat="server" CssClass="form-input" MaxLength="150" />
                    <asp:RequiredFieldValidator ID="rfvFullName" runat="server"
                        ControlToValidate="tbFullName" ValidationGroup="InfoGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Full name is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-mail" aria-hidden="true"></i>Email Address</label>
                    <asp:TextBox ID="tbEmail" runat="server" CssClass="form-input"
                        TextMode="Email" MaxLength="255" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                        ControlToValidate="tbEmail" ValidationGroup="InfoGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Email is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server"
                        ControlToValidate="tbEmail" ValidationGroup="InfoGroup"
                        ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Enter a valid email address."
                        Text="<i class='ti ti-alert-circle'></i> Invalid email." />
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-phone" aria-hidden="true"></i>Phone Number</label>
                    <asp:TextBox ID="tbPhone" runat="server" CssClass="form-input" MaxLength="50"
                        placeholder="Optional" />
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-building" aria-hidden="true"></i>Department</label>
                    <asp:TextBox ID="tbDepartment" runat="server" CssClass="form-input" MaxLength="100"
                        placeholder="Optional" />
                </div>

                <asp:Button ID="btnSaveInfo" runat="server" CssClass="btn-primary"
                    ValidationGroup="InfoGroup" OnClick="btnSaveInfo_Click"
                    Text="Save Changes" />
            </div>

            <!-- RIGHT: account settings -->
            <div class="card">
                <div class="card-header">Account Settings</div>

                <asp:ValidationSummary ID="valSummaryPwd" runat="server"
                    ValidationGroup="PwdGroup"
                    CssClass="validation-summary-errors" HeaderText="Please fix:"
                    DisplayMode="BulletList" />

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-lock" aria-hidden="true"></i>Current Password</label>
                    <asp:TextBox ID="tbCurrentPwd" runat="server" CssClass="form-input"
                        TextMode="Password" placeholder="Password" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="rfvCurrentPwd" runat="server"
                        ControlToValidate="tbCurrentPwd" ValidationGroup="PwdGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Current password is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-lock-open" aria-hidden="true"></i>New Password</label>
                    <asp:TextBox ID="tbNewPwd" runat="server" CssClass="form-input"
                        TextMode="Password" placeholder="Min. 8 characters" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="rfvNewPwd" runat="server"
                        ControlToValidate="tbNewPwd" ValidationGroup="PwdGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="New password is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                    <asp:RegularExpressionValidator ID="revNewPwd" runat="server"
                        ControlToValidate="tbNewPwd" ValidationGroup="PwdGroup"
                        ValidationExpression=".{8,}"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="New password must be at least 8 characters."
                        Text="<i class='ti ti-alert-circle'></i> Min. 8 characters." />
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-lock-check" aria-hidden="true"></i>Confirm New Password</label>
                    <asp:TextBox ID="tbConfirmPwd" runat="server" CssClass="form-input"
                        TextMode="Password" placeholder="Repeat new password" MaxLength="100" />
                    <asp:CompareValidator ID="cvConfirmPwd" runat="server"
                        ControlToValidate="tbConfirmPwd" ControlToCompare="tbNewPwd"
                        ValidationGroup="PwdGroup" Display="Dynamic" CssClass="val-error"
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
