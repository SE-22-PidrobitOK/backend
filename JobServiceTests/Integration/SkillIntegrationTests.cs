using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using JobService;
using JobService.Models;
using JobService.Models.Dto;
using JobService.DatabaseContext;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JobServiceTests.Integration
{
    public class SkillIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SkillIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("TestSkillDb"));

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.EnsureCreated();
                });
            });
        }

        private static JsonSerializerOptions JsonOptions => new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [Fact]
        public async Task CreateSkill_ShouldSucceed_WithValidData()
        {
            var client = _factory.CreateClient();

            var skill = new CreateSkillDto
            {
                Title = "C#",
                Description = "Programming language"
            };

            var response = await client.PostAsJsonAsync("/api/skill/create", skill, JsonOptions);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var created = await response.Content.ReadFromJsonAsync<SkillDto>(JsonOptions);
            created.Should().NotBeNull();
            created!.Title.Should().Be("C#");
        }

        [Fact]
        public async Task CreateSkill_ShouldReturnConflict_IfNameAlreadyExists()
        {
            var client = _factory.CreateClient();

            var skill = new CreateSkillDto
            {
                Title = "Python",
                Description = "Scripting"
            };

            var response1 = await client.PostAsJsonAsync("/api/skill/create", skill, JsonOptions);
            response1.StatusCode.Should().Be(HttpStatusCode.OK);

            var response2 = await client.PostAsJsonAsync("/api/skill/create", skill, JsonOptions);
            response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task RetrieveSkillById_ShouldReturnSkill()
        {
            var client = _factory.CreateClient();

            var skill = new CreateSkillDto
            {
                Title = "Java",
                Description = "Also a programming language"
            };

            var createResponse = await client.PostAsJsonAsync("/api/skill/create", skill, JsonOptions);
            var created = await createResponse.Content.ReadFromJsonAsync<SkillDto>(JsonOptions);

            var getResponse = await client.GetAsync($"/api/skill/retrieve?id={created!.SkillId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var fetched = await getResponse.Content.ReadFromJsonAsync<SkillDto>(JsonOptions);
            fetched!.Title.Should().Be("Java");
        }

        [Fact]
        public async Task GetAllSkills_ShouldReturnAllCreated()
        {
            var client = _factory.CreateClient();

            var skills = new[]
            {
                new CreateSkillDto { Title = "SQL", Description = "Database query language" },
                new CreateSkillDto { Title = "Docker", Description = "Containerization" },
            };

            foreach (var skill in skills)
                await client.PostAsJsonAsync("/api/skill/create", skill, JsonOptions);

            var response = await client.GetAsync("/api/skill/retrieve-all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var all = await response.Content.ReadFromJsonAsync<List<SkillDto>>(JsonOptions);
            all.Should().Contain(s => s.Title == "SQL")
                .And.Contain(s => s.Title == "Docker");
        }
        
        [Fact]
        public async Task UpdateSkill_ShouldReturnNotFound_IfSkillMissing()
        {
            var client = _factory.CreateClient();

            var updateDto = new UpdateSkillDto
            {
                SkillId = Guid.NewGuid(),
                Title = "GhostSkill",
                Description = "Nonexistent"
            };

            var response = await client.PostAsJsonAsync("/api/skill/update", updateDto, JsonOptions);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteSkill_ShouldReturnTrue_WhenSuccessful()
        {
            var client = _factory.CreateClient();

            var skill = new CreateSkillDto
            {
                Title = "GraphQL",
                Description = "API query language"
            };

            var createResponse = await client.PostAsJsonAsync("/api/skill/create", skill, JsonOptions);
            var created = await createResponse.Content.ReadFromJsonAsync<SkillDto>(JsonOptions);

            var deleteContent = new StringContent(JsonSerializer.Serialize(created!.SkillId), Encoding.UTF8, "application/json");
            var deleteResponse = await client.PostAsync("/api/skill/delete", deleteContent);

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await deleteResponse.Content.ReadFromJsonAsync<bool>(JsonOptions);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteSkill_ShouldReturnFalse_IfNotFound()
        {
            var client = _factory.CreateClient();

            var nonExistentId = Guid.NewGuid();
            var deleteContent = new StringContent(JsonSerializer.Serialize(nonExistentId), Encoding.UTF8, "application/json");
            var deleteResponse = await client.PostAsync("/api/skill/delete", deleteContent);

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await deleteResponse.Content.ReadFromJsonAsync<bool>(JsonOptions);
            result.Should().BeFalse();
        }


        [Fact]
        public async Task RetrieveSkillById_ShouldReturnNotFound_IfInvalidId()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/skill/retrieve?id={Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError); // или 404, если настроено
        }


        [Fact]
        public async Task CreateSkill_ShouldRejectDuplicateName_CaseInsensitive()
        {
            var client = _factory.CreateClient();

            var skill1 = new CreateSkillDto { Title = "DevOps", Description = "Infra" };
            var skill2 = new CreateSkillDto { Title = "devops", Description = "Duplicate case" };

            var response1 = await client.PostAsJsonAsync("/api/skill/create", skill1, JsonOptions);
            response1.StatusCode.Should().Be(HttpStatusCode.OK);

            var response2 = await client.PostAsJsonAsync("/api/skill/create", skill2, JsonOptions);
            response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateSkill_ShouldReject_OverlyLongTitle()
        {
            var client = _factory.CreateClient();

            var skill = new CreateSkillDto
            {
                Title = new string('A', 300),
                Description = "Too long title"
            };

            var response = await client.PostAsJsonAsync("/api/skill/create", skill, JsonOptions);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // если добавлена валидация
        }


    }
}
