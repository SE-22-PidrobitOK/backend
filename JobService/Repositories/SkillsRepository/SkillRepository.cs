using AutoMapper;
using JobService.DatabaseContext;
using JobService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobService.Repositories.SkillsRepository
{
    public class SkillRepository : ISkillRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public SkillRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<bool> Insert(SkillDto skillDto)
        {
            var skill = _mapper.Map<Skill>(skillDto);
            await _dbContext.AddAsync(skill);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<SkillDto> Retrieve(Guid skillId)
        {
            var skill = await _dbContext.Skills.FirstOrDefaultAsync(x => x.SkillId == skillId);
            if (skill == null) throw new Exception("Skill not found");
            return _mapper.Map<SkillDto>(skill);
        }

        public async Task<List<SkillDto>> Retrieve()
        {
            var allSkills = await _dbContext.Skills.ToListAsync();
            return _mapper.Map<List<SkillDto>>(allSkills);
        }

        public async Task<bool> Update(SkillDto skillDto)
        {
            var existing = await _dbContext.Skills.FirstOrDefaultAsync(x => x.SkillId == skillDto.SkillId);
            if (existing == null) return false;

            _mapper.Map(skillDto, existing);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> Delete(Guid skillId)
        {
            var existing = await _dbContext.Skills.FirstOrDefaultAsync(x => x.SkillId == skillId);
            if (existing == null) return false;

            _dbContext.Skills.Remove(existing);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}