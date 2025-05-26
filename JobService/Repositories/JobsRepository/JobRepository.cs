using AutoMapper;
using JobService.DatabaseContext;
using JobService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobService.Repositories.JobsRepository
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        public JobRepository(ApplicationDbContext db, IMapper mapper)
        {
            _dbContext = db; _mapper = mapper;
        }

        public async Task<Job> Retrieve(Guid id)
        {
            return await _dbContext.Jobs.FindAsync(id);
        }

        public async Task<List<Job>> Retrieve()
        {
            return await _dbContext.Jobs.ToListAsync();
        }

        public async Task<Job> Insert(Job job)
        {
            if (job.Id == Guid.Empty)
                job.Id = Guid.NewGuid();
            job.Status = JobStatus.Open;
            await _dbContext.Jobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();
            return job;
        }

        public async Task<Job?> Update(Job job)
        {
            var existing = await _dbContext.Jobs.FindAsync(job.Id);
            if (existing == null) return null;

            _dbContext.Entry(existing).CurrentValues.SetValues(job);
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(Guid id)
        {
            var job = await _dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == id);
            if (job is null) return false;
            _dbContext.Jobs.Remove(job);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}