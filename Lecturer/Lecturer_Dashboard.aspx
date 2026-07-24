<%@ Page Title="Lecturer Dashboard – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Lecturer_Dashboard.aspx.cs" Inherits="CSA.Lecturer.Lecturer_Dashboard" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Lecturer menu">
        <div class="sidebar-section">Lecturer</div>
        <a href="Lecturer_Dashboard.aspx"      class="sidebar-link active"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="ManageContent.aspx"  class="sidebar-link"><i class="ti ti-files"></i>Manage Content</a>
        <a href="TerminalSandbox.aspx"class="sidebar-link"><i class="ti ti-terminal-2"></i>Terminal Sandbox</a>
        <a href="QuizEditor.aspx"     class="sidebar-link"><i class="ti ti-list-check"></i>Quiz Editor</a>
        <div class="sidebar-section">Students</div>
        <a href="ClassAnalytics.aspx" class="sidebar-link"><i class="ti ti-chart-bar"></i>Class Analytics</a>
        <a href="Mentorship.aspx"     class="sidebar-link"><i class="ti ti-messages"></i>Mentorship</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx" class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" OnClientClick="return showLogoutConfirm(this);" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">

        <!-- Header -->
        <div class="dash-header">
            <h2>Lecturer Dashboard</h2>
            <p>Welcome back, <asp:Literal ID="litName" runat="server" Text="Lecturer" /> &mdash;
               <asp:Literal ID="litDate" runat="server" /></p>
        </div>

        <!-- Metric readouts -->
        <div class="metrics" style="margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Active Modules</div>
                <div class="metric-val"><asp:Literal ID="litModules" runat="server" Text="0"/></div>
                <div class="metric-sub">published</div>
            </div>
            <div class="metric">
                <div class="metric-label">Enrolled Students</div>
                <div class="metric-val"><asp:Literal ID="litStudents" runat="server" Text="0"/></div>
                <div class="metric-sub">across all courses</div>
            </div>
            <div class="metric">
                <div class="metric-label">Lab Completion</div>
                <div class="metric-val" style="color:var(--accent2)">
                    <asp:Literal ID="litLabRate" runat="server" Text="0%"/>
                </div>
                <div class="metric-sub">class average</div>
            </div>
            <div class="metric">
                <div class="metric-label">Avg Quiz Score</div>
                <div class="metric-val" style="color:var(--accent2)">
                    <asp:Literal ID="litQuizAvg" runat="server" Text="—"/>
                </div>
                <div class="metric-sub">all quizzes</div>
            </div>
        </div>

        <!-- Lab completion bar per course -->
        <div class="cards-row" style="margin-bottom:24px">
            <div class="card">
                <div class="card-header">
                    Terminal Lab Completion
                    <a href="ClassAnalytics.aspx">Full report &rarr;</a>
                </div>
                <asp:Repeater ID="rptLabRates" runat="server">
                    <ItemTemplate>
                        <div style="margin-bottom:14px">
                            <div style="display:flex;justify-content:space-between;font-size:13px;margin-bottom:5px">
                                <span style="font-weight:600;color:var(--text)"><%#: Eval("CourseName") %></span>
                                <span style="color:var(--accent2);font-weight:700"><%# Eval("CompletionPct") %>%</span>
                            </div>
                            <div class="progress-bar" style="height:6px">
                                <div class="progress-fill" style="width:<%# Eval("CompletionPct") %>%"></div>
                            </div>
                            <div style="font-size:11px;color:var(--text3);margin-top:3px">
                                <%# Eval("CompletedCount") %> / <%# Eval("TotalStudents") %> students completed
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoLabs" runat="server" Visible="false">
                    <p class="text-muted text-small mt-8">No lab data yet.</p>
                </asp:Panel>
            </div>

            <!-- Quiz score distribution -->
            <div class="card">
                <div class="card-header">
                    Quiz Score Overview
                    <a href="ClassAnalytics.aspx">Full report &rarr;</a>
                </div>
                <asp:Repeater ID="rptQuizScores" runat="server">
                    <ItemTemplate>
                        <div style="margin-bottom:14px">
                            <div style="display:flex;justify-content:space-between;font-size:13px;margin-bottom:5px">
                                <span style="font-weight:600;color:var(--text)"><%#: Eval("QuizName") %></span>
                                <span style="font-weight:700;color:<%# GetScoreColor(Convert.ToInt32(Eval("AvgScore"))) %>">
                                    <%# Eval("AvgScore") %>%
                                </span>
                            </div>
                            <div class="progress-bar" style="height:6px">
                                <div class="progress-fill"
                                     style="width:<%# Eval("AvgScore") %>%;background:<%# GetScoreColor(Convert.ToInt32(Eval("AvgScore"))) %>">
                                </div>
                            </div>
                            <div style="font-size:11px;color:var(--text3);margin-top:3px">
                                <%# Eval("AttemptCount") %> attempts &middot; <%#: Eval("CourseName") %>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoQuiz" runat="server" Visible="false">
                    <p class="text-muted text-small mt-8">No quiz data yet.</p>
                </asp:Panel>
            </div>
        </div>

        <!-- Active modules summary grid -->
        <div class="card">
            <div class="card-header">
                Active Learning Modules
                <a href="ManageContent.aspx">Manage &rarr;</a>
            </div>
            <div style="overflow-x:auto">
                <table class="admin-table" role="grid">
                    <thead>
                        <tr>
                            <th scope="col">Module / Chapter</th>
                            <th scope="col">Course</th>
                            <th scope="col">Type</th>
                            <th scope="col">Status</th>
                            <th scope="col">Students Viewed</th>
                            <th scope="col">Last Updated</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptModules" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="fw-bold" style="color:var(--text)"><%#: Eval("Title") %></div>
                                        <div class="text-small text-muted"><%#: Eval("Subtitle") %></div>
                                    </td>
                                    <td class="text-muted"><%#: Eval("CourseName") %></td>
                                    <td><span class="badge <%# GetTypeBadge(Eval("ContentType").ToString()) %>"><%#: Eval("ContentType") %></span></td>
                                    <td><span class="badge <%# Eval("IsPublished").ToString()=="True" ? "badge-green" : "badge-amber" %>">
                                        <%# Eval("IsPublished").ToString()=="True" ? "Published" : "Draft" %>
                                    </span></td>
                                    <td class="text-muted"><%#: Eval("ViewCount") %></td>
                                    <td class="text-muted text-small"><%#: Eval("UpdatedDisplay") %></td>
                                    <td>
                                        <a href='ManageContent.aspx?id=<%# Eval("ContentID") %>' class="btn-sm secondary">
                                            <i class="ti ti-edit"></i> Edit
                                        </a>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
            <asp:Panel ID="pnlNoModules" runat="server" Visible="false">
                <div style="text-align:center;padding:32px 0">
                    <i class="ti ti-files" style="font-size:36px;color:var(--text3)" aria-hidden="true"></i>
                    <p class="text-muted mt-8">No modules yet. <a href="ManageContent.aspx">Create your first module</a></p>
                </div>
            </asp:Panel>
        </div>

    </main>
</div>
</asp:Content>
