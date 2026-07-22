<%@ Page Title="Certificates – CyberShield Academy"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Certificates.aspx.cs"
    Inherits="CSA.Student.Student_Certificates" %>

<asp:Content ID="cMain"
    ContentPlaceHolderID="MainContent"
    runat="server">

<style>
    /* Certificate list cards */

    .certificate-grid {
        display:grid;
        grid-template-columns:repeat(auto-fill, minmax(300px, 1fr));
        gap:20px;
    }

    .cert-card {
        overflow:hidden;
        background:#0d2923;
        border:1px solid #287c68;
        border-radius:12px;
        box-shadow:0 4px 14px rgba(0, 0, 0, 0.18);
        transition:transform 0.2s ease,
                   border-color 0.2s ease,
                   box-shadow 0.2s ease;
    }

    .cert-card:hover {
        transform:translateY(-3px);
        border-color:#54b395;
        box-shadow:0 8px 20px rgba(0, 0, 0, 0.25);
    }

    .cert-header {
        position:relative;
        display:flex;
        align-items:center;
        justify-content:center;
        height:98px;
        color:#b7f4dd;
        background:
            linear-gradient(
                135deg,
                #246f5c,
                #459a7e
            );
        border-bottom:1px solid #297b68;
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
        color:#d4f8eb;
        font-size:14px;
        font-weight:800;
        line-height:1.35;
    }

    .cert-sub {
        margin-bottom:12px;
        color:#4aa68a;
        font-size:10px;
        font-weight:800;
        letter-spacing:0.8px;
        text-transform:uppercase;
    }

    .cert-meta {
        display:flex;
        flex-direction:column;
        gap:6px;
        color:#73b9a2;
        font-size:11px;
    }

    .cert-meta span {
        display:flex;
        align-items:center;
        gap:6px;
    }

    .cert-meta i {
        color:#439f83;
        font-size:12px;
    }

    .cert-footer {
        display:flex;
        gap:7px;
        padding:9px 11px;
        border-top:1px solid #245b4e;
    }

    .cert-footer .btn-sm {
        display:inline-flex;
        align-items:center;
        justify-content:center;
        gap:5px;
        min-height:30px;
        padding:6px 12px;
        color:#071d17;
        background:#4ca98d;
        border:1px solid #62b99e;
        border-radius:7px;
        font-size:11px;
        font-weight:800;
        text-decoration:none;
        cursor:pointer;
        transition:filter 0.2s ease,
                   transform 0.2s ease;
    }

    .cert-footer .btn-sm:hover {
        color:#071d17;
        filter:brightness(1.1);
        transform:translateY(-1px);
    }

    .cert-footer .btn-sm.secondary {
        color:#092019;
        background:#459d83;
        border-color:#459d83;
    }

    .cert-footer .btn-sm i {
        font-size:12px;
    }

    /* Certificate preview */

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
        margin:24px 0;
        font-size:38px;
    }

    .certificate-preview h2 {
        margin:18px 0;
        color:var(--accent2);
    }

    .certificate-logo {
        font-weight:800;
        letter-spacing:3px;
    }

    .certificate-preview-actions {
        display:flex;
        align-items:center;
        justify-content:space-between;
        max-width:850px;
        margin:0 auto 18px;
        gap:12px;
    }

    .certificate-download-button {
        border:none;
        cursor:pointer;
        font-family:inherit;
    }

    .certificate-download-button:disabled {
        opacity:0.65;
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
                    View and download certificates
                    for completed courses.
                </p>
            </div>

            <div class="metrics"
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

        const downloadButton =
            document.getElementById(
                "btnDownloadCertificatePdf"
            );

        if (!certificate) {
            alert("Certificate could not be found.");
            return;
        }

        if (!window.html2canvas ||
            !window.jspdf) {

            alert(
                "PDF tools could not be loaded. " +
                "Please check your internet connection."
            );

            return;
        }

        const originalButtonText =
            downloadButton.innerHTML;

        downloadButton.disabled = true;

        downloadButton.innerHTML =
            '<i class="ti ti-loader-2"></i> Generating PDF...';

        try {
            if (document.fonts &&
                document.fonts.ready) {

                await document.fonts.ready;
            }

            const canvas =
                await html2canvas(
                    certificate,
                    {
                        scale: 3,
                        useCORS: true,
                        logging: false,
                        backgroundColor: "#102a25"
                    }
                );

            const imageData =
                canvas.toDataURL(
                    "image/png",
                    1.0
                );

            const jsPDF =
                window.jspdf.jsPDF;

            const pdf =
                new jsPDF({
                    orientation: "landscape",
                    unit: "mm",
                    format: "a4",
                    compress: true
                });

            const pageWidth =
                pdf.internal.pageSize.getWidth();

            const pageHeight =
                pdf.internal.pageSize.getHeight();

            const margin = 8;

            const availableWidth =
                pageWidth - margin * 2;

            const availableHeight =
                pageHeight - margin * 2;

            const imageRatio =
                canvas.width / canvas.height;

            const pageRatio =
                availableWidth /
                availableHeight;

            let imageWidth;
            let imageHeight;

            if (imageRatio > pageRatio) {
                imageWidth = availableWidth;

                imageHeight =
                    availableWidth / imageRatio;
            }
            else {
                imageHeight = availableHeight;

                imageWidth =
                    availableHeight * imageRatio;
            }

            const imageX =
                (pageWidth - imageWidth) / 2;

            const imageY =
                (pageHeight - imageHeight) / 2;

            pdf.setFillColor(
                16,
                42,
                37
            );

            pdf.rect(
                0,
                0,
                pageWidth,
                pageHeight,
                "F"
            );

            pdf.addImage(
                imageData,
                "PNG",
                imageX,
                imageY,
                imageWidth,
                imageHeight,
                undefined,
                "FAST"
            );

            const certificateIdElement =
                document.getElementById(
                    "certificateIdValue"
                );

            let certificateId =
                "Certificate";

            if (certificateIdElement) {
                certificateId =
                    certificateIdElement
                        .textContent
                        .trim();
            }

            certificateId =
                certificateId.replace(
                    /[^a-zA-Z0-9_-]/g,
                    "_"
                );

            pdf.save(
                "CyberShield_Certificate_" +
                certificateId +
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
            downloadButton.disabled = false;

            downloadButton.innerHTML =
                originalButtonText;
        }
    }
</script>

</asp:Content>