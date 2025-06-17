using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PidrobitOK.ModeratorService.Models.DTO;
using PidrobitOK.ModeratorService.Repositories;

namespace PidrobitOK.ModeratorService.Controllers
{
    [Authorize(Roles = "Moderator")]
    [Route("api/[controller]")]
    public class JobModerationController : ControllerBase
    {
        private readonly IJobModerationRepository _jobModerationRepository;

        public JobModerationController(IJobModerationRepository jobModerationRepository)
        {
            _jobModerationRepository = jobModerationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var jobs = await _jobModerationRepository.GetAllAsync();
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var job = await _jobModerationRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] JobModerationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _jobModerationRepository.AddAsync(dto);
                return Ok();
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] JobModerationDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            try
            {
                await _jobModerationRepository.UpdateAsync(dto);
                return Ok();
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
