<%@ Page Title="Sign In – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs" Inherits="CSA.Login" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="auth-page">
        <div class="auth-box">

            <!-- Logo -->
            <div class="auth-logo">
                <img src="Images/logo.png" alt="CyberShield Academy" class="auth-logo-img"
                     onerror="this.style.display='none'" />
                <h2>CyberShield Academy</h2>
                <p>Secure your learning journey</p>
            </div>

            <!-- Sign In / Register tabs -->
            <div class="auth-tabs" role="tablist">
                <button type="button" class="auth-tab active" role="tab"
                        onclick="location.href='Login.aspx'">Sign In</button>
                <button type="button" class="auth-tab" role="tab"
                        onclick="location.href='Register.aspx'">Register</button>
            </div>

            <!-- Validation summary -->
            <asp:ValidationSummary ID="valSummary" runat="server"
                CssClass="validation-summary-errors" HeaderText="Please fix the following:"
                DisplayMode="BulletList" />

            <!-- Error message from code-behind -->
            <asp:Panel ID="pnlError" runat="server" Visible="false">
                <div class="validation-summary-errors">
                    <asp:Literal ID="litError" runat="server" />
                </div>
            </asp:Panel>

            <!-- Email -->
            <div class="form-group">
                <label class="form-label" for="<%= tbEmail.ClientID %>">
                    <i class="ti ti-mail" aria-hidden="true"></i>Email Address
                </label>
                <div class="input-icon-wrap">
                    <i class="ti ti-mail" aria-hidden="true"></i>
                    <asp:TextBox ID="tbEmail" runat="server" CssClass="form-input"
                        TextMode="Email" placeholder="you@example.com" MaxLength="150" />
                </div>
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                    ControlToValidate="tbEmail" Display="Dynamic"
                    CssClass="val-error" ErrorMessage="Email is required."
                    Text="<i class='ti ti-alert-circle'></i> Email is required." />
                <asp:RegularExpressionValidator ID="revEmail" runat="server"
                    ControlToValidate="tbEmail" Display="Dynamic"
                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                    CssClass="val-error" ErrorMessage="Enter a valid email address."
                    Text="<i class='ti ti-alert-circle'></i> Enter a valid email address." />
            </div>

            <!-- Password -->
            <div class="form-group">
                <label class="form-label" for="<%= tbPassword.ClientID %>">
                    <i class="ti ti-lock" aria-hidden="true"></i>Password
                </label>
                <div class="input-icon-wrap">
                    <i class="ti ti-lock" aria-hidden="true"></i>
                    <asp:TextBox ID="tbPassword" runat="server" CssClass="form-input"
                        TextMode="Password" placeholder="••••••••" MaxLength="100" />
                </div>
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                    ControlToValidate="tbPassword" Display="Dynamic"
                    CssClass="val-error" ErrorMessage="Password is required."
                    Text="<i class='ti ti-alert-circle'></i> Password is required." />
            </div>

            <!-- Remember me + Forgot -->
            <div class="d-flex align-center" style="justify-content:space-between;margin-bottom:18px">
                <label style="display:flex;align-items:center;gap:6px;font-size:12px;color:var(--text2);cursor:pointer">
                    <asp:CheckBox ID="cbRemember" runat="server" />
                    Remember me
                </label>
                <a href="ForgotPassword.aspx" style="font-size:12px;color:var(--accent2)">Forgot password?</a>
            </div>

            <!-- Submit -->
            <asp:Button ID="btnLogin" runat="server" Text="" CssClass="form-submit"
                OnClick="btnLogin_Click" UseSubmitBehavior="true">
            </asp:Button>
            <%-- Button label set in code-behind to avoid encoding issues --%>

            <div class="form-footer">
                Don't have an account? <a href="Register.aspx">Register here</a>
            </div>

        </div>
    </div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<script>
    // Render button text with icon (avoids HTML in Text attribute)
    document.getElementById('<%= btnLogin.ClientID %>').innerHTML =
        '<i class="ti ti-login" aria-hidden="true"></i> Sign In';
</script>
</asp:Content>
