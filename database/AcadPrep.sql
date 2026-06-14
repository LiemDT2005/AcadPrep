-- 1. Khởi tạo Cơ sở dữ liệu AcadPrep
CREATE DATABASE AcadPrepDB;
GO
USE AcadPrepDB;
GO

-- ==========================================
-- 2. TẠO CÁC BẢNG ĐỘC LẬP (KHÔNG CHỨA KHÓA NGOẠI)
-- ==========================================

-- Bảng ROLES (Vai trò người dùng)
CREATE TABLE ROLES (
    RoleId INT IDENTITY(1,1),
    RoleName VARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT PK_ROLES PRIMARY KEY (RoleId)
);

-- Bảng EXAMS (Đề thi)
CREATE TABLE EXAMS (
    ExamId INT IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Duration INT NOT NULL, -- Thời gian làm bài (phút)
    IsDeleted BIT DEFAULT 0, -- Phục vụ cơ chế Soft Delete (BR-15)
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_EXAMS PRIMARY KEY (ExamId)
);

-- Bảng VOCABULARIES (Từ vựng)
CREATE TABLE VOCABULARIES (
    VocabularyId INT IDENTITY(1,1),
    Word NVARCHAR(100) NOT NULL UNIQUE,
    Phonetic VARCHAR(100),
    Meaning NVARCHAR(500) NOT NULL,
    Example NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_VOCABULARIES PRIMARY KEY (VocabularyId)
);

-- ==========================================
-- 3. TẠO CÁC BẢNG CHỨA KHÓA NGOẠI TẦNG 1
-- ==========================================

-- Bảng USERS (Người dùng)
CREATE TABLE USERS (
    UserId INT IDENTITY(1,1),
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL, -- Đã băm theo BR-25
    FullName NVARCHAR(150) NOT NULL,
    Status VARCHAR(50) DEFAULT 'Active', -- Active, Inactive (BR-19)
    RoleId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_USERS PRIMARY KEY (UserId),
    CONSTRAINT FK_USERS_ROLES FOREIGN KEY (RoleId) REFERENCES ROLES(RoleId),
    CONSTRAINT CHK_UserStatus CHECK (Status IN ('Active', 'Inactive'))
);

-- Bảng PASSAGES (Đoạn văn đọc hiểu dùng cho Part 6, 7)
CREATE TABLE PASSAGES (
    PassageId INT IDENTITY(1,1),
    Content NVARCHAR(MAX) NOT NULL,
    ExamId INT NOT NULL,
    CONSTRAINT PK_PASSAGES PRIMARY KEY (PassageId),
    CONSTRAINT FK_PASSAGES_EXAMS FOREIGN KEY (ExamId) REFERENCES EXAMS(ExamId) ON DELETE CASCADE
);

-- Bảng VOCAB_PASSAGES (Đoạn văn ngữ cảnh của từ vựng - BR-11)
CREATE TABLE VOCAB_PASSAGES (
    VocabPassageId INT IDENTITY(1,1),
    Content NVARCHAR(MAX) NOT NULL,
    VocabularyId INT NOT NULL,
    CONSTRAINT PK_VOCAB_PASSAGES PRIMARY KEY (VocabPassageId),
    CONSTRAINT FK_VOCAB_PASSAGES_VOCABULARIES FOREIGN KEY (VocabularyId) REFERENCES VOCABULARIES(VocabularyId) ON DELETE CASCADE
);

-- ==========================================
-- 4. TẠO CÁC BẢNG CHỨA KHÓA NGOẠI TẦNG 2
-- ==========================================

-- Bảng QUESTIONS (Câu hỏi thi)
CREATE TABLE QUESTIONS (
    QuestionId INT IDENTITY(1,1),
    QuestionNumber INT NOT NULL,
    Part INT NOT NULL, -- Từ Part 1 đến Part 7 (BR-16)
    QuestionText NVARCHAR(MAX),
    AudioUrl VARCHAR(500), -- File âm thanh cho Listening Parts (BR-03)
    CorrectOption CHAR(1) NOT NULL, -- A, B, C, D
    ExamId INT NOT NULL,
    PassageId INT NULL, -- Có thể NULL nếu thuộc Part 1-5, bắt buộc có nếu thuộc Part 6-7 (BR-17)
    CONSTRAINT PK_QUESTIONS PRIMARY KEY (QuestionId),
    CONSTRAINT FK_QUESTIONS_EXAMS FOREIGN KEY (ExamId) REFERENCES EXAMS(ExamId),
    CONSTRAINT FK_QUESTIONS_PASSAGES FOREIGN KEY (PassageId) REFERENCES PASSAGES(PassageId),
    CONSTRAINT CHK_QuestionPart CHECK (Part BETWEEN 1 AND 7),
    CONSTRAINT CHK_CorrectOption CHECK (CorrectOption IN ('A', 'B', 'C', 'D'))
);

-- Bảng EXAM_ATTEMPTS (Lượt làm bài thi thử của người dùng)
CREATE TABLE EXAM_ATTEMPTS (
    AttemptId INT IDENTITY(1,1),
    UserId INT NOT NULL,
    ExamId INT NOT NULL,
    ListeningScore INT DEFAULT 0,
    ReadingScore INT DEFAULT 0,
    TotalScore INT DEFAULT 0, -- Tổng điểm quy đổi chuẩn TOEIC 0 - 990 (BR-06)
    RemainingTime INT NOT NULL, -- Lưu thời gian còn lại khi bị ngắt quãng (BR-04)
    IsSubmitted BIT DEFAULT 0, -- Kích hoạt tính điểm khi bằng 1 (BR-05)
    StartedAt DATETIME DEFAULT GETDATE(),
    CompletedAt DATETIME NULL,
    CONSTRAINT PK_EXAM_ATTEMPTS PRIMARY KEY (AttemptId),
    CONSTRAINT FK_ATTEMPTS_USERS FOREIGN KEY (UserId) REFERENCES USERS(UserId),
    CONSTRAINT FK_ATTEMPTS_EXAMS FOREIGN KEY (ExamId) REFERENCES EXAMS(ExamId)
);

-- ==========================================
-- 5. TẠO CÁC BẢNG TRUNG GIAN & LOGS (MỐI QUAN HỆ NHIỀU - NHIỀU)
-- ==========================================

-- Bảng QUESTION_OPTIONS (Các lựa chọn đáp án nhiễu A, B, C, D)
CREATE TABLE QUESTION_OPTIONS (
    OptionId INT IDENTITY(1,1),
    QuestionId INT NOT NULL,
    OptionLetter CHAR(1) NOT NULL, -- A, B, C, D
    OptionText NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_QUESTION_OPTIONS PRIMARY KEY (OptionId),
    CONSTRAINT FK_OPTIONS_QUESTIONS FOREIGN KEY (QuestionId) REFERENCES QUESTIONS(QuestionId) ON DELETE CASCADE,
    CONSTRAINT CHK_OptionLetter CHECK (OptionLetter IN ('A', 'B', 'C', 'D'))
);

-- Bảng ATTEMPT_ANSWERS (Chi tiết đáp án người dùng đã tích chọn trong lượt thi - BR-04)
CREATE TABLE ATTEMPT_ANSWERS (
    AttemptId INT NOT NULL,
    QuestionId INT NOT NULL,
    SelectedOption CHAR(1) NULL, -- A, B, C, D (Có thể NULL nếu chưa chọn)
    IsCorrect BIT DEFAULT 0,
    CONSTRAINT PK_ATTEMPT_ANSWERS PRIMARY KEY (AttemptId, QuestionId),
    CONSTRAINT FK_ANSWERS_ATTEMPTS FOREIGN KEY (AttemptId) REFERENCES EXAM_ATTEMPTS(AttemptId) ON DELETE CASCADE,
    CONSTRAINT FK_ANSWERS_QUESTIONS FOREIGN KEY (QuestionId) REFERENCES QUESTIONS(QuestionId),
    CONSTRAINT CHK_SelectedOption CHECK (SelectedOption IN ('A', 'B', 'C', 'D'))
);

-- Bảng SAVED_VOCABULARIES (Sổ tay từ vựng của người dùng - Ràng buộc duy nhất BR-10)
CREATE TABLE SAVED_VOCABULARIES (
    UserId INT NOT NULL,
    VocabularyId INT NOT NULL,
    Interval INT DEFAULT 1, -- Tham số tính chu kỳ lặp lại ngắt quãng (BR-09)
    DateSaved DATETIME DEFAULT GETDATE(), -- Mặc định sắp xếp theo trường này (BR-21)
    CONSTRAINT PK_SAVED_VOCABULARIES PRIMARY KEY (UserId, VocabularyId),
    CONSTRAINT FK_SAVED_USERS FOREIGN KEY (UserId) REFERENCES USERS(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_SAVED_VOCABULARIES FOREIGN KEY (VocabularyId) REFERENCES VOCABULARIES(VocabularyId) ON DELETE CASCADE
);

-- Bảng STUDY_STREAKS (Theo dõi chuỗi ngày học liên tục - BR-12)
CREATE TABLE STUDY_STREAKS (
    UserId INT NOT NULL,
    CurrentStreak INT DEFAULT 0,
    MaxStreak INT DEFAULT 0,
    LastActiveDate DATE NOT NULL, -- Tính toán dựa trên UTC+7
    CONSTRAINT PK_STUDY_STREAKS PRIMARY KEY (UserId),
    CONSTRAINT FK_STREAKS_USERS FOREIGN KEY (UserId) REFERENCES USERS(UserId) ON DELETE CASCADE
);

-- Bảng AUDITLOGS (Ghi vết lịch sử thao tác hệ thống)
CREATE TABLE AUDITLOGS (
    LogId INT IDENTITY(1,1),
    UserId INT NULL, -- NULL nếu là hành động của Guest trước khi kích hoạt
    Action NVARCHAR(255) NOT NULL,
    TableAffected VARCHAR(100),
    Timestamp DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_AUDITLOGS PRIMARY KEY (LogId),
    CONSTRAINT FK_LOGS_USERS FOREIGN KEY (UserId) REFERENCES USERS(UserId) ON DELETE SET NULL
);
GO