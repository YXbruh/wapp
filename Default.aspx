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
            <div class="stat-num"><asp:Literal ID="litStudents" runat="server" Text="0" /></div>
            <div class="stat-label">Students Enrolled</div>
        </div>
        <div class="stat-card">
            <div class="stat-num"><asp:Literal ID="litCourses" runat="server" Text="0" /></div>
            <div class="stat-label">Active Courses</div>
        </div>
        <div class="stat-card">
            <div class="stat-num"><asp:Literal ID="litLabs" runat="server" Text="0" /></div>
            <div class="stat-label">Virtual Labs</div>
        </div>
        <div class="stat-card">
            <div class="stat-num"><asp:Literal ID="litSatisfaction" runat="server" Text="-" /></div>
            <div class="stat-label">Satisfaction Rate</div>
        </div>
    </div>

    <!-- FEATURES -->
    <section class="section" aria-labelledby="featuresHeading">
        <div class="section-title" id="featuresHeading">Why CyberShield Academy?</div>
        <div class="section-sub">A practical way to learn security - courses, labs and quizzes in one place</div>
        <div class="features-grid">
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-terminal-2" aria-hidden="true"></i></div>
                <h3>Hands-On Virtual Labs</h3>
                <p>Work through guided lab scenarios in an in-browser terminal sandbox, with your commands checked against each lab's goal.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-chart-line" aria-hidden="true"></i></div>
                <h3>Track Your Progress</h3>
                <p>Completed chapters, quizzes and labs roll up into a clear progress percentage for every course you take.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-certificate" aria-hidden="true"></i></div>
                <h3>Achievements &amp; Certificates</h3>
                <p>Earn badges as you reach milestones and a certificate once you complete a course.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-school" aria-hidden="true"></i></div>
                <h3>Learn From Lecturers</h3>
                <p>Every chapter, quiz and lab is written and reviewed by lecturers - and you can leave feedback and get a reply.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-books" aria-hidden="true"></i></div>
                <h3>Structured Courses</h3>
                <p>Each course is organised into chapters, quizzes and labs, with beginner, intermediate and advanced levels.</p>
            </div>
            <div class="feature-card">
                <div class="feat-icon"><i class="ti ti-device-desktop" aria-hidden="true"></i></div>
                <h3>Learn Anywhere</h3>
                <p>A responsive platform that works on desktop, tablet or mobile - nothing to install.</p>
            </div>
        </div>
    </section>

</asp:Content>
