using PidrobitOK.ModeratorService.Models.DTO;

namespace PidrobitOK.ModeratorService.Repositories
{
    public interface IComplaintRepository
    {
        Task<IEnumerable<ComplaintDto>> GetAllAsync();
        Task<ComplaintDto?> GetByIdAsync(Guid id);
        Task AddAsync(ComplaintDto dto);
        Task UpdateAsync(ComplaintDto dto);
    }
}
