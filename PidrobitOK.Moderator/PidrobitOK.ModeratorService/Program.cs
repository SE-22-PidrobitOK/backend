using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AutoMapper;
using PidrobitOK.ModeratorService;
using DotNetEnv;
using System.Security.Claims;
using PidrobitOK.ModeratorService.Repositories;

var builder = WebApplication.CreateBuilder(args);

var ownEnvironment = builder.Environment.EnvironmentName;
var connectionString = string.Empty;

if (builder.Environment.IsDevelopment())
{
    var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
    Env.Load(envPath);

    connectionString = $"Data Source=ARTEXXX;Database={Environment.GetEnvironmentVariable("SQL_DATABASE")};Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
}
else if (ownEnvironment == "DockerDevelopment")
{
    connectionString = $"Server={Environment.GetEnvironmentVariable("SQL_SERVER")};" +
    $"Database={Environment.GetEnvironmentVariable("SQL_DATABASE")};" +
    $"User Id={Environment.GetEnvironmentVariable("SQL_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("SQL_PASSWORD")};" +
    "TrustServerCertificate=True";
}

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

builder.Services.AddDbContext<ModerationDbContext>(options =>
    options.UseSqlServer(connectionString));

IMapper mapper = MapperProfile.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IJobModerationRepository, JobModerationRepository>();
builder.Services.AddScoped<IModerationLogRepository, ModerationLogRepository>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JobService", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Format: \"Bearer {your JWT token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    if (builder.Environment.EnvironmentName == "DockerDevelopment")
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ModerationDbContext>();
        if (dbContext != null)
        {
            dbContext.Database.Migrate();
        }
        else
        {
            throw new Exception("An error occurred while migrating a database to PidrobitOK.AuthService. DbContext is null");
        }
    }
}

app.Run();