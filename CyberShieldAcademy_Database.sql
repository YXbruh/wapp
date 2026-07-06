-- ============================================================
-- CyberShield Academy – SQL Server Database Schema
-- Project: CT050-3-2-WAPP Group 8
-- Description: Full schema for the CyberShield Academy
--              web-based cybersecurity learning platform.
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CyberShieldAcademy')
    CREATE DATABASE CyberShieldAcademy;
GO

USE CyberShieldAcademy;
GO

-- ============================================================
-- 1. ROLES
-- ============================================================
CREATE TABLE Roles (
    RoleID   INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE   -- 'Student', 'Lecturer', 'Admin'
);
GO

-- ============================================================
-- 2. USERS
-- ============================================================
CREATE TABLE Users (
    UserID          INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentID       NVARCHAR(20)   NULL UNIQUE,              -- e.g. TP075006 (students only)
    FullName        NVARCHAR(150)  NOT NULL,
    Email           NVARCHAR(255)  NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(512)  NOT NULL,
    RoleID          INT            NOT NULL,
    ProfilePicture  NVARCHAR(500)  NULL,                     -- relative path to uploaded image
    TotalPoints     INT            NOT NULL DEFAULT 0,        -- gamification score
    StreakDays      INT            NOT NULL DEFAULT 0,        -- consecutive-day streak
    LastLoginDate   DATETIME       NULL,
    IsActive        BIT            NOT NULL DEFAULT 1,
    CreatedAt       DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

-- ============================================================
-- 3. COURSE CATEGORIES
-- ============================================================
CREATE TABLE CourseCategories (
    CategoryID   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description  NVARCHAR(500) NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- 4. COURSES
-- ============================================================
CREATE TABLE Courses (
    CourseID      INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    CourseName    NVARCHAR(200)  NOT NULL,
    Description   NVARCHAR(2000) NULL,
    CategoryID    INT            NULL,
    InstructorID  INT            NOT NULL,              -- FK → Users (Lecturer)
    Level         NVARCHAR(50)   NOT NULL DEFAULT 'Beginner',  -- Beginner / Intermediate / Advanced
    DurationHours INT            NOT NULL DEFAULT 0,
    ThumbnailPath NVARCHAR(500)  NULL,
    IsPublished   BIT            NOT NULL DEFAULT 0,
    CreatedAt     DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt     DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Courses_Category    FOREIGN KEY (CategoryID)   REFERENCES CourseCategories(CategoryID),
    CONSTRAINT FK_Courses_Instructor  FOREIGN KEY (InstructorID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 5. CHAPTERS  (modules inside a course)
-- ============================================================
CREATE TABLE Chapters (
    ChapterID    INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    CourseID     INT            NOT NULL,
    ChapterTitle NVARCHAR(200)  NOT NULL,
    Content      NVARCHAR(MAX)  NULL,           -- lesson notes / HTML body
    FilePath     NVARCHAR(500)  NULL,           -- supplemental file (~/App_Data/Uploads/)
    SortOrder    INT            NOT NULL DEFAULT 0,
    IsPublished  BIT            NOT NULL DEFAULT 0,
    CreatedByID  INT            NOT NULL,       -- FK → Users (Lecturer)
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Chapters_Course     FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Chapters_CreatedBy  FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 6. ENROLLMENTS
-- ============================================================
CREATE TABLE Enrollments (
    EnrollmentID  INT      NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentID     INT      NOT NULL,
    CourseID      INT      NOT NULL,
    EnrolledAt    DATETIME NOT NULL DEFAULT GETDATE(),
    Progress      DECIMAL(5,2) NOT NULL DEFAULT 0.00,  -- percentage 0–100
    Status        NVARCHAR(20) NOT NULL DEFAULT 'Not Started',  -- Not Started / In Progress / Completed
    CompletedAt   DATETIME     NULL,
    CONSTRAINT FK_Enroll_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enroll_Course  FOREIGN KEY (CourseID)  REFERENCES Courses(CourseID),
    CONSTRAINT UQ_Enrollment UNIQUE (StudentID, CourseID)
);
GO

-- ============================================================
-- 7. CHAPTER PROGRESS  (per-student chapter completion)
-- ============================================================
CREATE TABLE ChapterProgress (
    ProgressID   INT      NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentID    INT      NOT NULL,
    ChapterID    INT      NOT NULL,
    IsCompleted  BIT      NOT NULL DEFAULT 0,
    CompletedAt  DATETIME NULL,
    CONSTRAINT FK_ChProg_Student  FOREIGN KEY (StudentID)  REFERENCES Users(UserID),
    CONSTRAINT FK_ChProg_Chapter  FOREIGN KEY (ChapterID)  REFERENCES Chapters(ChapterID),
    CONSTRAINT UQ_ChapterProgress UNIQUE (StudentID, ChapterID)
);
GO

-- ============================================================
-- 8. QUIZZES
-- ============================================================
CREATE TABLE Quizzes (
    QuizID       INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    CourseID     INT            NOT NULL,
    ChapterID    INT            NULL,           -- NULL = course-level quiz
    Title        NVARCHAR(200)  NOT NULL,
    Description  NVARCHAR(1000) NULL,
    MaxAttempts  INT            NOT NULL DEFAULT 3,
    PassMark     DECIMAL(5,2)   NOT NULL DEFAULT 50.00,  -- percentage
    CreatedByID  INT            NOT NULL,
    IsPublished  BIT            NOT NULL DEFAULT 0,
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Quiz_Course     FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Quiz_Chapter    FOREIGN KEY (ChapterID)   REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_Quiz_CreatedBy  FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 9. QUIZ QUESTIONS
-- ============================================================
CREATE TABLE QuizQuestions (
    QuestionID    INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    QuizID        INT            NOT NULL,
    QuestionText  NVARCHAR(2000) NOT NULL,
    QuestionType  NVARCHAR(20)   NOT NULL DEFAULT 'MCQ',  -- 'MCQ', 'TrueFalse', 'FillBlank'
    -- MCQ options (NULL for TrueFalse / FillBlank questions)
    OptionA       NVARCHAR(500)  NULL,
    OptionB       NVARCHAR(500)  NULL,
    OptionC       NVARCHAR(500)  NULL,
    OptionD       NVARCHAR(500)  NULL,
    -- Correct answer storage per type:
    --   MCQ       -> 'A', 'B', 'C', or 'D'
    --   TrueFalse -> 'True' or 'False'
    --   FillBlank -> exact expected string (compared case-insensitively)
    CorrectAnswer NVARCHAR(500)  NOT NULL,
    Explanation   NVARCHAR(1000) NULL,        -- shown after attempt
    SortOrder     INT            NOT NULL DEFAULT 0,
    CONSTRAINT FK_QQ_Quiz  FOREIGN KEY (QuizID) REFERENCES Quizzes(QuizID),
    CONSTRAINT CK_QQ_Type  CHECK (QuestionType IN ('MCQ', 'TrueFalse', 'FillBlank'))
);
GO

-- ============================================================
-- 10. QUIZ ATTEMPTS
-- ============================================================
CREATE TABLE QuizAttempts (
    AttemptID    INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    QuizID       INT          NOT NULL,
    StudentID    INT          NOT NULL,
    Score        DECIMAL(5,2) NOT NULL DEFAULT 0.00,   -- percentage
    TotalMarks   INT          NOT NULL DEFAULT 0,
    ObtainedMarks INT         NOT NULL DEFAULT 0,
    IsPassed     BIT          NOT NULL DEFAULT 0,
    AttemptedAt  DATETIME     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_QA_Quiz    FOREIGN KEY (QuizID)    REFERENCES Quizzes(QuizID),
    CONSTRAINT FK_QA_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 11. QUIZ ANSWERS  (per-question answer in an attempt)
-- ============================================================
CREATE TABLE QuizAnswers (
    AnswerID        INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    AttemptID       INT            NOT NULL,
    QuestionID      INT            NOT NULL,
    -- Stores the student's response regardless of question type:
    --   MCQ       -> 'A', 'B', 'C', or 'D'
    --   TrueFalse -> 'True' or 'False'
    --   FillBlank -> the text the student typed
    StudentAnswer   NVARCHAR(500)  NULL,      -- NULL if skipped
    IsCorrect       BIT            NOT NULL DEFAULT 0,
    CONSTRAINT FK_Ans_Attempt  FOREIGN KEY (AttemptID)  REFERENCES QuizAttempts(AttemptID),
    CONSTRAINT FK_Ans_Question FOREIGN KEY (QuestionID) REFERENCES QuizQuestions(QuestionID)
);
GO

-- ============================================================
-- 12. VIRTUAL LABS
-- ============================================================
CREATE TABLE VirtualLabs (
    LabID            INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    CourseID         INT            NOT NULL,
    ChapterID        INT            NULL,
    LabTitle         NVARCHAR(200)  NOT NULL,
    Scenario         NVARCHAR(MAX)  NULL,        -- task instructions shown to student
    HintText         NVARCHAR(2000) NULL,
    ExpectedCommand  NVARCHAR(2000) NOT NULL,    -- validation key (private, used for grading)
    ValidationType   NVARCHAR(20)   NOT NULL DEFAULT 'ExactMatch',  -- ExactMatch / Contains / Regex
    Difficulty       NVARCHAR(20)   NOT NULL DEFAULT 'Beginner',    -- Beginner / Intermediate / Advanced
    TimeLimitMinutes INT            NULL,        -- optional time limit; NULL = untimed
    PointsReward     INT            NOT NULL DEFAULT 10,
    SkillTag         NVARCHAR(100)  NULL,        -- e.g. 'Network Scanning'
    IsPublished      BIT            NOT NULL DEFAULT 0,  -- 'Active' toggle in the editor
    CreatedByID      INT            NOT NULL,
    CreatedAt        DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt        DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Lab_Course     FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Lab_Chapter    FOREIGN KEY (ChapterID)   REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_Lab_CreatedBy  FOREIGN KEY (CreatedByID) REFERENCES Users(UserID),
    CONSTRAINT CK_Lab_ValType    CHECK (ValidationType IN ('ExactMatch', 'Contains', 'Regex')),
    CONSTRAINT CK_Lab_Difficulty CHECK (Difficulty IN ('Beginner', 'Intermediate', 'Advanced'))
);
GO

-- ============================================================
-- 13. LAB SUBMISSIONS
-- ============================================================
CREATE TABLE LabSubmissions (
    SubmissionID    INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    LabID           INT            NOT NULL,
    StudentID       INT            NOT NULL,
    CommandSubmitted NVARCHAR(2000) NOT NULL,    -- actual terminal input from student
    IsCorrect       BIT            NOT NULL DEFAULT 0,
    Result          NVARCHAR(20)   NOT NULL DEFAULT 'Incomplete',  -- Passed / Incomplete
    Feedback        NVARCHAR(500)  NULL,         -- e.g. 'Correct – target port identified'
    PointsEarned    INT            NOT NULL DEFAULT 0,
    SubmittedAt     DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_LS_Lab     FOREIGN KEY (LabID)      REFERENCES VirtualLabs(LabID),
    CONSTRAINT FK_LS_Student FOREIGN KEY (StudentID)  REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 14. ACHIEVEMENTS / BADGES
-- ============================================================
CREATE TABLE Achievements (
    AchievementID   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    BadgeName       NVARCHAR(100) NOT NULL UNIQUE,   -- e.g. 'First Quiz Passed'
    Description     NVARCHAR(500) NULL,
    IconPath        NVARCHAR(500) NULL,
    PointsGranted   INT           NOT NULL DEFAULT 0,
    TriggerType     NVARCHAR(50)  NULL    -- 'QuizPass', 'LabComplete', 'Streak', etc.
);
GO

-- ============================================================
-- 15. USER ACHIEVEMENTS
-- ============================================================
CREATE TABLE UserAchievements (
    UserAchievementID INT      NOT NULL IDENTITY(1,1) PRIMARY KEY,
    UserID            INT      NOT NULL,
    AchievementID     INT      NOT NULL,
    EarnedAt          DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UA_User        FOREIGN KEY (UserID)        REFERENCES Users(UserID),
    CONSTRAINT FK_UA_Achievement FOREIGN KEY (AchievementID) REFERENCES Achievements(AchievementID),
    CONSTRAINT UQ_UserAchievement UNIQUE (UserID, AchievementID)
);
GO

-- ============================================================
-- 16. FEEDBACK
-- ============================================================
CREATE TABLE Feedback (
    FeedbackID   INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentID    INT            NOT NULL,
    CourseID     INT            NULL,
    ChapterID    INT            NULL,
    QuizID       INT            NULL,
    LabID        INT            NULL,
    StarRating   TINYINT        NOT NULL CHECK (StarRating BETWEEN 1 AND 5),
    Comment      NVARCHAR(2000) NULL,
    SubmittedAt  DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_FB_Student  FOREIGN KEY (StudentID) REFERENCES Users(UserID),
    CONSTRAINT FK_FB_Course   FOREIGN KEY (CourseID)  REFERENCES Courses(CourseID),
    CONSTRAINT FK_FB_Chapter  FOREIGN KEY (ChapterID) REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_FB_Quiz     FOREIGN KEY (QuizID)    REFERENCES Quizzes(QuizID),
    CONSTRAINT FK_FB_Lab      FOREIGN KEY (LabID)     REFERENCES VirtualLabs(LabID)
);
GO

-- ============================================================
-- 17. ACTIVITY LOG  (student recent-activity feed)
-- ============================================================
CREATE TABLE ActivityLog (
    ActivityID   INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    UserID       INT            NOT NULL,
    Description  NVARCHAR(500)  NOT NULL,   -- e.g. 'Completed Lab: Nmap Basics'
    ActivityType NVARCHAR(50)   NULL,       -- 'LabComplete', 'QuizAttempt', 'Enroll', etc.
    ReferenceID  INT            NULL,       -- FK-like pointer (LabID / QuizID / CourseID)
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AL_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 18. ANNOUNCEMENTS
-- ============================================================
CREATE TABLE Announcements (
    AnnouncementID INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Title          NVARCHAR(200)  NOT NULL,
    Body           NVARCHAR(MAX)  NOT NULL,
    PublishedByID  INT            NOT NULL,   -- Admin user
    IsActive       BIT            NOT NULL DEFAULT 1,
    PublishedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    ExpiresAt      DATETIME       NULL,
    CONSTRAINT FK_Ann_Publisher FOREIGN KEY (PublishedByID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 19. CONTENT FLAGS / REPORTS
-- ============================================================
CREATE TABLE ContentFlags (
    FlagID        INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    ReportedByID  INT            NOT NULL,
    ContentType   NVARCHAR(50)   NOT NULL,  -- 'Chapter', 'Quiz', 'Lab', 'Comment'
    ContentID     INT            NOT NULL,
    Reason        NVARCHAR(1000) NULL,
    Status        NVARCHAR(20)   NOT NULL DEFAULT 'Pending',  -- Pending / Dismissed / Removed / Escalated
    ReviewedByID  INT            NULL,
    ReviewedAt    DATETIME       NULL,
    FlaggedAt     DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CF_Reporter FOREIGN KEY (ReportedByID) REFERENCES Users(UserID),
    CONSTRAINT FK_CF_Reviewer FOREIGN KEY (ReviewedByID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 20. SYSTEM CONFIGURATION
-- ============================================================
CREATE TABLE SystemConfiguration (
    ConfigID     INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    ConfigKey    NVARCHAR(100)  NOT NULL UNIQUE,
    ConfigValue  NVARCHAR(1000) NOT NULL,
    Description  NVARCHAR(500)  NULL,
    UpdatedAt    DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- 21. ERROR LOGS
-- ============================================================
CREATE TABLE ErrorLogs (
    ErrorID     INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    ErrorType   NVARCHAR(100)  NULL,
    Message     NVARCHAR(MAX)  NULL,
    PageURL     NVARCHAR(500)  NULL,
    UserID      INT            NULL,
    UserAgent   NVARCHAR(500)  NULL,
    Severity    NVARCHAR(20)   NOT NULL DEFAULT 'Error',  -- Info / Warning / Error / Critical
    IsResolved  BIT            NOT NULL DEFAULT 0,
    OccurredAt  DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_EL_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 22. AUDIT LOG  (admin actions – append-only)
-- ============================================================
CREATE TABLE AuditLog (
    AuditID       INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    PerformedByID INT            NOT NULL,
    Action        NVARCHAR(100)  NOT NULL,   -- e.g. 'DELETE_USER', 'PUBLISH_COURSE'
    TableAffected NVARCHAR(100)  NULL,
    RecordID      INT            NULL,
    BeforeValue   NVARCHAR(MAX)  NULL,       -- JSON snapshot of old record
    AfterValue    NVARCHAR(MAX)  NULL,       -- JSON snapshot of new record
    IPAddress     NVARCHAR(50)   NULL,
    OccurredAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Audit_User FOREIGN KEY (PerformedByID) REFERENCES Users(UserID)
);
GO

INSERT INTO Roles (RoleName, Description) VALUES
    ('Student',  'Authenticated learner with access to courses, labs, and quizzes'),
    ('Lecturer', 'Content creator who can upload chapters and manage quizzes'),
    ('Admin',    'Super administrator with full system access');
GO

INSERT INTO SystemConfiguration (ConfigKey, ConfigValue, Description) VALUES
    ('MinPasswordLength',         '8',    'Minimum number of characters required for a password'),
    ('MaxFailedLoginAttempts',    '5',    'Account lockout threshold for failed login attempts'),
    ('SessionTimeoutMinutes',     '30',   'Idle session timeout in minutes'),
    ('AdminSessionTimeoutMinutes','15',   'Idle session timeout for admin accounts'),
    ('MaxAdminConcurrentSessions','2',    'Maximum concurrent sessions allowed per admin account'),
    ('DefaultPassMark',           '50',   'Default quiz pass percentage'),
    ('MaintenanceMode',           'false','Set to true to show maintenance page to all users');
GO

INSERT INTO Achievements (BadgeName, Description, PointsGranted, TriggerType) VALUES
    ('First Login',        'Logged into CyberShield Academy for the first time',   10,  'Login'),
    ('First Enrollment',   'Enrolled in your first course',                         20,  'Enroll'),
    ('First Quiz Passed',  'Passed a quiz for the first time',                      50,  'QuizPass'),
    ('First Lab Completed','Completed a virtual lab for the first time',            50,  'LabComplete'),
    ('Lab Master',         'Completed 10 or more virtual labs',                    200,  'LabComplete'),
    ('Quiz Ace',           'Scored 100% on any quiz',                              100,  'QuizPass'),
    ('Course Graduate',    'Completed an entire course',                           300,  'CourseComplete'),
    ('7-Day Streak',       'Logged in for 7 consecutive days',                     150,  'Streak'),
    ('30-Day Streak',      'Logged in for 30 consecutive days',                    500,  'Streak'),
    ('Network Scanner',    'Unlocked Network Scanning skill tag via a lab',        100,  'LabComplete');
GO