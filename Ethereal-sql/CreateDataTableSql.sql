-- ethereal_status
CREATE TABLE [dbo].[ethereal_status]
(
    [StatusId] INT IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR(20)        NOT NULL,
    CONSTRAINT [PK_ethereal_status] PRIMARY KEY CLUSTERED ([StatusId] ASC),
-- 防止状态名重复
    CONSTRAINT [UQ_ethereal_status_Name] UNIQUE ([Name])
);

-- ethereal_user
CREATE TABLE [dbo].[ethereal_user]
(
    [UserId]           INT IDENTITY (1, 1) NOT NULL,
    UserName           NVARCHAR(50)        NOT NULL,
    PasswordHash       NVARCHAR(256)       NOT NULL,
    Email              NVARCHAR(100)       NOT NULL,
    FullName           NVARCHAR(100)       NOT NULL,
-- Admin | Manager | Member
    Role               NVARCHAR(20)        NOT NULL DEFAULT 'Member',
    Department         NVARCHAR(100)       NULL,
    Phone              NVARCHAR(30)        NULL,
    Position           NVARCHAR(100)       NULL,
    IsActive           BIT                 NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    RefreshToken       NVARCHAR(500)       NULL,
    RefreshTokenExpiry DATETIME2           NULL,

    CONSTRAINT [PK_ethereal_user] PRIMARY KEY CLUSTERED ([UserId] ASC),
-- 保证数据的唯一性（Unique）
    CONSTRAINT UQ_ethereal_user_UserName UNIQUE (UserName),
    CONSTRAINT UQ_ethereal_user_Email UNIQUE (Email),
-- 限制 Role 取值范围
    CONSTRAINT CK_ethereal_user_Role CHECK (Role IN ('System Admin', 'Manager', 'Member','Owner'))
);

-- ethereal_record
CREATE TABLE [dbo].[ethereal_record]
(
    [RecordId]          INT IDENTITY (1, 1) NOT NULL,
    SubNo               NVARCHAR(50)        NOT NULL,
-- e.g. 26aa01
    Title               NVARCHAR(300)       NOT NULL,
    Description         NVARCHAR(MAX)       NULL,
-- 1-Todo|2-InProgress|3-Review|4-Done
    StatusId            INT                 NOT NULL DEFAULT 1,
-- Low | Medium | High
    Priority            NVARCHAR(20)        NOT NULL DEFAULT 'Medium',
    AssigneeUserId      INT                 NULL,
    CreatorRecordUserId INT                 NULL,
    StartDate           DATETIME2           NULL,
    DueDate             DATETIME2           NULL,
    CompletedAt         DATETIME2           NULL,
-- JSON array
    [OrderSortNumber]   INT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ethereal_record PRIMARY KEY CLUSTERED ([RecordId] ASC),
    CONSTRAINT UQ_ethereal_record_SubNo UNIQUE (SubNo),
    CONSTRAINT CK_ethereal_record_Priority CHECK (Priority IN ('Low', 'Medium', 'High', 'Emergency')),
    CONSTRAINT FK_ethereal_record_Assignee FOREIGN KEY (AssigneeUserId) REFERENCES [dbo].[ethereal_user] (UserId),
    CONSTRAINT FK_ethereal_record_Creator FOREIGN KEY (CreatorRecordUserId) REFERENCES [dbo].[ethereal_user] (UserId),
    CONSTRAINT FK_ethereal_record_Status FOREIGN KEY (StatusId) REFERENCES [dbo].[ethereal_status] (StatusId)
);

-- ethereal_attachment
CREATE TABLE [dbo].[ethereal_attachment]
(
    [Id]        INT IDENTITY (1, 1) NOT NULL,
    RecordId    INT                 NOT NULL,
    UserId      INT                 NOT NULL,
    FileName    NVARCHAR(300)       NOT NULL,
    FilePath    NVARCHAR(MAX)       NOT NULL,
    FileSize    BIGINT              NOT NULL DEFAULT 0,
    FileVersion INT                 NOT NULL DEFAULT 1,
    FileStatus  BIT                 NOT NULL DEFAULT 1,
    ContentType NVARCHAR(100)       NOT NULL DEFAULT '',
    UploadedAt  DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_ethereal_attachment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT FK_ethereal_attachment_Record FOREIGN KEY (RecordId) REFERENCES [dbo].[ethereal_record] (RecordId) ON DELETE CASCADE,
    CONSTRAINT FK_ethereal_attachment_User FOREIGN KEY (UserId) REFERENCES [dbo].[ethereal_user] (UserId)
);

-- ethereal_comment (评论与活动日志 Comments & Logs) -New
CREATE TABLE [dbo].[ethereal_comment]
(
    [Id]      INT IDENTITY (1, 1) NOT NULL,
    RecordId  INT                 NOT NULL,
    UserId    INT                 NOT NULL,
    Content   NVARCHAR(MAX)       NOT NULL,
    CreatedAt DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_ethereal_comment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT FK_ethereal_comment_Record FOREIGN KEY (RecordId) REFERENCES [dbo].[ethereal_record] (RecordId) ON DELETE CASCADE,
    CONSTRAINT FK_ethereal_comment_User FOREIGN KEY (UserId) REFERENCES [dbo].[ethereal_user] (UserId)
);
