<%@ Page Title="Certificates – CyberShield Academy" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeFile="Certificates.aspx.cs" Inherits="CSA.Student.Student_Certificates" %>

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
        <a href="Certificates.aspx" class="sidebar-link active"><i class="ti ti-certificate"></i>Certificates</a>
        <a href="Achievements.aspx" class="sidebar-link"><i class="ti ti-star"></i>Achievements</a>
        <div class="sidebar-section">Account</div>
        <a href="Profile.aspx"      class="sidebar-link"><i class="ti ti-user"></i>Profile</a>
        <asp:LinkButton ID="lbLogout" runat="server" CssClass="sidebar-link" OnClick="lbLogout_Click">
            <i class="ti ti-logout"></i>Sign Out
        </asp:LinkButton>
    </aside>

    <main class="dash-content">
        <div class="dash-header">
            <h2>My Certificates</h2>
            <p>Download and share your earned certificates.</p>
        </div>

        <!-- Count -->
        <div class="metrics" style="grid-template-columns:repeat(2,1fr);max-width:400px;margin-bottom:28px">
            <div class="metric">
                <div class="metric-label">Earned</div>
                <div class="metric-val"><asp:Literal ID="litCount" runat="server" Text="0" /></div>
                <div class="metric-sub">certificates</div>
            </div>
            <div class="metric">
                <div class="metric-label">Latest</div>
                <div class="metric-val" style="font-size:16px">
                    <asp:Literal ID="litLatest" runat="server" Text="—" />
                </div>
                <div class="metric-sub">issued date</div>
            </div>
        </div>

        <!-- Certificate cards grid -->
        <div class="courses-grid" role="list">
            <asp:Repeater ID="rptCerts" runat="server">
                <ItemTemplate>
                    <div class="cert-card" role="listitem">
                        <!-- Card header with gradient -->
                        <div class="cert-header">
                            <i class="ti ti-certificate" aria-hidden="true"></i>
                            <div class="cert-seal">&#10003;</div>
                        </div>
                        <div class="cert-body">
                            <div class="cert-title"><%# Eval("CourseName") %></div>
                            <div class="cert-sub">Certificate of Completion</div>
                            <div class="cert-meta">
                                <span><i class="ti ti-calendar" aria-hidden="true"></i> Issued: <%# Eval("IssuedDate") %></span>
                                <span><i class="ti ti-id-badge" aria-hidden="true"></i> ID: <%# Eval("CertificateID") %></span>
                            </div>
                        </div>
                        <div class="cert-footer">
                            <a href='DownloadCert.aspx?id=<%# Eval("CertificateID") %>'
                               class="btn-sm" target="_blank">
                                <i class="ti ti-download" aria-hidden="true"></i>Download PDF
                            </a>
                            <a href='ViewCert.aspx?id=<%# Eval("CertificateID") %>'
                               class="btn-sm secondary">
                                <i class="ti ti-eye" aria-hidden="true"></i>View
                            </a>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false">
            <div style="text-align:center;padding:60px 20px">
                <i class="ti ti-certificate" style="font-size:48px;color:var(--text3)" aria-hidden="true"></i>
                <p class="text-muted mt-16">No certificates earned yet.</p>
                <p class="text-muted text-small mt-8">Complete a course 100% to receive your certificate.</p>
                <a href="MyCourses.aspx" class="btn-primary" style="display:inline-flex;margin-top:16px">
                    <i class="ti ti-books" aria-hidden="true"></i>Go to My Courses
                </a>
            </div>
        </asp:Panel>
    </main>
</div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="Scripts" runat="server">
<style>
.cert-card{background:var(--surface);border:1px solid var(--border);border-radius:12px;overflow:hidden;transition:border-color .2s,transform .2s;display:flex;flex-direction:column}
.cert-card:hover{border-color:var(--accent2);transform:translateY(-2px)}
.cert-header{height:110px;background:linear-gradient(135deg,var(--accent1),var(--accent2));display:flex;align-items:center;justify-content:center;font-size:44px;color:var(--accent3);position:relative}
.cert-seal{position:absolute;top:10px;right:12px;width:28px;height:28px;border-radius:50%;background:var(--accent3);color:var(--accent1);display:flex;align-items:center;justify-content:center;font-size:14px;font-weight:900}
.cert-body{padding:16px;flex:1}
.cert-title{font-size:14px;font-weight:700;color:var(--text);margin-bottom:4px}
.cert-sub{font-size:11px;color:var(--accent2);font-weight:600;letter-spacing:.5px;text-transform:uppercase;margin-bottom:10px}
.cert-meta{display:flex;flex-direction:column;gap:4px;font-size:11px;color:var(--text3)}
.cert-meta i{font-size:12px;vertical-align:-1px;margin-right:4px}
.cert-footer{padding:12px 16px;border-top:1px solid var(--border);display:flex;gap:8px}
</style>
</asp:Content>
