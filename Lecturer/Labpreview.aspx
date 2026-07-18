<%@ Page Title="Lab Preview – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Labpreview.aspx.cs" Inherits="CSA.Lecturer.LabPreview" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Lecturer menu">
        <div class="sidebar-section">Lecturer</div>
        <a href="Lecturer_Dashboard.aspx"       class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="ManageContent.aspx"   class="sidebar-link"><i class="ti ti-files"></i>Manage Content</a>
        <a href="TerminalSandbox.aspx" class="sidebar-link active"><i class="ti ti-terminal-2"></i>Terminal Sandbox</a>
        <a href="QuizEditor.aspx"      class="sidebar-link"><i class="ti ti-list-check"></i>Quiz Editor</a>
        <div class="sidebar-section">Students</div>
        <a href="ClassAnalytics.aspx"  class="sidebar-link"><i class="ti ti-chart-bar"></i>Class Analytics</a>
        <a href="Mentorship.aspx"      class="sidebar-link"><i class="ti ti-messages"></i>Mentorship</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">

        <div style="font-size:12px;color:var(--text3);margin-bottom:16px">
            <a href="TerminalSandbox.aspx" style="color:var(--accent2)">
                <i class="ti ti-arrow-left" aria-hidden="true"></i> Back to Terminal Sandbox
            </a>
        </div>

        <div class="dash-header">
            <h2>Lab Preview
                <span class="badge badge-amber" style="vertical-align:middle;margin-left:8px">Lecturer Preview</span>
            </h2>
            <p>This is exactly what a student sees — test your validation key here before publishing.</p>
        </div>

        <div class="sandbox-layout">

            <!-- LEFT: Lab briefing + answer key (lecturer-only) -->
            <div class="card">
                <div class="card-header">
                    <span><i class="ti ti-info-circle" style="margin-right:6px" aria-hidden="true"></i>Lab Briefing</span>
                    <span id="spanStatusBadge" runat="server" class="badge badge-amber">
                        <asp:Literal ID="litStatusText" runat="server" Text="Draft" />
                    </span>
                </div>

                <div class="terminal-lab-info">
                    <div style="font-size:15px;font-weight:700;color:var(--text);margin-bottom:6px">
                        <asp:Literal ID="litTitle" runat="server" />
                    </div>
                    <div class="text-small text-muted mb-8">
                        <asp:Literal ID="litCourse" runat="server" /> &middot;
                        <asp:Literal ID="litDifficulty" runat="server" />
                    </div>
                    <div style="font-size:13px;color:var(--text2);line-height:1.6;margin-bottom:12px;white-space:pre-wrap"><asp:Literal ID="litInstructions" runat="server" /></div>

                    <asp:Panel ID="pnlHint" runat="server" Visible="false"
                        CssClass="hint-box">
                        <strong style="color:var(--accent3)"><i class="ti ti-bulb"></i> Hint:</strong>
                        <asp:Literal ID="litHint" runat="server" />
                    </asp:Panel>
                </div>

                <!-- Answer key: visible to the lecturer only, never rendered on the student page -->
                <div class="answer-key-box">
                    <div style="font-size:11px;font-weight:700;letter-spacing:.4px;color:var(--danger);margin-bottom:6px">
                        <i class="ti ti-shield-lock"></i> ANSWER KEY (LECTURER ONLY)
                    </div>
                    <div class="text-small" style="color:var(--text2);margin-bottom:4px">
                        Validation: <span class="badge badge-blue"><asp:Literal ID="litValType" runat="server" /></span>
                    </div>
                    <code class="answer-key-code"><asp:Literal ID="litExpected" runat="server" /></code>
                </div>
            </div>

            <!-- RIGHT: Student terminal — tests a command against the answer key -->
            <div class="card terminal-preview-card">
                <div class="card-header">
                    <span><i class="ti ti-terminal-2" style="margin-right:6px" aria-hidden="true"></i>Student Terminal</span>
                </div>

                <div class="terminal-window">
                    <div class="terminal-titlebar">
                        <span class="terminal-dot" style="background:#E24B4A"></span>
                        <span class="terminal-dot" style="background:#EF9F27"></span>
                        <span class="terminal-dot" style="background:#6FCF97"></span>
                        <span style="margin-left:10px;font-size:11px;color:var(--text3)">student@cybershield:~$</span>
                    </div>
                    <div class="terminal-body" id="terminalOutput">
                        <div class="term-line">
                            <span class="term-output">Type the command that solves this lab, then press Run to test your key.</span>
                        </div>
                    </div>
                </div>

                <div class="form-group mt-16">
                    <label class="form-label"><i class="ti ti-command" aria-hidden="true"></i>Command</label>
                    <asp:TextBox ID="tbCommand" runat="server" CssClass="form-input"
                        placeholder="e.g. nmap -sS 192.168.1.1" MaxLength="2000" />
                </div>

                <asp:Button ID="btnRun" runat="server" CssClass="btn-primary"
                    OnClick="btnRun_Click" Text="Run &amp; Validate" />

                <asp:Panel ID="pnlResult" runat="server" Visible="false" CssClass="result-box mt-16">
                    <div style="display:flex;align-items:center;gap:8px;font-weight:700">
                        <i id="iResultIcon" runat="server" class="ti" style="font-size:18px"></i>
                        <asp:Literal ID="litResultTitle" runat="server" />
                    </div>
                    <div class="text-small text-muted mt-4">
                        <asp:Literal ID="litResultDetail" runat="server" />
                    </div>
                </asp:Panel>

                <div class="val-hint mt-8">
                    <i class="ti ti-info-circle" aria-hidden="true"></i>
                    This runs the same matching logic students hit on the Challenges page. No submission is saved.
                </div>
            </div>

        </div>

        <!-- ===== In-browser terminal (shared with StartLab), full width at the bottom ===== -->
        <div class="card mt-16">
            <div class="card-header">
                <span><i class="ti ti-terminal-2" style="margin-right:6px" aria-hidden="true"></i>Browser Terminal</span>
                <span>
                    <span class="text-muted text-small">runs entirely in your browser</span>
                    <button type="button" class="btn-sm secondary" style="margin-left:8px" onclick="csaTerm.clear()">
                        <i class="ti ti-eraser"></i> Clear
                    </button>
                </span>
            </div>

            <div id="termRoot"
                 data-user="<%= Server.HtmlEncode(TermUser) %>"
                 data-host="<%= Server.HtmlEncode(TermHost) %>"
                 data-flag="<%= Server.HtmlEncode(TermFlag) %>"
                 data-motd="<%= Server.HtmlEncode(TermMotd) %>">
                <div class="term-screen" id="termScreen" tabindex="0"></div>
                <div class="term-input-row">
                    <span class="term-prompt" id="termPrompt">$</span>
                    <input type="text" id="termInput" class="term-input" autocomplete="off" spellcheck="false"
                           placeholder="type a command — try: help" />
                </div>
            </div>

            <div class="val-hint mt-8">
                <i class="ti ti-info-circle" aria-hidden="true"></i>
                A free-play shell that runs entirely in your browser. It does not check the answer key — it is
                for trying Linux commands. The Student Terminal above is what validates against your key.
            </div>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.sandbox-layout{display:grid;grid-template-columns:1fr 1fr;gap:16px;align-items:start}
.terminal-lab-info{padding:12px;background:var(--bg2);border-radius:8px;margin-bottom:14px;border:1px solid var(--border)}
.hint-box{background:var(--bg3);border-left:3px solid var(--accent2);border-radius:4px;padding:10px;font-size:12px;color:var(--text2)}
.answer-key-box{background:rgba(226,75,74,0.06);border:1px solid rgba(226,75,74,0.3);border-radius:8px;padding:12px}
.answer-key-code{display:block;margin-top:4px;padding:8px 10px;background:#0a0f0e;border-radius:6px;color:#B0E4CC;font-family:'Cascadia Code','Fira Mono',monospace;font-size:13px;word-break:break-all}
.terminal-window{background:#0a0f0e;border:1px solid var(--border);border-radius:10px;overflow:hidden;font-family:'Cascadia Code','Fira Mono',monospace;font-size:13px}
.terminal-titlebar{background:#1a2a28;padding:8px 14px;display:flex;align-items:center;gap:6px;border-bottom:1px solid #1e3530}
.terminal-dot{width:12px;height:12px;border-radius:50%;display:inline-block}
.terminal-body{padding:12px 14px;min-height:120px;color:#B0E4CC}
.term-line{display:flex;align-items:flex-start;gap:6px;margin-bottom:4px}
.term-output{color:#B0E4CC}
.result-box{padding:12px 14px;border-radius:8px;border:1px solid var(--border);background:var(--bg2)}
@media(max-width:900px){.sandbox-layout{grid-template-columns:1fr}}
</style>
<script src='<%= ResolveUrl("~/Scripts/browser-terminal.js") %>'></script>
</asp:Content>