using AutoMapper;
using PidrobitOK.ModeratorService.Models.DTO;
using PidrobitOK.ModeratorService.Models;

namespace PidrobitOK.ModeratorService
{
    public class MapperProfile
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingCofig = new MapperConfiguration(config =>
            {
                config.CreateMap<Complaint, ComplaintDto>().ReverseMap();
                config.CreateMap<JobModeration, JobModerationDto>().ReverseMap();
                config.CreateMap<ModerationLog, ModerationLogDto>().ReverseMap();
            });

            return mappingCofig;
        }
    }
}
