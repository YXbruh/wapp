-- ============================================================
-- CyberShield Academy - DUMMY DATA (for the attached .mdf)
-- >>> Run this INSIDE your CyberShieldAcademy.mdf, AFTER the schema. <<<
--
-- All IDs are explicit codes (PREFIX+3letters+3digits).
-- Passwords are salted SHA-256 (salt$hash). Verify with HashHelper.Verify().
--
-- LOGIN (email / password):
--   admin@cybershield.edu       / Admin@123     (Admin)
--   farah.aziz@cybershield.edu  / Lecturer@123  (Lecturer)
--   daniel.wong@cybershield.edu / Lecturer@123  (Lecturer)
--   aiman.hakim@mail.com + other students / Student@123
-- ============================================================

DELETE FROM LabSubmissions;   DELETE FROM VirtualLabs;
DELETE FROM QuizAnswers;      DELETE FROM QuizAttempts;
DELETE FROM QuizQuestions;    DELETE FROM Quizzes;
DELETE FROM ChapterProgress;  DELETE FROM Enrollments;
DELETE FROM Chapters;         DELETE FROM Courses;
DELETE FROM CourseCategories; DELETE FROM UserAchievements;
DELETE FROM Users;
GO

INSERT INTO Roles (RoleID, RoleName) VALUES
('ROLIDQ053','Student'), ('ROLBNA400','Lecturer'), ('ROLLVD269','Admin');
GO

-- USERS
INSERT INTO Users (UserID, StudentID, FullName, Email, PasswordHash, RoleID, TotalPoints, StreakDays, IsActive) VALUES
('USRAME107', NULL, N'System Administrator', 'admin@cybershield.edu', 'dfbca3ef4fca8790$d0696358080aea7baa203ec06ea5d7df2876e58b2c9cc5c319223afb2cac8a3f', 'ROLLVD269', 0, 0, 1),
('USRDGK804', NULL, N'Dr. Farah Aziz', 'farah.aziz@cybershield.edu', 'd776381f9882b4d2$ae7bc910a08345fdcd98cde17f0c0a73922c78d380754484bb09c3b70b853a0b', 'ROLBNA400', 0, 0, 1),
('USROWV824', NULL, N'Daniel Wong', 'daniel.wong@cybershield.edu', '299df10eaf5f22e5$54ebbd583704a35a9d1b69ee3fa4a8ba30580ce53652490c6d4862de4ae7b765', 'ROLBNA400', 0, 0, 1),
('USRJWY112', 'TP074921', N'Aiman Hakim', 'aiman.hakim@mail.com', 'b2dc38f2ec76c5ff$42f956e8dc53712526c5626a8bca011334f47c5afd0c3ac83eaa3384a03c558c', 'ROLIDQ053', 320, 5, 1),
('USRGMP204', 'TP061358', N'Chong Wei Jian', 'weijian.chong@mail.com', 'e586529739f76a3c$30e3e365772c0423d7a3a079573405a86783c299986b8d0d620a1806a1270417', 'ROLIDQ053', 150, 2, 1),
('USRJOY656', 'TP079842', N'Nurul Izzah', 'nurul.izzah@mail.com', '31086ddc2a56a755$8f26c07aed03e572054bb3d5e7ec349e393a4a66db6590f19196d07a7139755b', 'ROLIDQ053', 480, 9, 1),
('USRRBX787', 'TP065117', N'Rajesh Kumar', 'rajesh.kumar@mail.com', '123c253e07d4925e$7aa04d669fa56fabea9cd002303a5c3713148d13e55bbc49ebefac9d4331bd5b', 'ROLIDQ053', 90, 1, 1),
('USRKKC600', 'TP073506', N'Lee Mei Xin', 'meixin.lee@mail.com', '732f0ad7645888c5$c41b3602d1aa951efd0ec8b825a4d19f6dcf52e4082a33f56965806452c08e1d', 'ROLIDQ053', 210, 3, 1),
('USRFEI001', 'TP068293', N'Tan Jun Hao', 'junhao.tan@mail.com', '6bfab089e665465e$5d5d61f04ee3b74448c0f29b73f29076cece28814f743f778a8304ed92c15eee', 'ROLIDQ053', 0, 0, 1);
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

-- QUIZZES
INSERT INTO Quizzes (QuizID, CourseID, ChapterID, Title, Description, MaxAttempts, PassMark, CreatedByID, IsPublished) VALUES
('QUZGKD934', 'CRSOAN986', 'CHPFFQ984', 'CIA Triad Quiz', 'Test your CIA triad knowledge.', 3, 50.00, 'USRDGK804', 1),
('QUZPXK955', 'CRSOAN986', 'CHPQUC697', 'Threats Quiz', 'Identify common threats.', 3, 50.00, 'USRDGK804', 1),
('QUZNAL107', 'CRSGJE757', 'CHPTME738', 'Nmap Basics Quiz', 'Basic Nmap concepts.', 3, 60.00, 'USRDGK804', 1),
('QUZEMS535', 'CRSIFV988', 'CHPZRJ510', 'File Permissions Quiz', 'Linux permissions knowledge.', 3, 50.00, 'USROWV824', 1);
GO

-- QUIZ QUESTIONS
INSERT INTO QuizQuestions (QuestionID, QuizID, QuestionText, QuestionType, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, Explanation, SortOrder) VALUES
('QSNBFI372', 'QUZGKD934', 'What does the "C" in the CIA triad stand for?', 'MCQ', 'Control', 'Confidentiality', 'Compliance', 'Certification', 'B', 'C = Confidentiality.', 1),
('QSNNEJ020', 'QUZGKD934', 'Integrity ensures data is not altered by unauthorized parties.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', 'Integrity = accuracy and trust.', 2),
('QSNTOE491', 'QUZGKD934', 'The "A" in the CIA triad stands for ______.', 'FillBlank', NULL, NULL, NULL, NULL, 'Availability', 'Availability = accessible when needed.', 3),
('QSNVLM835', 'QUZPXK955', 'Which of the following is a social engineering attack?', 'MCQ', 'Phishing', 'Firewall', 'Encryption', 'Patching', 'A', 'Phishing tricks users.', 1),
('QSNRZI876', 'QUZPXK955', 'Malware is short for malicious software.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', 'malicious + software.', 2),
('QSNKJB107', 'QUZNAL107', 'Which Nmap flag performs a SYN scan?', 'MCQ', '-sT', '-sS', '-sU', '-sP', 'B', '-sS = stealthy SYN scan.', 1),
('QSNGEC886', 'QUZNAL107', 'Nmap can discover open ports on a target.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', 'Core Nmap function.', 2),
('QSNHGH414', 'QUZNAL107', 'The flag to scan all 65535 ports is -p-.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', '-p- scans every port.', 3),
('QSNGZZ529', 'QUZEMS535', 'Which command changes file permissions in Linux?', 'MCQ', 'chown', 'chmod', 'chgrp', 'chdir', 'B', 'chmod = change mode.', 1),
('QSNIJA345', 'QUZEMS535', 'The numeric permission 755 gives the owner full control.', 'TrueFalse', NULL, NULL, NULL, NULL, 'True', '7 = rwx for owner.', 2);
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

 
INSERT INTO SystemConfiguration (ConfigID, ConfigKey, ConfigValue, Description) VALUES
('CFGCVH922','MinPasswordLength','8','Minimum password length'),
('CFGNEJ988','MaxFailedLoginAttempts','5','Account lockout threshold'),
('CFGQXY570','SessionTimeoutMinutes','30','Idle session timeout in minutes'),
('CFGTLT620','DefaultPassMark','50','Default quiz pass percentage'),
('CFGYDM327','MaintenanceMode','false','Show maintenance page when true');
GO
 
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

-- VERIFY
SELECT 'Users' t, COUNT(*) n FROM Users
UNION ALL SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL SELECT 'VirtualLabs', COUNT(*) FROM VirtualLabs;
GO
SELECT UserID, StudentID, FullName, RoleID FROM Users;
GO