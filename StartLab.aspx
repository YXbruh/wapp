<%@ Page Title="Virtual Lab – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="StartLab.aspx.cs" Inherits="CSA.StartLab" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="container" style="max-width:820px;margin:0 auto;padding:24px 0">

    <asp:Panel ID="pnlBack" runat="server" Visible="false">
        <div style="font-size:12px;color:var(--text3);margin-bottom:16px">
            <asp:HyperLink ID="lnkBack" runat="server" style="color:var(--accent2)">
                <i class="ti ti-arrow-left" aria-hidden="true"></i> Back to Virtual Labs
            </asp:HyperLink>
        </div>
    </asp:Panel>

    <div class="dash-header">
        <h2><asp:Literal ID="litHeading" runat="server" Text="Virtual Lab Terminal" /></h2>
        <p>Practise Linux in a terminal that runs entirely in your browser — nothing is installed and nothing runs on the server.</p>
    </div>

    <!-- Lab brief, shown when this page was opened from a specific lab -->
    <asp:Panel ID="pnlLab" runat="server" Visible="false" CssClass="card mb-16">
        <div class="card-header">
            <span><i class="ti ti-flask" style="margin-right:6px" aria-hidden="true"></i>Lab Brief</span>
            <span>
                <asp:Label ID="lblDifficulty" runat="server" CssClass="badge badge-blue" />
                <asp:Label ID="lblTimeLimit" runat="server" CssClass="text-muted text-small" />
            </span>
        </div>

        <div class="form-group">
            <label class="form-label"><i class="ti ti-list-details" aria-hidden="true"></i>Instructions</label>
            <div class="lab-scenario"><asp:Literal ID="litScenario" runat="server" /></div>
        </div>

        <asp:Panel ID="pnlHint" runat="server" Visible="false">
            <div class="val-hint">
                <i class="ti ti-bulb" aria-hidden="true"></i>
                <asp:Literal ID="litHint" runat="server" />
            </div>
        </asp:Panel>
    </asp:Panel>

    <!-- ===== In-browser terminal (100% client-side, nothing runs on the server) ===== -->
    <div class="card mb-16">
        <div class="card-header">
            <span><i class="ti ti-terminal-2" style="margin-right:6px" aria-hidden="true"></i>Browser Terminal</span>
            <span>
                <span class="text-muted text-small">runs entirely in your browser</span>
                <button type="button" class="btn-sm secondary" style="margin-left:8px" onclick="csaTerm.reload()"
                        title="Save, then reboot the terminal — your files are kept">
                    <i class="ti ti-refresh"></i> Reload
                </button>
                <button type="button" class="btn-sm secondary" style="margin-left:4px" onclick="csaTerm.clear()">
                    <i class="ti ti-eraser"></i> Clear
                </button>
            </span>
        </div>

        <div id="termRoot"
             data-user="<%= Server.HtmlEncode(TermUser) %>"
             data-host="<%= Server.HtmlEncode(TermHost) %>"
             data-flag="<%= Server.HtmlEncode(TermFlag) %>"
             data-motd="<%= Server.HtmlEncode(TermMotd) %>"
             data-savekey="<%= Server.HtmlEncode(TermSaveKey) %>"
             data-stateurl="<%= ResolveUrl("~/TerminalState.ashx") %>"
             data-v86base="<%= ResolveUrl("~/Scripts/v86/") %>">
            <div class="wasm-term" id="termScreen" tabindex="0"></div>
        </div>

        <div class="val-hint" style="margin-top:10px">
            <i class="ti ti-info-circle" aria-hidden="true"></i>
            A real Linux system (Buildroot) booted inside your browser with the v86 WebAssembly
            emulator. Nothing you type leaves your browser and nothing runs on the server. The
            image downloads once (~10 MB) and boots in a few seconds; standard GNU/BusyBox tools
            (<span class="term-code">ls</span>, <span class="term-code">grep</span>,
            <span class="term-code">vi</span>, pipes, redirection) all work for real.
            Your session auto-saves to your account every minute, and is restored the next time you
            sign in and open this lab — even from a different browser or after clearing browser
            data, the files you created will still be there.
        </div>
    </div>

</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<link rel="stylesheet" href='<%= ResolveUrl("~/Scripts/v86/xterm.css") %>' />
<style>
.lab-scenario{white-space:pre-wrap;font-size:13px;line-height:1.6;color:var(--text2);background:var(--bg2);border:1px solid var(--border);border-radius:8px;padding:14px}
.wasm-term{background:#0a0f0e;border:1px solid var(--border);border-radius:10px;padding:10px;min-height:380px}
.wasm-term .xterm{height:100%}
</style>
<script src='<%= ResolveUrl("~/Scripts/v86/xterm.js") %>'></script>
<script src='<%= ResolveUrl("~/Scripts/v86/libv86.js") %>'></script>
<script src='<%= ResolveUrl("~/Scripts/linux-wasm-terminal.js") %>'></script>
</asp:Content>
