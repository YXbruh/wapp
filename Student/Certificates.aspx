<%@ Page Title="Certificates – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Certificates.aspx.cs"
    Inherits="CSA.Student.Student_Certificates" %>

<asp:Content
    ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    .certificate-grid {
        display:grid;
        grid-template-columns:repeat(auto-fill, minmax(300px, 1fr));
        gap:20px;
    }

    .cert-card {
        overflow:hidden;
        color:var(--text);
        background:var(--surface);
        border:1px solid var(--border);
        border-radius:12px;
        box-shadow:0 4px 14px rgba(0, 0, 0, .10);
        transition:transform .2s ease,
                   border-color .2s ease,
                   box-shadow .2s ease;
    }

    .cert-card:hover {
        transform:translateY(-3px);
        border-color:var(--accent2);
        box-shadow:0 8px 20px rgba(0, 0, 0, .14);
    }

    .cert-header {
        position:relative;
        display:flex;
        align-items:center;
        justify-content:center;
        height:98px;
        color:#d8fff2;
        background:linear-gradient(135deg, #246f5c, #459a7e);
        border-bottom:1px solid var(--border);
    }

    .cert-header > i {
        font-size:38px;
    }

    .cert-check {
        position:absolute;
        top:10px;
        right:11px;
        display:flex;
        align-items:center;
        justify-content:center;
        width:25px;
        height:25px;
        color:#286d5a;
        background:#b4ead5;
        border-radius:50%;
        font-size:15px;
        font-weight:900;
    }

    .cert-body {
        padding:15px 14px 13px;
    }

    .cert-title {
        margin-bottom:5px;
        color:var(--text);
        font-size:14px;
        font-weight:800;
        line-height:1.35;
    }

    .cert-sub {
        margin-bottom:12px;
        color:var(--accent2);
        font-size:10px;
        font-weight:800;
        letter-spacing:.8px;
        text-transform:uppercase;
    }

    .cert-meta {
        display:flex;
        flex-direction:column;
        gap:6px;
        color:var(--text2);
        font-size:11px;
    }

    .cert-meta span {
        display:flex;
        align-items:center;
        gap:6px;
    }

    .cert-meta i {
        color:var(--accent2);
        font-size:12px;
    }

    .cert-footer {
        display:flex;
        gap:7px;
        padding:9px 11px;
        background:var(--bg2);
        border-top:1px solid var(--border);
    }

    .cert-footer .btn-sm {
        display:inline-flex;
        align-items:center;
        justify-content:center;
        gap:5px;
        min-height:30px;
        padding:6px 12px;
        color:#071d17;
        background:var(--accent2);
        border:1px solid var(--accent2);
        border-radius:7px;
        font-size:11px;
        font-weight:800;
        text-decoration:none;
        cursor:pointer;
    }

    .cert-footer .btn-sm:hover {
        color:#071d17;
        filter:brightness(1.08);
    }

    .cert-footer .btn-sm.secondary {
        color:var(--text);
        background:var(--surface);
        border-color:var(--border);
    }

    .certificate-preview-actions {
        display:flex;
        align-items:center;
        justify-content:space-between;
        max-width:850px;
        margin:0 auto 18px;
        gap:12px;
    }

    .certificate-preview {
        max-width:850px;
        margin:20px auto;
        padding:65px 45px;
        color:var(--text);
        text-align:center;
        background:var(--surface);
        border:8px double var(--accent2);
        border-radius:8px;
    }

    .certificate-preview h1 {
        margin:24px 0;
        color:var(--text);
        font-size:38px;
    }

    .certificate-preview h2 {
        margin:18px 0;
        color:var(--accent2);
    }

    .certificate-logo {
        color:var(--text);
        font-weight:800;
        letter-spacing:3px;
    }

    .certificate-preview.pdf-mode {
        color:#d9fff3;
        background:#071713;
        border-color:#42b892;
    }

    .certificate-preview.pdf-mode h1,
    .certificate-preview.pdf-mode .certificate-logo {
        color:#d9fff3;
    }

    .certificate-preview.pdf-mode h2 {
        color:#63d8b3;
    }

    .certificate-download-button {
        border:none;
        cursor:pointer;
        font-family:inherit;
    }

    .certificate-download-button:disabled {
        opacity:.65;
        cursor:not-allowed;
    }

    .certificate-empty {
        padding:60px 20px;
        text-align:center;
    }

    @media (max-width:700px) {
        .certificate-grid {
            grid-template-columns:1fr;
        }

        .certificate-preview-actions {
            align-items:stretch;
            flex-direction:column;
        }

        .certificate-preview {
            padding:40px 20px;
        }

        .certificate-preview h1 {
            font-size:28px;
        }
    }
</style>

<div class="dash-layout">

    <aside class="sidebar">

        <div class="sidebar-section">Main</div>

        <a href="Student_Dashboard.aspx" class="sidebar-link">
            <i class="ti ti-layout-dashboard"></i>
            Dashboard
        </a>

        <a href="MyCourses.aspx" class="sidebar-link">
            <i class="ti ti-books"></i>
            My Courses
        </a>

        <a href="Labs.aspx" class="sidebar-link">
            <i class="ti ti-terminal-2"></i>
            Virtual Labs
        </a>

        <a href="Challenges.aspx" class="sidebar-link">
            <i class="ti ti-trophy"></i>
            Challenges
        </a>

        <div class="sidebar-section">Progress</div>

        <a href="Analytics.aspx" class="sidebar-link">
            <i class="ti ti-chart-bar"></i>
            Analytics
        </a>

        <a href="Certificates.aspx" class="sidebar-link active">
            <i class="ti ti-certificate"></i>
            Certificates
        </a>

        <a href="Achievements.aspx" class="sidebar-link">
            <i class="ti ti-star"></i>
            Achievements
        </a>

        <a href="viewFeedback.aspx" class="sidebar-link">
            <i class="ti ti-message-star"></i>
            My Feedback
        </a>

        <div class="sidebar-section">Account</div>

        <a href="Profile.aspx" class="sidebar-link">
            <i class="ti ti-user"></i>
            Profile
        </a>

        <asp:LinkButton
            ID="lbLogout"
            runat="server"
            CssClass="sidebar-link"
            CausesValidation="false"
            OnClientClick="return showLogoutConfirm(this);"
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
                    View and download certificates for completed courses.
                </p>

            </div>

            <div
                class="metrics"
                style="grid-template-columns:repeat(2,1fr);
                       max-width:400px;
                       margin-bottom:28px">

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

                    <div
                        class="metric-val"
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

            <div class="certificate-grid">

                <asp:Repeater
                    ID="rptCerts"
                    runat="server"
                    OnItemCommand="rptCerts_ItemCommand">

                    <ItemTemplate>

                        <div class="cert-card">

                            <div class="cert-header">

                                <i class="ti ti-certificate"></i>

                                <div class="cert-check">
                                    <i class="ti ti-check"></i>
                                </div>

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

                                        <i class="ti ti-calendar"></i>

                                        Issued:
                                        <%# Eval("IssuedDate") %>

                                    </span>

                                    <span>

                                        <i class="ti ti-id"></i>

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

                                    <i class="ti ti-download"></i>
                                    Download PDF

                                </asp:LinkButton>

                                <asp:LinkButton
                                    ID="btnView"
                                    runat="server"
                                    CssClass="btn-sm secondary"
                                    CausesValidation="false"
                                    CommandName="View"
                                    CommandArgument='<%# Eval("CertificateID") %>'>

                                    <i class="ti ti-eye"></i>
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

                <div class="certificate-empty">

                    <p class="text-muted">
                        No certificates earned yet.
                    </p>

                    <p class="text-muted text-small">
                        Complete a course to receive its certificate.
                    </p>

                </div>

            </asp:Panel>

        </asp:Panel>

        <asp:Panel
            ID="pnlCertificatePreview"
            runat="server"
            Visible="false">

            <div class="certificate-preview-actions">

                <asp:LinkButton
                    ID="btnBackToCertificates"
                    runat="server"
                    CssClass="btn-sm secondary"
                    CausesValidation="false"
                    OnClick="btnBackToCertificates_Click">

                    <i class="ti ti-arrow-left"></i>
                    Back

                </asp:LinkButton>

                <button
                    type="button"
                    id="btnDownloadCertificatePdf"
                    class="btn-primary certificate-download-button"
                    onclick="downloadCertificatePdf();">

                    <i class="ti ti-download"></i>
                    Download PDF

                </button>

            </div>

            <div
                id="certificateArea"
                class="certificate-preview">

                <div class="certificate-logo">
                    CYBERSHIELD ACADEMY
                </div>

                <h1>
                    Certificate of Completion
                </h1>

                <p>
                    This certificate is proudly presented to
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

                    <span id="certificateIdValue">

                        <asp:Literal
                            ID="litPreviewId"
                            runat="server" />

                    </span>

                </p>

            </div>

        </asp:Panel>

    </main>

</div>

<script src="https://cdn.jsdelivr.net/npm/html2canvas@1.4.1/dist/html2canvas.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/jspdf@2.5.2/dist/jspdf.umd.min.js"></script>

<script>
    async function downloadCertificatePdf() {
        const certificate =
            document.getElementById("certificateArea");

        const button =
            document.getElementById("btnDownloadCertificatePdf");

        if (!certificate ||
            !window.html2canvas ||
            !window.jspdf) {

            alert("PDF tools could not be loaded.");
            return;
        }

        const originalText =
            button.innerHTML;

        button.disabled = true;

        button.innerHTML =
            '<i class="ti ti-loader-2"></i> Generating PDF...';

        certificate.classList.add("pdf-mode");

        try {
            const canvas =
                await html2canvas(certificate, {
                    scale: 3,
                    useCORS: true,
                    logging: false,
                    backgroundColor: "#071713"
                });

            const pdf =
                new window.jspdf.jsPDF({
                    orientation: "landscape",
                    unit: "mm",
                    format: "a4"
                });

            const pageWidth =
                pdf.internal.pageSize.getWidth();

            const pageHeight =
                pdf.internal.pageSize.getHeight();

            const margin = 8;

            const maxWidth =
                pageWidth - margin * 2;

            const maxHeight =
                pageHeight - margin * 2;

            const ratio =
                canvas.width / canvas.height;

            let width = maxWidth;
            let height = width / ratio;

            if (height > maxHeight) {
                height = maxHeight;
                width = height * ratio;
            }

            pdf.addImage(
                canvas.toDataURL("image/png"),
                "PNG",
                (pageWidth - width) / 2,
                (pageHeight - height) / 2,
                width,
                height
            );

            const id =
                document
                    .getElementById("certificateIdValue")
                    .textContent
                    .trim()
                    .replace(
                        /[^a-zA-Z0-9_-]/g,
                        "_"
                    );

            pdf.save(
                "CyberShield_Certificate_" +
                id +
                ".pdf"
            );
        }
        catch (error) {
            console.error(error);

            alert(
                "The certificate PDF could not be generated."
            );
        }
        finally {
            certificate.classList.remove("pdf-mode");

            button.disabled = false;
            button.innerHTML = originalText;
        }
    }
</script>

</asp:Content>