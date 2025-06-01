using AutoMapper;
using FluentAssertions;
using JobService.Controllers;
using JobService.Models;
using JobService.Models.Dto;
using JobService.Repositories.JobsRepository;
using JobServiceTests.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace JobServiceTests.Controllers
{
    public class JobControllerTests
    {
        private readonly Mock<IJobRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly JobController _controller;

        public JobControllerTests()
        {
            _repoMock = new Mock<IJobRepository>();
            _mapperMock = new Mock<IMapper>();
            _controller = new JobController(_repoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Get_ReturnsJobDto()
        {
            var id = Guid.NewGuid();
            var job = new JobDto { Id = id };

            _repoMock.Setup(r => r.Retrieve(id)).ReturnsAsync(job);

            var result = await _controller.Get(id);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(job);
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var jobs = new List<JobDto> { new JobDto { Id = Guid.NewGuid() } };
            _repoMock.Setup(r => r.Retrieve()).ReturnsAsync(jobs);

            var result = await _controller.GetAll();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(jobs);
        }

        [Fact]
        public async Task Create_ValidInput_ReturnsCreatedJob()
        {
            var dto = TestDataGenerator.GenerateCreateJobDto();
            var jobDto = new JobDto { Id = Guid.NewGuid(), Title = dto.Title };

            _mapperMock.Setup(m => m.Map<JobDto>(dto)).Returns(jobDto);
            _repoMock.Setup(r => r.Insert(jobDto)).ReturnsAsync(jobDto);
            _mapperMock.Setup(m => m.Map<JobDto>(jobDto)).Returns(jobDto);

            var result = await _controller.Create(dto);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(jobDto);
        }

        [Fact]
        public async Task Update_ReturnsUpdatedJob()
        {
            var dto = TestDataGenerator.GenerateUpdateJobDto(Guid.NewGuid());
            var jobDto = new JobDto { Id = dto.Id, Title = dto.Title };

            _mapperMock.Setup(m => m.Map<JobDto>(dto)).Returns(jobDto);
            _repoMock.Setup(r => r.Update(jobDto)).ReturnsAsync(jobDto);
            _mapperMock.Setup(m => m.Map<JobDto>(jobDto)).Returns(jobDto);

            var result = await _controller.Update(dto);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(jobDto);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNull()
        {
            var dto = TestDataGenerator.GenerateUpdateJobDto(Guid.NewGuid());
            var jobDto = new JobDto { Id = dto.Id, Title = dto.Title };

            _mapperMock.Setup(m => m.Map<JobDto>(dto)).Returns(jobDto);
            _repoMock.Setup(r => r.Update(jobDto)).ReturnsAsync((JobDto?)null);

            var result = await _controller.Update(dto);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_ReturnsTrue()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.Delete(id)).ReturnsAsync(true);

            var result = await _controller.Delete(id);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(true);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotFound()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.Delete(id)).ReturnsAsync(false);

            var result = await _controller.Delete(id);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(false);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenStatusClosedButExperienceTooLow()
        {
            var dto = TestDataGenerator.GenerateUpdateJobDto(Guid.NewGuid());
            dto.RequiredExperience = 1;
            dto.Status = JobStatus.Closed;

            var jobDto = new JobDto
            {
                Id = dto.Id,
                Title = dto.Title,
                RequiredExperience = dto.RequiredExperience,
                Status = dto.Status
            };

            _mapperMock.Setup(m => m.Map<JobDto>(dto)).Returns(jobDto);

            // Имитация бизнес-логики: репозиторий отказывается обновить
            _repoMock.Setup(r => r.Update(jobDto)).ReturnsAsync((JobDto?)null);

            var result = await _controller.Update(dto);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Get_ThrowsException_WhenRepositoryFails()
        {
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.Retrieve(id)).ThrowsAsync(new Exception("DB failure"));

            Func<Task> act = async () => await _controller.Get(id);

            await act.Should().ThrowAsync<Exception>().WithMessage("DB failure");
        }

        [Fact]
        public async Task Delete_BehavesDifferentlyOnRepeatedCalls()
        {
            var id = Guid.NewGuid();

            _repoMock.SetupSequence(r => r.Delete(id))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            var result1 = await _controller.Delete(id);
            var result2 = await _controller.Delete(id);

            var ok1 = result1.Should().BeOfType<OkObjectResult>().Subject;
            var ok2 = result2.Should().BeOfType<OkObjectResult>().Subject;

            ok1.Value.Should().BeOfType<bool>().Which.Should().BeTrue();
            ok2.Value.Should().BeOfType<bool>().Which.Should().BeFalse();
        }


        [Fact]
        public async Task Create_CallsInsertExactlyOnce()
        {
            var dto = TestDataGenerator.GenerateCreateJobDto();
            var jobDto = new JobDto { Id = Guid.NewGuid(), Title = dto.Title };

            _mapperMock.Setup(m => m.Map<JobDto>(dto)).Returns(jobDto);
            _repoMock.Setup(r => r.Insert(jobDto)).ReturnsAsync(jobDto);
            _mapperMock.Setup(m => m.Map<JobDto>(jobDto)).Returns(jobDto);

            var result = await _controller.Create(dto);

            _repoMock.Verify(r => r.Insert(It.IsAny<JobDto>()), Times.Once);
        }


    }
}
