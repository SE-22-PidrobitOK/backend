using JobService.Models;
using JobService.Repositories.JobRequiredSkillsRepository;
using Microsoft.AspNetCore.Mvc;

namespace JobService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobRequiredSkillController : ControllerBase
    {
        private readonly IJobRequiredSkillRepository _repo;

        public JobRequiredSkillController(IJobRequiredSkillRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("retrieve-all")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _repo.Retrieve());

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] JobRequiredSkillDto dto) =>
            Ok(await _repo.Insert(dto));

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] Guid id) =>
            Ok(await _repo.Delete(id));
    }
}
