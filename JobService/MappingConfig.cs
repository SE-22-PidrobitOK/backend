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
            });
            return mappingConfig;
        }
    }
}
