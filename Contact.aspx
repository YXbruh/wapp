<%@ Page Title="Contact – CyberShield Academy" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="CSA.Contact" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <section class="section" aria-labelledby="contactHeading">
        <div class="section-title" id="contactHeading">Contact &amp; Support</div>
        <div class="section-sub">Every request is handled inside the platform, so it reaches the right person and is logged</div>

        <div class="cards-row">

            <div class="card">
                <div class="card-header">
                    <i class="ti ti-user" aria-hidden="true"></i>&nbsp;I'm a student
                </div>
                <p class="text-muted">
                    Questions about course material, a quiz result or a lab that will not accept your
                    answer should go to the lecturer who wrote it. Open the course from
                    <strong>My Courses</strong>, leave feedback, and your lecturer's reply comes back to
                    you under <strong>View Feedback</strong> &mdash; no email needed.
                </p>
                <p class="text-muted mt-8">
                    For anything about your own account &mdash; name, email or password &mdash; use
                    <strong>Profile</strong> in your dashboard sidebar.
                </p>
                <div class="btn-row mt-16">
                    <a href="Student/Feedback.aspx" class="btn-sm">
                        <i class="ti ti-message-2" aria-hidden="true"></i>Leave Feedback
                    </a>
                    <a href="Student/Profile.aspx" class="btn-sm secondary">
                        <i class="ti ti-user-cog" aria-hidden="true"></i>My Profile
                    </a>
                </div>
            </div>

            <div class="card">
                <div class="card-header">
                    <i class="ti ti-school" aria-hidden="true"></i>&nbsp;I'm a lecturer
                </div>
                <p class="text-muted">
                    Student reviews and your replies both live on the <strong>Mentorship</strong> page.
                    If a chapter, quiz or lab you submitted is still sitting unpublished, it is waiting
                    in the administrator's content-review queue &mdash; drafts are never visible to
                    students until an admin approves them.
                </p>
                <div class="btn-row mt-16">
                    <a href="Lecturer/Mentorship.aspx" class="btn-sm">
                        <i class="ti ti-messages" aria-hidden="true"></i>Mentorship
                    </a>
                    <a href="Lecturer/ManageContent.aspx" class="btn-sm secondary">
                        <i class="ti ti-files" aria-hidden="true"></i>Manage Content
                    </a>
                </div>
            </div>

        </div>

        <div class="cards-row">

            <div class="card">
                <div class="card-header">
                    <i class="ti ti-lock" aria-hidden="true"></i>&nbsp;Account and access problems
                </div>
                <p class="text-muted">
                    If you cannot sign in, your account is locked, or you need a role changed, this is
                    an administrator task. Administrators manage every account from the admin dashboard
                    and can reset access, change a role, or reactivate a disabled account.
                </p>
                <p class="text-muted mt-8">
                    Do not create a second account to work around a sign-in problem &mdash; your
                    enrolments, quiz scores, lab submissions and certificates are all tied to the
                    original one and cannot be merged afterwards.
                </p>
                <div class="btn-row mt-16">
                    <a href="Login.aspx" class="btn-sm secondary">
                        <i class="ti ti-login" aria-hidden="true"></i>Sign In
                    </a>
                </div>
            </div>

            <div class="card">
                <div class="card-header">
                    <i class="ti ti-bell" aria-hidden="true"></i>&nbsp;Platform announcements
                </div>
                <p class="text-muted">
                    Maintenance windows, new course launches and policy changes are published as
                    announcements by the administration team and emailed to the affected users. Keep the
                    email address on your profile current so those notices actually reach you.
                </p>
                <p class="text-muted mt-8">
                    Outbound email is configured per deployment, so on a local or classroom install
                    notifications may be captured by a local mail catcher rather than delivered
                    externally.
                </p>
            </div>

        </div>

        <div class="card">
            <div class="card-header">
                <i class="ti ti-info-circle" aria-hidden="true"></i>&nbsp;About this deployment
            </div>
            <p class="text-muted">
                CyberShield Academy is an ASP.NET Web Forms application built as a Web Applications
                coursework project. It is not a commercial service and has no call centre or postal
                enquiries desk &mdash; the in-platform channels above are the supported way to reach a
                lecturer or an administrator.
            </p>
        </div>

    </section>

</asp:Content>
