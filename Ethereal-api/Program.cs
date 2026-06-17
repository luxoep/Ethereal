using System.Text;
using Ethereal_api;
using Ethereal_api.Auth;
using Ethereal_api.IService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Ethereal_v1", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// 依赖注入Token
builder.Services.AddSingleton<TokenService.ITokenService, Token>();
builder.Services.AddScoped<RefreshToken>();

// 检查密钥是否为空
string secretKey = builder.Configuration["SecretKey"] ??
                   throw new InvalidOperationException(
                       "Please make sure you have a SecretKey in the appSettings.json file.");
// 配置jwt身份验证服务
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Issuer"],
            ValidAudience = builder.Configuration["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = con =>
            {
                // 捕获“过期”异常
                // con.Exception.GetType() == typeof(SecurityTokenExpiredException)) 改为使用is关键字
                if (con.Exception is SecurityTokenExpiredException)
                {
                    con.Response.Headers.Append("Token-Expired", "true");
                }

                return Task.CompletedTask;
            }
        };
    });
// 配置授权策略
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        // 访客必须是“已认证”的用户
        .RequireAuthenticatedUser()
        // 访客用来证明自己身份的凭证，必须是 JWT (Bearer Token)
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .Build();
});
// 配置跨域
builder.Services.AddCors(options =>
{
    options.AddPolicy("cors", corsPolicy =>
    {
        corsPolicy.AllowAnyHeader();
        corsPolicy.AllowAnyMethod();
        corsPolicy.AllowAnyOrigin();
        corsPolicy.WithExposedHeaders("Token-Expired");
    });
});

// 连接到app settings中的数据连接字符串
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionTest"))
);
// 防止出现报错：检测到了一个可能的物体循环轨迹
// 1. IgnoreCycles 遇到循环时，直接忽略后续对象
// 2. Preserve 用 $id / $ref 维护对象引用关系
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<IEtherealUserService, ReviewApi.EtherealUserApi>();
builder.Services.AddScoped<IEtherealRecordService, ReviewApi.EtherealRecordApi>();
builder.Services.AddScoped<IEtherealAttachmentService, ReviewApi.EtherealAttachmentApi>();
builder.Services.AddScoped<IEtherealCommentService, ReviewApi.EtherealCommentApi>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("cors");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();