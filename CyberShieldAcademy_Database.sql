-- ============================================================
-- 1. ROLES
-- ============================================================
CREATE TABLE Roles (
    RoleID   NVARCHAR(10) NOT NULL PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- ============================================================
-- 2. USERS
-- ============================================================
CREATE TABLE Users (
    UserID         NVARCHAR(10)  NOT NULL PRIMARY KEY,
    StudentID      NVARCHAR(20)  NULL,
    FullName       NVARCHAR(150) NOT NULL,
    Email          NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash   NVARCHAR(512) NOT NULL,
    RoleID         NVARCHAR(10)  NOT NULL,
    PhoneNumber    NVARCHAR(50)  NULL,
    Department     NVARCHAR(100) NULL,
    ProfilePicture NVARCHAR(500) NULL,
    TotalPoints    INT           NOT NULL DEFAULT 0,
    StreakDays     INT           NOT NULL DEFAULT 0,
    LastLoginDate  DATETIME      NULL,
    IsActive       BIT           NOT NULL DEFAULT 1,
    CreatedAt      DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),
    -- Optional text columns must be NULL when unset, never ''. StudentID carries a
    -- filtered unique index (below) and '' is a real value to that index, so a second
    -- user saved with a blank Student ID would collide with the first and could not be
    -- created or edited at all. These CHECKs stop an empty string ever being stored.
    CONSTRAINT CK_Users_StudentID_NotBlank  CHECK (StudentID   IS NULL OR LEN(StudentID)   > 0),
    CONSTRAINT CK_Users_Department_NotBlank CHECK (Department  IS NULL OR LEN(Department)  > 0),
    CONSTRAINT CK_Users_Phone_NotBlank      CHECK (PhoneNumber IS NULL OR LEN(PhoneNumber) > 0)
);
GO

SET QUOTED_IDENTIFIER ON;
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_StudentID')
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Users_StudentID ON Users(StudentID) WHERE StudentID IS NOT NULL;
GO

-- ============================================================
-- 3. COURSE CATEGORIES
-- ============================================================
CREATE TABLE CourseCategories (
    CategoryID   NVARCHAR(10)  NOT NULL PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description  NVARCHAR(500) NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- 4. COURSES
-- ============================================================
CREATE TABLE Courses (
    CourseID      NVARCHAR(10)  NOT NULL PRIMARY KEY,
    CourseName    NVARCHAR(200) NOT NULL,
    Description   NVARCHAR(2000) NULL,
    CategoryID    NVARCHAR(10)  NULL,
    LecturerID    NVARCHAR(10)  NOT NULL,
    Level         NVARCHAR(50)  NOT NULL DEFAULT 'Beginner',
    DurationHours INT           NOT NULL DEFAULT 0,
    ThumbnailPath NVARCHAR(500) NULL,
    IsPublished   BIT           NOT NULL DEFAULT 0,
    CreatedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Courses_Category   FOREIGN KEY (CategoryID)   REFERENCES CourseCategories(CategoryID),
    CONSTRAINT FK_Courses_Lecturer   FOREIGN KEY (LecturerID)   REFERENCES Users(UserID),
    -- A course cannot run for a negative number of hours. Previously unchecked, so a
    -- typo such as -5 was accepted straight into the table.
    CONSTRAINT CK_Courses_Duration CHECK (DurationHours >= 0 AND DurationHours <= 1000)
);
GO

-- ============================================================
-- 5. CHAPTERS
-- ============================================================
CREATE TABLE Chapters (
    ChapterID    NVARCHAR(10)  NOT NULL PRIMARY KEY,
    CourseID     NVARCHAR(10)  NOT NULL,
    ChapterTitle NVARCHAR(200) NOT NULL,
    Content      NVARCHAR(MAX) NULL,
    FilePath     NVARCHAR(500) NULL,
    SortOrder    INT           NOT NULL DEFAULT 0,
    IsPublished  BIT           NOT NULL DEFAULT 0,
    CreatedByID  NVARCHAR(10)  NOT NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Chapters_Course    FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Chapters_CreatedBy FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 6. ENROLLMENTS
-- ============================================================
CREATE TABLE Enrollments (
    EnrollmentID  NVARCHAR(10)  NOT NULL PRIMARY KEY,
    StudentID     NVARCHAR(10)  NOT NULL,
    CourseID      NVARCHAR(10)  NOT NULL,
    EnrolledAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    Progress      DECIMAL(5,2)  NOT NULL DEFAULT 0.00,
    Status        NVARCHAR(20)  NOT NULL DEFAULT 'Not Started',
    CompletedAt   DATETIME      NULL,
    CONSTRAINT FK_Enroll_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enroll_Course  FOREIGN KEY (CourseID)  REFERENCES Courses(CourseID),
    CONSTRAINT UQ_Enrollment UNIQUE (StudentID, CourseID)
);
GO

-- ============================================================
-- 7. CHAPTER PROGRESS
-- ============================================================
CREATE TABLE ChapterProgress (
    ProgressID   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentID    NVARCHAR(10)  NOT NULL,
    ChapterID    NVARCHAR(10)  NOT NULL,
    IsCompleted  BIT           NOT NULL DEFAULT 0,
    CompletedAt  DATETIME      NULL,
    CONSTRAINT FK_ChProg_Student FOREIGN KEY (StudentID)  REFERENCES Users(UserID),
    CONSTRAINT FK_ChProg_Chapter FOREIGN KEY (ChapterID)  REFERENCES Chapters(ChapterID),
    CONSTRAINT UQ_ChapterProgress UNIQUE (StudentID, ChapterID)
);
GO

-- ============================================================
-- 8. QUIZZES
-- ============================================================
CREATE TABLE Quizzes (
    QuizID       NVARCHAR(10)  NOT NULL PRIMARY KEY,
    CourseID     NVARCHAR(10)  NOT NULL,
    ChapterID    NVARCHAR(10)  NULL,
    Title        NVARCHAR(200) NOT NULL,
    Description  NVARCHAR(1000) NULL,
    -- Availability window and time limit. All optional: a quiz may simply be a
    -- downloadable file (see Attachments) with no schedule and no questions.
    StartDate       DATETIME   NULL,
    EndDate         DATETIME   NULL,
    DurationMinutes INT        NULL,
    -- Marks the quiz is out of. The Points of its questions must add up to this;
    -- the Quiz Editor blocks a batch that would overshoot it.
    TotalMarks      INT        NULL,
    MaxAttempts  INT           NOT NULL DEFAULT 3,
    PassMark     DECIMAL(5,2)  NOT NULL DEFAULT 50.00,
    CreatedByID  NVARCHAR(10)  NOT NULL,
    IsPublished  BIT           NOT NULL DEFAULT 0,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Quiz_Course    FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Quiz_Chapter   FOREIGN KEY (ChapterID)   REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_Quiz_CreatedBy FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 9. QUIZ QUESTIONS
-- ============================================================
CREATE TABLE QuizQuestions (
    QuestionID    NVARCHAR(10)  NOT NULL PRIMARY KEY,
    QuizID        NVARCHAR(10)  NOT NULL,
    QuestionText  NVARCHAR(2000) NOT NULL,
    QuestionType  NVARCHAR(20)  NOT NULL DEFAULT 'MCQ',
    -- MCQ options. Unused by Structure and TrueFalse questions.
    OptionA       NVARCHAR(500) NULL,
    OptionB       NVARCHAR(500) NULL,
    OptionC       NVARCHAR(500) NULL,
    OptionD       NVARCHAR(500) NULL,
    -- MCQ      : comma-separated option keys, one or more may be correct ("A" or "A,C")
    -- TrueFalse: 'True' or 'False'
    -- Structure: the expected answer text
    CorrectAnswer NVARCHAR(500) NOT NULL,
    MatchStrategy NVARCHAR(20)  NULL,
    Explanation   NVARCHAR(1000) NULL,
    Points        INT           NOT NULL DEFAULT 5,   -- marks for this question
    SortOrder     INT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_QQ_Quiz FOREIGN KEY (QuizID) REFERENCES Quizzes(QuizID),
    CONSTRAINT CK_QQ_Type CHECK (QuestionType IN ('MCQ', 'TrueFalse', 'Structure'))
);
GO

-- ============================================================
-- 10. QUIZ ATTEMPTS
-- ============================================================
CREATE TABLE QuizAttempts (
    AttemptID     NVARCHAR(10)  NOT NULL PRIMARY KEY,
    QuizID        NVARCHAR(10)  NOT NULL,
    StudentID     NVARCHAR(10)  NOT NULL,
    Score         DECIMAL(5,2)  NOT NULL DEFAULT 0.00,
    TotalMarks    INT           NOT NULL DEFAULT 0,
    ObtainedMarks INT           NOT NULL DEFAULT 0,
    IsPassed      BIT           NOT NULL DEFAULT 0,
    AttemptedAt   DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_QA_Quiz    FOREIGN KEY (QuizID)    REFERENCES Quizzes(QuizID),
    CONSTRAINT FK_QA_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 11. QUIZ ANSWERS
-- ============================================================
CREATE TABLE QuizAnswers (
    AnswerID      INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    AttemptID     NVARCHAR(10)  NOT NULL,
    QuestionID    NVARCHAR(10)  NOT NULL,
    StudentAnswer NVARCHAR(500) NULL,
    IsCorrect     BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_Ans_Attempt  FOREIGN KEY (AttemptID)  REFERENCES QuizAttempts(AttemptID),
    CONSTRAINT FK_Ans_Question FOREIGN KEY (QuestionID) REFERENCES QuizQuestions(QuestionID)
);
GO

-- ============================================================
-- 12. VIRTUAL LABS
-- ============================================================
CREATE TABLE VirtualLabs (
    LabID            NVARCHAR(10)  NOT NULL PRIMARY KEY,
    CourseID         NVARCHAR(10)  NOT NULL,
    ChapterID        NVARCHAR(10)  NULL,
    LabTitle         NVARCHAR(200) NOT NULL,
    Scenario         NVARCHAR(MAX) NULL,
    HintText         NVARCHAR(2000) NULL,
    ExpectedCommand  NVARCHAR(2000) NOT NULL,
    ValidationType   NVARCHAR(20)  NOT NULL DEFAULT 'ExactMatch',
    Difficulty       NVARCHAR(20)  NOT NULL DEFAULT 'Beginner',
    TimeLimitMinutes INT           NULL,
    PointsReward     INT           NOT NULL DEFAULT 10,
    SkillTag         NVARCHAR(100) NULL,
    IsPublished      BIT           NOT NULL DEFAULT 0,
    CreatedByID      NVARCHAR(10)  NOT NULL,
    CreatedAt        DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt        DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Lab_Course    FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Lab_Chapter   FOREIGN KEY (ChapterID)   REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_Lab_CreatedBy FOREIGN KEY (CreatedByID) REFERENCES Users(UserID),
    CONSTRAINT CK_Lab_ValType   CHECK (ValidationType IN ('ExactMatch', 'Contains', 'Regex')),
    CONSTRAINT CK_Lab_Difficulty CHECK (Difficulty IN ('Beginner', 'Intermediate', 'Advanced'))
);
GO

-- ============================================================
-- 13. LAB SUBMISSIONS
-- ============================================================
CREATE TABLE LabSubmissions (
    SubmissionID     NVARCHAR(10)  NOT NULL PRIMARY KEY,
    LabID            NVARCHAR(10)  NOT NULL,
    StudentID        NVARCHAR(10)  NOT NULL,
    CommandSubmitted NVARCHAR(2000) NOT NULL,
    IsCorrect        BIT           NOT NULL DEFAULT 0,
    Result           NVARCHAR(20)  NOT NULL DEFAULT 'Incomplete',
    Feedback         NVARCHAR(500) NULL,
    PointsEarned     INT           NOT NULL DEFAULT 0,
    SubmittedAt      DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_LS_Lab     FOREIGN KEY (LabID)      REFERENCES VirtualLabs(LabID),
    CONSTRAINT FK_LS_Student FOREIGN KEY (StudentID)  REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 14. ACHIEVEMENTS / BADGES
-- ============================================================
CREATE TABLE Achievements (
    AchievementID NVARCHAR(10)  NOT NULL PRIMARY KEY,
    BadgeName     NVARCHAR(100) NOT NULL UNIQUE,
    Description   NVARCHAR(500) NULL,
    IconPath      NVARCHAR(500) NULL,
    PointsGranted INT           NOT NULL DEFAULT 0,
    TriggerType   NVARCHAR(50)  NULL
);
GO

-- ============================================================
-- 15. USER ACHIEVEMENTS
-- ============================================================
CREATE TABLE UserAchievements (
    UserAchievementID NVARCHAR(10) NOT NULL PRIMARY KEY,
    UserID            NVARCHAR(10) NOT NULL,
    AchievementID     NVARCHAR(10) NOT NULL,
    EarnedAt          DATETIME     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UA_User        FOREIGN KEY (UserID)        REFERENCES Users(UserID),
    CONSTRAINT FK_UA_Achievement FOREIGN KEY (AchievementID) REFERENCES Achievements(AchievementID),
    CONSTRAINT UQ_UserAchievement UNIQUE (UserID, AchievementID)
);
GO

-- ============================================================
-- 16. FEEDBACK
-- ============================================================
CREATE TABLE Feedback (
    FeedbackID  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    StudentID   NVARCHAR(10)  NOT NULL,
    LecturerID  NVARCHAR(10)  NULL,   -- NULL = student-submitted review; set = lecturer authored/replied
    CourseID    NVARCHAR(10)  NULL,
    ChapterID   NVARCHAR(10)  NULL,
    QuizID      NVARCHAR(10)  NULL,
    LabID       NVARCHAR(10)  NULL,
    StarRating  TINYINT       NULL CHECK (StarRating BETWEEN 1 AND 5),
    Comment     NVARCHAR(2000) NULL,
    SubmittedAt DATETIME      NOT NULL DEFAULT GETDATE(),
    InstReadAt  DATETIME      NULL,   -- NULL = unread by lecturer; timestamp = when they read it
    RepText     NVARCHAR(2000) NULL,  -- lecturer's reply text
    RepAt       DATETIME      NULL,   -- when the lecturer replied
    CONSTRAINT FK_FB_Student  FOREIGN KEY (StudentID)  REFERENCES Users(UserID),
    CONSTRAINT FK_FB_Lecturer FOREIGN KEY (LecturerID) REFERENCES Users(UserID),
    CONSTRAINT FK_FB_Course   FOREIGN KEY (CourseID)   REFERENCES Courses(CourseID),
    CONSTRAINT FK_FB_Chapter  FOREIGN KEY (ChapterID)  REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_FB_Quiz     FOREIGN KEY (QuizID)     REFERENCES Quizzes(QuizID),
    CONSTRAINT FK_FB_Lab      FOREIGN KEY (LabID)      REFERENCES VirtualLabs(LabID)
);
GO

-- ============================================================
-- 16b. ATTACHMENTS (files, images, links attached to a chapter, lab, or quiz)
-- ============================================================
CREATE TABLE Attachments (
    AttachmentID   NVARCHAR(10)  NOT NULL PRIMARY KEY,
    ChapterID      NVARCHAR(10)  NULL,
    LabID          NVARCHAR(10)  NULL,
    QuizID         NVARCHAR(10)  NULL,
    QuestionID     NVARCHAR(10)  NULL,
    AttachmentType NVARCHAR(20)  NOT NULL,   -- 'File' | 'Image' | 'Link'
    Title          NVARCHAR(200) NOT NULL,
    FilePath       NVARCHAR(500) NULL,       -- site-relative path, set for File/Image
    LinkUrl        NVARCHAR(500) NULL,       -- external URL, set for Link
    FileSizeBytes  INT           NULL,
    UploadedByID   NVARCHAR(10)  NOT NULL,
    UploadedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Att_Chapter    FOREIGN KEY (ChapterID)    REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_Att_Lab        FOREIGN KEY (LabID)        REFERENCES VirtualLabs(LabID),
    CONSTRAINT FK_Att_Quiz       FOREIGN KEY (QuizID)       REFERENCES Quizzes(QuizID),
    CONSTRAINT FK_Att_Question   FOREIGN KEY (QuestionID)   REFERENCES QuizQuestions(QuestionID),
    CONSTRAINT FK_Att_UploadedBy FOREIGN KEY (UploadedByID) REFERENCES Users(UserID),
    CONSTRAINT CK_Att_Type CHECK (AttachmentType IN ('File', 'Image', 'Link')),
    -- A quiz may carry its own files (e.g. the quiz paper as a PDF) while each
    -- question inside it carries its own; every row belongs to exactly one parent.
    CONSTRAINT CK_Att_OneParent CHECK (
        (CASE WHEN ChapterID  IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN LabID      IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN QuizID     IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN QuestionID IS NOT NULL THEN 1 ELSE 0 END) = 1
    )
);
GO

-- ============================================================
-- 17. ACTIVITY LOG
-- ============================================================
CREATE TABLE ActivityLog (
    ActivityID   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    UserID       NVARCHAR(10)  NOT NULL,
    Description  NVARCHAR(500) NOT NULL,
    ActivityType NVARCHAR(50)  NULL,
    ReferenceID  INT           NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AL_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 18. ANNOUNCEMENTS
-- ============================================================
CREATE TABLE Announcements (
    AnnouncementID INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Title          NVARCHAR(200) NOT NULL,
    Body           NVARCHAR(MAX) NOT NULL,
    PublishedByID  NVARCHAR(10)  NOT NULL,
    IsActive       BIT           NOT NULL DEFAULT 1,
    PublishedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    Audience       NVARCHAR(50)  NOT NULL DEFAULT 'All',
    Priority       NVARCHAR(20)  NOT NULL DEFAULT 'Normal',
    CONSTRAINT FK_Ann_Publisher FOREIGN KEY (PublishedByID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 19. AUDIT LOG
-- ============================================================
CREATE TABLE AuditLog (
    AuditID       INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    PerformedByID NVARCHAR(10)  NOT NULL,
    Action        NVARCHAR(100) NOT NULL,
    TableAffected NVARCHAR(100) NULL,
    RecordID      NVARCHAR(10)  NULL,
    BeforeValue   NVARCHAR(MAX) NULL,
    AfterValue    NVARCHAR(MAX) NULL,
    IPAddress     NVARCHAR(50)  NULL,
    OccurredAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Audit_User FOREIGN KEY (PerformedByID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- 20. DATABASE BACKUPS
-- ============================================================
CREATE TABLE DatabaseBackups (
    BackupID    INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    BackupLabel NVARCHAR(200) NULL,
    BackupType  NVARCHAR(50)  NOT NULL DEFAULT 'Full',
    FilePath    NVARCHAR(500) NOT NULL,
    FileSize    BIGINT        NOT NULL DEFAULT 0,
    Status      NVARCHAR(20)  NOT NULL DEFAULT 'Success',
    CreatedByID NVARCHAR(10)  NOT NULL,
    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DB_User FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
);
GO



-- ============================================================
-- 21. PERFORMANCE INDEXES
--
-- Primary keys are indexed automatically, but foreign-key columns are not. Without
-- these every join and every "count the child rows" subquery on the dashboards
-- (enrolments per course, attempts per quiz, progress per student) is a full table
-- scan. Each index is guarded so this script stays re-runnable.
-- ============================================================

-- Enrolments: joined from almost every student and course screen.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enrollments_StudentID')
    CREATE NONCLUSTERED INDEX IX_Enrollments_StudentID ON Enrollments(StudentID) INCLUDE (CourseID, Progress, Status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Enrollments_CourseID')
    CREATE NONCLUSTERED INDEX IX_Enrollments_CourseID ON Enrollments(CourseID);
GO

-- Users: role filtering on the admin list, and the login lookup by email.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_RoleID')
    CREATE NONCLUSTERED INDEX IX_Users_RoleID ON Users(RoleID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email) INCLUDE (FullName, IsActive, PasswordHash);
GO

-- Courses: lecturer/category joins and the published-course catalogue.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Courses_LecturerID')
    CREATE NONCLUSTERED INDEX IX_Courses_LecturerID ON Courses(LecturerID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Courses_CategoryID')
    CREATE NONCLUSTERED INDEX IX_Courses_CategoryID ON Courses(CategoryID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Courses_IsPublished')
    CREATE NONCLUSTERED INDEX IX_Courses_IsPublished ON Courses(IsPublished) INCLUDE (CourseName, Level, CreatedAt);
GO

-- Chapters and chapter progress: drive the course progress calculation.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Chapters_CourseID')
    CREATE NONCLUSTERED INDEX IX_Chapters_CourseID ON Chapters(CourseID) INCLUDE (IsPublished, SortOrder);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChapterProgress_StudentID')
    CREATE NONCLUSTERED INDEX IX_ChapterProgress_StudentID ON ChapterProgress(StudentID) INCLUDE (ChapterID, IsCompleted);
GO

-- Quizzes: question loading, attempt counting and the max-attempts check.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Quizzes_CourseID')
    CREATE NONCLUSTERED INDEX IX_Quizzes_CourseID ON Quizzes(CourseID) INCLUDE (IsPublished);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QuizQuestions_QuizID')
    CREATE NONCLUSTERED INDEX IX_QuizQuestions_QuizID ON QuizQuestions(QuizID) INCLUDE (SortOrder, Points);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QuizAttempts_Student_Quiz')
    CREATE NONCLUSTERED INDEX IX_QuizAttempts_Student_Quiz ON QuizAttempts(StudentID, QuizID) INCLUDE (Score, IsPassed, AttemptedAt);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QuizAnswers_AttemptID')
    CREATE NONCLUSTERED INDEX IX_QuizAnswers_AttemptID ON QuizAnswers(AttemptID);
GO

-- Labs: the lab list and the "has this student already passed?" check.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_VirtualLabs_CourseID')
    CREATE NONCLUSTERED INDEX IX_VirtualLabs_CourseID ON VirtualLabs(CourseID) INCLUDE (IsPublished);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LabSubmissions_Student_Lab')
    CREATE NONCLUSTERED INDEX IX_LabSubmissions_Student_Lab ON LabSubmissions(StudentID, LabID) INCLUDE (IsCorrect, PointsEarned);
GO

-- Feedback, achievements, attachments and the audit trail.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Feedback_StudentID')
    CREATE NONCLUSTERED INDEX IX_Feedback_StudentID ON Feedback(StudentID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Feedback_CourseID')
    CREATE NONCLUSTERED INDEX IX_Feedback_CourseID ON Feedback(CourseID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserAchievements_UserID')
    CREATE NONCLUSTERED INDEX IX_UserAchievements_UserID ON UserAchievements(UserID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Attachments_Parents')
    CREATE NONCLUSTERED INDEX IX_Attachments_Parents ON Attachments(ChapterID, LabID, QuizID, QuestionID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_OccurredAt')
    CREATE NONCLUSTERED INDEX IX_AuditLog_OccurredAt ON AuditLog(OccurredAt DESC) INCLUDE (PerformedByID, Action);
GO


PRINT '=== CyberShield Academy schema created successfully! ===';
GO