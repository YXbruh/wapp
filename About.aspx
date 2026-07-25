<%@ Page Title="About – CyberShield Academy" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="CSA.About" %>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <!-- INTRO -->
    <section class="section" aria-labelledby="aboutHeading">
        <div class="section-title" id="aboutHeading">About CyberShield Academy</div>
        <div class="section-sub">A hands-on cybersecurity learning platform &mdash; courses, quizzes and virtual labs in one place</div>

        <div class="card mb-16">
            <p class="text-muted">
                CyberShield Academy exists because security is a practical skill. Reading about a
                privilege-escalation flaw is not the same as finding one, and a multiple-choice question
                cannot tell you whether a learner can actually drive a terminal. So every course here
                pairs written material with something you have to do: a quiz that checks you understood
                it, and a lab where you type real commands and get judged on the result.
            </p>
            <p class="text-muted mt-8">
                The platform is built around three roles that keep each other honest. Lecturers author
                the material, administrators review it before students ever see it, and students work
                through it while their progress is tracked automatically. Nothing is published on
                someone's say-so alone.
            </p>
        </div>
    </section>

    <!-- HOW IT WORKS -->
    <section class="section" aria-labelledby="rolesHeading">
        <div class="section-title" id="rolesHeading">How the platform works</div>
        <div class="section-sub">Three roles, one content pipeline</div>

        <div class="features-grid">
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-school" aria-hidden="true"></i></div>
                <h3>Lecturers author</h3>
                <p>
                    Lecturers write chapters, build quizzes in the quiz editor, and design terminal labs
                    with an expected-command answer key. Everything starts life as an unpublished draft,
                    and the lab preview lets them run their own scenario exactly as a student would
                    before anyone else sees it.
                </p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-shield-check" aria-hidden="true"></i></div>
                <h3>Admins review and publish</h3>
                <p>
                    Draft chapters, quizzes and labs land in a single content-review queue. An
                    administrator reads each one, then publishes or sends it back. Admins also manage
                    user accounts and course categories, post announcements, and take database backups.
                </p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-user-check" aria-hidden="true"></i></div>
                <h3>Students learn and are measured</h3>
                <p>
                    Students enrol in published courses and work through the chapters, quizzes and labs.
                    Completion of each rolls up into one honest progress percentage per course, which in
                    turn drives achievements and end-of-course certificates.
                </p>
            </div>
        </div>
    </section>

    <!-- WHAT'S IN A COURSE -->
    <section class="section" aria-labelledby="courseHeading">
        <div class="section-title" id="courseHeading">What a course contains</div>
        <div class="section-sub">Every course is built from the same three components</div>

        <div class="features-grid">
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-book" aria-hidden="true"></i></div>
                <h3>Chapters</h3>
                <p>
                    The written material, ordered into a sequence you work through. Lecturers can attach
                    supporting articles, images, documents and media links to any chapter.
                </p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-list-check" aria-hidden="true"></i></div>
                <h3>Quizzes</h3>
                <p>
                    Timed, auto-marked question sets with a configurable pass mark. You can reattempt a
                    quiz; your best score is the one that counts toward the course.
                </p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-terminal-2" aria-hidden="true"></i></div>
                <h3>Virtual labs</h3>
                <p>
                    An in-browser terminal sandbox where you solve a scenario by typing real commands.
                    Your submission is matched against the lab's objective, so a lab is passed by doing
                    it, not by claiming you did.
                </p>
            </div>
        </div>
    </section>

    <!-- FEEDBACK LOOP -->
    <section class="section" aria-labelledby="feedbackHeading">
        <div class="section-title" id="feedbackHeading">Teaching is a conversation</div>
        <div class="section-sub">Students are not left talking into the void</div>

        <div class="cards-row">
            <div class="card">
                <div class="card-header">Feedback that gets answered</div>
                <p class="text-muted">
                    Students rate and review a course from their dashboard. Those reviews go straight to
                    the lecturer's mentorship page, where they can reply directly &mdash; and the reply
                    comes back to the student. Lecturers can also start the conversation themselves when
                    they notice someone struggling.
                </p>
            </div>
            <div class="card">
                <div class="card-header">Analytics for both sides</div>
                <p class="text-muted">
                    Students see their own quiz scores and lab completion over time. Lecturers see the
                    same data across the whole class &mdash; average scores, per-quiz breakdowns, pass
                    rates and per-student detail &mdash; so weak spots in the material become visible
                    rather than anecdotal.
                </p>
            </div>
        </div>
    </section>

    <!-- CTA -->
    <section class="section" aria-labelledby="ctaHeading">
        <div class="section-title" id="ctaHeading">Start learning</div>
        <div class="section-sub">Browse the catalogue, or create an account and enrol</div>
        <div class="btn-row">
            <a href="Register.aspx" class="btn-primary">
                <i class="ti ti-rocket" aria-hidden="true"></i>Create an Account
            </a>
            <a href="Courses.aspx" class="btn-outline">
                <i class="ti ti-books" aria-hidden="true"></i>Browse Courses
            </a>
        </div>
    </section>

</asp:Content>
