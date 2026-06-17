<%@ Page Title="Courses – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Courses.aspx.cs" Inherits="CSA.Courses" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <div class="page-header">
        <h2>Course Catalogue</h2>
        <p>Explore courses across network security, web exploitation, forensics, and more.</p>
    </div>

    <div class="page-body">

        <!-- Search bar — icon on the RIGHT -->
        <div style="display:flex;gap:12px;margin-bottom:20px">
            <div class="search-wrap" style="max-width:420px">
                <asp:TextBox ID="tbSearch" runat="server" CssClass="search-input"
                    placeholder="Search courses..."
                    AutoPostBack="true" OnTextChanged="tbSearch_TextChanged" />
                <i class="ti ti-search" aria-hidden="true"></i>
            </div>
        </div>

        <!-- Category filter chips -->
        <div class="filter-bar" role="group" aria-label="Filter courses by category">
            <asp:Repeater ID="rptCategories" runat="server"
                          OnItemCommand="rptCategories_ItemCommand">
                <ItemTemplate>
                    <asp:LinkButton runat="server"
                        CssClass='<%# GetChipClass(Eval("CategoryName").ToString()) %>'
                        CommandName="Filter"
                        CommandArgument='<%# Eval("CategoryName") %>'>
                        <%# Eval("CategoryName") %>
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <!-- Courses grid -->
        <div class="courses-grid" role="list">
            <asp:Repeater ID="rptCourses" runat="server">
                <ItemTemplate>
                    <div class="course-card" role="listitem">
                        <div class="course-thumb">
                            <i class="ti <%# Eval("IconClass") %>" aria-hidden="true"></i>
                            <span class="course-level badge <%# Eval("LevelBadgeClass") %>">
                                <%# Eval("Level") %>
                            </span>
                        </div>
                        <div class="course-body">
                            <h3><%# Eval("CourseName") %></h3>
                            <p><%# Eval("Description") %></p>
                            <div class="course-meta">
                                <span><i class="ti ti-clock" aria-hidden="true"></i> <%# Eval("DurationHours") %>h</span>
                                <span><i class="ti ti-terminal-2" aria-hidden="true"></i> <%# Eval("LabCount") %> labs</span>
                                <span><i class="ti ti-users" aria-hidden="true"></i> <%# Eval("EnrolledCount") %></span>
                            </div>
                        </div>
                        <div class="course-footer">
                            <span class="text-muted text-small"><%# Eval("InstructorName") %></span>
                            <asp:LinkButton runat="server"
                                CssClass='<%# (bool)Eval("IsEnrolled") ? "enroll-btn enrolled" : "enroll-btn" %>'
                                CommandName="Enroll"
                                CommandArgument='<%# Eval("CourseID") %>'
                                Enabled='<%# !(bool)Eval("IsEnrolled") %>'>
                                <%# (bool)Eval("IsEnrolled") ? "Enrolled" : "Enroll Now" %>
                            </asp:LinkButton>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="pnlNoCourses" runat="server" Visible="false">
            <p class="text-muted mt-24" style="text-align:center">
                No courses found. Try a different search or category.
            </p>
        </asp:Panel>

    </div>

</asp:Content>
