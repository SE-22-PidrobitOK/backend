using AutoMapper;
using JobService.DatabaseContext;
using JobService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobService.Repositories.JobsRepository
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public JobRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db; _mapper = mapper;
        }

        public async Task<JobDto> Retrieve(Guid id)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id)
                      ?? throw new Exception("Job not found");
            return _mapper.Map<JobDto>(job);
        }

        public async Task<List<JobDto>> Retrieve()
        {
            var jobs = await _db.Jobs.ToListAsync();
            return _mapper.Map<List<JobDto>>(jobs);
        }

        public async Task<bool> Insert(JobDto dto)
        {
            var job = _mapper.Map<Job>(dto);
            job.CreatedAt = DateTime.UtcNow;
            await _db.Jobs.AddAsync(job);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> Update(JobDto dto)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (job is null) return false;
            _mapper.Map(dto, job);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id);
            if (job is null) return false;
            _db.Jobs.Remove(job);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}