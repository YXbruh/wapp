<%@ Page Title="Certificates – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeFile="Certificates.aspx.cs"
    Inherits="CSA.Student.Student_Certificates" %>

<asp:Content ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .certificate-preview {
        max-width:850px;
        margin:20px auto;
        padding:65px 45px;
        text-align:center;
        background:var(--surface);
        border:8px double var(--accent2);
        border-radius:8px;
    }

    .certificate-preview h1 {
        font-size:38px;
        margin:24px 0;
    }

    .certificate-preview h2 {
        color:var(--accent2);
        margin:18px 0;
    }

    .certificate-logo {
        font-weight:800;
        letter-spacing:3px;
    }
</style>

<div class="dash-layout">

    <aside class="sidebar">

        <div class="sidebar-section">Main</div>

        <a href="Student_Dashboard.aspx"
            class="sidebar-link">
            <i class="ti ti-layout-dashboard"></i>
            Dashboard
        </a>

        <a href="MyCourses.aspx"
            class="sidebar-link">
            <i class="ti ti-books"></i>
            My Courses
        </a>

        <a href="Labs.aspx"
            class="sidebar-link">
            <i class="ti ti-terminal-2"></i>
            Virtual Labs
        </a>

        <a href="Challenges.aspx"
            class="sidebar-link">
            <i class="ti ti-trophy"></i>
            Challenges
        </a>

        <div class="sidebar-section">Progress</div>

        <a href="Analytics.aspx"
            class="sidebar-link">
            <i class="ti ti-chart-bar"></i>
            Analytics
        </a>

        <a href="Certificates.aspx"
            class="sidebar-link active">
            <i class="ti ti-certificate"></i>
            Certificates
        </a>

        <a href="Achievements.aspx"
            class="sidebar-link">
            <i class="ti ti-star"></i>
            Achievements
        </a>

        <div class="sidebar-section">Account</div>

        <a href="Profile.aspx"
            class="sidebar-link">
            <i class="ti ti-user"></i>
            Profile
        </a>

        <asp:LinkButton
            ID="lbLogout"
            runat="server"
            CssClass="sidebar-link"
            CausesValidation="false"
            OnClick="lbLogout_Click">

            <i class="ti ti-logout"></i>
            Sign Out

        </asp:LinkButton>

    </aside>

    <main class="dash-content">

        <asp:Panel
            ID="pnlCertificateList"
            runat="server">

            <div class="dash-header">
                <h2>My Certificates</h2>
                <p>
                    View and download certificates
                    for completed courses.
                </p>
            </div>

            <div class="metrics"
                style="grid-template-columns:repeat(2,1fr);max-width:400px;margin-bottom:28px">

                <div class="metric">

                    <div class="metric-label">
                        Earned
                    </div>

                    <div class="metric-val">
                        <asp:Literal
                            ID="litCount"
                            runat="server"
                            Text="0" />
                    </div>

                    <div class="metric-sub">
                        certificates
                    </div>

                </div>

                <div class="metric">

                    <div class="metric-label">
                        Latest
                    </div>

                    <div class="metric-val"
                        style="font-size:16px">

                        <asp:Literal
                            ID="litLatest"
                            runat="server"
                            Text="—" />

                    </div>

                    <div class="metric-sub">
                        issued date
                    </div>

                </div>

            </div>

            <div class="courses-grid">

                <asp:Repeater
                    ID="rptCerts"
                    runat="server"
                    OnItemCommand="rptCerts_ItemCommand">

                    <ItemTemplate>

                        <div class="cert-card">

                            <div class="cert-header">
                                <i class="ti ti-certificate"></i>
                            </div>

                            <div class="cert-body">

                                <div class="cert-title">
                                    <%# Eval("CourseName") %>
                                </div>

                                <div class="cert-sub">
                                    Certificate of Completion
                                </div>

                                <div class="cert-meta">

                                    <span>
                                        Issued:
                                        <%# Eval("IssuedDate") %>
                                    </span>

                                    <span>
                                        ID:
                                        <%# Eval("CertificateID") %>
                                    </span>

                                </div>

                            </div>

                            <div class="cert-footer">

                                <asp:LinkButton
                                    ID="btnDownload"
                                    runat="server"
                                    CssClass="btn-sm"
                                    CausesValidation="false"
                                    CommandName="Download"
                                    CommandArgument='<%# Eval("CertificateID") %>'>

                                    Download PDF

                                </asp:LinkButton>

                                <asp:LinkButton
                                    ID="btnView"
                                    runat="server"
                                    CssClass="btn-sm secondary"
                                    CausesValidation="false"
                                    CommandName="View"
                                    CommandArgument='<%# Eval("CertificateID") %>'>

                                    View

                                </asp:LinkButton>

                            </div>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

            </div>

            <asp:Panel
                ID="pnlEmpty"
                runat="server"
                Visible="false">

                <div style="text-align:center;padding:60px 20px">

                    <p class="text-muted">
                        No certificates earned yet.
                    </p>

                    <p class="text-muted text-small">
                        Complete a course to receive
                        its certificate.
                    </p>

                </div>

            </asp:Panel>

        </asp:Panel>

        <asp:Panel
            ID="pnlCertificatePreview"
            runat="server"
            Visible="false">

            <asp:LinkButton
                ID="btnBackToCertificates"
                runat="server"
                CssClass="btn-sm secondary"
                CausesValidation="false"
                OnClick="btnBackToCertificates_Click">

                <i class="ti ti-arrow-left"></i>
                Back

            </asp:LinkButton>

            <div class="certificate-preview">

                <div class="certificate-logo">
                    CYBERSHIELD ACADEMY
                </div>

                <h1>
                    Certificate of Completion
                </h1>

                <p>
                    This certificate is proudly
                    presented to
                </p>

                <h2>
                    <asp:Literal
                        ID="litPreviewStudent"
                        runat="server" />
                </h2>

                <p>
                    for successfully completing
                </p>

                <h2>
                    <asp:Literal
                        ID="litPreviewCourse"
                        runat="server" />
                </h2>

                <p>
                    Issued on
                    <asp:Literal
                        ID="litPreviewDate"
                        runat="server" />
                </p>

                <p>
                    Certificate ID:
                    <asp:Literal
                        ID="litPreviewId"
                        runat="server" />
                </p>

                <asp:LinkButton
                    ID="btnDownloadPreview"
                    runat="server"
                    CssClass="btn-primary"
                    CausesValidation="false"
                    OnClick="btnDownloadPreview_Click">

                    Download PDF

                </asp:LinkButton>

            </div>

        </asp:Panel>

    </main>

</div>

</asp:Content>