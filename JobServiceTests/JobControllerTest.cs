using AutoMapper;
using JobService.Controllers;
using JobService.Models;
using JobService.Models.Dto;
using JobService.Repositories.JobsRepository;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace JobService.Tests.Controllers
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
            // Arrange
            var jobId = Guid.NewGuid();
            var jobDto = new JobDto { Id = jobId };
            _repoMock.Setup(r => r.Retrieve(jobId)).ReturnsAsync(jobDto);

            // Act
            var result = await _controller.Get(jobId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(jobDto, okResult.Value);
        }

        [Fact]
        public async Task GetAll_ReturnsListOfJobs()
        {
            // Arrange
            var jobs = new List<JobDto> { new JobDto { Id = Guid.NewGuid() } };
            _repoMock.Setup(r => r.Retrieve()).ReturnsAsync(jobs);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(jobs, okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedJob()
        {
            // Arrange
            var createDto = new CreateJobDto { Title = "Test Job" };
            var jobDto = new JobDto { Id = Guid.NewGuid(), Title = "Test Job" };

            _mapperMock.Setup(m => m.Map<JobDto>(createDto)).Returns(jobDto);
            _repoMock.Setup(r => r.Insert(jobDto)).ReturnsAsync(jobDto);
            _mapperMock.Setup(m => m.Map<JobDto>(jobDto)).Returns(jobDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(jobDto, okResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsUpdatedJob()
        {
            // Arrange
            var updateDto = new UpdateJobDto { Id = Guid.NewGuid(), Title = "Updated" };
            var jobDto = new JobDto { Id = updateDto.Id, Title = "Updated" };

            _mapperMock.Setup(m => m.Map<JobDto>(updateDto)).Returns(jobDto);
            _repoMock.Setup(r => r.Update(jobDto)).ReturnsAsync(jobDto);
            _mapperMock.Setup(m => m.Map<JobDto>(jobDto)).Returns(jobDto);

            // Act
            var result = await _controller.Update(updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(jobDto, okResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenUpdateFails()
        {
            // Arrange
            var updateDto = new UpdateJobDto { Id = Guid.NewGuid(), Title = "Updated" };
            var jobDto = new JobDto { Id = updateDto.Id, Title = "Updated" };

            _mapperMock.Setup(m => m.Map<JobDto>(updateDto)).Returns(jobDto);
            _repoMock.Setup(r => r.Update(jobDto)).ReturnsAsync((JobDto?)null);

            // Act
            var result = await _controller.Update(updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var deleted = true;
            _repoMock.Setup(r => r.Delete(id)).ReturnsAsync(deleted);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(deleted, okResult.Value);
        }
    }
}
