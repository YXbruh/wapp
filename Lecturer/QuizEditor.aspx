<%@ Page Title="Quiz Editor – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="QuizEditor.aspx.cs" Inherits="CSA.Lecturer.QuizEditor" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Lecturer menu">
        <div class="sidebar-section">Lecturer</div>
        <a href="Lecturer_Dashboard.aspx"       class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="ManageContent.aspx"   class="sidebar-link"><i class="ti ti-files"></i>Manage Content</a>
        <a href="TerminalSandbox.aspx" class="sidebar-link"><i class="ti ti-terminal-2"></i>Terminal Sandbox</a>
        <a href="QuizEditor.aspx"      class="sidebar-link active"><i class="ti ti-list-check"></i>Quiz Editor</a>
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
            <h2>Quiz Editor &amp; Question Bank</h2>
            <p>Create quizzes, add MCQ questions and configure string-match grading criteria.</p>
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

        <!-- ===== Create New Quiz (collapsible) ===== -->
        <div class="card mb-16">
            <div class="card-header" style="display:flex;justify-content:space-between;align-items:center">
                <span><i class="ti ti-square-plus" style="margin-right:6px" aria-hidden="true"></i>Create New Quiz</span>
                <button type="button" class="btn-sm secondary" onclick="toggleNewQuiz()">
                    <span id="newQuizToggleLabel">Show</span>
                </button>
            </div>
            <asp:HiddenField ID="hfNewQuizOpen" runat="server" Value="0" />
            <%-- Choosing a course reloads the chapter list, so the panel has to survive
                 that postback instead of snapping shut mid-entry. --%>
            <div id="newQuizBody" style="<%= hfNewQuizOpen.Value == "1" ? "display:block" : "display:none" %>">
                <div class="newquiz-grid">
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label"><i class="ti ti-books" aria-hidden="true"></i>Course</label>
                        <asp:DropDownList ID="ddlNewQuizCourse" runat="server" CssClass="form-select"
                            AutoPostBack="true" CausesValidation="false"
                            OnSelectedIndexChanged="ddlNewQuizCourse_Changed" />
                        <asp:RequiredFieldValidator ID="rfvNewCourse" runat="server"
                            ControlToValidate="ddlNewQuizCourse" ValidationGroup="NewQuizGroup"
                            InitialValue="" Display="Dynamic" CssClass="val-error"
                            ErrorMessage="Select a course."
                            Text="<i class='ti ti-alert-circle'></i> Required." />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label"><i class="ti ti-book" aria-hidden="true"></i>Chapter</label>
                        <asp:DropDownList ID="ddlNewQuizChapter" runat="server" CssClass="form-select" />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label"><i class="ti ti-list-check" aria-hidden="true"></i>Quiz Title</label>
                        <asp:TextBox ID="tbNewQuizTitle" runat="server" CssClass="form-input"
                            placeholder="e.g. Firewall Basics Quiz" MaxLength="200" />
                        <asp:RequiredFieldValidator ID="rfvNewTitle" runat="server"
                            ControlToValidate="tbNewQuizTitle" ValidationGroup="NewQuizGroup"
                            Display="Dynamic" CssClass="val-error"
                            ErrorMessage="Quiz title is required."
                            Text="<i class='ti ti-alert-circle'></i> Required." />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label">Pass Mark %</label>
                        <asp:TextBox ID="tbNewPassMark" runat="server" CssClass="form-input"
                            TextMode="Number" Text="50" MaxLength="6" />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label">Max Attempts</label>
                        <asp:TextBox ID="tbNewMaxAttempts" runat="server" CssClass="form-input"
                            TextMode="Number" Text="3" MaxLength="2" />
                    </div>
                    <div class="form-group" style="margin-bottom:0;grid-column:1 / -1">
                        <label class="form-label"><i class="ti ti-align-left" aria-hidden="true"></i>Description
                            <span class="text-muted" style="font-weight:400">(optional)</span>
                        </label>
                        <asp:TextBox ID="tbNewQuizDescription" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="2" MaxLength="1000"
                            placeholder="What this quiz covers, or instructions for a file-only assessment..." />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label"><i class="ti ti-calendar-event" aria-hidden="true"></i>Start Date
                            <span class="text-muted" style="font-weight:400">(optional)</span>
                        </label>
                        <asp:TextBox ID="tbNewStartDate" runat="server" CssClass="form-input"
                            placeholder="dd/MM/yyyy HH:mm" MaxLength="20" />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label"><i class="ti ti-calendar-off" aria-hidden="true"></i>End Date
                            <span class="text-muted" style="font-weight:400">(optional)</span>
                        </label>
                        <asp:TextBox ID="tbNewEndDate" runat="server" CssClass="form-input"
                            placeholder="dd/MM/yyyy HH:mm" MaxLength="20" />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label"><i class="ti ti-clock" aria-hidden="true"></i>Duration (minutes)
                            <span class="text-muted" style="font-weight:400">(optional)</span>
                        </label>
                        <asp:TextBox ID="tbNewDuration" runat="server" CssClass="form-input"
                            TextMode="Number" MaxLength="4" placeholder="e.g. 30" />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <label class="form-label"><i class="ti ti-sum" aria-hidden="true"></i>Total Marks</label>
                        <asp:TextBox ID="tbNewTotalMarks" runat="server" CssClass="form-input"
                            TextMode="Number" MaxLength="4" placeholder="e.g. 50" />
                    </div>
                    <div class="form-group" style="margin-bottom:0">
                        <asp:Button ID="btnCreateQuiz" runat="server" CssClass="btn-primary"
                            ValidationGroup="NewQuizGroup" OnClick="btnCreateQuiz_Click"
                            Text="Create Quiz" />
                    </div>
                    <div class="val-hint" style="grid-column:1 / -1">
                        <i class="ti ti-info-circle" aria-hidden="true"></i>
                        A quiz can be saved with no questions — attach a worksheet or PDF below and it works as a file-only assessment.
                        The marks of its questions must add up to Total Marks.
                    </div>
                </div>
            </div>
        </div>

        <div class="cards-row" style="align-items:start">

            <div class="stack-col">
            <!-- LEFT: Quiz selection and quiz-level attachments -->
            <div class="card">
                <div class="card-header">Selected Quiz</div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-books" aria-hidden="true"></i>Course</label>
                    <asp:DropDownList ID="ddlFilterCourse" runat="server" CssClass="form-select"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlFilterCourse_Changed" />
                    <div class="val-hint">
                        <i class="ti ti-info-circle" aria-hidden="true"></i>
                        Pick a course first, then choose one of its quizzes below.
                    </div>
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-list-check" aria-hidden="true"></i>Quiz</label>
                    <div style="display:flex;gap:8px;align-items:flex-start">
                        <asp:DropDownList ID="ddlQuiz" runat="server" CssClass="form-select"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlQuiz_Changed" />
                        <asp:LinkButton ID="btnDeleteQuiz" runat="server" CssClass="btn-danger" CausesValidation="false"
                            OnClick="btnDeleteQuiz_Click" style="flex-shrink:0"
                            OnClientClick="return confirm('Delete this entire quiz and all its questions? This cannot be undone.');">
                            <i class="ti ti-trash"></i>
                        </asp:LinkButton>
                    </div>
                    <div class="val-hint">
                        <i class="ti ti-info-circle" aria-hidden="true"></i>
                        Selecting a quiz filters the question bank and sets where new questions are saved.
                    </div>
                </div>

                <!-- ===== EDIT the selected quiz's details ===== -->
                <asp:Panel ID="pnlEditQuiz" runat="server" Visible="false" CssClass="edit-quiz-block">
                    <div class="card-header" style="margin-top:0;display:flex;justify-content:space-between;align-items:center">
                        <span><i class="ti ti-pencil" style="margin-right:6px" aria-hidden="true"></i>Quiz Details</span>
                        <button type="button" class="btn-sm secondary" onclick="toggleEditQuiz()">
                            <span id="editQuizToggleLabel">Edit</span>
                        </button>
                    </div>

                    <div id="editQuizBody" style="display:none">
                        <div class="editquiz-grid">
                            <div class="form-group" style="margin-bottom:0;grid-column:1 / -1">
                                <label class="form-label"><i class="ti ti-list-check" aria-hidden="true"></i>Quiz Title</label>
                                <asp:TextBox ID="tbEditTitle" runat="server" CssClass="form-input" MaxLength="200" />
                            </div>
                            <div class="form-group" style="margin-bottom:0;grid-column:1 / -1">
                                <label class="form-label"><i class="ti ti-book" aria-hidden="true"></i>Chapter</label>
                                <asp:DropDownList ID="ddlEditChapter" runat="server" CssClass="form-select" />
                            </div>
                            <div class="form-group" style="margin-bottom:0;grid-column:1 / -1">
                                <label class="form-label"><i class="ti ti-align-left" aria-hidden="true"></i>Description</label>
                                <asp:TextBox ID="tbEditDescription" runat="server" CssClass="form-input"
                                    TextMode="MultiLine" Rows="2" MaxLength="1000" />
                            </div>
                            <div class="form-group" style="margin-bottom:0">
                                <label class="form-label"><i class="ti ti-calendar-event" aria-hidden="true"></i>Start Date</label>
                                <asp:TextBox ID="tbEditStartDate" runat="server" CssClass="form-input" placeholder="dd/MM/yyyy HH:mm" MaxLength="20" />
                            </div>
                            <div class="form-group" style="margin-bottom:0">
                                <label class="form-label"><i class="ti ti-calendar-off" aria-hidden="true"></i>End Date</label>
                                <asp:TextBox ID="tbEditEndDate" runat="server" CssClass="form-input" placeholder="dd/MM/yyyy HH:mm" MaxLength="20" />
                            </div>
                            <div class="form-group" style="margin-bottom:0">
                                <label class="form-label"><i class="ti ti-clock" aria-hidden="true"></i>Duration (min)</label>
                                <asp:TextBox ID="tbEditDuration" runat="server" CssClass="form-input" TextMode="Number" MaxLength="4" />
                            </div>
                            <div class="form-group" style="margin-bottom:0">
                                <label class="form-label"><i class="ti ti-sum" aria-hidden="true"></i>Total Marks</label>
                                <asp:TextBox ID="tbEditTotalMarks" runat="server" CssClass="form-input" TextMode="Number" MaxLength="4" />
                            </div>
                            <div class="form-group" style="margin-bottom:0">
                                <label class="form-label">Pass Mark %</label>
                                <asp:TextBox ID="tbEditPassMark" runat="server" CssClass="form-input" TextMode="Number" MaxLength="6" />
                            </div>
                            <div class="form-group" style="margin-bottom:0">
                                <label class="form-label">Max Attempts</label>
                                <asp:TextBox ID="tbEditMaxAttempts" runat="server" CssClass="form-input" TextMode="Number" MaxLength="2" />
                            </div>
                        </div>
                        <asp:Button ID="btnUpdateQuiz" runat="server" CssClass="btn-primary" style="margin-top:12px"
                            CausesValidation="false" OnClick="btnUpdateQuiz_Click" Text="Save Quiz Details" />
                    </div>
                </asp:Panel>

                <!-- ===== ATTACHMENTS for the selected quiz (articles, pictures, media links, documents) ===== -->
                <asp:Panel ID="pnlAttachments" runat="server" Visible="true" CssClass="attachments-block">
                    <div class="card-header" style="margin-top:0">
                        <i class="ti ti-paperclip" style="margin-right:6px" aria-hidden="true"></i>Quiz Attachments
                        <span class="text-muted text-small">(<asp:Literal ID="litAttCount" runat="server" Text="0" />)</span>
                    </div>

                    <asp:Repeater ID="rptAttachments" runat="server" OnItemCommand="rptAttachments_ItemCommand">
                        <ItemTemplate>
                            <div class="attachment-row">
                                <i class="ti <%# GetAttachmentIcon(Eval("AttachmentType").ToString()) %>" aria-hidden="true"></i>
                                <div class="attachment-info">
                                    <a href='<%# GetAttachmentHref(Eval("AttachmentType"), Eval("FilePath"), Eval("LinkUrl"), Eval("IsPending")) %>'
                                       target="_blank" rel="noopener"><%#: Eval("Title") %></a>
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

        <!-- ===== ADD MULTIPLE QUESTIONS AT ONCE ===== -->
        <div class="card">
            <div class="card-header">
                <span><i class="ti ti-list-numbers" style="margin-right:6px" aria-hidden="true"></i>Add Multiple Questions</span>
                <asp:Button ID="btnBulkAddRow" runat="server" CssClass="btn-sm secondary"
                    CausesValidation="false" OnClick="btnBulkAddRow_Click" Text="+ Add Row" />
            </div>

            <asp:Panel ID="pnlMarks" runat="server" CssClass="marks-summary">
                <i class="ti ti-sum" aria-hidden="true"></i>
                <asp:Literal ID="litMarks" runat="server" />
            </asp:Panel>

            <div class="val-hint" style="margin-bottom:12px">
                <i class="ti ti-info-circle" aria-hidden="true"></i>
                Fill in as many rows as you need, then save them all to the selected quiz in one go.
                Only the fields matching each row's type are used.
            </div>

            <asp:Repeater ID="rptBulk" runat="server" OnItemCommand="rptBulk_ItemCommand">
                <ItemTemplate>
                    <div class="bulk-row">
                        <div class="bulk-row-head">
                            <span class="bulk-row-num">#<%# Container.ItemIndex + 1 %></span>
                            <asp:Label runat="server" CssClass="badge badge-amber" Text="editing"
                                Visible='<%# (bool)Eval("IsEdit") %>' />
                            <asp:DropDownList ID="ddlBulkType" runat="server" CssClass="form-select" style="max-width:190px"
                                SelectedValue='<%# Eval("Type") %>'>
                                <asp:ListItem Value="MCQ">Multiple Choice (MCQ)</asp:ListItem>
                                <asp:ListItem Value="Structure">Structure</asp:ListItem>
                                <asp:ListItem Value="TrueFalse">True / False</asp:ListItem>
                            </asp:DropDownList>
                            <asp:TextBox ID="tbBulkMarks" runat="server" CssClass="form-input" style="max-width:90px"
                                TextMode="Number" placeholder="Marks" Text='<%# Eval("Marks") %>' />
                            <asp:LinkButton ID="lbBulkRemove" runat="server" CssClass="btn-danger" CausesValidation="false"
                                CommandName="RemoveRow" CommandArgument='<%# Container.ItemIndex %>'
                                ToolTip="Remove this row">
                                <i class="ti ti-trash"></i>
                            </asp:LinkButton>
                        </div>

                        <asp:TextBox ID="tbBulkText" runat="server" CssClass="form-input" style="margin-bottom:8px"
                            TextMode="MultiLine" Rows="2" placeholder="Question text..."
                            Text='<%# Eval("Text") %>' />

                        <div class="bulk-opts">
                            <div class="type-mcq">
                                <div class="bulk-lbl">MCQ options — tick every correct one</div>
                                <div class="bulk-opt"><asp:CheckBox ID="cbBulkA" runat="server" Checked='<%# Eval("CorrectA") %>' />
                                    <asp:TextBox ID="tbBulkOptA" runat="server" CssClass="form-input" placeholder="Option A" Text='<%# Eval("OptA") %>' /></div>
                                <div class="bulk-opt"><asp:CheckBox ID="cbBulkB" runat="server" Checked='<%# Eval("CorrectB") %>' />
                                    <asp:TextBox ID="tbBulkOptB" runat="server" CssClass="form-input" placeholder="Option B" Text='<%# Eval("OptB") %>' /></div>
                                <div class="bulk-opt"><asp:CheckBox ID="cbBulkC" runat="server" Checked='<%# Eval("CorrectC") %>' />
                                    <asp:TextBox ID="tbBulkOptC" runat="server" CssClass="form-input" placeholder="Option C (optional)" Text='<%# Eval("OptC") %>' /></div>
                                <div class="bulk-opt"><asp:CheckBox ID="cbBulkD" runat="server" Checked='<%# Eval("CorrectD") %>' />
                                    <asp:TextBox ID="tbBulkOptD" runat="server" CssClass="form-input" placeholder="Option D (optional)" Text='<%# Eval("OptD") %>' /></div>
                            </div>
                            <div>
                                <div class="type-structure">
                                    <div class="bulk-lbl">Structure answer</div>
                                    <asp:TextBox ID="tbBulkAnswer" runat="server" CssClass="form-input"
                                        TextMode="MultiLine" Rows="2" placeholder="Expected answer..."
                                        Text='<%# Eval("AnswerText") %>' />
                                </div>
                                <div class="type-truefalse">
                                    <div class="bulk-lbl">True / False answer</div>
                                    <asp:DropDownList ID="ddlBulkTF" runat="server" CssClass="form-select"
                                        SelectedValue='<%# Eval("TfAnswer") %>'>
                                        <asp:ListItem Value="True">True</asp:ListItem>
                                        <asp:ListItem Value="False">False</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>

                        <div class="bulk-lbl" style="margin-top:10px">
                            Explanation <span class="text-muted" style="font-weight:400">(optional — shown to students after submission)</span>
                        </div>
                        <asp:TextBox ID="tbBulkExplanation" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="2" MaxLength="1000"
                            placeholder="Why the correct answer is correct..."
                            Text='<%# Eval("Explanation") %>' />

                        <!-- Attachments for this question, staged until the batch is saved -->
                        <div class="bulk-att">
                            <div class="bulk-lbl">
                                <i class="ti ti-paperclip" aria-hidden="true"></i>
                                Attachments for this question
                                (<asp:Literal ID="litRowAttCount" runat="server" Text='<%# Eval("AttachmentCount") %>' />)
                            </div>

                            <asp:Repeater ID="rptRowAtt" runat="server">
                                <ItemTemplate>
                                    <div class="attachment-row">
                                        <i class="ti <%# GetAttachmentIcon(Eval("AttachmentType").ToString()) %>" aria-hidden="true"></i>
                                        <div class="attachment-info">
                                            <span style="font-size:13px;font-weight:600;color:var(--text)"><%#: Eval("Title") %></span>
                                            <div class="text-small text-muted"><%# Eval("AttachmentType") %> &middot; pending</div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <div class="attachment-add-grid" style="margin-top:8px">
                                <asp:FileUpload ID="fuRowFiles" runat="server" AllowMultiple="true" CssClass="file-input"
                                    accept=".pdf,.doc,.docx,.txt,.ppt,.pptx,.xls,.xlsx,.zip,.png,.jpg,.jpeg,.gif,.webp" />
                                <asp:LinkButton ID="lbRowUpload" runat="server" CssClass="btn-sm secondary" CausesValidation="false"
                                    CommandName="RowUpload" CommandArgument='<%# Container.ItemIndex %>'>Upload</asp:LinkButton>
                            </div>

                            <div class="attachment-add-grid" style="margin-top:8px">
                                <div style="display:flex;gap:8px">
                                    <asp:TextBox ID="tbRowLinkTitle" runat="server" CssClass="form-input" placeholder="Link title" MaxLength="200" style="flex:1" />
                                    <asp:TextBox ID="tbRowLinkUrl" runat="server" CssClass="form-input" placeholder="https://..." MaxLength="500" style="flex:2" />
                                </div>
                                <asp:LinkButton ID="lbRowLink" runat="server" CssClass="btn-sm secondary" CausesValidation="false"
                                    CommandName="RowAddLink" CommandArgument='<%# Container.ItemIndex %>'>Add Link</asp:LinkButton>
                            </div>

                            <asp:LinkButton ID="lbRowClearAtt" runat="server" CssClass="btn-danger" CausesValidation="false"
                                style="margin-top:8px;display:inline-block"
                                CommandName="RowClearAtt" CommandArgument='<%# Container.ItemIndex %>'
                                Visible='<%# (int)Eval("AttachmentCount") > 0 %>'
                                OnClientClick="return confirm('Remove all attachments staged for this question?');">
                                <i class="ti ti-trash"></i> Clear attachments
                            </asp:LinkButton>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlBulkEmpty" runat="server" Visible="false">
                <p class="text-muted text-small" style="padding:12px 0">
                    No rows yet. Click <strong>+ Add Row</strong> to start building a batch of questions.
                </p>
            </asp:Panel>

            <div style="margin-top:12px">
                <asp:Button ID="btnBulkSaveAll" runat="server" CssClass="btn-primary"
                    CausesValidation="false" OnClick="btnBulkSaveAll_Click" Text="Save All Questions" />
                <asp:Button ID="btnBulkClear" runat="server" CssClass="btn-danger" style="margin-left:8px"
                    CausesValidation="false" OnClick="btnBulkClear_Click" Text="Clear Rows"
                    OnClientClick="return confirm('Discard all unsaved rows?');" />
            </div>
        </div>
            </div>

            <!-- RIGHT: Question Bank -->
            <div class="card">
                <div class="card-header">
                    Question Bank
                    <span class="text-muted text-small">
                        (<asp:Literal ID="litCount" runat="server" Text="0" /> questions)
                    </span>
                </div>

                <div class="toolbar" style="margin-bottom:14px">
                    <div class="search-wrap" style="flex:1">
                        <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                            placeholder="Search questions..." AutoPostBack="true"
                            OnTextChanged="tbSearch_TextChanged" />
                        <i class="ti ti-search" aria-hidden="true"></i>
                    </div>
                    <asp:DropDownList ID="ddlFilterType" runat="server" CssClass="form-select"
                        style="width:150px" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlFilterType_Changed">
                        <asp:ListItem Value="">All Types</asp:ListItem>
                        <asp:ListItem Value="MCQ">MCQ</asp:ListItem>
                        <asp:ListItem Value="Structure">Structure</asp:ListItem>
                        <asp:ListItem Value="TrueFalse">True/False</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <asp:Repeater ID="rptQuestions" runat="server"
                              OnItemCommand="rptQuestions_ItemCommand">
                    <ItemTemplate>
                        <div class="question-row">
                            <div class="question-body">
                                <div style="display:flex;align-items:center;gap:8px;margin-bottom:5px">
                                    <span class="badge <%# GetTypeBadge(Eval("QuestionType").ToString()) %>"><%#: Eval("TypeLabel") %></span>
                                    <span class="text-small text-muted"><%# Eval("Points") %> pts &middot; <%#: Eval("QuizName") %></span>
                                </div>
                                <div style="font-size:13px;color:var(--text);font-weight:600"><%#: Eval("QuestionText") %></div>
                            </div>
                            <div class="action-btns" style="flex-shrink:0">
                                <asp:LinkButton runat="server" CssClass="btn-sm secondary" CausesValidation="false"
                                    CommandName="Edit" CommandArgument='<%# Eval("QuestionID") %>'
                                    ToolTip="Load this question into the editor below">
                                    <i class="ti ti-edit"></i>
                                </asp:LinkButton>
                                <asp:LinkButton runat="server" CssClass="btn-danger" CausesValidation="false"
                                    CommandName="Delete" CommandArgument='<%# Eval("QuestionID") %>'
                                    OnClientClick="return confirm('Delete this question from the bank?');">
                                    <i class="ti ti-trash"></i>
                                </asp:LinkButton>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                    <p class="text-muted text-small mt-16" style="text-align:center;padding:16px 0">
                        No questions yet. Use the form to add your first question.
                    </p>
                </asp:Panel>
            </div>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.alert-success{background:rgba(111,207,151,0.12);border:1px solid rgba(111,207,151,0.4);border-radius:8px;padding:12px 16px;font-size:13px;color:var(--success);display:flex;align-items:center;gap:8px}
.question-row{display:flex;align-items:flex-start;justify-content:space-between;gap:12px;padding:12px 0;border-bottom:1px solid var(--bg3)}
.question-row:last-child{border:none}
.question-body{flex:1;min-width:0}
.newquiz-grid{display:grid;grid-template-columns:2fr 2fr 1fr 1fr auto;gap:12px;align-items:end}
@media(max-width:900px){.newquiz-grid{grid-template-columns:1fr 1fr}}
.bulk-row{border:1px solid var(--border);border-radius:8px;padding:14px;margin-bottom:12px;background:var(--bg2)}
.bulk-row-head{display:flex;align-items:center;gap:10px;margin-bottom:10px}
.bulk-row-num{font-size:12px;font-weight:700;color:var(--accent3);min-width:28px}
.bulk-opts{display:grid;grid-template-columns:1fr 1fr;gap:16px}
@media(max-width:900px){.bulk-opts{grid-template-columns:1fr}}
.bulk-lbl{font-size:11px;color:var(--text2);font-weight:600;letter-spacing:.3px;margin-bottom:6px}
.bulk-opt{display:flex;align-items:center;gap:8px;margin-bottom:6px}
.bulk-att{margin-top:12px;padding-top:12px;border-top:1px dashed var(--border)}
/* Left column stacks the quiz selector above the bulk question builder. */
.stack-col{display:flex;flex-direction:column;gap:14px;min-width:0}
.marks-summary{display:flex;align-items:center;gap:8px;font-size:12px;padding:10px 12px;border-radius:8px;background:var(--bg2);border:1px solid var(--border);margin-bottom:12px}
.marks-summary.ok{border-color:var(--success);color:var(--success)}
.marks-summary.warn{border-color:var(--danger);color:var(--danger)}
.edit-quiz-block{border-top:1px solid var(--border);margin-top:16px;padding-top:4px}
.editquiz-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px;align-items:end}
@media(max-width:700px){.editquiz-grid{grid-template-columns:1fr}}
</style>
<script>
    // Each bulk row shows only the answer fields its question type needs.
    function syncBulkRowType(select) {
        var row = select.closest('.bulk-row');
        if (!row) return;
        var t = select.value;
        var show = function (cls, on) {
            var el = row.querySelector(cls);
            if (el) el.style.display = on ? '' : 'none';
        };
        show('.type-mcq', t === 'MCQ');
        show('.type-structure', t === 'Structure');
        show('.type-truefalse', t === 'TrueFalse');
    }

    function syncAllBulkRows() {
        document.querySelectorAll('.bulk-row select').forEach(function (sel) {
            // the type dropdown is the first select in the row header
            if (sel.closest('.bulk-row-head')) {
                syncBulkRowType(sel);
                sel.onchange = function () { syncBulkRowType(sel); };
            }
        });
    }

    document.addEventListener('DOMContentLoaded', syncAllBulkRows);
    // Re-apply after any WebForms postback re-renders the rows.
    if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(syncAllBulkRows);
    }

    function toggleEditQuiz() {
        var body = document.getElementById('editQuizBody');
        var label = document.getElementById('editQuizToggleLabel');
        if (!body) return;
        var open = body.style.display !== 'none';
        body.style.display = open ? 'none' : 'block';
        label.textContent = open ? 'Edit' : 'Hide';
    }

    function toggleNewQuiz() {
        var body = document.getElementById('newQuizBody');
        var label = document.getElementById('newQuizToggleLabel');
        var state = document.getElementById('<%= hfNewQuizOpen.ClientID %>');
        var open = body.style.display !== 'none';
        body.style.display = open ? 'none' : 'block';
        label.textContent = open ? 'Show' : 'Hide';
        if (state) state.value = open ? '0' : '1';
    }

    // Restore the toggle button's caption when the panel came back open from a postback.
    (function () {
        var body = document.getElementById('newQuizBody');
        var label = document.getElementById('newQuizToggleLabel');
        if (body && label && body.style.display !== 'none') label.textContent = 'Hide';
    })();
</script>
</asp:Content>