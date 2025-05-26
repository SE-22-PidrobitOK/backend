using AutoMapper;
using JobService.Models;
using JobService.Models.Dto;

namespace JobService
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Skill, SkillDto>().ReverseMap();
                config.CreateMap<CreateSkillDto, Skill>();
                config.CreateMap<UpdateSkillDto, Skill>();
                config.CreateMap<Skill, Skill>().ForMember(dest => dest.SkillId, opt => opt.Ignore());
                config.CreateMap<Job, JobDto>().ReverseMap();
                config.CreateMap<CreateJobDto, Job>();
                config.CreateMap<UpdateJobDto, Job>();
                config.CreateMap<Job, Job>().ForMember(dest => dest.Id, opt => opt.Ignore());
                config.CreateMap<JobRequiredSkill, JobRequiredSkillDto>().ReverseMap();
                config.CreateMap<CreateJobRequiredSkillDto, JobRequiredSkill>();
                config.CreateMap<UpdateJobRequiredSkillDto, JobRequiredSkill>();
                config.CreateMap<JobRequiredSkill, JobRequiredSkill>().ForMember(dest => dest.Id, opt => opt.Ignore());
            });
            return mappingConfig;
        }
    }
}
