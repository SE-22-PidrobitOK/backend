using AutoMapper;
using JobService.Models;
using JobService.Repositories;
using JobService.Repositories.SkillsRepository;
using Microsoft.AspNetCore.Mvc;

namespace JobService.Controllers
{
    [Route("api/[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISkillRepository _skillRepository;

        public SkillController(IMapper mapper, ISkillRepository skillRepository)
        {
            _mapper = mapper;
            _skillRepository = skillRepository;
        }

        [HttpGet("Retrieve")]
        public async Task<IActionResult> Get(Guid id)
        {
            var skill = await _skillRepository.Retrieve(id);

            return Ok(skill);
        }

        [HttpPost]
        public async Task<IActionResult> Insert(SkillDto skillDto)
        {
            var result = await _skillRepository.Insert(skillDto);

            return Ok(result);
        }
    }
}
