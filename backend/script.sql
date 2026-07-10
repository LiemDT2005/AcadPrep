CREATE TABLE [Achievements] (
    [AchievementId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [IconUrl] nvarchar(500) NULL,
    [ConditionType] nvarchar(max) NOT NULL,
    [ConditionValue] int NOT NULL,
    CONSTRAINT [PK_Achievements] PRIMARY KEY ([AchievementId])
);
GO


CREATE TABLE [EXAM_SERIES] (
    [ExamSeriesId] int NOT NULL IDENTITY,
    [Name] nvarchar(255) NOT NULL,
    [Year] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [CoverImageUrl] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [LastModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_EXAM_SERIES] PRIMARY KEY ([ExamSeriesId])
);
GO


CREATE TABLE [ROLES] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] varchar(50) NOT NULL,
    CONSTRAINT [PK_ROLES] PRIMARY KEY ([RoleId])
);
GO


CREATE TABLE [VOCABULARIES] (
    [VocabularyId] int NOT NULL IDENTITY,
    [Word] nvarchar(100) NOT NULL,
    [Phonetic] varchar(100) NULL,
    [Meaning] nvarchar(500) NOT NULL,
    [Example] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [LastModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_VOCABULARIES] PRIMARY KEY ([VocabularyId])
);
GO


CREATE TABLE [EXAMS] (
    [ExamId] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Duration] int NOT NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [LastModifiedAt] datetime2 NULL,
    [AudioUrl] varchar(500) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [ExamSeriesId] int NOT NULL,
    CONSTRAINT [PK_EXAMS] PRIMARY KEY ([ExamId]),
    CONSTRAINT [FK_EXAMS_EXAM_SERIES_ExamSeriesId] FOREIGN KEY ([ExamSeriesId]) REFERENCES [EXAM_SERIES] ([ExamSeriesId]) ON DELETE CASCADE
);
GO


CREATE TABLE [USERS] (
    [UserId] int NOT NULL IDENTITY,
    [Email] varchar(150) NOT NULL,
    [PasswordHash] varchar(255) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [GoogleId] nvarchar(max) NULL,
    [Status] varchar(50) NOT NULL DEFAULT 'Active',
    [RoleId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [LastModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_USERS] PRIMARY KEY ([UserId]),
    CONSTRAINT [CHK_UserStatus] CHECK ([Status] IN ('Active', 'Inactive')),
    CONSTRAINT [FK_USERS_ROLES_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [ROLES] ([RoleId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [VOCAB_PASSAGES] (
    [VocabPassageId] int NOT NULL IDENTITY,
    [Content] nvarchar(max) NOT NULL,
    [VocabularyId] int NOT NULL,
    CONSTRAINT [PK_VOCAB_PASSAGES] PRIMARY KEY ([VocabPassageId]),
    CONSTRAINT [FK_VOCAB_PASSAGES_VOCABULARIES_VocabularyId] FOREIGN KEY ([VocabularyId]) REFERENCES [VOCABULARIES] ([VocabularyId]) ON DELETE CASCADE
);
GO


CREATE TABLE [PARTS] (
    [PartId] int NOT NULL IDENTITY,
    [ExamId] int NOT NULL,
    [PartNumber] int NOT NULL,
    [TotalQuestions] int NOT NULL,
    CONSTRAINT [PK_PARTS] PRIMARY KEY ([PartId]),
    CONSTRAINT [FK_PARTS_EXAMS_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [EXAMS] ([ExamId]) ON DELETE CASCADE
);
GO


CREATE TABLE [PASSAGES] (
    [PassageId] int NOT NULL IDENTITY,
    [Content] nvarchar(max) NULL,
    [ImageUrl] varchar(500) NULL,
    [ExamId] int NOT NULL,
    CONSTRAINT [PK_PASSAGES] PRIMARY KEY ([PassageId]),
    CONSTRAINT [FK_PASSAGES_EXAMS_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [EXAMS] ([ExamId]) ON DELETE CASCADE
);
GO


CREATE TABLE [QUESTION_GROUPS] (
    [QuestionGroupId] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [AudioUrl] varchar(500) NULL,
    [AudioStartSecond] int NULL,
    [AudioEndSecond] int NULL,
    [ImageUrl] varchar(500) NULL,
    [ExamId] int NOT NULL,
    CONSTRAINT [PK_QUESTION_GROUPS] PRIMARY KEY ([QuestionGroupId]),
    CONSTRAINT [FK_QUESTION_GROUPS_EXAMS_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [EXAMS] ([ExamId]) ON DELETE CASCADE
);
GO


CREATE TABLE [AUDITLOGS] (
    [LogId] int NOT NULL IDENTITY,
    [UserId] int NULL,
    [Action] nvarchar(255) NOT NULL,
    [TableAffected] varchar(100) NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_AUDITLOGS] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_AUDITLOGS_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [USERS] ([UserId]) ON DELETE SET NULL
);
GO


CREATE TABLE [EXAM_ATTEMPTS] (
    [AttemptId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ExamId] int NOT NULL,
    [ListeningScore] int NOT NULL DEFAULT 0,
    [ReadingScore] int NOT NULL DEFAULT 0,
    [TotalScore] int NOT NULL DEFAULT 0,
    [RemainingTime] int NOT NULL,
    [IsSubmitted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [StartedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_EXAM_ATTEMPTS] PRIMARY KEY ([AttemptId]),
    CONSTRAINT [FK_EXAM_ATTEMPTS_EXAMS_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [EXAMS] ([ExamId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EXAM_ATTEMPTS_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [USERS] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [PRACTICE_SESSIONS] (
    [PracticeSessionId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ExamId] int NOT NULL,
    [SelectedParts] nvarchar(100) NOT NULL,
    [SelectedTags] nvarchar(500) NULL,
    [TimeLimit] int NULL,
    [CombinedQuestionsList] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PRACTICE_SESSIONS] PRIMARY KEY ([PracticeSessionId]),
    CONSTRAINT [FK_PRACTICE_SESSIONS_EXAMS_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [EXAMS] ([ExamId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PRACTICE_SESSIONS_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [USERS] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [SAVED_VOCABULARIES] (
    [UserId] int NOT NULL,
    [VocabularyId] int NOT NULL,
    [Interval] int NOT NULL DEFAULT 1,
    [NextReviewDate] datetime2 NOT NULL,
    [DateSaved] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_SAVED_VOCABULARIES] PRIMARY KEY ([UserId], [VocabularyId]),
    CONSTRAINT [FK_SAVED_VOCABULARIES_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [USERS] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_SAVED_VOCABULARIES_VOCABULARIES_VocabularyId] FOREIGN KEY ([VocabularyId]) REFERENCES [VOCABULARIES] ([VocabularyId]) ON DELETE CASCADE
);
GO


CREATE TABLE [STUDY_STREAKS] (
    [UserId] int NOT NULL,
    [CurrentStreak] int NOT NULL DEFAULT 0,
    [MaxStreak] int NOT NULL DEFAULT 0,
    [LastActiveDate] date NOT NULL,
    [UserId1] int NULL,
    CONSTRAINT [PK_STUDY_STREAKS] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_STUDY_STREAKS_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [USERS] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_STUDY_STREAKS_USERS_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [USERS] ([UserId])
);
GO


CREATE TABLE [UserAchievements] (
    [UserId] int NOT NULL,
    [AchievementId] int NOT NULL,
    [UnlockedAt] datetime2 NOT NULL,
    [IsNotified] bit NOT NULL,
    CONSTRAINT [PK_UserAchievements] PRIMARY KEY ([UserId], [AchievementId]),
    CONSTRAINT [FK_UserAchievements_Achievements_AchievementId] FOREIGN KEY ([AchievementId]) REFERENCES [Achievements] ([AchievementId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserAchievements_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [USERS] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [QUESTIONS] (
    [QuestionId] int NOT NULL IDENTITY,
    [QuestionNumber] int NOT NULL,
    [Part] int NOT NULL,
    [QuestionText] nvarchar(max) NULL,
    [AudioUrl] varchar(500) NULL,
    [AudioStartSecond] int NULL,
    [AudioEndSecond] int NULL,
    [ImageUrl] varchar(500) NULL,
    [CorrectOption] varchar(1) NOT NULL,
    [ExamId] int NOT NULL,
    [PassageId] int NULL,
    [QuestionType] nvarchar(100) NULL,
    [TopicTag] nvarchar(150) NULL,
    [QuestionGroupId] int NULL,
    CONSTRAINT [PK_QUESTIONS] PRIMARY KEY ([QuestionId]),
    CONSTRAINT [CHK_CorrectOption] CHECK ([CorrectOption] IN ('A', 'B', 'C', 'D')),
    CONSTRAINT [CHK_QuestionPart] CHECK ([Part] BETWEEN 1 AND 7),
    CONSTRAINT [FK_QUESTIONS_EXAMS_ExamId] FOREIGN KEY ([ExamId]) REFERENCES [EXAMS] ([ExamId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_QUESTIONS_PASSAGES_PassageId] FOREIGN KEY ([PassageId]) REFERENCES [PASSAGES] ([PassageId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_QUESTIONS_QUESTION_GROUPS_QuestionGroupId] FOREIGN KEY ([QuestionGroupId]) REFERENCES [QUESTION_GROUPS] ([QuestionGroupId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ATTEMPT_ANSWERS] (
    [AttemptId] int NOT NULL,
    [QuestionId] int NOT NULL,
    [SelectedOption] varchar(1) NULL,
    [IsCorrect] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_ATTEMPT_ANSWERS] PRIMARY KEY ([AttemptId], [QuestionId]),
    CONSTRAINT [CHK_SelectedOption] CHECK ([SelectedOption] IN ('A', 'B', 'C', 'D')),
    CONSTRAINT [FK_ATTEMPT_ANSWERS_EXAM_ATTEMPTS_AttemptId] FOREIGN KEY ([AttemptId]) REFERENCES [EXAM_ATTEMPTS] ([AttemptId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ATTEMPT_ANSWERS_QUESTIONS_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [QUESTIONS] ([QuestionId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [QUESTION_OPTIONS] (
    [OptionId] int NOT NULL IDENTITY,
    [QuestionId] int NOT NULL,
    [OptionLetter] varchar(1) NOT NULL,
    [OptionText] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_QUESTION_OPTIONS] PRIMARY KEY ([OptionId]),
    CONSTRAINT [CHK_OptionLetter] CHECK ([OptionLetter] IN ('A', 'B', 'C', 'D')),
    CONSTRAINT [FK_QUESTION_OPTIONS_QUESTIONS_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [QUESTIONS] ([QuestionId]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_ATTEMPT_ANSWERS_QuestionId] ON [ATTEMPT_ANSWERS] ([QuestionId]);
GO


CREATE INDEX [IX_AUDITLOGS_UserId] ON [AUDITLOGS] ([UserId]);
GO


CREATE INDEX [IX_EXAM_ATTEMPTS_ExamId] ON [EXAM_ATTEMPTS] ([ExamId]);
GO


CREATE INDEX [IX_EXAM_ATTEMPTS_UserId] ON [EXAM_ATTEMPTS] ([UserId]);
GO


CREATE INDEX [IX_EXAMS_ExamSeriesId] ON [EXAMS] ([ExamSeriesId]);
GO


CREATE INDEX [IX_PARTS_ExamId] ON [PARTS] ([ExamId]);
GO


CREATE INDEX [IX_PASSAGES_ExamId] ON [PASSAGES] ([ExamId]);
GO


CREATE INDEX [IX_PRACTICE_SESSIONS_ExamId] ON [PRACTICE_SESSIONS] ([ExamId]);
GO


CREATE INDEX [IX_PRACTICE_SESSIONS_UserId] ON [PRACTICE_SESSIONS] ([UserId]);
GO


CREATE INDEX [IX_QUESTION_GROUPS_ExamId] ON [QUESTION_GROUPS] ([ExamId]);
GO


CREATE INDEX [IX_QUESTION_OPTIONS_QuestionId] ON [QUESTION_OPTIONS] ([QuestionId]);
GO


CREATE INDEX [IX_QUESTIONS_ExamId] ON [QUESTIONS] ([ExamId]);
GO


CREATE INDEX [IX_QUESTIONS_PassageId] ON [QUESTIONS] ([PassageId]);
GO


CREATE INDEX [IX_QUESTIONS_QuestionGroupId] ON [QUESTIONS] ([QuestionGroupId]);
GO


CREATE UNIQUE INDEX [IX_ROLES_RoleName] ON [ROLES] ([RoleName]);
GO


CREATE INDEX [IX_SAVED_VOCABULARIES_VocabularyId] ON [SAVED_VOCABULARIES] ([VocabularyId]);
GO


CREATE INDEX [IX_STUDY_STREAKS_UserId1] ON [STUDY_STREAKS] ([UserId1]);
GO


CREATE INDEX [IX_UserAchievements_AchievementId] ON [UserAchievements] ([AchievementId]);
GO


CREATE UNIQUE INDEX [IX_USERS_Email] ON [USERS] ([Email]);
GO


CREATE INDEX [IX_USERS_RoleId] ON [USERS] ([RoleId]);
GO


CREATE INDEX [IX_VOCAB_PASSAGES_VocabularyId] ON [VOCAB_PASSAGES] ([VocabularyId]);
GO


CREATE UNIQUE INDEX [IX_VOCABULARIES_Word] ON [VOCABULARIES] ([Word]);
GO


