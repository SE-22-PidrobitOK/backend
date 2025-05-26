﻿using AutoMapper;
using JobService.Models;
using JobService.Models.Dto;
using JobService.Repositories.JobsRepository;
using Microsoft.AspNetCore.Mvc;

namespace JobService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly IJobRepository _repo;
        public JobController(IJobRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

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
        public async Task<IActionResult> Create([FromBody] CreateJobDto dto)
        {
            var job = _mapper.Map<Job>(dto);
            var created = await _repo.Insert(job);
            return Ok(_mapper.Map<JobDto>(created));
        }

        // POST api/job/update
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateJobDto dto)
        {
            var job = _mapper.Map<Job>(dto);
            var updated = await _repo.Update(job);
            if (updated == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<JobDto>(updated));
        }

        // POST api/job/delete
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] Guid id) =>
            Ok(await _repo.Delete(id));
    }
}
