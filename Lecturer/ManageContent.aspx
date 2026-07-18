<%@ Page Title="Manage Content – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ManageContent.aspx.cs" Inherits="CSA.Lecturer.ManageContent" %>

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
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Manage Content</h2>
            <p>Create and edit course chapters, with articles, pictures, media links, and documents attached.</p>
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

        <asp:HiddenField ID="hfEditID" runat="server" Value="" />

        <div class="cards-row" style="align-items:start">

            <!-- ===== FORM PANEL ===== -->
            <div class="card" style="grid-column:1">

                <div class="card-header">
                    <asp:Literal ID="litFormTitle" runat="server" Text="New Chapter" />
                    <asp:LinkButton ID="lbCancelEdit" runat="server" Visible="false"
                        CausesValidation="false" OnClick="lbCancelEdit_Click" style="font-size:12px;color:var(--text3)">
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
                        placeholder="Chapter title" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server"
                        ControlToValidate="tbTitle" ValidationGroup="ContentGroup"
                        Display="Dynamic" CssClass="val-error"
                        ErrorMessage="Title is required."
                        Text="<i class='ti ti-alert-circle'></i> Required." />
                </div>

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

                <asp:Button ID="btnSave" runat="server" CssClass="btn-primary"
                    ValidationGroup="ContentGroup" OnClick="btnSave_Click"
                    Text="Save Chapter" style="margin-top:8px" />

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
                            <div class="val-hint">
                                <i class="ti ti-info-circle" aria-hidden="true"></i>
                                Documents: PDF, Word, PowerPoint, Excel, TXT, ZIP. Pictures: PNG, JPG, GIF, WEBP.
                            </div>
                        </div>
                        <asp:Button ID="btnUploadFiles" runat="server" CssClass="btn-sm secondary"
                            CausesValidation="false" OnClick="btnUploadFiles_Click" Text="Upload" />
                    </div>

                    <div class="attachment-add-grid" style="margin-top:12px">
                        <div class="form-group" style="margin-bottom:0">
                            <label class="form-label"><i class="ti ti-link" aria-hidden="true"></i>Add a Media Link
                                <span class="text-muted" style="font-weight:400">(e.g. a video or external article)</span>
                            </label>
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

            <!-- ===== EXISTING CONTENT LIST ===== -->
            <div class="card" style="grid-column:2">
                <div class="card-header">
                    Existing Chapters
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
                                                    OnClientClick="return showConfirmAction(this, 'Delete this chapter? This cannot be undone.', 'Delete');">
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
                        No chapters yet. Use the form to create your first one.
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
