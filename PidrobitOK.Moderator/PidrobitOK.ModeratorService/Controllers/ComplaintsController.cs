using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PidrobitOK.ModeratorService.Models.DTO;
using PidrobitOK.ModeratorService.Repositories;

namespace PidrobitOK.ModeratorService.Controllers
{
    [Authorize(Roles = "Moderator")]
    [Route("api/[controller]")]
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintRepository _complaintRepository;

        public ComplaintsController(IComplaintRepository complaintRepository)
        {
            _complaintRepository = complaintRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var complaints = await _complaintRepository.GetAllAsync();
            return Ok(complaints);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var complaint = await _complaintRepository.GetByIdAsync(id);
            if (complaint == null)
                return NotFound();

            return Ok(complaint);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ComplaintDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _complaintRepository.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ComplaintDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            await _complaintRepository.UpdateAsync(dto);
            return NoContent();
        }
    }
}
