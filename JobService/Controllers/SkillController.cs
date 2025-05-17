using AutoMapper;
using JobService.Models;
using JobService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JobService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISkillRepository _skillRepository;

        public SkillController(IMapper mapper, ISkillRepository skillRepository)
        {
            _mapper = mapper;
            _skillRepository = skillRepository;
        }

        [HttpGet("retrieve")]
        public async Task<IActionResult> Get([FromQuery] Guid id)
        {
            var skill = await _skillRepository.Retrieve(id);
            return Ok(skill);
        }

        [HttpGet("retrieve-all")]
        public async Task<IActionResult> GetAll()
        {
            var skills = await _skillRepository.Retrieve();
            return Ok(skills);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] SkillDto skillDto)
        {
            var result = await _skillRepository.Insert(skillDto);
            return Ok(result);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] SkillDto skillDto)
        {
            var result = await _skillRepository.Update(skillDto);
            return Ok(result);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] Guid id)
        {
            var result = await _skillRepository.Delete(id);
            return Ok(result);
        }
    }
}