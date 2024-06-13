using PidrobitOK.ModeratorService.Models.DTO;
using PidrobitOK.ModeratorService.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace PidrobitOK.ModeratorService.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly ModerationDbContext _context;
        private readonly IMapper _mapper;

        public ComplaintRepository(ModerationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ComplaintDto>> GetAllAsync()
        {
            var complaints = await _context.Complaints.ToListAsync();
            return _mapper.Map<List<ComplaintDto>>(complaints);
        }

        public async Task<ComplaintDto?> GetByIdAsync(Guid id)
        {
            var complaint = await _context.Complaints.FirstOrDefaultAsync(x => x.Id == id);
            return _mapper.Map<ComplaintDto>(complaint);
        }

        public async Task AddAsync(ComplaintDto dto)
        {
            var complaint = _mapper.Map<Complaint>(dto);
            await _context.Complaints.AddAsync(complaint);
            await _context.SaveChangesAsync();

            dto.Id = complaint.Id;

            await AddLogAsync("Create", complaint.Id, dto.Reason, complaint.ModeratorId);
        }

        public async Task UpdateAsync(ComplaintDto dto)
        {
            var existingComplaint = await _context.Complaints.FindAsync(dto.Id);
            if (existingComplaint != null)
            {
                _mapper.Map(dto, existingComplaint);
                _context.Complaints.Update(existingComplaint);
                await _context.SaveChangesAsync();

                await AddLogAsync("Update", dto.Id, dto.Reason, existingComplaint.ModeratorId);
            }
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
