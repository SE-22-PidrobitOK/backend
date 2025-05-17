using AutoMapper;
using JobService.Models;
using JobService.Repositories.JobsRepository;
using Microsoft.AspNetCore.Mvc;

namespace JobService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobRepository _repo;
        public JobController(IJobRepository repo) { _repo = repo; }

        // GET api/job/retrieve?id=...
        [HttpGet("retrieve")]
        public async Task<IActionResult> Get([FromQuery] Guid id) =>
            Ok(await _repo.Retrieve(id));

        // GET api/job/retrieve-all
        [HttpGet("retrieve-all")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _repo.Retrieve());

        // POST api/job/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] JobDto dto) =>
            Ok(await _repo.Insert(dto));

        // POST api/job/update
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] JobDto dto) =>
            Ok(await _repo.Update(dto));

        // POST api/job/delete
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] Guid id) =>
            Ok(await _repo.Delete(id));
    }
}
