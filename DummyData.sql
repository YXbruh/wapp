-- ============================================================
-- CyberShield Academy - DUMMY DATA (ALL 24 TABLES)
-- >>> Run this INSIDE your CyberShieldAcademy.mdf, AFTER the schema. <<<
--
-- All main-entity IDs are explicit codes (PREFIX+3letters+3digits).
-- Log/child tables (ChapterProgress, QuizAnswers, Feedback, ActivityLog,
-- Announcements, ContentFlags, ErrorLogs, AuditLog, DatabaseBackups,
-- SecurityAlerts) use INT IDENTITY, so no IDs are supplied for them.
--
-- Passwords are salted SHA-256 (salt$hash). Verify with HashHelper.Verify().
-- LOGIN (email / password):
--   admin@cybershield.edu       / Admin@123     (Admin)
--   farah.aziz@cybershield.edu  / Lecturer@123  (Lecturer)
--   daniel.wong@cybershield.edu / Lecturer@123  (Lecturer)
--   aiman.hakim@mail.com + other students / Student@123
--
-- Script is RE-RUNNABLE: it clears all 24 tables first (children before
-- parents), then re-inserts everything.
-- ============================================================

-- ---------- CLEAR (children first, parents last) ----------
DELETE FROM Attachments;      DELETE FROM QuizAnswers;
DELETE FROM LabSubmissions;
DELETE FROM ChapterProgress;  DELETE FROM Feedback;
DELETE FROM UserAchievements; DELETE FROM ContentFlags;
DELETE FROM SecurityAlerts;   DELETE FROM ActivityLog;
DELETE FROM Announcements;    DELETE FROM ErrorLogs;
DELETE FROM AuditLog;         DELETE FROM DatabaseBackups;
DELETE FROM QuizAttempts;     DELETE FROM QuizQuestions;
DELETE FROM Quizzes;          DELETE FROM VirtualLabs;
DELETE FROM Enrollments;      DELETE FROM Chapters;
DELETE FROM Courses;          DELETE FROM CourseCategories;
DELETE FROM Achievements;     DELETE FROM SystemConfiguration;
DELETE FROM Users;            DELETE FROM Roles;
GO

-- ROLES
INSERT INTO Roles (RoleID, RoleName) VALUES
('ROLIDQ053','Student'), ('ROLBNA400','Lecturer'), ('ROLLVD269','Admin');
GO

-- USERS
INSERT INTO Users (UserID, StudentID, FullName, Email, PasswordHash, RoleID, PhoneNumber, Department, TotalPoints, StreakDays, IsActive) VALUES
('USRAME107', NULL, N'System Administrator', 'admin@cybershield.edu', 'dfbca3ef4fca8790$d0696358080aea7baa203ec06ea5d7df2876e58b2c9cc5c319223afb2cac8a3f', 'ROLLVD269', '+60 3-8996 1000', N'IT Services', 0, 0, 1),
('USRDGK804', NULL, N'Dr. Farah Aziz', 'farah.aziz@cybershield.edu', 'd776381f9882b4d2$ae7bc910a08345fdcd98cde17f0c0a73922c78d380754484bb09c3b70b853a0b', 'ROLBNA400', '+60 3-8996 1234', N'Faculty of Computing – Cybersecurity', 0, 0, 1),
('USROWV824', NULL, N'Daniel Wong', 'daniel.wong@cybershield.edu', '299df10eaf5f22e5$54ebbd583704a35a9d1b69ee3fa4a8ba30580ce53652490c6d4862de4ae7b765', 'ROLBNA400', '+60 3-8996 5678', N'Faculty of Computing – Network Security', 0, 0, 1),
('USRJWY112', 'TP074921', N'Aiman Hakim', 'aiman.hakim@mail.com', 'b2dc38f2ec76c5ff$42f956e8dc53712526c5626a8bca011334f47c5afd0c3ac83eaa3384a03c558c', 'ROLIDQ053', '+60 12-345 6781', NULL, 320, 5, 1),
('USRGMP204', 'TP061358', N'Chong Wei Jian', 'weijian.chong@mail.com', 'e586529739f76a3c$30e3e365772c0423d7a3a079573405a86783c299986b8d0d620a1806a1270417', 'ROLIDQ053', '+60 12-345 6782', NULL, 150, 2, 1),
('USRJOY656', 'TP079842', N'Nurul Izzah', 'nurul.izzah@mail.com', '31086ddc2a56a755$8f26c07aed03e572054bb3d5e7ec349e393a4a66db6590f19196d07a7139755b', 'ROLIDQ053', '+60 13-876 5431', NULL, 480, 9, 1),
('USRRBX787', 'TP065117', N'Rajesh Kumar', 'rajesh.kumar@mail.com', '123c253e07d4925e$7aa04d669fa56fabea9cd002303a5c3713148d13e55bbc49ebefac9d4331bd5b', 'ROLIDQ053', '+60 16-223 4455', NULL, 90, 1, 1),
('USRKKC600', 'TP073506', N'Lee Mei Xin', 'meixin.lee@mail.com', '732f0ad7645888c5$c41b3602d1aa951efd0ec8b825a4d19f6dcf52e4082a33f56965806452c08e1d', 'ROLIDQ053', '+60 11-2233 4455', NULL, 210, 3, 1),
('USRFEI001', 'TP068293', N'Tan Jun Hao', 'junhao.tan@mail.com', '6bfab089e665465e$5d5d61f04ee3b74448c0f29b73f29076cece28814f743f778a8304ed92c15eee', 'ROLIDQ053', '+60 17-889 0011', NULL, 0, 0, 1);
GO

-- COURSE CATEGORIES
INSERT INTO CourseCategories (CategoryID, CategoryName, Description) VALUES
('CATCJA861', 'Fundamentals', 'Core cybersecurity concepts'),
('CATGJJ189', 'Network Security', 'Protecting networks'),
('CATMMC132', 'Ethical Hacking', 'Offensive security and pen testing'),
('CATVEA951', 'Web Security', 'Securing web applications');
GO

-- COURSES
INSERT INTO Courses (CourseID, CourseName, Description, CategoryID, InstructorID, Level, DurationHours, IsPublished) VALUES
('CRSOAN986', 'Introduction to Cybersecurity', 'Understand the CIA triad, threats, and basic defenses.', 'CATCJA861', 'USRDGK804', 'Beginner', 6, 1),
('CRSGJE757', 'Network Scanning 101', 'Discover live hosts, open ports, and services with Nmap.', 'CATGJJ189', 'USRDGK804', 'Intermediate', 8, 1),
('CRSIFV988', 'Linux Command Line Basics', 'Navigate the terminal, manage files, and use permissions.', 'CATCJA861', 'USROWV824', 'Beginner', 5, 1),
('CRSVTF530', 'Web Application Attacks', 'Explore SQL injection, XSS, and how to defend against them.', 'CATVEA951', 'USROWV824', 'Advanced', 10, 1),
('CRSAHG694', 'Penetration Testing Fundamentals', 'Learn the phases of a professional penetration test.', 'CATMMC132', 'USRDGK804', 'Advanced', 12, 0);
GO

-- CHAPTERS
INSERT INTO Chapters (ChapterID, CourseID, ChapterTitle, Content, SortOrder, IsPublished, CreatedByID) VALUES
('CHPYZY322', 'CRSOAN986', 'What is Cybersecurity?', 'An overview of the field.', 1, 1, 'USRDGK804'),
('CHPFFQ984', 'CRSOAN986', 'The CIA Triad', 'Confidentiality, Integrity, Availability.', 2, 1, 'USRDGK804'),
('CHPQUC697', 'CRSOAN986', 'Common Threats', 'Malware, phishing, social engineering.', 3, 1, 'USRDGK804'),
('CHPTME738', 'CRSGJE757', 'Introduction to Nmap', 'Installing and running your first scan.', 1, 1, 'USRDGK804'),
('CHPZKK971', 'CRSGJE757', 'Port Scanning Techniques', 'TCP SYN, connect, and UDP scans.', 2, 1, 'USRDGK804'),
('CHPDDX818', 'CRSIFV988', 'Navigating the Filesystem', 'cd, ls, pwd, directory structure.', 1, 1, 'USROWV824'),
('CHPZRJ510', 'CRSIFV988', 'File Permissions', 'Understanding chmod and chown.', 2, 1, 'USROWV824'),
('CHPZQN948', 'CRSVTF530', 'SQL Injection Basics', 'How untrusted input breaks queries.', 1, 1, 'USROWV824'),
('CHPVFG225', 'CRSVTF530', 'Cross-Site Scripting (XSS)', 'Reflected, stored, DOM-based XSS.', 2, 0, 'USROWV824');
GO

-- ENROLLMENTS
INSERT INTO Enrollments (EnrollmentID, StudentID, CourseID, Progress, Status, CompletedAt) VALUES
('ENRBGB678', 'USRJWY112', 'CRSOAN986', 100.00, 'Completed', GETDATE()),
('ENRESR189', 'USRJWY112', 'CRSGJE757', 60.00, 'In Progress', NULL),
('ENRFYK498', 'USRJWY112', 'CRSIFV988', 30.00, 'In Progress', NULL),
('ENRELN313', 'USRGMP204', 'CRSOAN986', 75.00, 'In Progress', NULL),
('ENRSAO403', 'USRGMP204', 'CRSIFV988', 100.00, 'Completed', GETDATE()),
('ENRQNB979', 'USRJOY656', 'CRSOAN986', 100.00, 'Completed', GETDATE()),
('ENRCGB721', 'USRJOY656', 'CRSGJE757', 100.00, 'Completed', GETDATE()),
('ENRKXV219', 'USRJOY656', 'CRSVTF530', 45.00, 'In Progress', NULL),
('ENROSC064', 'USRRBX787', 'CRSOAN986', 20.00, 'In Progress', NULL),
('ENRBYQ808', 'USRKKC600', 'CRSGJE757', 50.00, 'In Progress', NULL),
('ENRBWL359', 'USRKKC600', 'CRSIFV988', 90.00, 'In Progress', NULL),
('ENRGDN211', 'USRFEI001', 'CRSOAN986', 0.00, 'Not Started', NULL);
GO

-- CHAPTER PROGRESS  (mirrors Enrollments.Progress)
INSERT INTO ChapterProgress (StudentID, ChapterID, IsCompleted, CompletedAt) VALUES
('USRJWY112', 'CHPYZY322', 1, DATEADD(DAY,-20,GETDATE())),
('USRJWY112', 'CHPFFQ984', 1, DATEADD(DAY,-18,GETDATE())),
('USRJWY112', 'CHPQUC697', 1, DATEADD(DAY,-15,GETDATE())),
('USRJWY112', 'CHPTME738', 1, DATEADD(DAY,-9,GETDATE())),
('USRJWY112', 'CHPZKK971', 0, NULL),
('USRJWY112', 'CHPDDX818', 0, NULL),
('USRGMP204', 'CHPYZY322', 1, DATEADD(DAY,-12,GETDATE())),
('USRGMP204', 'CHPFFQ984', 1, DATEADD(DAY,-10,GETDATE())),
('USRGMP204', 'CHPQUC697', 0, NULL),
('USRGMP204', 'CHPDDX818', 1, DATEADD(DAY,-7,GETDATE())),
('USRGMP204', 'CHPZRJ510', 1, DATEADD(DAY,-5,GETDATE())),
('USRJOY656', 'CHPYZY322', 1, DATEADD(DAY,-25,GETDATE())),
('USRJOY656', 'CHPFFQ984', 1, DATEADD(DAY,-24,GETDATE())),
('USRJOY656', 'CHPQUC697', 1, DATEADD(DAY,-22,GETDATE())),
('USRJOY656', 'CHPTME738', 1, DATEADD(DAY,-14,GETDATE())),
('USRJOY656', 'CHPZKK971', 1, DATEADD(DAY,-11,GETDATE())),
('USRJOY656', 'CHPZQN948', 1, DATEADD(DAY,-3,GETDATE())),
('USRRBX787', 'CHPYZY322', 0, NULL),
('USRKKC600', 'CHPTME738', 1, DATEADD(DAY,-6,GETDATE())),
('USRKKC600', 'CHPDDX818', 1, DATEADD(DAY,-8,GETDATE())),
('USRKKC600', 'CHPZRJ510', 0, NULL);
GO


-- QUIZZES
-- StartDate/EndDate/DurationMinutes are optional; a quiz may run without a window
-- or time limit. The last two rows show a quiz with no questions at all, and a
-- file-only quiz whose content is an uploaded worksheet (see Attachments).
-- TotalMarks equals the sum of Points across each quiz's questions (0 when it has none).
INSERT INTO Quizzes (QuizID, CourseID, ChapterID, Title, Description, StartDate, EndDate, DurationMinutes, TotalMarks, MaxAttempts, PassMark, CreatedByID, IsPublished) VALUES
('QUZGKD934', 'CRSOAN986', 'CHPFFQ984', 'CIA Triad Quiz', 'Test your CIA triad knowledge.', DATEADD(DAY,-7,GETDATE()), DATEADD(DAY,14,GETDATE()), 30, 20, 3, 50.00, 'USRDGK804', 1),
('QUZPXK955', 'CRSOAN986', 'CHPQUC697', 'Threats Quiz', 'Identify common threats.', DATEADD(DAY,-3,GETDATE()), DATEADD(DAY,21,GETDATE()), 45, 20, 3, 50.00, 'USRDGK804', 1),
('QUZNAL107', 'CRSGJE757', 'CHPTME738', 'Nmap Basics Quiz', 'Basic Nmap concepts.', NULL, NULL, NULL, 30, 3, 60.00, 'USRDGK804', 1),
('QUZEMS535', 'CRSIFV988', 'CHPZRJ510', 'File Permissions Quiz', 'Linux permissions knowledge.', DATEADD(DAY,-1,GETDATE()), DATEADD(DAY,28,GETDATE()), 60, 10, 3, 50.00, 'USROWV824', 1),
-- No questions yet: a quiz is valid while it is still being written.
('QUZEMP001', 'CRSOAN986', NULL, 'Week 5 Revision Quiz', 'Questions will be added before release.', NULL, NULL, NULL, 0, 3, 50.00, 'USRDGK804', 0),
-- File-only: the assessment itself is the attached worksheet, so it has no questions.
('QUZDOC001', 'CRSGJE757', NULL, 'Network Scanning Worksheet', 'Download the worksheet, complete it offline and submit in class.', DATEADD(DAY,-2,GETDATE()), DATEADD(DAY,12,GETDATE()), 90, 0, 1, 50.00, 'USRDGK804', 1);
GO

-- QUIZ QUESTIONS
-- Columns Points and MatchStrategy are used by the Quiz Editor page.
-- MatchStrategy applies only to Structure questions; NULL otherwise.
-- MCQ CorrectAnswer holds comma-separated option keys ('B' or 'A,C' when several are correct).
INSERT INTO QuizQuestions (QuestionID, QuizID, QuestionText, QuestionType, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, MatchStrategy, Explanation, Points, SortOrder) VALUES
('QSNBFI372', 'QUZGKD934', 'What does the "C" in the CIA triad stand for?', 'MCQ', 'Control', 'Confidentiality', 'Compliance', 'Certification', 'B', NULL, 'C = Confidentiality.', 5, 1),
('QSNNEJ020', 'QUZGKD934', 'Integrity ensures data is not altered by unauthorized parties.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', NULL, 'Integrity = accuracy and trust.', 5, 2),
('QSNTOE491', 'QUZGKD934', 'The "A" in the CIA triad stands for ______.', 'Structure', NULL, NULL, NULL, NULL, 'Availability', 'ExactIgnoreCase', 'Availability = accessible when needed.', 10, 3),
('QSNVLM835', 'QUZPXK955', 'Which of the following is a social engineering attack?', 'MCQ', 'Phishing', 'Firewall', 'Encryption', 'Patching', 'A', NULL, 'Phishing tricks users.', 5, 1),
('QSNRZI876', 'QUZPXK955', 'Malware is short for malicious software.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', NULL, 'malicious + software.', 5, 2),
('QSNKJB107', 'QUZNAL107', 'Which Nmap flag performs a SYN scan?', 'MCQ', '-sT', '-sS', '-sU', '-sP', 'B', NULL, '-sS = stealthy SYN scan.', 5, 1),
('QSNGEC886', 'QUZNAL107', 'Nmap can discover open ports on a target.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', NULL, 'Core Nmap function.', 5, 2),
('QSNHGH414', 'QUZNAL107', 'The flag to scan all 65535 ports is -p-.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', NULL, '-p- scans every port.', 5, 3),
('QSNGZZ529', 'QUZEMS535', 'Which command changes file permissions in Linux?', 'MCQ', 'chown', 'chmod', 'chgrp', 'chdir', 'B', NULL, 'chmod = change mode.', 5, 1),
('QSNIJA345', 'QUZEMS535', 'The numeric permission 755 gives the owner full control.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', NULL, '7 = rwx for owner.', 5, 2),
-- Multi-correct MCQ: CorrectAnswer lists every correct option key.
('QSNMUL001', 'QUZPXK955', 'Which of the following are social engineering techniques? (select all that apply)', 'MCQ', 'Phishing', 'Port scanning', 'Pretexting', 'Baiting', 'A,C,D', NULL, 'Port scanning is a technical reconnaissance step, not social engineering.', 10, 3),
-- Structure question: question text plus the expected answer only.
('QSNSTR001', 'QUZNAL107', 'Explain, in one sentence, what a SYN scan does.', 'Structure', NULL, NULL, NULL, NULL, 'It sends SYN packets and infers port state from the response without completing the TCP handshake.', 'Contains', 'Look for the half-open handshake idea.', 15, 4);
GO

-- QUIZ ATTEMPTS
INSERT INTO QuizAttempts (AttemptID, QuizID, StudentID, Score, TotalMarks, ObtainedMarks, IsPassed) VALUES
('QATFNA203', 'QUZGKD934', 'USRJWY112', 100, 3, 3, 1),
('QATBAH255', 'QUZPXK955', 'USRJWY112', 50, 2, 1, 1),
('QATTRS833', 'QUZGKD934', 'USRGMP204', 66.67, 3, 2, 1),
('QATZDS608', 'QUZGKD934', 'USRJOY656', 100, 3, 3, 1),
('QATXQT815', 'QUZNAL107', 'USRJOY656', 66.67, 3, 2, 1),
('QATNVU858', 'QUZEMS535', 'USRGMP204', 100, 2, 2, 1),
('QATRSF013', 'QUZGKD934', 'USRRBX787', 33.33, 3, 1, 0);
GO

-- QUIZ ANSWERS  (per-question answers; counts match QuizAttempts.ObtainedMarks)
INSERT INTO QuizAnswers (AttemptID, QuestionID, StudentAnswer, IsCorrect) VALUES
-- QATFNA203: Aiman, CIA Triad Quiz, 3/3
('QATFNA203', 'QSNBFI372', 'B', 1),
('QATFNA203', 'QSNNEJ020', 'True', 1),
('QATFNA203', 'QSNTOE491', 'Availability', 1),
-- QATBAH255: Aiman, Threats Quiz, 1/2
('QATBAH255', 'QSNVLM835', 'A', 1),
('QATBAH255', 'QSNRZI876', 'False', 0),
-- QATTRS833: Chong Wei Jian, CIA Triad Quiz, 2/3
('QATTRS833', 'QSNBFI372', 'B', 1),
('QATTRS833', 'QSNNEJ020', 'True', 1),
('QATTRS833', 'QSNTOE491', 'Integrity', 0),
-- QATZDS608: Nurul, CIA Triad Quiz, 3/3
('QATZDS608', 'QSNBFI372', 'B', 1),
('QATZDS608', 'QSNNEJ020', 'True', 1),
('QATZDS608', 'QSNTOE491', 'Availability', 1),
-- QATXQT815: Nurul, Nmap Basics Quiz, 2/3
('QATXQT815', 'QSNKJB107', 'B', 1),
('QATXQT815', 'QSNGEC886', 'True', 1),
('QATXQT815', 'QSNHGH414', 'False', 0),
-- QATNVU858: Chong Wei Jian, File Permissions Quiz, 2/2
('QATNVU858', 'QSNGZZ529', 'B', 1),
('QATNVU858', 'QSNIJA345', 'True', 1),
-- QATRSF013: Rajesh, CIA Triad Quiz, 1/3 (failed)
('QATRSF013', 'QSNBFI372', 'A', 0),
('QATRSF013', 'QSNNEJ020', 'True', 1),
('QATRSF013', 'QSNTOE491', 'Confidentiality', 0);
GO


-- VIRTUAL LABS
INSERT INTO VirtualLabs (LabID, CourseID, ChapterID, LabTitle, Scenario, HintText, ExpectedCommand, ValidationType, Difficulty, TimeLimitMinutes, PointsReward, SkillTag, IsPublished, CreatedByID) VALUES
('LABOUC522', 'CRSIFV988', 'CHPDDX818', 'List Directory Contents', 'List all files including hidden ones.', 'Try the -a flag.', 'ls -a', 'ExactMatch', 'Beginner', 10, 10, 'Linux Basics', 1, 'USROWV824'),
('LABUNO794', 'CRSIFV988', 'CHPZRJ510', 'Make a Script Executable', 'Give execute permission to deploy.sh.', 'chmod with a mode.', 'chmod +x deploy.sh', 'Contains', 'Beginner', 15, 15, 'Linux Permissions', 1, 'USROWV824'),
('LABPNN645', 'CRSGJE757', 'CHPTME738', 'Scan a Target Host', 'Run a basic Nmap scan on 10.0.0.5.', 'Just nmap plus the IP.', 'nmap 10.0.0.5', 'Contains', 'Intermediate', 20, 20, 'Network Scanning', 1, 'USRDGK804'),
('LABMYS892', 'CRSGJE757', 'CHPZKK971', 'Stealth SYN Scan', 'Perform a stealthy SYN scan on 192.168.1.1.', 'The -sS flag.', 'nmap -sS 192.168.1.1', 'ExactMatch', 'Advanced', 25, 30, 'Network Scanning', 1, 'USRDGK804'),
('LABOYV114', 'CRSVTF530', 'CHPZQN948', 'Find the Injection Point', 'Enter an SQLi payload that is always true.', 'A quote and OR 1=1.', '.* OR .*1.*=.*1', 'Regex', 'Advanced', 30, 40, 'Web Security', 0, 'USROWV824');
GO

-- LAB SUBMISSIONS
INSERT INTO LabSubmissions (SubmissionID, LabID, StudentID, CommandSubmitted, IsCorrect, Result, Feedback, PointsEarned) VALUES
('LSBCVO666', 'LABOUC522', 'USRJWY112', 'ls -a', 1, 'Passed', 'Correct - all files listed.', 10),
('LSBMAU755', 'LABOUC522', 'USRGMP204', 'ls', 0, 'Incomplete', 'Missing -a flag.', 0),
('LSBRBT202', 'LABUNO794', 'USRJWY112', 'chmod +x deploy.sh', 1, 'Passed', 'Execute permission granted.', 15),
('LSBSFT943', 'LABPNN645', 'USRJOY656', 'nmap 10.0.0.5', 1, 'Passed', 'Scan completed.', 20),
('LSBMRT660', 'LABMYS892', 'USRJOY656', 'nmap -sS 192.168.1.1', 1, 'Passed', 'Stealth scan done.', 30);
GO

-- ATTACHMENTS (articles, pictures, media links, and documents attached to a
-- chapter, lab, or quiz - each row belongs to exactly one of the three)
-- Every row belongs to exactly one parent: a chapter, a lab, a quiz, or a question.
-- Files are stored as site-relative paths and links as URLs; never as binary data.
INSERT INTO Attachments (AttachmentID, ChapterID, LabID, QuizID, QuestionID, AttachmentType, Title, FilePath, LinkUrl, FileSizeBytes, UploadedByID) VALUES
('ATTCIA001', 'CHPFFQ984', NULL, NULL, NULL, 'Link', 'CIA Triad Reference Article', NULL, 'https://example.com/articles/cia-triad-overview', NULL, 'USRDGK804'),
('ATTCIA002', 'CHPFFQ984', NULL, NULL, NULL, 'File', 'CIA-Triad-Cheatsheet.pdf', '~/Content/Uploads/Chapter/CIA-Triad-Cheatsheet.pdf', NULL, 245760, 'USRDGK804'),
('ATTLAB001', NULL, 'LABUNO794', NULL, NULL, 'Link', 'chmod Permissions Walkthrough Video', NULL, 'https://example.com/videos/chmod-permissions-walkthrough', NULL, 'USROWV824'),
('ATTQUZ001', NULL, NULL, 'QUZGKD934', NULL, 'File', 'CIA-Triad-Quiz-StudyGuide.docx', '~/Content/Uploads/Quiz/CIA-Triad-Quiz-StudyGuide.docx', NULL, 51200, 'USRDGK804'),
-- The whole content of the file-only quiz.
('ATTQUZ002', NULL, NULL, 'QUZDOC001', NULL, 'File', 'Network-Scanning-Worksheet.pdf', '~/Content/Uploads/Quiz/Network-Scanning-Worksheet.pdf', NULL, 184320, 'USRDGK804'),
-- Supporting image for a single question.
('ATTQSN001', NULL, NULL, NULL, 'QSNMUL001', 'Image', 'social-engineering-diagram.png', '~/Content/Uploads/Question/social-engineering-diagram.png', NULL, 96256, 'USRDGK804');
GO

-- ACHIEVEMENTS  (must come BEFORE UserAchievements - FK)
INSERT INTO Achievements (AchievementID, BadgeName, Description, PointsGranted, TriggerType) VALUES
('ACHPKZ082','First Login','Logged in for the first time',10,'Login'),
('ACHDDI815','First Enrollment','Enrolled in your first course',20,'Enroll'),
('ACHQJO002','First Quiz Passed','Passed a quiz for the first time',50,'QuizPass'),
('ACHRLI542','First Lab Completed','Completed a virtual lab for the first time',50,'LabComplete'),
('ACHUSG558','Lab Master','Completed 10 or more virtual labs',200,'LabComplete'),
('ACHSHZ147','Quiz Ace','Scored 100% on any quiz',100,'QuizPass'),
('ACHDMB675','Course Graduate','Completed an entire course',300,'CourseComplete'),
('ACHWIS554','7-Day Streak','Logged in 7 consecutive days',150,'Streak'),
('ACHVYM607','30-Day Streak','Logged in 30 consecutive days',500,'Streak'),
('ACHQZV236','Network Scanner','Unlocked the Network Scanning skill',100,'LabComplete');
GO

-- USER ACHIEVEMENTS
INSERT INTO UserAchievements (UserAchievementID, UserID, AchievementID) VALUES
('UACDGT350', 'USRJWY112', 'ACHPKZ082'),
('UACBGR662', 'USRJWY112', 'ACHDDI815'),
('UACNMM181', 'USRJWY112', 'ACHQJO002'),
('UACZYA489', 'USRJWY112', 'ACHRLI542'),
('UACLGF925', 'USRGMP204', 'ACHPKZ082'),
('UACDNY185', 'USRGMP204', 'ACHDDI815'),
('UACXSG840', 'USRGMP204', 'ACHRLI542'),
('UACAML313', 'USRJOY656', 'ACHPKZ082'),
('UACIVA781', 'USRJOY656', 'ACHDDI815'),
('UACYSX233', 'USRJOY656', 'ACHQJO002'),
('UACZPJ420', 'USRJOY656', 'ACHDMB675');
GO

-- FEEDBACK  (one target FK per row, the rest NULL)
-- Lecturer reply workflow columns:
--   InstReadAt: NULL = unread by lecturer; a timestamp = when they opened it.
--   RepText / RepAt: the lecturer's reply text and time (NULL = not replied yet).
INSERT INTO Feedback (StudentID, LecturerID, CourseID, ChapterID, QuizID, LabID, StarRating, Comment, SubmittedAt, InstReadAt, RepText, RepAt) VALUES
-- Unread, no reply yet
('USRJWY112', NULL, 'CRSOAN986', NULL, NULL, NULL, 5, 'Great introduction, the CIA triad explanation was very clear.', DATEADD(DAY,-6,GETDATE()), NULL, NULL, NULL),
-- Read, not yet replied
('USRJOY656', NULL, 'CRSGJE757', NULL, NULL, NULL, 4, 'Nmap chapters are good but I would like more practice targets.', DATEADD(DAY,-5,GETDATE()), DATEADD(DAY,-4,GETDATE()), NULL, NULL),
-- Read AND replied (by Daniel Wong, who owns the Linux Command Line Basics course/lab)
('USRGMP204', 'USROWV824', NULL, NULL, NULL, 'LABUNO794', 5, 'The chmod lab finally made file permissions click for me.', DATEADD(DAY,-5,GETDATE()), DATEADD(DAY,-4,GETDATE()), N'Awesome, glad it clicked! Try the executable-script lab next.', DATEADD(DAY,-4,GETDATE())),
-- Read AND replied (by Dr. Farah Aziz, who owns Introduction to Cybersecurity)
('USRJWY112', 'USRDGK804', NULL, NULL, 'QUZGKD934', NULL, 4, 'Fair questions, and the explanation after each answer helps a lot.', DATEADD(DAY,-4,GETDATE()), DATEADD(DAY,-3,GETDATE()), N'Thanks for the feedback Aiman - more explained quizzes coming soon.', DATEADD(DAY,-3,GETDATE())),
-- Unread (student complaint)
('USRKKC600', NULL, NULL, 'CHPDDX818', NULL, NULL, 3, 'A bit fast for beginners, please add more screenshots.', DATEADD(DAY,-2,GETDATE()), NULL, NULL, NULL),
-- Read, not yet replied
('USRRBX787', NULL, 'CRSOAN986', NULL, NULL, NULL, 4, 'Enjoying the course so far.', DATEADD(DAY,-1,GETDATE()), DATEADD(HOUR,-8,GETDATE()), NULL, NULL),
-- Lecturer-initiated message (no student rating/comment - Dr. Farah Aziz reaching out proactively
-- after Ryan failed the CIA Triad Quiz on his first attempt)
('USRRBX787', 'USRDGK804', 'CRSOAN986', NULL, NULL, NULL, NULL, NULL, DATEADD(DAY,-1,GETDATE()), NULL, N'Hi Ryan, I noticed your first CIA Triad Quiz attempt didn''t pass - revisit Chapter 2 and try again, happy to help if you have questions.', DATEADD(DAY,-1,GETDATE()));
GO


-- ACTIVITY LOG
INSERT INTO ActivityLog (UserID, Description, ActivityType, ReferenceID, CreatedAt) VALUES
('USRJWY112', 'Logged in', 'Login', NULL, DATEADD(DAY,-1,GETDATE())),
('USRJWY112', 'Enrolled in Introduction to Cybersecurity', 'Enroll', NULL, DATEADD(DAY,-21,GETDATE())),
('USRJWY112', 'Passed CIA Triad Quiz with 100%', 'QuizPass', NULL, DATEADD(DAY,-15,GETDATE())),
('USRJWY112', 'Completed lab: List Directory Contents', 'LabComplete', NULL, DATEADD(DAY,-10,GETDATE())),
('USRJOY656', 'Completed course: Network Scanning 101', 'CourseComplete', NULL, DATEADD(DAY,-11,GETDATE())),
('USRDGK804', 'Published course: Network Scanning 101', 'CoursePublish', NULL, DATEADD(DAY,-30,GETDATE())),
('USROWV824', 'Created lab: Find the Injection Point', 'LabCreate', NULL, DATEADD(DAY,-8,GETDATE())),
('USRGMP204', 'Logged in', 'Login', NULL, DATEADD(HOUR,-5,GETDATE())),
('USRRBX787', 'Attempted CIA Triad Quiz (failed)', 'QuizFail', NULL, DATEADD(DAY,-2,GETDATE())),
('USRAME107', 'Reviewed pending content flags', 'AdminAction', NULL, DATEADD(HOUR,-2,GETDATE()));
GO


-- ANNOUNCEMENTS
INSERT INTO Announcements (Title, Body, PublishedByID, IsActive, PublishedAt, ExpiresAt, Audience, Priority) VALUES
('Welcome to CyberShield Academy', 'The new semester is live. Browse the course catalogue and enrol to start earning points and badges.', 'USRAME107', 1, DATEADD(DAY,-14,GETDATE()), NULL, 'All', 'Normal'),
('Scheduled Maintenance This Saturday', 'The platform will be unavailable from 2:00 AM to 4:00 AM for database maintenance. Save your work before then.', 'USRAME107', 1, DATEADD(DAY,-2,GETDATE()), DATEADD(DAY,5,GETDATE()), 'All', 'High'),
('New Lab: Stealth SYN Scan', 'An advanced Nmap lab has been added to Network Scanning 101. Complete it to earn 30 points.', 'USRDGK804', 1, DATEADD(DAY,-1,GETDATE()), NULL, 'Students', 'Normal'),
('Grading Deadline Reminder', 'Please finish reviewing quiz attempts for your courses by Friday.', 'USRAME107', 0, DATEADD(DAY,-30,GETDATE()), DATEADD(DAY,-20,GETDATE()), 'Lecturers', 'Normal');
GO


-- CONTENT FLAGS
-- NOTE: ContentFlags.ContentID is INT in the schema, so it cannot hold the
-- NVARCHAR code IDs (e.g. 'CHPFFQ984'). Placeholder numbers are used here.
-- If you want real linkage, change ContentID to NVARCHAR(10) like AuditLog.RecordID.
INSERT INTO ContentFlags (ReportedByID, ContentType, ContentID, Reason, Status, ReviewedByID, ReviewedAt, FlaggedAt) VALUES
('USRGMP204', 'Chapter', 2, 'Possible typo in the CIA triad diagram.', 'Pending', NULL, NULL, DATEADD(DAY,-1,GETDATE())),
('USRRBX787', 'Quiz', 1, 'Question 3 wording is confusing.', 'Reviewed', 'USRAME107', DATEADD(HOUR,-20,GETDATE()), DATEADD(DAY,-3,GETDATE())),
('USRJOY656', 'Lab', 5, 'Regex validation rejects a valid payload.', 'Resolved', 'USRAME107', DATEADD(HOUR,-4,GETDATE()), DATEADD(DAY,-2,GETDATE()));
GO


-- SYSTEM CONFIGURATION
INSERT INTO SystemConfiguration (ConfigID, ConfigKey, ConfigValue, Description) VALUES
('CFGCVH922','MinPasswordLength','8','Minimum password length'),
('CFGNEJ988','MaxFailedLoginAttempts','5','Account lockout threshold'),
('CFGQXY570','SessionTimeoutMinutes','30','Idle session timeout in minutes'),
('CFGTLT620','DefaultPassMark','50','Default quiz pass percentage'),
('CFGYDM327','MaintenanceMode','false','Show maintenance page when true');
GO

-- ERROR LOGS
INSERT INTO ErrorLogs (ErrorType, Message, PageURL, UserID, UserAgent, Severity, IsResolved, OccurredAt) VALUES
('NullReferenceException', 'Object reference not set to an instance of an object.', '/Lecturer/QuizEditor.aspx', 'USRDGK804', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/126.0', 'Error', 1, DATEADD(DAY,-3,GETDATE())),
('SqlException', 'Timeout expired while connecting to the database.', '/Student/Dashboard.aspx', 'USRJWY112', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Edge/126.0', 'Critical', 1, DATEADD(DAY,-2,GETDATE())),
('HttpException', 'The file /Student/Report.aspx does not exist.', '/Student/Report.aspx', NULL, 'Mozilla/5.0 (Macintosh; Intel Mac OS X) Safari/17.4', 'Warning', 0, DATEADD(HOUR,-6,GETDATE())),
('FormatException', 'Input string was not in a correct format.', '/Lecturer/TerminalSandbox.aspx', 'USROWV824', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Firefox/128.0', 'Warning', 0, DATEADD(HOUR,-1,GETDATE()));
GO


-- AUDIT LOG  (admin actions, append-only)
INSERT INTO AuditLog (PerformedByID, Action, TableAffected, RecordID, BeforeValue, AfterValue, IPAddress, OccurredAt) VALUES
('USRAME107', 'PUBLISH_COURSE', 'Courses', 'CRSGJE757', '{"IsPublished":0}', '{"IsPublished":1}', '192.168.1.10', DATEADD(DAY,-7,GETDATE())),
('USRAME107', 'UPDATE_CONFIG', 'SystemConfiguration', 'CFGQXY570', '{"ConfigValue":"20"}', '{"ConfigValue":"30"}', '192.168.1.10', DATEADD(DAY,-5,GETDATE())),
('USRAME107', 'RESET_PASSWORD', 'Users', 'USRRBX787', NULL, NULL, '192.168.1.10', DATEADD(DAY,-2,GETDATE())),
('USRAME107', 'DELETE_CHAPTER', 'Chapters', 'CHPOLD001', '{"ChapterTitle":"Deprecated Intro"}', NULL, '192.168.1.10', DATEADD(DAY,-1,GETDATE()));
GO


-- DATABASE BACKUPS
INSERT INTO DatabaseBackups (BackupLabel, BackupType, FilePath, FileSize, Status, CreatedByID, CreatedAt) VALUES
('Weekly full backup',       'Full',         'C:\Backups\CSA_Full_20260705.bak', 52428800, 'Success', 'USRAME107', DATEADD(DAY,-11,GETDATE())),
('Weekly full backup',       'Full',         'C:\Backups\CSA_Full_20260712.bak', 55574528, 'Success', 'USRAME107', DATEADD(DAY,-4,GETDATE())),
('Pre-deployment snapshot',  'Differential', 'C:\Backups\CSA_Diff_20260714.bak',  8388608, 'Success', 'USRAME107', DATEADD(DAY,-2,GETDATE())),
('Manual backup',            'Full',         'C:\Backups\CSA_Full_20260715.bak',        0, 'Failed',  'USRAME107', DATEADD(DAY,-1,GETDATE()));
GO


-- SECURITY ALERTS
INSERT INTO SecurityAlerts (AlertType, Description, Severity, IPAddress, AlertStatus, AffectedUserID, ReviewedByID, ReviewedAt, DetectedAt) VALUES
('Multiple Failed Logins', '5 failed login attempts within 10 minutes.', 'Medium', '203.0.113.45', 'Resolved', 'USRRBX787', 'USRAME107', DATEADD(DAY,-2,GETDATE()), DATEADD(DAY,-3,GETDATE())),
('SQL Injection Attempt', 'Suspicious payload detected in the login form input.', 'High', '198.51.100.23', 'Open', 'USRFEI001', NULL, NULL, DATEADD(HOUR,-12,GETDATE())),
('Unusual Login Location', 'Login from a new country for this account.', 'Low', '192.0.2.88', 'Dismissed', 'USRJOY656', 'USRAME107', DATEADD(HOUR,-1,GETDATE()), DATEADD(DAY,-1,GETDATE()));
GO


-- ---------- VERIFY: row count for all 24 tables ----------
SELECT 'Roles' t, COUNT(*) n FROM Roles
UNION ALL SELECT 'Users',               COUNT(*) FROM Users
UNION ALL SELECT 'CourseCategories',    COUNT(*) FROM CourseCategories
UNION ALL SELECT 'Courses',             COUNT(*) FROM Courses
UNION ALL SELECT 'Chapters',            COUNT(*) FROM Chapters
UNION ALL SELECT 'Enrollments',         COUNT(*) FROM Enrollments
UNION ALL SELECT 'ChapterProgress',     COUNT(*) FROM ChapterProgress
UNION ALL SELECT 'Quizzes',             COUNT(*) FROM Quizzes
UNION ALL SELECT 'QuizQuestions',       COUNT(*) FROM QuizQuestions
UNION ALL SELECT 'QuizAttempts',        COUNT(*) FROM QuizAttempts
UNION ALL SELECT 'QuizAnswers',         COUNT(*) FROM QuizAnswers
UNION ALL SELECT 'VirtualLabs',         COUNT(*) FROM VirtualLabs
UNION ALL SELECT 'LabSubmissions',      COUNT(*) FROM LabSubmissions
UNION ALL SELECT 'Attachments',         COUNT(*) FROM Attachments
UNION ALL SELECT 'Achievements',        COUNT(*) FROM Achievements
UNION ALL SELECT 'UserAchievements',    COUNT(*) FROM UserAchievements
UNION ALL SELECT 'Feedback',            COUNT(*) FROM Feedback
UNION ALL SELECT 'ActivityLog',         COUNT(*) FROM ActivityLog
UNION ALL SELECT 'Announcements',       COUNT(*) FROM Announcements
UNION ALL SELECT 'ContentFlags',        COUNT(*) FROM ContentFlags
UNION ALL SELECT 'SystemConfiguration', COUNT(*) FROM SystemConfiguration
UNION ALL SELECT 'ErrorLogs',           COUNT(*) FROM ErrorLogs
UNION ALL SELECT 'AuditLog',            COUNT(*) FROM AuditLog
UNION ALL SELECT 'DatabaseBackups',     COUNT(*) FROM DatabaseBackups
UNION ALL SELECT 'SecurityAlerts',      COUNT(*) FROM SecurityAlerts
ORDER BY t;
GO
SELECT UserID, StudentID, FullName, RoleID FROM Users;
GO