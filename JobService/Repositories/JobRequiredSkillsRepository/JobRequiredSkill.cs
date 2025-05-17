using AutoMapper;
using JobService.DatabaseContext;
using JobService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobService.Repositories.JobRequiredSkillsRepository
{
    public class JobRequiredSkillRepository : IJobRequiredSkillRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public JobRequiredSkillRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db; _mapper = mapper;
        }

        public async Task<List<JobRequiredSkillDto>> Retrieve()
        {
            var list = await _db.JobRequiredSkills.ToListAsync();
            return _mapper.Map<List<JobRequiredSkillDto>>(list);
        }

        public async Task<bool> Insert(JobRequiredSkillDto dto)
        {
            var entity = _mapper.Map<JobRequiredSkill>(dto);
            await _db.JobRequiredSkills.AddAsync(entity);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var entity = await _db.JobRequiredSkills.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null) return false;
            _db.JobRequiredSkills.Remove(entity);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}