using AutoMapper;
using FluentAssertions;
using JobService.Controllers;
using JobService.Models;
using JobService.Repositories;
using JobService.Repositories.JobRequiredSkillsRepository;
using JobService.Repositories.JobsRepository;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace JobServiceTests.Controllers
{
    public class JobRequiredSkillControllerTests
    {
        private readonly Mock<IJobRequiredSkillRepository> _repoMock;
        private readonly Mock<IJobRepository> _jobRepoMock;
        private readonly Mock<ISkillRepository> _skillRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly JobRequiredSkillController _controller;

        public JobRequiredSkillControllerTests()
        {
            _repoMock = new Mock<IJobRequiredSkillRepository>();
            _jobRepoMock = new Mock<IJobRepository>();
            _skillRepoMock = new Mock<ISkillRepository>();
            _mapperMock = new Mock<IMapper>();

            _controller = new JobRequiredSkillController(
                _repoMock.Object,
                _mapperMock.Object,
                _jobRepoMock.Object,
                _skillRepoMock.Object
            );
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var list = new List<JobRequiredSkillDto> {
                new JobRequiredSkillDto { Id = Guid.NewGuid(), JobId = Guid.NewGuid(), SkillId = Guid.NewGuid() }
            };

            _repoMock.Setup(r => r.Retrieve()).ReturnsAsync(list);

            var result = await _controller.GetAll();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenJobNotExists()
        {
            var dto = new CreateJobRequiredSkillDto { JobId = Guid.NewGuid(), SkillId = Guid.NewGuid() };

            _jobRepoMock.Setup(r => r.Retrieve(dto.JobId)).ReturnsAsync((JobDto?)null);

            var result = await _controller.Create(dto);

            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().Be($"Job with ID {dto.JobId} not found");
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenSkillNotExists()
        {
            var dto = new CreateJobRequiredSkillDto { JobId = Guid.NewGuid(), SkillId = Guid.NewGuid() };

            _jobRepoMock.Setup(r => r.Retrieve(dto.JobId)).ReturnsAsync(new JobDto());
            _skillRepoMock.Setup(r => r.Retrieve(dto.SkillId)).ReturnsAsync((SkillDto?)null);

            var result = await _controller.Create(dto);

            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().Be($"Skill with ID {dto.SkillId} not found");
        }

        [Fact]
        public async Task Create_ReturnsConflict_WhenAlreadyExists()
        {
            var dto = new CreateJobRequiredSkillDto { JobId = Guid.NewGuid(), SkillId = Guid.NewGuid() };

            _jobRepoMock.Setup(r => r.Retrieve(dto.JobId)).ReturnsAsync(new JobDto());
            _skillRepoMock.Setup(r => r.Retrieve(dto.SkillId)).ReturnsAsync(new SkillDto());
            _repoMock.Setup(r => r.Exists(dto.JobId, dto.SkillId)).ReturnsAsync(true);

            var result = await _controller.Create(dto);

            var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflict.Value.Should().Be("This job already requires this skill.");
        }

        [Fact]
        public async Task Create_ReturnsCreatedItem_WhenValid()
        {
            var dto = new CreateJobRequiredSkillDto { JobId = Guid.NewGuid(), SkillId = Guid.NewGuid() };
            var mapped = new JobRequiredSkillDto { Id = Guid.NewGuid(), JobId = dto.JobId, SkillId = dto.SkillId };

            _jobRepoMock.Setup(r => r.Retrieve(dto.JobId)).ReturnsAsync(new JobDto());
            _skillRepoMock.Setup(r => r.Retrieve(dto.SkillId)).ReturnsAsync(new SkillDto());
            _repoMock.Setup(r => r.Exists(dto.JobId, dto.SkillId)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<JobRequiredSkillDto>(dto)).Returns(mapped);
            _repoMock.Setup(r => r.Insert(mapped)).ReturnsAsync(mapped);

            var result = await _controller.Create(dto);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task Delete_ReturnsTrue()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.Delete(id)).ReturnsAsync(true);

            var result = await _controller.Delete(id);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeOfType<bool>().Which.Should().BeTrue();
        }


        [Fact]
        public async Task GetSkillsByJob_ReturnsList()
        {
            var jobId = Guid.NewGuid();
            var list = new List<SkillDto> { new SkillDto { SkillId = Guid.NewGuid(), Title = "C#", Description = "..." } };

            _repoMock.Setup(r => r.GetSkillsByJobId(jobId)).ReturnsAsync(list);

            var result = await _controller.GetSkillsByJob(jobId);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetJobsBySkill_ReturnsList()
        {
            var skillId = Guid.NewGuid();
            var list = new List<JobDto> { new JobDto { Id = Guid.NewGuid(), Title = "Backend Dev" } };

            _repoMock.Setup(r => r.GetJobsBySkillId(skillId)).ReturnsAsync(list);

            var result = await _controller.GetJobsBySkill(skillId);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetAll_ThrowsException_WhenRepoFails()
        {
            _repoMock.Setup(r => r.Retrieve()).ThrowsAsync(new Exception("DB error"));

            Func<Task> act = async () => await _controller.GetAll();

            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        [Fact]
        public async Task Create_ThrowsException_WhenInsertFails()
        {
            var dto = new CreateJobRequiredSkillDto { JobId = Guid.NewGuid(), SkillId = Guid.NewGuid() };
            var mapped = new JobRequiredSkillDto { Id = Guid.NewGuid(), JobId = dto.JobId, SkillId = dto.SkillId };

            _jobRepoMock.Setup(r => r.Retrieve(dto.JobId)).ReturnsAsync(new JobDto());
            _skillRepoMock.Setup(r => r.Retrieve(dto.SkillId)).ReturnsAsync(new SkillDto());
            _repoMock.Setup(r => r.Exists(dto.JobId, dto.SkillId)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<JobRequiredSkillDto>(dto)).Returns(mapped);
            _repoMock.Setup(r => r.Insert(mapped)).ThrowsAsync(new Exception("Insert failed"));

            Func<Task> act = async () => await _controller.Create(dto);

            await act.Should().ThrowAsync<Exception>().WithMessage("Insert failed");
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
        public async Task Delete_ThrowsException_WhenRepoFails()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.Delete(id)).ThrowsAsync(new Exception("Delete error"));

            Func<Task> act = async () => await _controller.Delete(id);

            await act.Should().ThrowAsync<Exception>().WithMessage("Delete error");
        }

        [Fact]
        public async Task GetSkillsByJob_ReturnsEmptyList_WhenNoSkillsFound()
        {
            var jobId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetSkillsByJobId(jobId)).ReturnsAsync(new List<SkillDto>());

            var result = await _controller.GetSkillsByJob(jobId);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeAssignableTo<IEnumerable<SkillDto>>().Which.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSkillsByJob_ThrowsException_WhenRepoFails()
        {
            var jobId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetSkillsByJobId(jobId)).ThrowsAsync(new Exception("Skill lookup failed"));

            Func<Task> act = async () => await _controller.GetSkillsByJob(jobId);

            await act.Should().ThrowAsync<Exception>().WithMessage("Skill lookup failed");
        }

        [Fact]
        public async Task GetJobsBySkill_ReturnsEmptyList_WhenNoJobsFound()
        {
            var skillId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetJobsBySkillId(skillId)).ReturnsAsync(new List<JobDto>());

            var result = await _controller.GetJobsBySkill(skillId);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeAssignableTo<IEnumerable<JobDto>>().Which.Should().BeEmpty();
        }

        [Fact]
        public async Task GetJobsBySkill_ThrowsException_WhenRepoFails()
        {
            var skillId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetJobsBySkillId(skillId)).ThrowsAsync(new Exception("Job lookup failed"));

            Func<Task> act = async () => await _controller.GetJobsBySkill(skillId);

            await act.Should().ThrowAsync<Exception>().WithMessage("Job lookup failed");
        }



        [Fact]
        public async Task Create_ReturnsOkWithNull_WhenInsertReturnsNull()
        {
            var dto = new CreateJobRequiredSkillDto { JobId = Guid.NewGuid(), SkillId = Guid.NewGuid() };
            var mapped = new JobRequiredSkillDto { JobId = dto.JobId, SkillId = dto.SkillId };

            _jobRepoMock.Setup(r => r.Retrieve(dto.JobId)).ReturnsAsync(new JobDto());
            _skillRepoMock.Setup(r => r.Retrieve(dto.SkillId)).ReturnsAsync(new SkillDto());
            _repoMock.Setup(r => r.Exists(dto.JobId, dto.SkillId)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<JobRequiredSkillDto>(dto)).Returns(mapped);
            _repoMock.Setup(r => r.Insert(mapped)).ReturnsAsync((JobRequiredSkillDto?)null);

            var result = await _controller.Create(dto);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsTrueThenFalse_WhenCalledTwice()
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
        public async Task GetAll_ReturnsNull_WhenRepoReturnsNull()
        {
            _repoMock.Setup(r => r.Retrieve()).ReturnsAsync((List<JobRequiredSkillDto>?)null);

            var result = await _controller.GetAll();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeNull();
        }

        [Fact]
        public async Task Create_CallsInsertExactlyOnce()
        {
            var dto = new CreateJobRequiredSkillDto { JobId = Guid.NewGuid(), SkillId = Guid.NewGuid() };
            var mapped = new JobRequiredSkillDto { Id = Guid.NewGuid(), JobId = dto.JobId, SkillId = dto.SkillId };

            _jobRepoMock.Setup(r => r.Retrieve(dto.JobId)).ReturnsAsync(new JobDto());
            _skillRepoMock.Setup(r => r.Retrieve(dto.SkillId)).ReturnsAsync(new SkillDto());
            _repoMock.Setup(r => r.Exists(dto.JobId, dto.SkillId)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<JobRequiredSkillDto>(dto)).Returns(mapped);
            _repoMock.Setup(r => r.Insert(mapped)).ReturnsAsync(mapped);

            await _controller.Create(dto);

            _repoMock.Verify(r => r.Insert(It.IsAny<JobRequiredSkillDto>()), Times.Once);
        }


    }
}
