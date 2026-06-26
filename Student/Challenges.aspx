<%@ Page Title="Challenges – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Challenges.aspx.cs" Inherits="CSA.Student.Student_Challenges" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Student menu">
        <div class="sidebar-section">Main</div>
        <a href="Student_Dashboard.aspx"    class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="MyCourses.aspx"    class="sidebar-link"><i class="ti ti-books"></i>My Courses</a>
        <a href="Labs.aspx"         class="sidebar-link"><i class="ti ti-terminal-2"></i>Virtual Labs</a>
        <a href="Challenges.aspx"   class="sidebar-link active"><i class="ti ti-trophy"></i>Challenges</a>
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
            <h2>Challenges</h2>
            <p>Complete challenges to earn XP and unlock achievements.</p>
        </div>

        <!-- Summary -->
        <div class="metrics" style="grid-template-columns:repeat(3,1fr);margin-bottom:24px">
            <div class="metric">
                <div class="metric-label">Total</div>
                <div class="metric-val"><asp:Literal ID="litTotal" runat="server" Text="0" /></div>
                <div class="metric-sub">challenges</div>
            </div>
            <div class="metric">
                <div class="metric-label">Completed</div>
                <div class="metric-val" style="color:var(--success)">
                    <asp:Literal ID="litDone" runat="server" Text="0" />
                </div>
                <div class="metric-sub">finished</div>
            </div>
            <div class="metric">
                <div class="metric-label">XP Earned</div>
                <div class="metric-val" style="color:var(--accent2)">
                    <asp:Literal ID="litXP" runat="server" Text="0" />
                </div>
                <div class="metric-sub">points</div>
            </div>
        </div>

        <!-- Filters -->
        <div class="filter-bar" role="group" aria-label="Filter challenges">
            <button type="button" class="filter-chip active" onclick="filterItems('all',this,'challenge-row')">All</button>
            <button type="button" class="filter-chip" onclick="filterItems('not-started',this,'challenge-row')">Not Started</button>
            <button type="button" class="filter-chip" onclick="filterItems('in-progress',this,'challenge-row')">In Progress</button>
            <button type="button" class="filter-chip" onclick="filterItems('done',this,'challenge-row')">Done</button>
        </div>

        <!-- Challenges list -->
        <div class="card">
            <asp:Repeater ID="rptChallenges" runat="server">
                <ItemTemplate>
                    <div class="challenge-row" data-status='<%# Eval("StatusKey") %>'>
                        <div class="challenge-icon">
                            <i class="ti ti-trophy" aria-hidden="true"></i>
                        </div>
                        <div class="challenge-info">
                            <div class="challenge-name"><%# Eval("ChallengeName") %></div>
                            <div class="challenge-meta">
                                <span><i class="ti ti-category" aria-hidden="true"></i> <%# Eval("Category") %></span>
                                <span><i class="ti ti-flame" aria-hidden="true"></i> <%# Eval("Difficulty") %></span>
                                <span><i class="ti ti-coin" aria-hidden="true"></i> <%# Eval("XPReward") %> XP</span>
                                <span><i class="ti ti-clock" aria-hidden="true"></i> <%# Eval("DeadlineDisplay") %></span>
                            </div>
                        </div>
                        <div class="challenge-actions">
                            <span class="badge <%# Eval("StatusBadgeClass") %>"><%# Eval("StatusLabel") %></span>
                            <a href='ChallengeDetail.aspx?id=<%# Eval("ChallengeID") %>'
                               class="btn-sm <%# Eval("StatusKey").ToString()=="done" ? "secondary" : "" %>">
                                <i class="ti <%# Eval("StatusKey").ToString()=="done" ? "ti-eye" : "ti-player-play" %>" aria-hidden="true"></i>
                                <%# Eval("StatusKey").ToString()=="done" ? "Review" : Eval("StatusKey").ToString()=="in-progress" ? "Continue" : "Attempt" %>
                            </a>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
                <div style="text-align:center;padding:40px 20px">
                    <i class="ti ti-trophy" style="font-size:40px;color:var(--text3)" aria-hidden="true"></i>
                    <p class="text-muted mt-16">No challenges available yet. Check back soon!</p>
                </div>
            </asp:Panel>
        </div>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.challenge-row{display:flex;align-items:center;gap:14px;padding:12px 0;border-bottom:1px solid var(--bg3)}
.challenge-row:last-child{border:none}
.challenge-icon{width:40px;height:40px;background:var(--accent1);border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:18px;color:var(--accent3);flex-shrink:0}
.challenge-info{flex:1;min-width:0}
.challenge-name{font-size:14px;font-weight:600;color:var(--text);margin-bottom:4px}
.challenge-meta{display:flex;gap:14px;font-size:11px;color:var(--text3);flex-wrap:wrap}
.challenge-meta i{font-size:12px;vertical-align:-1px}
.challenge-actions{display:flex;align-items:center;gap:10px;flex-shrink:0}
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
