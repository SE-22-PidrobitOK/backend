using AutoMapper;
using FluentAssertions;
using JobService.Controllers;
using JobService.Models;
using JobService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace JobServiceTests.Controllers
{
    public class SkillControllerTests
    {
        private readonly Mock<ISkillRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SkillController _controller;

        public SkillControllerTests()
        {
            _repoMock = new Mock<ISkillRepository>();
            _mapperMock = new Mock<IMapper>();
            _controller = new SkillController(_mapperMock.Object, _repoMock.Object);
        }

        [Fact]
        public async Task Get_ReturnsSkill()
        {
            var id = Guid.NewGuid();
            var skill = new SkillDto { SkillId = id, Title = "C#", Description = "desc" };

            _repoMock.Setup(r => r.Retrieve(id)).ReturnsAsync(skill);

            var result = await _controller.Get(id);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(skill);
        }

        [Fact]
        public async Task Get_ThrowsException_WhenRepoFails()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.Retrieve(id)).ThrowsAsync(new Exception("DB failed"));

            Func<Task> act = async () => await _controller.Get(id);

            await act.Should().ThrowAsync<Exception>().WithMessage("DB failed");
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var skills = new List<SkillDto> { new SkillDto { SkillId = Guid.NewGuid(), Title = "JS" } };
            _repoMock.Setup(r => r.Retrieve()).ReturnsAsync(skills);

            var result = await _controller.GetAll();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(skills);
        }

        [Fact]
        public async Task Create_ReturnsConflict_WhenSkillExists()
        {
            var dto = new CreateSkillDto { Title = "Duplicate", Description = "..." };
            _repoMock.Setup(r => r.ExistsByName(dto.Title)).ReturnsAsync(true);

            var result = await _controller.Create(dto);

            var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflict.Value.Should().Be("Skill with this name already exists.");
        }

        [Fact]
        public async Task Create_ReturnsCreatedSkill()
        {
            var dto = new CreateSkillDto { Title = "New", Description = "desc" };
            var skill = new SkillDto { SkillId = Guid.NewGuid(), Title = dto.Title, Description = dto.Description };

            _repoMock.Setup(r => r.ExistsByName(dto.Title)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<SkillDto>(dto)).Returns(skill);
            _repoMock.Setup(r => r.Insert(skill)).ReturnsAsync(skill);

            var result = await _controller.Create(dto);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(skill);
        }

        [Fact]
        public async Task Create_ThrowsException_WhenInsertFails()
        {
            var dto = new CreateSkillDto { Title = "X", Description = "..." };
            var skill = new SkillDto { Title = dto.Title, Description = dto.Description };

            _repoMock.Setup(r => r.ExistsByName(dto.Title)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<SkillDto>(dto)).Returns(skill);
            _repoMock.Setup(r => r.Insert(skill)).ThrowsAsync(new Exception("Insert failed"));

            Func<Task> act = async () => await _controller.Create(dto);

            await act.Should().ThrowAsync<Exception>().WithMessage("Insert failed");
        }

        [Fact]
        public async Task Update_ReturnsUpdatedSkill()
        {
            var dto = new UpdateSkillDto { SkillId = Guid.NewGuid(), Title = "Updated", Description = "..." };
            var skill = new SkillDto { SkillId = dto.SkillId, Title = dto.Title, Description = dto.Description };

            _mapperMock.Setup(m => m.Map<SkillDto>(dto)).Returns(skill);
            _repoMock.Setup(r => r.Update(skill)).ReturnsAsync(skill);

            var result = await _controller.Update(dto);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(skill);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNull()
        {
            var dto = new UpdateSkillDto { SkillId = Guid.NewGuid(), Title = "Updated", Description = "..." };
            var skill = new SkillDto { SkillId = dto.SkillId, Title = dto.Title, Description = dto.Description };

            _mapperMock.Setup(m => m.Map<SkillDto>(dto)).Returns(skill);
            _repoMock.Setup(r => r.Update(skill)).ReturnsAsync((SkillDto?)null);

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
            ok.Value.Should().BeOfType<bool>().Which.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenNotFound()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.Delete(id)).ReturnsAsync(false);

            var result = await _controller.Delete(id);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeOfType<bool>().Which.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_ThrowsException_WhenRepoFails()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.Delete(id)).ThrowsAsync(new Exception("Delete failed"));

            Func<Task> act = async () => await _controller.Delete(id);

            await act.Should().ThrowAsync<Exception>().WithMessage("Delete failed");
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoSkillsExist()
        {
            _repoMock.Setup(r => r.Retrieve()).ReturnsAsync(new List<SkillDto>());

            var result = await _controller.GetAll();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeAssignableTo<IEnumerable<SkillDto>>().Which.Should().BeEmpty();
        }


        [Fact]
        public async Task Update_Throws_WhenRepoFails()
        {
            var dto = new UpdateSkillDto { SkillId = Guid.NewGuid(), Title = "X", Description = "..." };
            var skill = new SkillDto { SkillId = dto.SkillId, Title = dto.Title, Description = dto.Description };

            _mapperMock.Setup(m => m.Map<SkillDto>(dto)).Returns(skill);
            _repoMock.Setup(r => r.Update(skill)).ThrowsAsync(new Exception("Update failed"));

            Func<Task> act = async () => await _controller.Update(dto);

            await act.Should().ThrowAsync<Exception>().WithMessage("Update failed");
        }

        [Fact]
        public async Task Create_Fails_WhenTitleIsEmpty()
        {
            var dto = new CreateSkillDto { Title = "", Description = "..." };

            _repoMock.Setup(r => r.ExistsByName(dto.Title)).ReturnsAsync(false);

            // Обычно маппер упадёт, либо результат будет невалидный
            _mapperMock.Setup(m => m.Map<SkillDto>(dto)).Throws<ArgumentException>();

            Func<Task> act = async () => await _controller.Create(dto);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Update_Fails_WhenSkillIdIsEmpty()
        {
            var dto = new UpdateSkillDto { SkillId = Guid.Empty, Title = "Some", Description = "..." };
            var skill = new SkillDto { SkillId = Guid.Empty, Title = "Some", Description = "..." };

            _mapperMock.Setup(m => m.Map<SkillDto>(dto)).Returns(skill);
            _repoMock.Setup(r => r.Update(skill)).ReturnsAsync((SkillDto?)null);

            var result = await _controller.Update(dto);

            result.Should().BeOfType<NotFoundResult>();
        }

    }
}
