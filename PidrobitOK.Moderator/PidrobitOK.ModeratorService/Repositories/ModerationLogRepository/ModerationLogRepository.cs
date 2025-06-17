using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PidrobitOK.ModeratorService.Models;
using PidrobitOK.ModeratorService.Models.DTO;

namespace PidrobitOK.ModeratorService.Repositories
{
    public class ModerationLogRepository : IModerationLogRepository
    {
        private readonly ModerationDbContext _context;
        private readonly IMapper _mapper;

        public ModerationLogRepository(ModerationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddAsync(ModerationLogDto dto)
        {
            var entity = _mapper.Map<ModerationLog>(dto);
            await _context.ModerationLogs.AddAsync(entity);
            await _context.SaveChangesAsync();

            await AddLogAsync("Create", dto.ContentId, dto.Reason, dto.ModeratorId);
        }

        public async Task<IEnumerable<ModerationLogDto>> GetAllAsync()
        {
            var allModerationLogs = await _context.ModerationLogs.ToListAsync();
            return _mapper.Map<IEnumerable<ModerationLogDto>>(allModerationLogs);
        }

        public async Task<ModerationLogDto?> GetByIdAsync(Guid id)
        {
            var entity = await _context.ModerationLogs.FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : _mapper.Map<ModerationLogDto>(entity);
        }

        public async Task UpdateAsync(ModerationLogDto dto)
        {
            var entity = await _context.ModerationLogs.FindAsync(dto.Id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"ModerationLog with Id {dto.Id} not found.");
            }

            _mapper.Map(dto, entity);
            _context.ModerationLogs.Update(entity);
            await _context.SaveChangesAsync();

            await AddLogAsync("Update", dto.ContentId, dto.Reason, dto.ModeratorId);
        }

        private async Task AddLogAsync(string action, Guid contentId, string? reason, Guid? moderatorId)
        {
            var log = new ModerationLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                ContentId = contentId,
                Reason = reason,
                ModeratorId = moderatorId,
                Timestamp = DateTime.UtcNow
            };

            await _context.ModerationLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
