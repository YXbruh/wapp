<%@ Page Title="Something went wrong – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Error.aspx.cs" Inherits="CSA.ErrorPage" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="auth-page">
        <div class="auth-box">

            <div class="auth-logo">
                <h2><i class="ti ti-alert-triangle" aria-hidden="true"></i> Something went wrong</h2>
                <p><asp:Literal ID="litMessage" runat="server" /></p>
            </div>

            <div class="form-footer" style="margin-top:18px">
                <a href="<%= ResolveUrl("~/Default.aspx") %>">Return to the home page</a>
            </div>

        </div>
    </div>

</asp:Content>
