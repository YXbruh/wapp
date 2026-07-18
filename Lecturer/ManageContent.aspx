<%@ Page Title="Manage Content – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="ManageContent.aspx.cs" Inherits="CSA.Lecturer.ManageContent" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Lecturer menu">
        <div class="sidebar-section">Lecturer</div>
        <a href="Lecturer_Dashboard.aspx"       class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="ManageContent.aspx"   class="sidebar-link active"><i class="ti ti-files"></i>Manage Content</a>
        <a href="TerminalSandbox.aspx" class="sidebar-link"><i class="ti ti-terminal-2"></i>Terminal Sandbox</a>
        <a href="QuizEditor.aspx"      class="sidebar-link"><i class="ti ti-list-check"></i>Quiz Editor</a>
        <div class="sidebar-section">Students</div>
        <a href="ClassAnalytics.aspx"  class="sidebar-link"><i class="ti ti-chart-bar"></i>Class Analytics</a>
        <a href="Mentorship.aspx"      class="sidebar-link"><i class="ti ti-messages"></i>Mentorship</a>
        <div class="sidebar-section">Account</div>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Manage Content</h2>
            <p>Create and edit chapters, security articles, and media resources.</p>
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

        <!-- Content type tabs -->
        <div class="auth-tabs" style="max-width:420px;margin-bottom:24px" role="tablist">
            <asp:LinkButton ID="tabChapter" runat="server" CssClass="auth-tab active"
                OnClick="tabChapter_Click" role="tab">
                <i class="ti ti-book"></i> Chapter
            </asp:LinkButton>
            <asp:LinkButton ID="tabArticle" runat="server" CssClass="auth-tab"
                OnClick="tabArticle_Click" role="tab">
                <i class="ti ti-article"></i> Article
            </asp:LinkButton>
            <asp:LinkButton ID="tabMedia" runat="server" CssClass="auth-tab"
                OnClick="tabMedia_Click" role="tab">
                <i class="ti ti-paperclip"></i> Media
            </asp:LinkButton>
        </div>

        <asp:HiddenField ID="hfActiveTab" runat="server" Value="Chapter" />
        <asp:HiddenField ID="hfEditID"    runat="server" Value="0" />

        <div class="cards-row" style="align-items:start">

            <!-- ===== FORM PANEL ===== -->
            <div class="card" style="grid-column:1">

                <!-- Common fields -->
                <div class="card-header">
                    <asp:Literal ID="litFormTitle" runat="server" Text="New Chapter" />
                    <asp:LinkButton ID="lbCancelEdit" runat="server" Visible="false"
                        OnClick="lbCancelEdit_Click" style="font-size:12px;color:var(--text3)">
                        Cancel edit
                    </asp:LinkButton>
                </div>

                <asp:ValidationSummary ID="valSummary" runat="server"
                    ValidationGroup="ContentGroup"
                    CssClass="validation-summary-errors" HeaderText="Please fix:"
                    DisplayMode="BulletList" />

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-books" aria-hidden="true"></i>Course</label>
                    <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" />
                    <asp:RequiredFieldValidator ID="rfvCourse" runat="server"
                        ControlToValidate="ddlCourse" ValidationGroup="ContentGroup"
                        InitialValue="" Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Please select a course."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <div class="form-group">
                    <label class="form-label"><i class="ti ti-heading" aria-hidden="true"></i>Title</label>
                    <asp:TextBox ID="tbTitle" runat="server" CssClass="form-input"
                        placeholder="Content title" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server"
                        ControlToValidate="tbTitle" ValidationGroup="ContentGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Title is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

                <!-- ===== CHAPTER TAB ===== -->
                <asp:Panel ID="pnlChapter" runat="server">
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-sort-ascending-numbers" aria-hidden="true"></i>Chapter Number</label>
                        <asp:TextBox ID="tbChapterNum" runat="server" CssClass="form-input"
                            TextMode="Number" placeholder="e.g. 3" MaxLength="3" />
                    </div>
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-align-left" aria-hidden="true"></i>Lesson Notes
                            <span class="text-muted" style="font-weight:400">(supports Markdown)</span>
                        </label>
                        <asp:TextBox ID="tbChapterBody" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="10"
                            placeholder="Enter detailed lesson notes and technical explanations..." />
                        <asp:RequiredFieldValidator ID="rfvChapterBody" runat="server"
                            ControlToValidate="tbChapterBody" ValidationGroup="ContentGroup"
                            Display="Dynamic" CssClass="val-error"
                            ErrorMessage="Lesson notes are required."
                            Text="<i class='ti ti-alert-circle'></i> Required." />
                        <div class="val-hint">
                            <i class="ti ti-info-circle" aria-hidden="true"></i>
                            Maximum 20,000 characters. Markdown formatting supported.
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-target" aria-hidden="true"></i>Learning Objectives</label>
                        <asp:TextBox ID="tbObjectives" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="3"
                            placeholder="One objective per line..." MaxLength="2000" />
                    </div>
                    <div class="form-group">
                        <label style="display:flex;align-items:center;gap:8px;font-size:12px;color:var(--text2);cursor:pointer">
                            <asp:CheckBox ID="cbPublishChapter" runat="server" />
                            Publish immediately (visible to enrolled students)
                        </label>
                    </div>
                </asp:Panel>

                <!-- ===== ARTICLE TAB ===== -->
                <asp:Panel ID="pnlArticle" runat="server" Visible="false">
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-tag" aria-hidden="true"></i>Category / Topic Tag</label>
                        <asp:TextBox ID="tbArticleTag" runat="server" CssClass="form-input"
                            placeholder="e.g. Network Security, OWASP" MaxLength="100" />
                    </div>
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-align-left" aria-hidden="true"></i>Article Body</label>
                        <asp:TextBox ID="tbArticleBody" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="12"
                            placeholder="Write the full security article here..." />
                        <asp:RequiredFieldValidator ID="rfvArticleBody" runat="server"
                            ControlToValidate="tbArticleBody" ValidationGroup="ContentGroup"
                            Enabled="false" Display="Dynamic" CssClass="val-error"
                            ErrorMessage="Article body is required."
                            Text="<i class='ti ti-alert-circle'></i> Required." />
                        <div class="val-hint">
                            <i class="ti ti-info-circle" aria-hidden="true"></i>
                            Minimum 100 characters. Maximum 50,000 characters.
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-link" aria-hidden="true"></i>External Reference URL
                            <span class="text-muted" style="font-weight:400">(optional)</span>
                        </label>
                        <asp:TextBox ID="tbArticleUrl" runat="server" CssClass="form-input"
                            placeholder="https://..." MaxLength="500" />
                        <asp:RegularExpressionValidator ID="revArticleUrl" runat="server"
                            ControlToValidate="tbArticleUrl" ValidationGroup="ContentGroup"
                            Enabled="false" Display="Dynamic" CssClass="val-error"
                            ValidationExpression="^(https?://)[^\s]{4,}$"
                            ErrorMessage="Enter a valid URL starting with http:// or https://"
                            Text="<i class='ti ti-alert-circle'></i> Enter a valid URL." />
                    </div>
                    <div class="form-group">
                        <label style="display:flex;align-items:center;gap:8px;font-size:12px;color:var(--text2);cursor:pointer">
                            <asp:CheckBox ID="cbPublishArticle" runat="server" />
                            Publish immediately
                        </label>
                    </div>
                </asp:Panel>

                <!-- ===== MEDIA TAB ===== -->
                <asp:Panel ID="pnlMedia" runat="server" Visible="false">
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-upload" aria-hidden="true"></i>Upload File
                            <span class="text-muted" style="font-weight:400">(PDF, PNG, JPG, TXT — max 10 MB)</span>
                        </label>
                        <asp:FileUpload ID="fuMedia" runat="server" CssClass="form-input"
                            style="padding:6px" />
                        <div class="val-hint">
                            <i class="ti ti-info-circle" aria-hidden="true"></i>
                            Allowed: .pdf, .png, .jpg, .jpeg, .txt — saved to ~/App_Data/Uploads/
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-align-left" aria-hidden="true"></i>File Description</label>
                        <asp:TextBox ID="tbMediaDesc" runat="server" CssClass="form-input"
                            TextMode="MultiLine" Rows="3"
                            placeholder="Describe what this file contains..." MaxLength="500" />
                        <asp:RequiredFieldValidator ID="rfvMediaDesc" runat="server"
                            ControlToValidate="tbMediaDesc" ValidationGroup="ContentGroup"
                            Enabled="false" Display="Dynamic" CssClass="val-error"
                            ErrorMessage="File description is required."
                            Text="<i class='ti ti-alert-circle'></i> Required." />
                    </div>
                    <div class="form-group">
                        <label class="form-label"><i class="ti ti-list" aria-hidden="true"></i>Media Type</label>
                        <asp:DropDownList ID="ddlMediaType" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Supplemental">Supplemental Reading</asp:ListItem>
                            <asp:ListItem Value="Reference">Reference Sheet</asp:ListItem>
                            <asp:ListItem Value="Template">Lab Template</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </asp:Panel>

                <asp:Button ID="btnSave" runat="server" CssClass="btn-primary"
                    ValidationGroup="ContentGroup" OnClick="btnSave_Click"
                    Text="Save Content" style="margin-top:8px" />
            </div>

            <!-- ===== EXISTING CONTENT LIST ===== -->
            <div class="card" style="grid-column:2">
                <div class="card-header">
                    Existing <asp:Literal ID="litListTitle" runat="server" Text="Chapters" />
                    <span class="text-muted text-small">
                        (<asp:Literal ID="litCount" runat="server" Text="0" />)
                    </span>
                </div>

                <div class="toolbar" style="margin-bottom:12px">
                    <div class="search-wrap" style="flex:1">
                        <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                            placeholder="Search..." AutoPostBack="true"
                            OnTextChanged="tbSearch_TextChanged" />
                        <i class="ti ti-search" aria-hidden="true"></i>
                    </div>
                </div>

                <div style="overflow-x:auto">
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Course</th>
                                <th>Status</th>
                                <th>Updated</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptContent" runat="server"
                                          OnItemCommand="rptContent_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <div class="fw-bold" style="color:var(--text)"><%# Eval("Title") %></div>
                                            <div class="text-small text-muted"><%# Eval("Subtitle") %></div>
                                        </td>
                                        <td class="text-muted text-small"><%# Eval("CourseName") %></td>
                                        <td><span class="badge <%# (bool)Eval("IsPublished") ? "badge-green":"badge-amber" %>">
                                            <%# (bool)Eval("IsPublished") ? "Published":"Draft" %>
                                        </span></td>
                                        <td class="text-muted text-small"><%# Eval("UpdatedDisplay") %></td>
                                        <td>
                                            <div class="action-btns">
                                                <asp:LinkButton runat="server" CssClass="btn-sm secondary"
                                                    CommandName="Edit" CommandArgument='<%# Eval("ContentID") %>'>
                                                    <i class="ti ti-edit"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton runat="server" CssClass="btn-danger"
                                                    CommandName="Delete" CommandArgument='<%# Eval("ContentID") %>'
                                                    OnClientClick="return showLogoutConfirm(this);">
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
                        No content yet. Use the form to create your first item.
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
</style>
</asp:Content>
