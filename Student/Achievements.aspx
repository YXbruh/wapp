<%@ Page Title="Achievements – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Achievements.aspx.cs" Inherits="CSA.Student.Student_Achievements" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
<div class="dash-layout">

    <aside class="sidebar" role="navigation" aria-label="Student menu">
        <div class="sidebar-section">Main</div>
        <a href="Student_Dashboard.aspx"    class="sidebar-link"><i class="ti ti-layout-dashboard"></i>Dashboard</a>
        <a href="MyCourses.aspx"    class="sidebar-link"><i class="ti ti-books"></i>My Courses</a>
        <a href="Labs.aspx"         class="sidebar-link"><i class="ti ti-terminal-2"></i>Virtual Labs</a>
        <a href="Challenges.aspx"   class="sidebar-link"><i class="ti ti-trophy"></i>Challenges</a>
        <div class="sidebar-section">Progress</div>
        <a href="Analytics.aspx"    class="sidebar-link"><i class="ti ti-chart-bar"></i>Analytics</a>
        <a href="Certificates.aspx" class="sidebar-link"><i class="ti ti-certificate"></i>Certificates</a>
        <a href="Achievements.aspx" class="sidebar-link active"><i class="ti ti-star"></i>Achievements</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx"      class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>Achievements</h2>
            <p>Badges you've earned on your cybersecurity journey.</p>
        </div>

        <!-- Stats row -->
        <div class="metrics" style="grid-template-columns:repeat(3,1fr);margin-bottom:28px">
            <div class="metric">
                <div class="metric-label">Badges Earned</div>
                <div class="metric-val"><asp:Literal ID="litEarned" runat="server" Text="0" /></div>
                <div class="metric-sub">out of <asp:Literal ID="litTotal" runat="server" Text="0" /></div>
            </div>
            <div class="metric">
                <div class="metric-label">Total XP</div>
                <div class="metric-val" style="color:var(--accent2)">
                    <asp:Literal ID="litXP" runat="server" Text="0" />
                </div>
                <div class="metric-sub">points</div>
            </div>
            <div class="metric">
                <div class="metric-label">Latest Badge</div>
                <div class="metric-val" style="font-size:14px;line-height:1.3">
                    <asp:Literal ID="litLatest" runat="server" Text="—" />
                </div>
                <div class="metric-sub">most recent</div>
            </div>
        </div>

        <!-- Filter chips -->
        <div class="filter-bar" role="group" aria-label="Filter achievements">
            <button type="button" class="filter-chip active" onclick="filterBadges('all',this)">All</button>
            <button type="button" class="filter-chip" onclick="filterBadges('earned',this)">Earned</button>
            <button type="button" class="filter-chip" onclick="filterBadges('locked',this)">Locked</button>
        </div>

        <!-- Badge grid -->
        <div class="badge-grid" id="badgeGrid">
            <asp:Repeater ID="rptAchievements" runat="server">
                <ItemTemplate>
                    <div class="badge-card <%# (bool)Eval("IsEarned") ? "earned" : "locked" %>"
                         data-earned='<%# (bool)Eval("IsEarned") ? "earned" : "locked" %>'>
                        <div class="badge-icon-wrap <%# (bool)Eval("IsEarned") ? "" : "locked-icon" %>">
                            <i class="ti <%# Eval("IconClass") %>" aria-hidden="true"></i>
                        </div>
                        <div class="badge-name"><%# Eval("BadgeName") %></div>
                        <div class="badge-desc"><%# Eval("Description") %></div>
                        <div class="badge-date">
                            <%# (bool)Eval("IsEarned")
                                ? "<span class='badge badge-green'>Earned " + Eval("EarnedDate") + "</span>"
                                : "<span class='badge badge-amber'>Locked</span>" %>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
            <div style="text-align:center;padding:60px 20px">
                <i class="ti ti-star" style="font-size:48px;color:var(--text3)" aria-hidden="true"></i>
                <p class="text-muted mt-16">No achievements yet — start completing labs and challenges!</p>
            </div>
        </asp:Panel>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.badge-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:16px}
.badge-card{background:var(--surface);border:1px solid var(--border);border-radius:12px;padding:20px;text-align:center;transition:border-color .2s,transform .2s}
.badge-card.earned:hover{border-color:var(--accent2);transform:translateY(-2px)}
.badge-card.locked{opacity:.5}
.badge-icon-wrap{width:56px;height:56px;background:var(--accent1);border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:24px;color:var(--accent3);margin:0 auto 12px;border:2px solid var(--accent2)}
.badge-icon-wrap.locked-icon{background:var(--bg3);border-color:var(--border);color:var(--text3)}
.badge-name{font-size:13px;font-weight:700;color:var(--text);margin-bottom:6px}
.badge-desc{font-size:11px;color:var(--text2);line-height:1.5;margin-bottom:10px}
.badge-date{font-size:11px}
@media(max-width:900px){.badge-grid{grid-template-columns:repeat(2,1fr)}}
@media(max-width:500px){.badge-grid{grid-template-columns:1fr 1fr}}
</style>
<script>
function filterBadges(type, btn) {
    document.querySelectorAll('.filter-chip').forEach(c => c.classList.remove('active'));
    btn.classList.add('active');
    document.querySelectorAll('.badge-card').forEach(card => {
        card.style.display = (type === 'all' || card.dataset.earned === type) ? '' : 'none';
    });
}
</script>
</asp:Content>
