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
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace JobServiceTests.Integration
{
    public class JobServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public JobServiceIntegrationTests(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("TestDb"));

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.EnsureCreated();
                });
            });
        }

        [Fact]
        public async Task CreateJob_ThenRetrieveAll_ShouldContainCreated()
        {
            var client = _factory.CreateClient();

            var createDto = new CreateJobDto
            {
                Title = "Integration QA",
                Description = "Testing JobService",
                Location = "Remote",
                Salary = 3500,
                Type = JobType.FullTime,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 2
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");

            var postResponse = await client.PostAsync("/api/job/create", content);
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var created = await postResponse.Content.ReadFromJsonAsync<JobDto>(options);
            created.Should().NotBeNull();
            created!.Title.Should().Be("Integration QA");

            var getResponse = await client.GetAsync("/api/job/retrieve-all");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var jobs = await getResponse.Content.ReadFromJsonAsync<List<JobDto>>(options);
            jobs.Should().ContainSingle(j => j.Id == created.Id);
        }

        [Fact]
        public async Task CreateJob_ShouldAcceptMinimalValidPayload()
        {
            var client = _factory.CreateClient();

            var minimalValid = new CreateJobDto
            {
                Title = "Valid",
                EmployerId = Guid.NewGuid(),
                Type = JobType.FullTime,
                RequiredExperience = 0
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(minimalValid, options), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/job/create", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RetrieveJob_ShouldReturnNoContent_WhenJobNotExists()
        {
            var client = _factory.CreateClient();

            var nonExistingId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/job/retrieve?id={nonExistingId}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateJob_ShouldModifyJobCorrectly()
        {
            var client = _factory.CreateClient();

            var createDto = new CreateJobDto
            {
                Title = "Original",
                Description = "Desc",
                Location = "Remote",
                Salary = 1000,
                Type = JobType.PartTime,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 1
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var createContent = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");
            var createResponse = await client.PostAsync("/api/job/create", createContent);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var created = await createResponse.Content.ReadFromJsonAsync<JobDto>(options);

            var updateDto = new UpdateJobDto
            {
                Id = created!.Id,
                Title = "Updated Title",
                Description = "Updated Desc",
                Location = "Office",
                Salary = 2000,
                Type = JobType.FullTime,
                EmployerId = created.EmployerId,
                Status = JobStatus.Closed,
                RequiredExperience = 3
            };

            var updateContent = new StringContent(JsonSerializer.Serialize(updateDto, options), Encoding.UTF8, "application/json");
            var updateResponse = await client.PostAsync("/api/job/update", updateContent);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await updateResponse.Content.ReadFromJsonAsync<JobDto>(options);
            updated.Should().NotBeNull();
            updated!.Title.Should().Be("Updated Title");
            updated.Status.Should().Be(JobStatus.Closed);
        }

        [Fact]
        public async Task UpdateJob_ShouldReturnNotFound_WhenJobDoesNotExist()
        {
            var client = _factory.CreateClient();

            var updateDto = new UpdateJobDto
            {
                Id = Guid.NewGuid(), // non-existing
                Title = "Doesn't Matter",
                Description = "Nope",
                Location = "Nowhere",
                Salary = 1000,
                Type = JobType.Internship,
                EmployerId = Guid.NewGuid(),
                Status = JobStatus.Open,
                RequiredExperience = 0
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(updateDto, options), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/job/update", content);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteJob_ShouldRemoveSuccessfully()
        {
            var client = _factory.CreateClient();

            var createDto = new CreateJobDto
            {
                Title = "To be deleted",
                Description = "Test",
                Location = "X",
                Salary = 500,
                Type = JobType.Internship,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 0
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var createContent = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");
            var createResponse = await client.PostAsync("/api/job/create", createContent);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var created = await createResponse.Content.ReadFromJsonAsync<JobDto>(options);

            var deleteContent = new StringContent(JsonSerializer.Serialize(created!.Id, options), Encoding.UTF8, "application/json");
            var deleteResponse = await client.PostAsync("/api/job/delete", deleteContent);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var deleted = await deleteResponse.Content.ReadFromJsonAsync<bool>(options);
            deleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteJob_ShouldReturnFalse_WhenJobDoesNotExist()
        {
            var client = _factory.CreateClient();
            var nonExistentId = Guid.NewGuid();

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(nonExistentId, options), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/job/delete", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<bool>(options);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RetrieveJob_ShouldReturnJobById()
        {
            var client = _factory.CreateClient();

            var createDto = new CreateJobDto
            {
                Title = "Retrievable",
                Description = "Test",
                Location = "Anywhere",
                Salary = 999,
                Type = JobType.PartTime,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 5
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var createContent = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");
            var createResponse = await client.PostAsync("/api/job/create", createContent);
            var created = await createResponse.Content.ReadFromJsonAsync<JobDto>(options);

            var response = await client.GetAsync($"/api/job/retrieve?id={created!.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var job = await response.Content.ReadFromJsonAsync<JobDto>(options);
            job.Should().NotBeNull();
            job!.Id.Should().Be(created.Id);
        }

        [Fact]
        public async Task RetrieveAllJobs_ShouldReturnAll()
        {
            var client = _factory.CreateClient();

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jobsToCreate = new[]
            {
                new CreateJobDto { Title = "Job1", EmployerId = Guid.NewGuid(), Type = JobType.FullTime, RequiredExperience = 1 },
                new CreateJobDto { Title = "Job2", EmployerId = Guid.NewGuid(), Type = JobType.Internship, RequiredExperience = 0 },
                new CreateJobDto { Title = "Job3", EmployerId = Guid.NewGuid(), Type = JobType.PartTime, RequiredExperience = 2 }
            };

            foreach (var dto in jobsToCreate)
            {
                var content = new StringContent(JsonSerializer.Serialize(dto, options), Encoding.UTF8, "application/json");
                await client.PostAsync("/api/job/create", content);
            }

            var response = await client.GetAsync("/api/job/retrieve-all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var allJobs = await response.Content.ReadFromJsonAsync<List<JobDto>>(options);
            allJobs.Should().NotBeNull();
            allJobs!.Count.Should().BeGreaterOrEqualTo(3);
        }

        [Fact]
        public async Task CreateJob_ShouldReturnCreated_WithMinimalFields()
        {
            var client = _factory.CreateClient();
            var createDto = new CreateJobDto
            {
                Title = "Minimalist Job",
                Type = JobType.PartTime,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 0
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/job/create", content);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var created = await response.Content.ReadFromJsonAsync<JobDto>(options);
            created.Should().NotBeNull();
            created!.Title.Should().Be("Minimalist Job");
            created.Type.Should().Be(JobType.PartTime);
        }

        [Fact]
        public async Task CreateJob_ShouldDefaultToOpenStatus()
        {
            var client = _factory.CreateClient();
            var createDto = new CreateJobDto
            {
                Title = "Default Status Job",
                Type = JobType.FullTime,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 1
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/job/create", content);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var created = await response.Content.ReadFromJsonAsync<JobDto>(options);
            created.Should().NotBeNull();
            created!.Status.Should().Be(JobStatus.Open);
        }

        [Fact]
        public async Task UpdateJob_ShouldReturnUpdatedFields()
        {
            var client = _factory.CreateClient();
            var createDto = new CreateJobDto
            {
                Title = "Job Before Update",
                Type = JobType.FullTime,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 1
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");
            var postResponse = await client.PostAsync("/api/job/create", content);
            var created = await postResponse.Content.ReadFromJsonAsync<JobDto>(options);

            var updateDto = new UpdateJobDto
            {
                Id = created!.Id,
                Title = "Updated Job",
                Description = "Now with details",
                Location = "Office",
                Salary = 9999,
                Type = JobType.FullTime,
                EmployerId = created.EmployerId,
                RequiredExperience = 2,
                Status = JobStatus.Closed
            };

            var updateContent = new StringContent(JsonSerializer.Serialize(updateDto, options), Encoding.UTF8, "application/json");
            var updateResponse = await client.PostAsync("/api/job/update", updateContent);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await updateResponse.Content.ReadFromJsonAsync<JobDto>(options);
            updated!.Title.Should().Be("Updated Job");
            updated.Description.Should().Be("Now with details");
            updated.Status.Should().Be(JobStatus.Closed);
        }

        [Fact]
        public async Task UpdateJob_ShouldReturnNotFound_IfIdInvalid()
        {
            var client = _factory.CreateClient();
            var updateDto = new UpdateJobDto
            {
                Id = Guid.NewGuid(),
                Title = "Non-existent",
                Type = JobType.PartTime,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 3,
                Status = JobStatus.Open
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(updateDto, options), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/job/update", content);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteJob_ShouldRemoveJob()
        {
            var client = _factory.CreateClient();
            var createDto = new CreateJobDto
            {
                Title = "To Be Deleted",
                Type = JobType.Internship,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 0
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");
            var postResponse = await client.PostAsync("/api/job/create", content);
            var created = await postResponse.Content.ReadFromJsonAsync<JobDto>(options);

            var deleteContent = new StringContent(JsonSerializer.Serialize(created!.Id), Encoding.UTF8, "application/json");
            var deleteResponse = await client.PostAsync("/api/job/delete", deleteContent);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var getResponse = await client.GetAsync($"/api/job/retrieve?id={created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task CreateJob_StressTest_HandlesMultipleRequests()
        {
            var client = _factory.CreateClient();
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < 1000; i++)
            {
                var dto = new CreateJobDto
                {
                    Title = $"Stress Test #{i}",
                    Description = "Load test",
                    Location = "Remote",
                    Salary = 1000 + i,
                    Type = JobType.PartTime,
                    EmployerId = Guid.NewGuid(),
                    RequiredExperience = i % 5
                };

                var content = new StringContent(JsonSerializer.Serialize(dto, options), Encoding.UTF8, "application/json");
                tasks.Add(client.PostAsync("/api/job/create", content));
            }

            var responses = await Task.WhenAll(tasks);

            foreach (var response in responses)
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            var allJobsResponse = await client.GetAsync("/api/job/retrieve-all");
            allJobsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var jobs = await allJobsResponse.Content.ReadFromJsonAsync<List<JobDto>>(options);
            jobs.Count(j => j.Title.StartsWith("Stress Test")).Should().Be(1000);

        }

        [Fact]
        public async Task CreateJob_WithInvalidEnum_DoesNotThrow_ButMayProduceUnexpectedType()
        {
            var client = _factory.CreateClient();

            var invalidPayload = @"{
        ""title"": ""Invalid Enum"",
        ""type"": 999,
        ""employerId"": ""00000000-0000-0000-0000-000000000001"",
        ""requiredExperience"": 1
    }";

            var content = new StringContent(invalidPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/job/create", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseText = await response.Content.ReadAsStringAsync();
            responseText.Should().Contain("999");
        }


        [Fact]
        public async Task UpdateJob_ShouldPreserveCreatedAtField()
        {
            var client = _factory.CreateClient();

            var createDto = new CreateJobDto
            {
                Title = "Created Timestamp Test",
                Type = JobType.FullTime,
                EmployerId = Guid.NewGuid(),
                RequiredExperience = 1
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var createContent = new StringContent(JsonSerializer.Serialize(createDto, options), Encoding.UTF8, "application/json");
            var createResponse = await client.PostAsync("/api/job/create", createContent);
            var created = await createResponse.Content.ReadFromJsonAsync<JobDto>(options);

            var updateDto = new UpdateJobDto
            {
                Id = created!.Id,
                Title = "Updated",
                Description = "Updated",
                Location = "Updated",
                Salary = 9999,
                Type = JobType.FullTime,
                EmployerId = created.EmployerId,
                Status = JobStatus.Open,
                RequiredExperience = 2
            };

            var updateContent = new StringContent(JsonSerializer.Serialize(updateDto, options), Encoding.UTF8, "application/json");
            await client.PostAsync("/api/job/update", updateContent);

            var getResponse = await client.GetAsync($"/api/job/retrieve?id={created.Id}");
            var updated = await getResponse.Content.ReadFromJsonAsync<JobDto>(options);

            updated!.Id.Should().Be(created.Id);
            updated!.Title.Should().Be("Updated");
        }

        [Fact]
        public async Task CreateJob_ShouldRejectMissingTitle()
        {
            var client = _factory.CreateClient();

            var invalidDto = new
            {
                type = JobType.PartTime,
                employerId = Guid.NewGuid(),
                requiredExperience = 0
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var content = new StringContent(JsonSerializer.Serialize(invalidDto, options), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/job/create", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }




    }
}
