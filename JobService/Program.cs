using AutoMapper;
using JobService;
using JobService.DatabaseContext;
using JobService.Repositories;
using JobService.Repositories.JobRequiredSkillsRepository;
using JobService.Repositories.JobsRepository;
using JobService.Repositories.SkillsRepository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Override database connection
builder.Configuration["ConnectionStrings:DefaultConnection"] =
    $"Server={Environment.GetEnvironmentVariable("SQL_SERVER")};" +
    $"Database={Environment.GetEnvironmentVariable("AUTH_DATABASE")};" +
    $"User Id={Environment.GetEnvironmentVariable("SQL_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("SQL_PASSWORD")};" +
    "TrustServerCertificate=True";

// 1. EF
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobRequiredSkillRepository, JobRequiredSkillRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
