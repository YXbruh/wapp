<%@ Page Title="Virtual Labs – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Labs.aspx.cs" Inherits="CSA.Student.Student_Labs" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Student menu">
        <div class="sidebar-section">Main</div>
        <a href="Student_Dashboard.aspx"    class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="MyCourses.aspx"    class="sidebar-link"><i class="ti ti-books"></i>My Courses</a>
        <a href="Labs.aspx"         class="sidebar-link active"><i class="ti ti-terminal-2"></i>Virtual Labs</a>
        <a href="Challenges.aspx"   class="sidebar-link"><i class="ti ti-trophy"></i>Challenges</a>
        <div class="sidebar-section">Progress</div>
        <a href="Analytics.aspx"    class="sidebar-link"><i class="ti ti-chart-bar"></i>Analytics</a>
        <a href="Certificates.aspx" class="sidebar-link"><i class="ti ti-certificate"></i>Certificates</a>
        <a href="Achievements.aspx" class="sidebar-link"><i class="ti ti-star"></i>Achievements</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx"      class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Virtual Labs</h2>
            <p>Hands-on labs from your enrolled courses.</p>
        </div>

        <!-- Summary metrics -->
        <div class="metrics" style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Total Labs</div>
                <div class="metric-val"><asp:Literal ID="litTotal" runat="server" Text="0" /></div>
                <div class="metric-sub">assigned</div>
            </div>
            <div class="metric">
                <div class="metric-label">Completed</div>
                <div class="metric-val" style="color:var(--success)">
                    <asp:Literal ID="litDone" runat="server" Text="0" />
                </div>
                <div class="metric-sub">finished</div>
            </div>
            <div class="metric">
                <div class="metric-label">Remaining</div>
                <div class="metric-val" style="color:var(--warning)">
                    <asp:Literal ID="litRemaining" runat="server" Text="0" />
                </div>
                <div class="metric-sub">to complete</div>
            </div>
        </div>

        <!-- Filter chips -->
        <div class="filter-bar" role="group" aria-label="Filter labs">
            <button type="button" class="filter-chip active" onclick="filterItems('all',this,'lab-row')">All</button>
            <button type="button" class="filter-chip" onclick="filterItems('not-started',this,'lab-row')">Not Started</button>
            <button type="button" class="filter-chip" onclick="filterItems('in-progress',this,'lab-row')">In Progress</button>
            <button type="button" class="filter-chip" onclick="filterItems('done',this,'lab-row')">Done</button>
        </div>

        <!-- Labs list -->
        <div class="card">
            <asp:Repeater ID="rptLabs" runat="server">
                <ItemTemplate>
                    <div class="lab-row" data-status='<%# Eval("StatusKey") %>'>
                        <div class="lab-icon">
                            <i class="ti ti-terminal-2" aria-hidden="true"></i>
                        </div>
                        <div class="lab-info">
                            <div class="lab-name"><%# Eval("LabName") %></div>
                            <div class="lab-meta">
                                <span><i class="ti ti-books" aria-hidden="true"></i> <%# Eval("CourseName") %></span>
                                <span><i class="ti ti-clock" aria-hidden="true"></i> ~<%# Eval("EstimatedMinutes") %> min</span>
                                <span><i class="ti ti-tool" aria-hidden="true"></i> <%# Eval("Difficulty") %></span>
                            </div>
                        </div>
                        <div class="lab-actions">
                            <span class="badge <%# Eval("StatusBadgeClass") %>"><%# Eval("StatusLabel") %></span>
                            <a href='LabDetail.aspx?id=<%# Eval("LabID") %>'
                               class="btn-sm <%# Eval("StatusKey").ToString()=="done" ? "secondary" : "" %>">
                                <i class="ti <%# Eval("StatusKey").ToString()=="done" ? "ti-eye" : "ti-player-play" %>" aria-hidden="true"></i>
                                <%# Eval("StatusKey").ToString()=="done" ? "Review" : Eval("StatusKey").ToString()=="in-progress" ? "Continue" : "Start Lab" %>
                            </a>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <div style="text-align:center;padding:40px 20px">
                    <i class="ti ti-terminal-2" style="font-size:40px;color:var(--text3)" aria-hidden="true"></i>
                    <p class="text-muted mt-16">No labs available. Enrol in a course to unlock labs.</p>
                </div>
            </asp:Panel>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.lab-row{display:flex;align-items:center;gap:14px;padding:12px 0;border-bottom:1px solid var(--bg3)}
.lab-row:last-child{border:none}
.lab-icon{width:40px;height:40px;background:var(--accent1);border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:18px;color:var(--accent3);flex-shrink:0}
.lab-info{flex:1;min-width:0}
.lab-name{font-size:14px;font-weight:600;color:var(--text);margin-bottom:4px}
.lab-meta{display:flex;gap:14px;font-size:11px;color:var(--text3);flex-wrap:wrap}
.lab-meta i{font-size:12px;vertical-align:-1px}
.lab-actions{display:flex;align-items:center;gap:10px;flex-shrink:0}
</style>
<script>
function filterItems(status, btn, cls) {
    document.querySelectorAll('.filter-chip').forEach(c => c.classList.remove('active'));
    btn.classList.add('active');
    document.querySelectorAll('.' + cls).forEach(row => {
        row.style.display = (status === 'all' || row.dataset.status === status) ? '' : 'none';
    });
}
</script>
</asp:Content>

