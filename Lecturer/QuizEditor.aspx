<%@ Page Title="Quiz Editor – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="QuizEditor.aspx.cs" Inherits="CSA.Lecturer.QuizEditor" %>

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
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Quiz Editor &amp; Question Bank</h2>
            <p>Create MCQ questions and configure string-match grading criteria.</p>
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

            <!-- LEFT: MCQ Question Form -->
            <div class="card">
                <div class="card-header">
                    <asp:Literal ID="litFormTitle" runat="server" Text="New Question" />
                    <asp:LinkButton ID="lbCancel" runat="server" Visible="false"
                        OnClick="lbCancel_Click" style="font-size:12px;color:var(--text3)">
                        Cancel edit
                    </asp:LinkButton>
                </div>

                <asp:HiddenField ID="hfQuestionID" runat="server" Value="0" />

                <asp:ValidationSummary ID="valSummary" runat="server"
                    ValidationGroup="QuizGroup"
                    CssClass="validation-summary-errors" HeaderText="Please fix:"
                    DisplayMode="BulletList" />

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-books" aria-hidden="true"></i>Quiz / Course</label>
                    <asp:DropDownList ID="ddlQuiz" runat="server" CssClass="form-select" />
                    <asp:RequiredFieldValidator ID="rfvQuiz" runat="server"
                        ControlToValidate="ddlQuiz" ValidationGroup="QuizGroup"
                        InitialValue="" Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Please select a quiz."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-list" aria-hidden="true"></i>Question Type</label>
                    <asp:DropDownList ID="ddlQType" runat="server" CssClass="form-select"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlQType_Changed">
                        <asp:ListItem Value="MCQ">Multiple Choice (MCQ)</asp:ListItem>
                        <asp:ListItem Value="StringMatch">String Match</asp:ListItem>
                        <asp:ListItem Value="TrueFalse">True / False</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-question-mark" aria-hidden="true"></i>Question Text</label>
                    <asp:TextBox ID="tbQuestion" runat="server" CssClass="form-input"
                        TextMode="MultiLine" Rows="3"
                        placeholder="Enter your question here..." MaxLength="1000" />
                    <asp:RequiredFieldValidator ID="rfvQuestion" runat="server"
                        ControlToValidate="tbQuestion" ValidationGroup="QuizGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Question text is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-coin" aria-hidden="true"></i>Points</label>
                    <asp:TextBox ID="tbPoints" runat="server" CssClass="form-input"
                        TextMode="Number" placeholder="e.g. 5" MaxLength="3" />
                </div>

                <!-- MCQ Options -->
                <asp:Panel ID="pnlMCQ" runat="server">
                    <div style="background:var(--bg2);border:1px solid var(--border);border-radius:8px;padding:14px;margin-bottom:16px">
                        <div style="font-size:12px;color:var(--text2);font-weight:600;margin-bottom:12px;letter-spacing:.3px">
                            ANSWER OPTIONS — select the correct one
                        </div>
                        <% string[] labels = {"A","B","C","D"}; %>
                        <div class="form-group">
                            <label class="form-label">Option A</label>
                            <div style="display:flex;gap:8px;align-items:center">
                                <asp:RadioButton ID="rbA" runat="server" GroupName="CorrectAnswer" />
                                <asp:TextBox ID="tbOptA" runat="server" CssClass="form-input" placeholder="Option A text" MaxLength="300" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="form-label">Option B</label>
                            <div style="display:flex;gap:8px;align-items:center">
                                <asp:RadioButton ID="rbB" runat="server" GroupName="CorrectAnswer" />
                                <asp:TextBox ID="tbOptB" runat="server" CssClass="form-input" placeholder="Option B text" MaxLength="300" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="form-label">Option C</label>
                            <div style="display:flex;gap:8px;align-items:center">
                                <asp:RadioButton ID="rbC" runat="server" GroupName="CorrectAnswer" />
                                <asp:TextBox ID="tbOptC" runat="server" CssClass="form-input" placeholder="Option C text" MaxLength="300" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="form-label">Option D</label>
                            <div style="display:flex;gap:8px;align-items:center">
                                <asp:RadioButton ID="rbD" runat="server" GroupName="CorrectAnswer" />
                                <asp:TextBox ID="tbOptD" runat="server" CssClass="form-input" placeholder="Option D text" MaxLength="300" />
                            </div>
                        </div>
                        <div class="val-hint">
                            <i class="ti ti-info-circle" aria-hidden="true"></i>
                            Select the radio button next to the correct answer.
                        </div>
                    </div>
                </asp:Panel>

                <!-- String Match Panel -->
                <asp:Panel ID="pnlStringMatch" runat="server" Visible="false">
                    <div style="background:var(--bg2);border:1px solid var(--border);border-radius:8px;padding:14px;margin-bottom:16px">
                        <div class="form-group">
                            <label class="form-label">
                                <i class="ti ti-key" aria-hidden="true"></i>Correct Answer String
                                <span class="badge badge-red" style="margin-left:4px">Private</span>
                            </label>
                            <asp:TextBox ID="tbCorrectString" runat="server" CssClass="form-input"
                                placeholder="Expected student answer..." MaxLength="500" />
                        </div>
                        <div class="form-group">
                            <label class="form-label"><i class="ti ti-settings" aria-hidden="true"></i>Match Strategy</label>
                            <asp:DropDownList ID="ddlMatchStrategy" runat="server" CssClass="form-select">
                                <asp:ListItem Value="ExactMatch">Exact Match (case-sensitive)</asp:ListItem>
                                <asp:ListItem Value="ExactIgnoreCase">Exact Match (ignore case)</asp:ListItem>
                                <asp:ListItem Value="Contains">Contains String</asp:ListItem>
                                <asp:ListItem Value="Regex">Regex Pattern</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="val-hint">
                            <i class="ti ti-shield-lock" aria-hidden="true"></i>
                            This answer is stored encrypted and never shown to students.
                        </div>
                    </div>
                </asp:Panel>

                <!-- True/False Panel -->
                <asp:Panel ID="pnlTrueFalse" runat="server" Visible="false">
                    <div style="background:var(--bg2);border:1px solid var(--border);border-radius:8px;padding:14px;margin-bottom:16px">
                        <label class="form-label">Correct Answer</label>
                        <div style="display:flex;gap:16px;margin-top:8px">
                            <label style="display:flex;align-items:center;gap:8px;font-size:13px;color:var(--text2);cursor:pointer">
                                <asp:RadioButton ID="rbTrue" runat="server" GroupName="TFAnswer" /> True
                            </label>
                            <label style="display:flex;align-items:center;gap:8px;font-size:13px;color:var(--text2);cursor:pointer">
                                <asp:RadioButton ID="rbFalse" runat="server" GroupName="TFAnswer" /> False
                            </label>
                        </div>
                    </div>
                </asp:Panel>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-info-circle" aria-hidden="true"></i>Explanation
                        <span class="text-muted" style="font-weight:400">(shown after submission)</span>
                    </label>
                    <asp:TextBox ID="tbExplanation" runat="server" CssClass="form-input"
                        TextMode="MultiLine" Rows="2"
                        placeholder="Explain why the correct answer is correct..." MaxLength="500" />
                </div>

                <asp:Button ID="btnSaveQuestion" runat="server" CssClass="btn-primary"
                    ValidationGroup="QuizGroup" OnClick="btnSaveQuestion_Click"
                    Text="Save Question" />
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
                        <asp:ListItem Value="StringMatch">String Match</asp:ListItem>
                        <asp:ListItem Value="TrueFalse">True/False</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <asp:Repeater ID="rptQuestions" runat="server"
                              OnItemCommand="rptQuestions_ItemCommand">
                    <ItemTemplate>
                        <div class="question-row">
                            <div class="question-body">
                                <div style="display:flex;align-items:center;gap:8px;margin-bottom:5px">
                                    <span class="badge <%# GetTypeBadge(Eval("QuestionType").ToString()) %>"><%# Eval("QuestionType") %></span>
                                    <span class="text-small text-muted"><%# Eval("Points") %> pts &middot; <%# Eval("QuizName") %></span>
                                </div>
                                <div style="font-size:13px;color:var(--text);font-weight:600"><%# Eval("QuestionText") %></div>
                            </div>
                            <div class="action-btns" style="flex-shrink:0">
                                <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                    CommandName="Edit" CommandArgument='<%# Eval("QuestionID") %>'>
                                    <i class="ti ti-edit"></i>
                                </asp:LinkButton>
                                <asp:LinkButton runat="server" CssClass="btn-danger"
                                    CommandName="Delete" CommandArgument='<%# Eval("QuestionID") %>'
                                    OnClientClick="return confirm('Delete this question?');">
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
</style>
</asp:Content>
