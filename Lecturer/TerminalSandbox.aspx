<%@ Page Title="Terminal Sandbox – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="TerminalSandbox.aspx.cs" Inherits="CSA.Lecturer.TerminalSandbox" %>

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
        <div class="dash-header">
            <h2>Terminal Sandbox Setup</h2>
            <p>Configure virtual lab scenarios and automated grading keys.</p>
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

        <!-- Existing labs list -->
        <div class="card mb-24">
            <div class="card-header">
                Lab Scenarios
                <asp:LinkButton ID="btnNewScenario" runat="server" CssClass="btn-sm"
                    CausesValidation="false" OnClick="btnNewScenario_Click">
                    <i class="ti ti-plus" aria-hidden="true"></i>New Scenario
                </asp:LinkButton>
            </div>
            <div style="overflow-x:auto">
                <table class="admin-table">
                    <thead>
                        <tr>
                            <th>Lab Title</th>
                            <th>Course</th>
                            <th>Difficulty</th>
                            <th>Validation Type</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptLabs" runat="server"
                                      OnItemCommand="rptLabs_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="fw-bold" style="color:var(--text)"><%# Eval("LabTitle") %></div>
                                        <div class="text-small text-muted"><%# Eval("ShortDesc") %></div>
                                    </td>
                                    <td class="text-muted text-small"><%# Eval("CourseName") %></td>
                                    <td><span class="badge <%# GetDiffBadge(Eval("Difficulty").ToString()) %>"><%# Eval("Difficulty") %></span></td>
                                    <td><span class="badge badge-blue"><%# Eval("ValidationType") %></span></td>
                                    <td><span class="badge <%# (bool)Eval("IsActive") ? "badge-green":"badge-amber" %>">
                                        <%# (bool)Eval("IsActive") ? "Active":"Draft" %>
                                    </span></td>
                                    <td>
                                        <div class="action-btns">
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="Edit" CommandArgument='<%# Eval("LabID") %>'>
                                                <i class="ti ti-edit"></i> Edit
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                CommandName="Preview" CommandArgument='<%# Eval("LabID") %>'>
                                                <i class="ti ti-eye"></i> Preview
                                            </asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-danger"
                                                CommandName="Delete" CommandArgument='<%# Eval("LabID") %>'
                                                OnClientClick="return showConfirmAction(this, 'Delete this lab scenario? This cannot be undone.', 'Delete');">
                                                <i class="ti ti-trash"></i>
                                            </asp:LinkButton>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <p class="text-muted text-small mt-16" style="text-align:center;padding:16px 0">
                    No lab scenarios yet. Click <strong>New Scenario</strong> to create one.
                </p>
            </asp:Panel>
        </div>

        <!-- ===== EDITOR + PREVIEW (side-by-side, hidden until toggled) ===== -->
        <div id="editorPanel" style="display:none">
            <div class="sandbox-layout">

                <!-- LEFT: Scenario Editor -->
                <div class="card">
                    <div class="card-header">
                        <span><i class="ti ti-pencil" style="margin-right:6px" aria-hidden="true"></i>
                            <asp:Literal ID="litEditorTitle" runat="server" Text="New Lab Scenario" />
                        </span>
                        <button type="button" onclick="toggleEditor(false)"
                            style="font-size:12px;color:var(--text3);background:none;border:none;cursor:pointer">
                            &times; Close
                        </button>
                    </div>

                    <asp:HiddenField ID="hfLabID" runat="server" Value="0" />

                    <asp:ValidationSummary ID="valSummary" runat="server"
                        ValidationGroup="LabGroup"
                        CssClass="validation-summary-errors" HeaderText="Please fix:"
                        DisplayMode="BulletList" />

                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-books" aria-hidden="true"></i>Course</label>
                        <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" />
                    </div>

                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-heading" aria-hidden="true"></i>Lab Title</label>
                        <asp:TextBox ID="tbLabTitle" runat="server" CssClass="form-input"
                            placeholder="e.g. Log File Analysis" MaxLength="200"
                            oninput="updatePreview()" />
                        <asp:RequiredFieldValidator ID="rfvLabTitle" runat="server"
                            ControlToValidate="tbLabTitle" ValidationGroup="LabGroup"
                            Display="Dynamic" CssClass="val-error"
                            ErrorMessage="Lab title is required."
                            Text="<i class='ti ti-alert-circle'></i> Required." />
                    </div>

                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-flame" aria-hidden="true"></i>Difficulty</label>
                        <asp:DropDownList ID="ddlDifficulty" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Beginner">Beginner</asp:ListItem>
                            <asp:ListItem Value="Intermediate">Intermediate</asp:ListItem>
                            <asp:ListItem Value="Advanced">Advanced</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-player-play" aria-hidden="true"></i>Task Instructions
                            <span class="text-muted" style="font-weight:400">(shown to student)</span>
                        </label>
                        <asp:TextBox ID="tbInstructions" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="5"
                            placeholder="e.g. Navigate to /var/log and examine the authentication log..."
                            MaxLength="3000" oninput="updatePreview()" />
                        <asp:RequiredFieldValidator ID="rfvInstructions" runat="server"
                            ControlToValidate="tbInstructions" ValidationGroup="LabGroup"
                            Display="Dynamic" CssClass="val-error"
                            ErrorMessage="Task instructions are required."
                            Text="<i class='ti ti-alert-circle'></i> Required." />
                    </div>

                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-bulb" aria-hidden="true"></i>Introductory Hint
                            <span class="text-muted" style="font-weight:400">(optional)</span>
                        </label>
                        <asp:TextBox ID="tbHint" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="2"
                            placeholder="e.g. Use the 'ls -la' command to list all files..."
                            MaxLength="500" oninput="updatePreview()" />
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            <i class="ti ti-key" aria-hidden="true"></i>Validation Key
                            <span class="badge badge-red" style="margin-left:6px">Private</span>
                        </label>
                        <div class="input-icon-wrap">
                            <i class="ti ti-lock" aria-hidden="true"></i>
                            <asp:TextBox ID="tbValidationKey" runat="server" CssClass="form-input"
                                placeholder="e.g. cd /var/log && cat auth.log"
                                MaxLength="500" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvValidationKey" runat="server"
                            ControlToValidate="tbValidationKey" ValidationGroup="LabGroup"
                            Display="Dynamic" CssClass="val-error"
                            ErrorMessage="Validation key is required for automated grading."
                            Text="<i class='ti ti-alert-circle'></i> Required." />
                        <div class="val-hint">
                            <i class="ti ti-shield-lock" aria-hidden="true"></i>
                            This key is never shown to students. Used for exact string-match grading.
                        </div>
                    </div>

                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-list" aria-hidden="true"></i>Validation Type</label>
                        <asp:DropDownList ID="ddlValidationType" runat="server" CssClass="form-select">
                            <asp:ListItem Value="ExactMatch">Exact Match</asp:ListItem>
                            <asp:ListItem Value="Contains">Contains String</asp:ListItem>
                            <asp:ListItem Value="Regex">Regex Pattern</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-clock" aria-hidden="true"></i>Time Limit (minutes)</label>
                        <asp:TextBox ID="tbTimeLimit" runat="server" CssClass="form-input"
                            TextMode="Number" placeholder="e.g. 30" MaxLength="3" />
                    </div>

                    <div class="form-group">
                        <label style="display:flex;align-items:center;gap:8px;font-size:12px;color:var(--text2);cursor:pointer">
                            <asp:CheckBox ID="cbActive" runat="server" />
                            Set as Active (visible to enrolled students)
                        </label>
                    </div>

                    <asp:Button ID="btnSaveLab" runat="server" CssClass="btn-primary"
                        ValidationGroup="LabGroup" OnClick="btnSaveLab_Click"
                        Text="Save Lab Scenario" />

                    <!-- ===== ATTACHMENTS (articles, pictures, media links, documents) ===== -->
                    <asp:Panel ID="pnlAttachments" runat="server" Visible="true" CssClass="attachments-block">
                        <div class="card-header" style="margin-top:24px">
                            <i class="ti ti-paperclip" style="margin-right:6px" aria-hidden="true"></i>Attachments
                            <span class="text-muted text-small">(<asp:Literal ID="litAttCount" runat="server" Text="0" />)</span>
                        </div>

                        <asp:Repeater ID="rptAttachments" runat="server" OnItemCommand="rptAttachments_ItemCommand">
                            <ItemTemplate>
                                <div class="attachment-row">
                                    <i class="ti <%# GetAttachmentIcon(Eval("AttachmentType").ToString()) %>" aria-hidden="true"></i>
                                    <div class="attachment-info">
                                        <a href='<%# GetAttachmentHref(Eval("AttachmentType"), Eval("FilePath"), Eval("LinkUrl"), Eval("IsPending")) %>'
                                           target="_blank" rel="noopener"><%# Eval("Title") %></a>
                                        <div class="text-small text-muted">
                                            <%# GetAttachmentMeta(Eval("AttachmentType"), Eval("UploadedByName"), Eval("UploadedAt"), Eval("IsPending")) %>
                                        </div>
                                    </div>
                                    <asp:LinkButton runat="server" CssClass="btn-danger" CausesValidation="false"
                                        CommandName="Delete" CommandArgument='<%# Eval("AttachmentID") %>'
                                        OnClientClick="return confirm('Remove this attachment?');">
                                        <i class="ti ti-trash"></i>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlNoAttachments" runat="server" Visible="false">
                            <p class="text-muted text-small" style="padding:8px 0">No attachments yet.</p>
                        </asp:Panel>

                        <div class="attachment-add-grid">
                            <div class="form-group" style="margin-bottom:0">
                                <label class="form-label"><i class="ti ti-upload" aria-hidden="true"></i>Upload Files / Pictures / Documents
                                    <span class="text-muted" style="font-weight:400">(multiple allowed, max 20 MB each)</span>
                                </label>
                                <asp:FileUpload ID="fuAttachFiles" runat="server" AllowMultiple="true" CssClass="file-input"
                                    accept=".pdf,.doc,.docx,.txt,.ppt,.pptx,.xls,.xlsx,.zip,.png,.jpg,.jpeg,.gif,.webp" />
                            </div>
                            <asp:Button ID="btnUploadFiles" runat="server" CssClass="btn-sm secondary"
                                CausesValidation="false" OnClick="btnUploadFiles_Click" Text="Upload" />
                        </div>

                        <div class="attachment-add-grid" style="margin-top:12px">
                            <div class="form-group" style="margin-bottom:0">
                                <label class="form-label"><i class="ti ti-link" aria-hidden="true"></i>Add a Media Link</label>
                                <div style="display:flex;gap:8px">
                                    <asp:TextBox ID="tbLinkTitle" runat="server" CssClass="form-input" placeholder="Link title" MaxLength="200" style="flex:1" />
                                    <asp:TextBox ID="tbLinkUrl" runat="server" CssClass="form-input" placeholder="https://..." MaxLength="500" style="flex:2" />
                                </div>
                            </div>
                            <asp:Button ID="btnAddLink" runat="server" CssClass="btn-sm secondary"
                                CausesValidation="false" OnClick="btnAddLink_Click" Text="Add Link" />
                        </div>
                    </asp:Panel>
                </div>

                <!-- RIGHT: Terminal Preview -->
                <div class="card terminal-preview-card">
                    <div class="card-header">
                        <span><i class="ti ti-terminal-2" style="margin-right:6px" aria-hidden="true"></i>Student Terminal Preview</span>
                        <span class="badge badge-amber">Preview Mode</span>
                    </div>

                    <!-- Lab info shown to student -->
                    <div class="terminal-lab-info">
                        <div style="font-size:14px;font-weight:700;color:var(--text);margin-bottom:6px" id="prevTitle">Lab Title</div>
                        <div style="font-size:12px;color:var(--text2);line-height:1.6;margin-bottom:12px" id="prevInstructions">Task instructions will appear here...</div>
                        <div id="prevHintWrap" style="display:none;background:var(--bg3);border-left:3px solid var(--accent2);border-radius:4px;padding:10px;font-size:12px;color:var(--text2);margin-bottom:12px">
                            <strong style="color:var(--accent3)"><i class="ti ti-bulb"></i> Hint:</strong>
                            <span id="prevHint"></span>
                        </div>
                    </div>

                    <!-- Simulated terminal window -->
                    <div class="terminal-window">
                        <div class="terminal-titlebar">
                            <span class="terminal-dot" style="background:#E24B4A"></span>
                            <span class="terminal-dot" style="background:#EF9F27"></span>
                            <span class="terminal-dot" style="background:#6FCF97"></span>
                            <span style="margin-left:10px;font-size:11px;color:var(--text3)">
                                student@cybershield:~$
                            </span>
                        </div>
                        <div class="terminal-body" id="terminalOutput">
                            <div class="term-line">
                                <span class="term-prompt">student@cybershield:~$</span>
                                <span class="term-cursor" id="termCursor">&#9646;</span>
                            </div>
                        </div>
                        <div class="terminal-input-row">
                            <span class="term-prompt">student@cybershield:~$</span>
                            <input type="text" class="terminal-input" id="termInput"
                                   placeholder="Type a command..." onkeydown="handleTermInput(event)" />
                        </div>
                    </div>

                    <div class="val-hint mt-8">
                        <i class="ti ti-info-circle" aria-hidden="true"></i>
                        This is a UI preview only. Live sandbox execution is handled server-side.
                    </div>
                </div>

            </div><!-- /.sandbox-layout -->
        </div><!-- /#editorPanel -->

    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
.sandbox-layout{display:grid;grid-template-columns:1fr 1fr;gap:16px;align-items:start}
.terminal-preview-card{}
.terminal-lab-info{padding:12px;background:var(--bg2);border-radius:8px;margin-bottom:14px;border:1px solid var(--border)}
.terminal-window{background:#0a0f0e;border:1px solid var(--border);border-radius:10px;overflow:hidden;font-family:'Cascadia Code','Fira Mono',monospace;font-size:13px}
.terminal-titlebar{background:#1a2a28;padding:8px 14px;display:flex;align-items:center;gap:6px;border-bottom:1px solid #1e3530}
.terminal-dot{width:12px;height:12px;border-radius:50%;display:inline-block}
.terminal-body{padding:12px 14px;min-height:160px;color:#B0E4CC}
.term-line{display:flex;align-items:center;gap:6px;margin-bottom:4px}
.term-prompt{color:#408A71;font-weight:600}
.term-output{color:#B0E4CC;margin-left:4px}
.term-error{color:#E24B4A;margin-left:4px}
.term-cursor{color:#B0E4CC;animation:blink 1s step-end infinite}
@keyframes blink{50%{opacity:0}}
.terminal-input-row{display:flex;align-items:center;gap:8px;padding:8px 14px;background:#0a0f0e;border-top:1px solid #1e3530}
.terminal-input{flex:1;background:transparent;border:none;outline:none;color:#B0E4CC;font-family:inherit;font-size:13px}
.terminal-input::placeholder{color:#285A48}
@media(max-width:900px){.sandbox-layout{grid-template-columns:1fr}}
</style>
<script>
// Toggle editor panel
function toggleEditor(show) {
    var panel = document.getElementById('editorPanel');
    panel.style.display = show ? 'block' : 'none';
    if (show) {
        updatePreview();
        panel.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

// Live preview updates from form fields
function updatePreview() {
    var title = document.getElementById('<%= tbLabTitle.ClientID %>');
    var instr = document.getElementById('<%= tbInstructions.ClientID %>');
    var hint  = document.getElementById('<%= tbHint.ClientID %>');

        if (title) document.getElementById('prevTitle').textContent = title.value || 'Lab Title';
        if (instr) document.getElementById('prevInstructions').textContent = instr.value || 'Task instructions will appear here...';

        var hintVal = hint ? hint.value.trim() : '';
        var hintWrap = document.getElementById('prevHintWrap');
        hintWrap.style.display = hintVal ? 'block' : 'none';
        document.getElementById('prevHint').textContent = hintVal;
    }

    // Simulated terminal interaction (preview only)
    var termHistory = [];
    var termPos = 0;
    function handleTermInput(e) {
        var input = document.getElementById('termInput');
        if (e.key === 'Enter') {
            // Stop WebForms from treating Enter as a form submit (postback),
            // which reloads the page and wipes the preview terminal.
            e.preventDefault();
            var cmd = input.value.trim();
            if (!cmd) return;
            termHistory.push(cmd);
            termPos = termHistory.length;
            appendTermLine(cmd, 'prompt');

            // Simulate a few basic commands for demo
            var out = simulateCmd(cmd);
            if (out.error) appendTermLine(out.text, 'error');
            else appendTermLine(out.text, 'output');
            input.value = '';
        } else if (e.key === 'ArrowUp') {
            if (termPos > 0) { termPos--; input.value = termHistory[termPos]; }
            e.preventDefault();
        } else if (e.key === 'ArrowDown') {
            if (termPos < termHistory.length - 1) { termPos++; input.value = termHistory[termPos]; }
            else { termPos = termHistory.length; input.value = ''; }
            e.preventDefault();
        }
    }

    function appendTermLine(text, type) {
        var body = document.getElementById('terminalOutput');
        var div = document.createElement('div');
        div.className = 'term-line';
        if (type === 'prompt') {
            div.innerHTML = '<span class="term-prompt">student@cybershield:~$</span><span class="term-output"> ' + escHtml(text) + '</span>';
        } else if (type === 'error') {
            div.innerHTML = '<span class="term-error">' + escHtml(text) + '</span>';
        } else {
            div.innerHTML = '<span class="term-output">' + escHtml(text) + '</span>';
        }
        body.appendChild(div);
        body.scrollTop = body.scrollHeight;
    }

    function simulateCmd(cmd) {
        var c = cmd.toLowerCase().trim();
        if (c === 'ls' || c === 'ls -la') return { text: 'auth.log  syslog  kern.log  dpkg.log' };
        if (c === 'pwd') return { text: '/home/student' };
        if (c === 'whoami') return { text: 'student' };
        if (c === 'clear') { document.getElementById('terminalOutput').innerHTML = ''; return { text: '' }; }
        if (c === 'help') return { text: 'Available: ls, pwd, whoami, cd, cat, clear' };
        if (c.startsWith('cd ')) return { text: '' };
        if (c.startsWith('cat ')) return { text: '[Preview] File contents would appear here in live environment.' };
        return { text: 'bash: ' + cmd.split(' ')[0] + ': command not found (preview mode)', error: true };
    }

    function escHtml(s) {
        return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }
</script>
</asp:Content>