using AutoMapper;
using JobService.Models;

namespace JobService
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Skill, SkillDto>().ReverseMap();
                config.CreateMap<Job, JobDto>().ReverseMap();
                config.CreateMap<JobRequiredSkill, JobRequiredSkillDto>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}
