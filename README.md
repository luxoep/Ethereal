# Ethereal 虚幻任务管理系统

## Ethereal

### 快速构建:vite

- npm create vite@latest

### React

- 路由：React route dom v7
    - npm i react-router-dom@7 -S
- 状态管理：Redux
    - npm i @reduxjs/toolkit react-redux
- 拖拽组件：dnd kit
    - npm i @dnd-kit/react

### Material-UI

- UI
    - npm i @mui/material @emotion/react @emotion/styled
- 时间组件：date-pickers
    - npm i @mui/x-date-pickers
    - npm i dayjs
- 图表：charts
    - npm i @mui/x-charts

### eslint

- npm i eslint-plugin-react -D

### vite.config

```js
import {defineConfig} from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    base: '/Apps/demo/react-counse/jerrypage/dist/',
})
```

#### appsettings.Development

```json
{
  "ConnectionStrings": {
    "DefaultConnectionTest": "Server=localhost\\MSSQLSERVER01;Database=ethereal;Trusted_Connection=True;Trust Server Certificate=True;Encrypt=True"
  },
  "Jwt": {
    "Issuer": "EtherealJWT",
    "Audience": "EtherealJWTUsers",
    "SecretKey": "ThisIsASecretKeyThatIsAtLeast32CharactersLong123!",
    "ExpiresMinutes": 30,
    "RefreshTokenExpiresDays": "1"
  }
}
```

## Ethereal-sql

### Create DataTable

#### ethereal_status

```sql
CREATE TABLE [dbo].[ethereal_status]
(
    [StatusId] INT IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR(20)        NOT NULL,
    CONSTRAINT [PK_ethereal_status] PRIMARY KEY CLUSTERED ([StatusId] ASC),
-- 防止状态名重复
    CONSTRAINT [UQ_ethereal_status_Name] UNIQUE ([Name])
);
```

#### ethereal_user

```sql
CREATE TABLE [dbo].[ethereal_user]
(
    [UserId]           INT IDENTITY (1, 1) NOT NULL,
    Username           NVARCHAR(50)        NOT NULL,
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
    CONSTRAINT UQ_ethereal_user_Username UNIQUE (Username),
    CONSTRAINT UQ_ethereal_user_Email UNIQUE (Email),
-- 限制 Role 取值范围
    CONSTRAINT CK_ethereal_user_Role CHECK (Role IN ('Admin', 'Manager', 'Member'))
);
```

#### ethereal_record

```sql
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
```

#### ethereal_attachment

```sql
CREATE TABLE [dbo].[ethereal_attachment]
(
    [Id]        INT IDENTITY (1, 1) NOT NULL,
    RecordId    INT                 NOT NULL,
    FileName    NVARCHAR(300)       NOT NULL,
    FilePath    NVARCHAR(MAX)       NOT NULL,
    FileSize    BIGINT              NOT NULL DEFAULT 0,
    ContentType NVARCHAR(100)       NOT NULL DEFAULT '',
    UploadedAt  DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_ethereal_attachment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT FK_ethereal_attachment_Record FOREIGN KEY (RecordId) REFERENCES [dbo].[ethereal_record] (RecordId) ON DELETE CASCADE
);
```

#### ethereal_comment (评论与活动日志 Comments & Logs) -New

```sql
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
```

## Ethereal-api (.net8)

### Swagger

### Entity Framework Core/Entity Framework Core SqlServer

- dotnet add package Microsoft.EntityFrameworkCore -v 8.0.22
- dotnet add package Microsoft.EntityFrameworkCore.SqlServer -v 8.0.22

### Auth

- dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer -v 8.0.22

### Hash

- dotnet add package BCrypt.Net-Next -v 4.1.0

### Sql Server

- Tools
    - SQL Server Management Studio 2022

- Database Name
    - ethereal

### Publish

#### .net Core Webapi

- .net core Webapi

```web.config
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\Ethereal-api.dll" stdoutLogEnabled="false"
        stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" />
          <environmentVariable name="COMPLUS_ForceENC" value="1" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```