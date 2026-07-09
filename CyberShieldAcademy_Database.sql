-- 1. ROLES
CREATE TABLE Roles (
    RoleID   NVARCHAR(9)  NOT NULL PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 2. USERS
CREATE TABLE Users (
    UserID          NVARCHAR(9)    NOT NULL PRIMARY KEY,
    StudentID       NVARCHAR(20)   NULL,                 -- e.g. TP074921 (students only; NULL for staff)
    FullName        NVARCHAR(150)  NOT NULL,
    Email           NVARCHAR(255)  NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(512)  NOT NULL,
    RoleID          NVARCHAR(9)    NOT NULL,
    ProfilePicture  NVARCHAR(500)  NULL,
    TotalPoints     INT            NOT NULL DEFAULT 0,
    StreakDays      INT            NOT NULL DEFAULT 0,
    LastLoginDate   DATETIME       NULL,
    IsActive        BIT            NOT NULL DEFAULT 1,
    CreatedAt       DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO
CREATE UNIQUE INDEX UQ_Users_StudentID ON Users(StudentID) WHERE StudentID IS NOT NULL;
GO

-- 3. COURSE CATEGORIES
CREATE TABLE CourseCategories (
    CategoryID   NVARCHAR(9)   NOT NULL PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description  NVARCHAR(500) NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- 4. COURSES
CREATE TABLE Courses (
    CourseID      NVARCHAR(9)    NOT NULL PRIMARY KEY,
    CourseName    NVARCHAR(200)  NOT NULL,
    Description   NVARCHAR(2000) NULL,
    CategoryID    NVARCHAR(9)    NULL,
    InstructorID  NVARCHAR(9)    NOT NULL,
    Level         NVARCHAR(50)   NOT NULL DEFAULT 'Beginner',
    DurationHours INT            NOT NULL DEFAULT 0,
    ThumbnailPath NVARCHAR(500)  NULL,
    IsPublished   BIT            NOT NULL DEFAULT 0,
    CreatedAt     DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt     DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Courses_Category   FOREIGN KEY (CategoryID)   REFERENCES CourseCategories(CategoryID),
    CONSTRAINT FK_Courses_Instructor FOREIGN KEY (InstructorID) REFERENCES Users(UserID)
);
GO

-- 5. CHAPTERS
CREATE TABLE Chapters (
    ChapterID    NVARCHAR(9)    NOT NULL PRIMARY KEY,
    CourseID     NVARCHAR(9)    NOT NULL,
    ChapterTitle NVARCHAR(200)  NOT NULL,
    Content      NVARCHAR(MAX)  NULL,
    FilePath     NVARCHAR(500)  NULL,
    SortOrder    INT            NOT NULL DEFAULT 0,
    IsPublished  BIT            NOT NULL DEFAULT 0,
    CreatedByID  NVARCHAR(9)    NOT NULL,
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Chapters_Course    FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Chapters_CreatedBy FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
);
GO

-- 6. ENROLLMENTS
CREATE TABLE Enrollments (
    EnrollmentID NVARCHAR(9)  NOT NULL PRIMARY KEY,
    StudentID    NVARCHAR(9)  NOT NULL,
    CourseID     NVARCHAR(9)  NOT NULL,
    EnrolledAt   DATETIME     NOT NULL DEFAULT GETDATE(),
    Progress     DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    Status       NVARCHAR(20) NOT NULL DEFAULT 'Not Started',
    CompletedAt  DATETIME     NULL,
    CONSTRAINT FK_Enroll_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enroll_Course  FOREIGN KEY (CourseID)  REFERENCES Courses(CourseID),
    CONSTRAINT UQ_Enrollment UNIQUE (StudentID, CourseID)
);
GO

-- 7. CHAPTER PROGRESS
CREATE TABLE ChapterProgress (
    ProgressID  NVARCHAR(9) NOT NULL PRIMARY KEY,
    StudentID   NVARCHAR(9) NOT NULL,
    ChapterID   NVARCHAR(9) NOT NULL,
    IsCompleted BIT         NOT NULL DEFAULT 0,
    CompletedAt DATETIME    NULL,
    CONSTRAINT FK_ChProg_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID),
    CONSTRAINT FK_ChProg_Chapter FOREIGN KEY (ChapterID) REFERENCES Chapters(ChapterID),
    CONSTRAINT UQ_ChapterProgress UNIQUE (StudentID, ChapterID)
);
GO

-- 8. QUIZZES
CREATE TABLE Quizzes (
    QuizID      NVARCHAR(9)    NOT NULL PRIMARY KEY,
    CourseID    NVARCHAR(9)    NOT NULL,
    ChapterID   NVARCHAR(9)    NULL,
    Title       NVARCHAR(200)  NOT NULL,
    Description NVARCHAR(1000) NULL,
    MaxAttempts INT            NOT NULL DEFAULT 3,
    PassMark    DECIMAL(5,2)   NOT NULL DEFAULT 50.00,
    CreatedByID NVARCHAR(9)    NOT NULL,
    IsPublished BIT            NOT NULL DEFAULT 0,
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Quiz_Course    FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Quiz_Chapter   FOREIGN KEY (ChapterID)   REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_Quiz_CreatedBy FOREIGN KEY (CreatedByID) REFERENCES Users(UserID)
);
GO

-- 9. QUIZ QUESTIONS
CREATE TABLE QuizQuestions (
    QuestionID    NVARCHAR(9)    NOT NULL PRIMARY KEY,
    QuizID        NVARCHAR(9)    NOT NULL,
    QuestionText  NVARCHAR(2000) NOT NULL,
    QuestionType  NVARCHAR(20)   NOT NULL DEFAULT 'MCQ',
    OptionA       NVARCHAR(500)  NULL,
    OptionB       NVARCHAR(500)  NULL,
    OptionC       NVARCHAR(500)  NULL,
    OptionD       NVARCHAR(500)  NULL,
    CorrectAnswer NVARCHAR(500)  NOT NULL,
    Explanation   NVARCHAR(1000) NULL,
    SortOrder     INT            NOT NULL DEFAULT 0,
    CONSTRAINT FK_QQ_Quiz FOREIGN KEY (QuizID) REFERENCES Quizzes(QuizID),
    CONSTRAINT CK_QQ_Type CHECK (QuestionType IN ('MCQ', 'TrueFalse', 'FillBlank'))
);
GO

-- 10. QUIZ ATTEMPTS
CREATE TABLE QuizAttempts (
    AttemptID     NVARCHAR(9)  NOT NULL PRIMARY KEY,
    QuizID        NVARCHAR(9)  NOT NULL,
    StudentID     NVARCHAR(9)  NOT NULL,
    Score         DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    TotalMarks    INT          NOT NULL DEFAULT 0,
    ObtainedMarks INT          NOT NULL DEFAULT 0,
    IsPassed      BIT          NOT NULL DEFAULT 0,
    AttemptedAt   DATETIME     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_QA_Quiz    FOREIGN KEY (QuizID)    REFERENCES Quizzes(QuizID),
    CONSTRAINT FK_QA_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID)
);
GO

-- 11. QUIZ ANSWERS
CREATE TABLE QuizAnswers (
    AnswerID      NVARCHAR(9)   NOT NULL PRIMARY KEY,
    AttemptID     NVARCHAR(9)   NOT NULL,
    QuestionID    NVARCHAR(9)   NOT NULL,
    StudentAnswer NVARCHAR(500) NULL,
    IsCorrect     BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_Ans_Attempt  FOREIGN KEY (AttemptID)  REFERENCES QuizAttempts(AttemptID),
    CONSTRAINT FK_Ans_Question FOREIGN KEY (QuestionID) REFERENCES QuizQuestions(QuestionID)
);
GO

-- 12. VIRTUAL LABS
CREATE TABLE VirtualLabs (
    LabID            NVARCHAR(9)    NOT NULL PRIMARY KEY,
    CourseID         NVARCHAR(9)    NOT NULL,
    ChapterID        NVARCHAR(9)    NULL,
    LabTitle         NVARCHAR(200)  NOT NULL,
    Scenario         NVARCHAR(MAX)  NULL,
    HintText         NVARCHAR(2000) NULL,
    ExpectedCommand  NVARCHAR(2000) NOT NULL,
    ValidationType   NVARCHAR(20)   NOT NULL DEFAULT 'ExactMatch',
    Difficulty       NVARCHAR(20)   NOT NULL DEFAULT 'Beginner',
    TimeLimitMinutes INT            NULL,
    PointsReward     INT            NOT NULL DEFAULT 10,
    SkillTag         NVARCHAR(100)  NULL,
    IsPublished      BIT            NOT NULL DEFAULT 0,
    CreatedByID      NVARCHAR(9)    NOT NULL,
    CreatedAt        DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt        DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Lab_Course     FOREIGN KEY (CourseID)    REFERENCES Courses(CourseID),
    CONSTRAINT FK_Lab_Chapter    FOREIGN KEY (ChapterID)   REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_Lab_CreatedBy  FOREIGN KEY (CreatedByID) REFERENCES Users(UserID),
    CONSTRAINT CK_Lab_ValType    CHECK (ValidationType IN ('ExactMatch', 'Contains', 'Regex')),
    CONSTRAINT CK_Lab_Difficulty CHECK (Difficulty IN ('Beginner', 'Intermediate', 'Advanced'))
);
GO

-- 13. LAB SUBMISSIONS
CREATE TABLE LabSubmissions (
    SubmissionID     NVARCHAR(9)    NOT NULL PRIMARY KEY,
    LabID            NVARCHAR(9)    NOT NULL,
    StudentID        NVARCHAR(9)    NOT NULL,
    CommandSubmitted NVARCHAR(2000) NOT NULL,
    IsCorrect        BIT            NOT NULL DEFAULT 0,
    Result           NVARCHAR(20)   NOT NULL DEFAULT 'Incomplete',
    Feedback         NVARCHAR(500)  NULL,
    PointsEarned     INT            NOT NULL DEFAULT 0,
    SubmittedAt      DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_LS_Lab     FOREIGN KEY (LabID)     REFERENCES VirtualLabs(LabID),
    CONSTRAINT FK_LS_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID)
);
GO

-- 14. ACHIEVEMENTS
CREATE TABLE Achievements (
    AchievementID NVARCHAR(9)   NOT NULL PRIMARY KEY,
    BadgeName     NVARCHAR(100) NOT NULL UNIQUE,
    Description   NVARCHAR(500) NULL,
    IconPath      NVARCHAR(500) NULL,
    PointsGranted INT           NOT NULL DEFAULT 0,
    TriggerType   NVARCHAR(50)  NULL
);
GO

-- 15. USER ACHIEVEMENTS
CREATE TABLE UserAchievements (
    UserAchievementID NVARCHAR(9) NOT NULL PRIMARY KEY,
    UserID            NVARCHAR(9) NOT NULL,
    AchievementID     NVARCHAR(9) NOT NULL,
    EarnedAt          DATETIME    NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UA_User        FOREIGN KEY (UserID)        REFERENCES Users(UserID),
    CONSTRAINT FK_UA_Achievement FOREIGN KEY (AchievementID) REFERENCES Achievements(AchievementID),
    CONSTRAINT UQ_UserAchievement UNIQUE (UserID, AchievementID)
);
GO

-- 16. FEEDBACK
CREATE TABLE Feedback (
    FeedbackID  NVARCHAR(9)    NOT NULL PRIMARY KEY,
    StudentID   NVARCHAR(9)    NOT NULL,
    CourseID    NVARCHAR(9)    NULL,
    ChapterID   NVARCHAR(9)    NULL,
    QuizID      NVARCHAR(9)    NULL,
    LabID       NVARCHAR(9)    NULL,
    StarRating  TINYINT        NOT NULL CHECK (StarRating BETWEEN 1 AND 5),
    Comment     NVARCHAR(2000) NULL,
    SubmittedAt DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_FB_Student FOREIGN KEY (StudentID) REFERENCES Users(UserID),
    CONSTRAINT FK_FB_Course  FOREIGN KEY (CourseID)  REFERENCES Courses(CourseID),
    CONSTRAINT FK_FB_Chapter FOREIGN KEY (ChapterID) REFERENCES Chapters(ChapterID),
    CONSTRAINT FK_FB_Quiz    FOREIGN KEY (QuizID)    REFERENCES Quizzes(QuizID),
    CONSTRAINT FK_FB_Lab     FOREIGN KEY (LabID)     REFERENCES VirtualLabs(LabID)
);
GO

-- 17. ACTIVITY LOG
CREATE TABLE ActivityLog (
    ActivityID   NVARCHAR(9)   NOT NULL PRIMARY KEY,
    UserID       NVARCHAR(9)   NOT NULL,
    Description  NVARCHAR(500) NOT NULL,
    ActivityType NVARCHAR(50)  NULL,
    ReferenceID  NVARCHAR(9)   NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AL_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- 18. ANNOUNCEMENTS
CREATE TABLE Announcements (
    AnnouncementID NVARCHAR(9)   NOT NULL PRIMARY KEY,
    Title          NVARCHAR(200) NOT NULL,
    Body           NVARCHAR(MAX) NOT NULL,
    PublishedByID  NVARCHAR(9)   NOT NULL,
    IsActive       BIT           NOT NULL DEFAULT 1,
    PublishedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    ExpiresAt      DATETIME      NULL,
    CONSTRAINT FK_Ann_Publisher FOREIGN KEY (PublishedByID) REFERENCES Users(UserID)
);
GO

-- 19. CONTENT FLAGS
CREATE TABLE ContentFlags (
    FlagID       NVARCHAR(9)    NOT NULL PRIMARY KEY,
    ReportedByID NVARCHAR(9)    NOT NULL,
    ContentType  NVARCHAR(50)   NOT NULL,
    ContentID    NVARCHAR(9)    NOT NULL,
    Reason       NVARCHAR(1000) NULL,
    Status       NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
    ReviewedByID NVARCHAR(9)    NULL,
    ReviewedAt   DATETIME       NULL,
    FlaggedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CF_Reporter FOREIGN KEY (ReportedByID) REFERENCES Users(UserID),
    CONSTRAINT FK_CF_Reviewer FOREIGN KEY (ReviewedByID) REFERENCES Users(UserID)
);
GO

-- 20. SYSTEM CONFIGURATION
CREATE TABLE SystemConfiguration (
    ConfigID    NVARCHAR(9)    NOT NULL PRIMARY KEY,
    ConfigKey   NVARCHAR(100)  NOT NULL UNIQUE,
    ConfigValue NVARCHAR(1000) NOT NULL,
    Description NVARCHAR(500)  NULL,
    UpdatedAt   DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

-- 21. ERROR LOGS
CREATE TABLE ErrorLogs (
    ErrorID    NVARCHAR(9)   NOT NULL PRIMARY KEY,
    ErrorType  NVARCHAR(100) NULL,
    Message    NVARCHAR(MAX) NULL,
    PageURL    NVARCHAR(500) NULL,
    UserID     NVARCHAR(9)   NULL,
    UserAgent  NVARCHAR(500) NULL,
    Severity   NVARCHAR(20)  NOT NULL DEFAULT 'Error',
    IsResolved BIT           NOT NULL DEFAULT 0,
    OccurredAt DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_EL_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

--22. AUDIT LOG
CREATE TABLE AuditLog (
    AuditID       NVARCHAR(9)   NOT NULL PRIMARY KEY,
    PerformedByID NVARCHAR(9)   NOT NULL,
    Action        NVARCHAR(100) NOT NULL,
    TableAffected NVARCHAR(100) NULL,
    RecordID      NVARCHAR(9)   NULL,
    BeforeValue   NVARCHAR(MAX) NULL,
    AfterValue    NVARCHAR(MAX) NULL,
    IPAddress     NVARCHAR(50)  NULL,
    OccurredAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Audit_User FOREIGN KEY (PerformedByID) REFERENCES Users(UserID)
);
GO

