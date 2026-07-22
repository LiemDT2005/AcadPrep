USE [AcadPrepDB]
GO

SET NOCOUNT ON;
PRINT '========================================================='
PRINT 'BẮT ĐẦU SEED DỮ LIỆU FULL CHO ACADPREP'
PRINT 'Lưu ý: Script này tạo ra lượng lớn dữ liệu bằng vòng lặp.'
PRINT 'Nên chạy trên Database trống để tránh lỗi trùng lặp ID.'
PRINT '========================================================='

-- Xóa dữ liệu cũ nếu cần thiết (Bạn có thể bỏ comment các dòng dưới nếu muốn reset DB)
-- DELETE FROM [ATTEMPT_ANSWERS];
-- DELETE FROM [EXAM_ATTEMPTS];
-- DELETE FROM [QUESTION_OPTIONS];
-- DELETE FROM [QUESTIONS];
-- DELETE FROM [PARTS];
-- DELETE FROM [EXAMS];
-- DELETE FROM [EXAM_SERIES];
-- DELETE FROM [VOCABULARIES];
-- DELETE FROM [Achievements];
-- DELETE FROM [USERS];
-- DELETE FROM [ROLES];

-- 1. SEED ROLES
PRINT 'Seeding ROLES...'
SET IDENTITY_INSERT [ROLES] ON;
INSERT INTO [ROLES] (RoleId, RoleName) VALUES 
(1, 'Admin'), 
(2, 'Learner'), 
(3, 'Moderator');
SET IDENTITY_INSERT [ROLES] OFF;
GO

-- 2. SEED USERS (20 Users)
PRINT 'Seeding USERS...'
SET IDENTITY_INSERT [USERS] ON;
INSERT INTO [USERS] (UserId, Email, PasswordHash, FullName, Status, RoleId, CreatedAt) VALUES 
(1, 'admin@acadprep.com', 'Password123!', 'Admin User', 'Active', 1, GETDATE()),
(2, 'learner@acadprep.com', 'Password123!', 'Test Learner', 'Active', 2, GETDATE()),
(3, 'moderator@acadprep.com', 'Password123!', 'Mod User', 'Active', 3, GETDATE());

DECLARE @i INT = 4;
WHILE @i <= 20
BEGIN
    INSERT INTO [USERS] (UserId, Email, PasswordHash, FullName, Status, RoleId, CreatedAt) 
    VALUES (@i, CONCAT('user', @i, '@acadprep.com'), 'Password123!', CONCAT('Student ', @i), 'Active', 2, GETDATE());
    SET @i = @i + 1;
END
SET IDENTITY_INSERT [USERS] OFF;
GO

-- 3. SEED EXAM_SERIES (5 Series)
PRINT 'Seeding EXAM_SERIES...'
SET IDENTITY_INSERT [EXAM_SERIES] ON;
INSERT INTO [EXAM_SERIES] (ExamSeriesId, Name, Year, Description, IsDeleted, CreatedAt) VALUES
(1, 'ETS TOEIC 2024', 2024, N'Bộ đề ETS mới nhất năm 2024', 0, GETDATE()),
(2, 'ETS TOEIC 2023', 2023, N'Bộ đề ETS 2023', 0, GETDATE()),
(3, 'Hacker TOEIC', 2023, N'Bộ đề nâng cao Hacker TOEIC', 0, GETDATE()),
(4, 'Economy TOEIC', 2024, N'Bộ đề Economy', 0, GETDATE()),
(5, 'YBM TOEIC Vol 3', 2024, N'YBM TOEIC', 0, GETDATE());
SET IDENTITY_INSERT [EXAM_SERIES] OFF;
GO

-- 4. SEED EXAMS (10 Exams)
PRINT 'Seeding EXAMS...'
SET IDENTITY_INSERT [EXAMS] ON;
DECLARE @examCount INT = 1;
WHILE @examCount <= 10
BEGIN
    DECLARE @seriesId INT = ((@examCount - 1) / 2) + 1;
    INSERT INTO [EXAMS] (ExamId, Title, Description, Duration, Status, ExamSeriesId, CreatedAt) 
    VALUES (@examCount, CONCAT('TOEIC Mock Test ', @examCount), CONCAT(N'Đề thi thử số ', @examCount), 120, 'Published', @seriesId, GETDATE());
    SET @examCount = @examCount + 1;
END
SET IDENTITY_INSERT [EXAMS] OFF;
GO

-- 5. SEED PARTS (7 Parts for each Exam)
PRINT 'Seeding PARTS...'
SET IDENTITY_INSERT [PARTS] ON;
DECLARE @p_examId INT = 1;
DECLARE @partId INT = 1;
WHILE @p_examId <= 10
BEGIN
    INSERT INTO [PARTS] (PartId, ExamId, PartNumber, TotalQuestions) VALUES
    (@partId, @p_examId, 1, 6),
    (@partId+1, @p_examId, 2, 25),
    (@partId+2, @p_examId, 3, 39),
    (@partId+3, @p_examId, 4, 30),
    (@partId+4, @p_examId, 5, 30),
    (@partId+5, @p_examId, 6, 16),
    (@partId+6, @p_examId, 7, 54);
    
    SET @partId = @partId + 7;
    SET @p_examId = @p_examId + 1;
END
SET IDENTITY_INSERT [PARTS] OFF;
GO

-- 6. SEED QUESTIONS (2000 Questions)
PRINT 'Seeding QUESTIONS (This might take a few seconds)...'
SET IDENTITY_INSERT [QUESTIONS] ON;

DECLARE @q_examId INT = 1;
DECLARE @qId INT = 1;

WHILE @q_examId <= 10
BEGIN
    DECLARE @qNum INT = 1;
    WHILE @qNum <= 200
    BEGIN
        DECLARE @partNum INT;
        IF @qNum <= 6 SET @partNum = 1;
        ELSE IF @qNum <= 31 SET @partNum = 2;
        ELSE IF @qNum <= 70 SET @partNum = 3;
        ELSE IF @qNum <= 100 SET @partNum = 4;
        ELSE IF @qNum <= 130 SET @partNum = 5;
        ELSE IF @qNum <= 146 SET @partNum = 6;
        ELSE SET @partNum = 7;
        
        DECLARE @correctChar CHAR(1);
        DECLARE @randOption INT = (ABS(CHECKSUM(NEWID())) % 4);
        IF @randOption = 0 SET @correctChar = 'A';
        IF @randOption = 1 SET @correctChar = 'B';
        IF @randOption = 2 SET @correctChar = 'C';
        IF @randOption = 3 SET @correctChar = 'D';

        INSERT INTO [QUESTIONS] (QuestionId, QuestionNumber, Part, QuestionText, CorrectOption, ExamId, QuestionType, TopicTag)
        VALUES (@qId, @qNum, @partNum, CONCAT('Question ', @qNum, ' for Exam ', @q_examId, ' (Part ', @partNum, ')'), @correctChar, @q_examId, 'General', 'Mixed');
        
        SET @qId = @qId + 1;
        SET @qNum = @qNum + 1;
    END
    SET @q_examId = @q_examId + 1;
END
SET IDENTITY_INSERT [QUESTIONS] OFF;
GO

-- 6.5 SEED QUESTION_OPTIONS (8000 Options)
PRINT 'Seeding QUESTION_OPTIONS...'
SET IDENTITY_INSERT [QUESTION_OPTIONS] ON;
DECLARE @opt_qId INT = 1;
DECLARE @optId INT = 1;

-- 10 Exams * 200 Questions = 2000 Questions
WHILE @opt_qId <= 2000
BEGIN
    INSERT INTO [QUESTION_OPTIONS] (OptionId, QuestionId, OptionLetter, OptionText) VALUES
    (@optId, @opt_qId, 'A', CONCAT('Option A for Q', @opt_qId)),
    (@optId+1, @opt_qId, 'B', CONCAT('Option B for Q', @opt_qId)),
    (@optId+2, @opt_qId, 'C', CONCAT('Option C for Q', @opt_qId)),
    (@optId+3, @opt_qId, 'D', CONCAT('Option D for Q', @opt_qId));
    
    SET @optId = @optId + 4;
    SET @opt_qId = @opt_qId + 1;
END
SET IDENTITY_INSERT [QUESTION_OPTIONS] OFF;
GO

-- 7. SEED VOCABULARIES (50 Vocabs)
PRINT 'Seeding VOCABULARIES...'
SET IDENTITY_INSERT [VOCABULARIES] ON;
DECLARE @vId INT = 1;
WHILE @vId <= 50
BEGIN
    INSERT INTO [VOCABULARIES] (VocabularyId, Word, Phonetic, Meaning, Example, CreatedAt) VALUES
    (@vId, CONCAT('Word', @vId), '/wɜːrd/', N'Đây là từ vựng số ' + CAST(@vId AS NVARCHAR), CONCAT('This is an example for word ', @vId), GETDATE());
    SET @vId = @vId + 1;
END
SET IDENTITY_INSERT [VOCABULARIES] OFF;
GO

-- 8. SEED ACHIEVEMENTS
PRINT 'Seeding ACHIEVEMENTS...'
SET IDENTITY_INSERT [Achievements] ON;
INSERT INTO [Achievements] (AchievementId, Name, Description, ConditionType, ConditionValue) VALUES
(1, 'First Blood', N'Hoàn thành bài thi đầu tiên', 'ExamsCompleted', 1),
(2, 'Streak Master', N'Duy trì học 7 ngày liên tục', 'Streak', 7),
(3, 'Perfect Score', N'Đạt điểm tuyệt đối 990', 'Score', 990),
(4, 'Vocab Beginner', N'Lưu 10 từ vựng', 'VocabCount', 10),
(5, 'Vocab Master', N'Lưu 100 từ vựng', 'VocabCount', 100);
SET IDENTITY_INSERT [Achievements] OFF;
GO

-- 9. SEED EXAM ATTEMPTS (Lịch sử thi cho user 2)
PRINT 'Seeding EXAM ATTEMPTS...'
SET IDENTITY_INSERT [EXAM_ATTEMPTS] ON;
DECLARE @attemptId INT = 1;
DECLARE @a_examId INT = 1;
WHILE @a_examId <= 5
BEGIN
    INSERT INTO [EXAM_ATTEMPTS] (AttemptId, UserId, ExamId, ListeningScore, ReadingScore, TotalScore, RemainingTime, IsSubmitted, StartedAt, CompletedAt)
    VALUES (@attemptId, 2, @a_examId, 350, 350, 700, 0, 1, DATEADD(day, -@a_examId, GETDATE()), DATEADD(day, -@a_examId, GETDATE()));
    SET @attemptId = @attemptId + 1;
    SET @a_examId = @a_examId + 1;
END
SET IDENTITY_INSERT [EXAM_ATTEMPTS] OFF;
GO

PRINT '========================================================='
PRINT 'HOÀN TẤT SEED DỮ LIỆU FULL!'
PRINT '========================================================='
