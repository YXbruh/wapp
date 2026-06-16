<%@ Page Title="CyberShield Academy" Language="C#" 
    MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="Default.aspx.cs" Inherits="CSA._Default" %>


<asp:Content ID="cHead" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <!-- HERO -->
    <section class="hero" aria-label="Hero banner">
        <div class="hero-inner">
            <div class="hero-badge">
                <i class="ti ti-shield-check" aria-hidden="true"></i>
                Defend &middot; Learn &middot; Innovate
            </div>
            <h1>Master <span>Cybersecurity</span><br />from the Ground Up</h1>
            <p>CyberShield Academy delivers hands-on virtual labs, interactive challenges,
               and expert-led courses to build real-world security skills.</p>
            <div class="btn-row">
                <a href="Register.aspx" class="btn-primary">
                    <i class="ti ti-rocket" aria-hidden="true"></i>Get Started Free
                </a>
                <a href="Courses.aspx" class="btn-outline">
                    <i class="ti ti-player-play" aria-hidden="true"></i>Browse Courses
                </a>
            </div>
        </div>
    </section>

    <!-- STATS -->
    <div class="stats-row" role="region" aria-label="Platform statistics">
        <div class="stat-card">
            <div class="stat-num"><asp:Literal ID="litStudents" runat="server" Text="12,400+" /></div>
            <div class="stat-label">Students Enrolled</div>
        </div>
        <div class="stat-card">
            <div class="stat-num"><asp:Literal ID="litCourses" runat="server" Text="86" /></div>
            <div class="stat-label">Active Courses</div>
        </div>
        <div class="stat-card">
            <div class="stat-num"><asp:Literal ID="litLabs" runat="server" Text="320+" /></div>
            <div class="stat-label">Virtual Labs</div>
        </div>
        <div class="stat-card">
            <div class="stat-num">98%</div>
            <div class="stat-label">Satisfaction Rate</div>
        </div>
    </div>

    <!-- FEATURES -->
    <section class="section" aria-labelledby="featuresHeading">
        <div class="section-title" id="featuresHeading">Why CyberShield Academy?</div>
        <div class="section-sub">Everything you need to become a cybersecurity professional</div>
        <div class="features-grid">
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-terminal-2" aria-hidden="true"></i></div>
                <h3>Virtual Labs</h3>
                <p>Practice with real Kali Linux terminals, CTF challenges, and live network simulations in a safe sandbox environment.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-chart-line" aria-hidden="true"></i></div>
                <h3>Progress Tracking</h3>
                <p>Detailed analytics on quiz scores, lab completion rates, and skill progression mapped to industry certifications.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-certificate" aria-hidden="true"></i></div>
                <h3>Achievements</h3>
                <p>Earn badges and certificates as you complete modules, challenges, and assessment milestones.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-users" aria-hidden="true"></i></div>
                <h3>Community</h3>
                <p>Join discussion boards, participate in team CTF events, and collaborate with security professionals worldwide.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-refresh" aria-hidden="true"></i></div>
                <h3>Always Updated</h3>
                <p>Content is continuously updated to reflect the latest CVEs, attack vectors, and defensive techniques.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-device-desktop" aria-hidden="true"></i></div>
                <h3>Learn Anywhere</h3>
                <p>Fully responsive platform accessible from desktop, tablet, or mobile — no installation required.</p>
            </div>
        </div>
    </section>

</asp:Content>
