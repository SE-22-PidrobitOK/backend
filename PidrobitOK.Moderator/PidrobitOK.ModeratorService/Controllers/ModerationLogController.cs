using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PidrobitOK.ModeratorService.Models.DTO;
using PidrobitOK.ModeratorService.Repositories;

namespace PidrobitOK.ModeratorService.Controllers
{
    [Authorize(Roles = "Moderator")]
    [ApiController]
    [Route("api/[controller]")]
    public class ModerationLogController : ControllerBase
    {
        private readonly IModerationLogRepository _logRepository;

        public ModerationLogController(IModerationLogRepository logService)
        {
            _logRepository = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _logRepository.GetAllAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var log = await _logRepository.GetByIdAsync(id);
            if (log == null)
                return NotFound();

            return Ok(log);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ModerationLogDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _logRepository.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [HttpPost("create/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ModerationLogDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch.");

            await _logRepository.UpdateAsync(dto);
            return NoContent();
        }
    }
}
