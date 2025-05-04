using AutoMapper;
using JobService.DatabaseContext;
using JobService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobService.Repositories.SkillsRepository
{
    public class SkillRepository : ISkillRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;
        public SkillRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<bool> Insert(SkillDto skillDto)
        {
            var skill = _mapper.Map<Skill>(skillDto);
            await _dbContext.AddAsync(skill);
            var result = await _dbContext.SaveChangesAsync();

            if(result != 0)
            {
                return true;
            }

            return false;
        }

        public async Task<SkillDto> Retrieve(Guid skillId)
        {
            var skill = await _dbContext.Skills.FirstOrDefaultAsync(x=>x.SkillId == skillId);

            if (skill == null)
            {
                throw new Exception("Doesn`t exists");
            }

            var skillDto = _mapper.Map<SkillDto>(skill);

            return skillDto;
        }

        public async Task<List<SkillDto>> Retrieve()
        {
            var allSkills = await _dbContext.Skills.ToListAsync();

            var allSkillsDto = _mapper.Map<List<SkillDto>>(allSkills);

            return allSkillsDto;
        }
    }
}
