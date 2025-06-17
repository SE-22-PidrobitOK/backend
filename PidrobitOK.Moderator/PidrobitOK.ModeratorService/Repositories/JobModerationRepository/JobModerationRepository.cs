using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PidrobitOK.ModeratorService.Models;
using PidrobitOK.ModeratorService.Models.DTO;

namespace PidrobitOK.ModeratorService.Repositories
{
    public class JobModerationRepository : IJobModerationRepository
    {
        private readonly ModerationDbContext _context;
        private readonly IMapper _mapper;

        public JobModerationRepository(ModerationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddAsync(JobModerationDto dto)
        {
            var entity = _mapper.Map<JobModeration>(dto);
            await _context.JobModerations.AddAsync(entity);
            await _context.SaveChangesAsync();

            dto.Id = entity.Id;

            await AddLogAsync("Create", entity.Id, dto.Reason, dto.ModeratorId);
        }

        public async Task<IEnumerable<JobModerationDto>> GetAllAsync()
        {
            var allModerations = await _context.JobModerations.ToListAsync();
            return _mapper.Map<IEnumerable<JobModerationDto>>(allModerations);
        }

        public async Task<JobModerationDto?> GetByIdAsync(Guid id)
        {
            var entity = await _context.JobModerations.FirstOrDefaultAsync(j => j.Id == id);
            return entity == null ? null : _mapper.Map<JobModerationDto>(entity);
        }

        public async Task UpdateAsync(JobModerationDto dto)
        {
            var entity = await _context.JobModerations.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"JobModeration with Id {dto.Id} not found.");

            _mapper.Map(dto, entity);
            _context.JobModerations.Update(entity);
            await _context.SaveChangesAsync();

            await AddLogAsync("Update", entity.Id, dto.Reason, dto.ModeratorId);
        }

        private async Task AddLogAsync(string action, Guid contentId, string? reason, Guid? userId)
        {
            var log = new ModerationLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                ContentId = contentId,
                Reason = reason,
                ModeratorId = userId,
                Timestamp = DateTime.UtcNow
            };

            await _context.ModerationLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
