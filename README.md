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
    - npm i @mui/x-charts / npm i react-vis

### eslint

- npm i eslint-plugin-react -D

### vite.config

```js
import {defineConfig} from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    base: 'Ethereal/Ethereal/dist/',
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

## Ethereal-sql(to CreateDataTableSql.sql)

### Create DataTable

#### ethereal_status

#### ethereal_user

#### ethereal_record

#### ethereal_attachment

#### ethereal_comment (评论与活动日志 Comments & Logs) -New

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

## Ethereal Config

### Fluent Api

- HasOne(): 指定主体实体(Principal)有一个依赖实体(Dependent)
- WithOne(): 指定依赖实体有一个主体实体
- HasForeignKey(): 指定依赖实体中的外键属性
- UnauthorizedAccessException表示: 没有权限
- KeyNotFoundException: 未找到异常
