<%@ Page Title="Register – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Register.aspx.cs" Inherits="CSA.Register" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="auth-page">
        <div class="auth-box">

            <div class="auth-logo">
                <img src="Images/logo.png" alt="CyberShield Academy" class="auth-logo-img"
                     onerror="this.style.display='none'" />
                <h2>CyberShield Academy</h2>
                <p>Create your free account</p>
            </div>

            <div class="auth-tabs" role="tablist">
                <button type="button" class="auth-tab" role="tab"
                        onclick="location.href='Login.aspx'">Sign In</button>
                <button type="button" class="auth-tab active" role="tab"
                        onclick="location.href='Register.aspx'">Register</button>
            </div>

            <asp:ValidationSummary ID="valSummary" runat="server"
                CssClass="validation-summary-errors" HeaderText="Please fix the following:"
                DisplayMode="BulletList" />

            <asp:Panel ID="pnlError" runat="server" Visible="false">
                <div class="validation-summary-errors">
                    <asp:Literal ID="litError" runat="server" />
                </div>
            </asp:Panel>

            <!-- Full Name -->
            <div class="form-group">
                <label class="form-label" for="<%= tbFullName.ClientID %>">Full Name</label>
                <div class="input-icon-wrap">
                    <i class="ti ti-user" aria-hidden="true"></i>
                    <asp:TextBox ID="tbFullName" runat="server" CssClass="form-input"
                        placeholder="John Smith" MaxLength="100" />
                </div>
                <asp:RequiredFieldValidator ID="rfvName" runat="server"
                    ControlToValidate="tbFullName" Display="Dynamic"
                    CssClass="val-error" ErrorMessage="Full name is required."
                    Text="<i class='ti ti-alert-circle'></i> Full name is required." />
            </div>

            <!-- Email -->
            <div class="form-group">
                <label class="form-label" for="<%= tbEmail.ClientID %>">Email Address</label>
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
                <label class="form-label" for="<%= tbPassword.ClientID %>">Password</label>
                <div class="input-icon-wrap">
                    <i class="ti ti-lock" aria-hidden="true"></i>
                    <asp:TextBox ID="tbPassword" runat="server" CssClass="form-input"
                        TextMode="Password" placeholder="Min. 8 characters" MaxLength="100" />
                </div>
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                    ControlToValidate="tbPassword" Display="Dynamic"
                    CssClass="val-error" ErrorMessage="Password is required."
                    Text="<i class='ti ti-alert-circle'></i> Password is required." />
                <asp:RegularExpressionValidator ID="revPassword" runat="server"
                    ControlToValidate="tbPassword" Display="Dynamic"
                    ValidationExpression="^(?=.*[A-Z])(?=.*\d).{8,}$"
                    CssClass="val-error"
                    ErrorMessage="Password must be at least 8 characters, include 1 uppercase letter and 1 number."
                    Text="<i class='ti ti-alert-circle'></i> Min 8 chars, 1 uppercase, 1 number." />
                <div class="val-hint">
                    <i class="ti ti-info-circle" aria-hidden="true"></i>
                    At least 8 characters, 1 uppercase letter, 1 number
                </div>
            </div>

            <!-- Confirm Password -->
            <div class="form-group">
                <label class="form-label" for="<%= tbConfirm.ClientID %>">Confirm Password</label>
                <div class="input-icon-wrap">
                    <i class="ti ti-lock-check" aria-hidden="true"></i>
                    <asp:TextBox ID="tbConfirm" runat="server" CssClass="form-input"
                        TextMode="Password" placeholder="Repeat password" MaxLength="100" />
                </div>
                <asp:RequiredFieldValidator ID="rfvConfirm" runat="server"
                    ControlToValidate="tbConfirm" Display="Dynamic"
                    CssClass="val-error" ErrorMessage="Please confirm your password."
                    Text="<i class='ti ti-alert-circle'></i> Please confirm your password." />
                <asp:CompareValidator ID="cvPasswords" runat="server"
                    ControlToValidate="tbConfirm" ControlToCompare="tbPassword"
                    Display="Dynamic" Operator="Equal"
                    CssClass="val-error" ErrorMessage="Passwords do not match."
                    Text="<i class='ti ti-alert-circle'></i> Passwords do not match." />
            </div>

            <!-- Terms checkbox -->
            <div class="form-group">
                <label style="display:flex;align-items:flex-start;gap:8px;font-size:12px;color:var(--text2);cursor:pointer;line-height:1.6">
                    <asp:CheckBox ID="cbTerms" runat="server" style="margin-top:2px" />
                    I agree to the <a href="Terms.aspx" style="color:var(--accent2)">Terms of Service</a>
                    and <a href="Privacy.aspx" style="color:var(--accent2)">Privacy Policy</a>
                </label>
                <asp:CustomValidator ID="cvTerms" runat="server"
                    ControlToValidate="cbTerms" Display="Dynamic"
                    OnServerValidate="cvTerms_ServerValidate"
                    CssClass="val-error" ErrorMessage="You must accept the Terms of Service."
                    Text="<i class='ti ti-alert-circle'></i> You must accept the Terms of Service." />
            </div>

            <asp:Button ID="btnRegister" runat="server" Text="" CssClass="form-submit"
                OnClick="btnRegister_Click" />

            <div class="form-footer">
                Already registered? <a href="Login.aspx">Sign in</a>
            </div>

        </div>
    </div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<script>
    document.getElementById('<%= btnRegister.ClientID %>').innerHTML =
        '<i class="ti ti-user-plus" aria-hidden="true"></i> Create Account';
</script>
</asp:Content>
