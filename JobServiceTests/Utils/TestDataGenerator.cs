using Bogus;
using JobService.Models;
using JobService.Models.Dto;

namespace JobServiceTests.Utils
{
    public static class TestDataGenerator
    {
        public static CreateJobDto GenerateCreateJobDto()
        {
            var faker = new Faker<CreateJobDto>()
                .RuleFor(x => x.Title, f => f.Name.JobTitle())
                .RuleFor(x => x.Description, f => f.Lorem.Sentence())
                .RuleFor(x => x.Location, f => f.Address.City())
                .RuleFor(x => x.Salary, f => f.Random.Decimal(1000, 5000))
                .RuleFor(x => x.Type, f => JobType.FullTime)
                .RuleFor(x => x.EmployerId, f => Guid.NewGuid())
                .RuleFor(x => x.RequiredExperience, f => f.Random.Int(1, 5));

            return faker.Generate();
        }

        public static UpdateJobDto GenerateUpdateJobDto(Guid id)
        {
            var faker = new Faker<UpdateJobDto>()
                .RuleFor(x => x.Id, _ => id)
                .RuleFor(x => x.Title, f => f.Name.JobTitle())
                .RuleFor(x => x.Description, f => f.Lorem.Sentence())
                .RuleFor(x => x.Location, f => f.Address.City())
                .RuleFor(x => x.Salary, f => f.Random.Decimal(1000, 5000))
                .RuleFor(x => x.Type, f => JobType.PartTime)
                .RuleFor(x => x.EmployerId, f => Guid.NewGuid())
                .RuleFor(x => x.Status, f => JobStatus.Open)
                .RuleFor(x => x.RequiredExperience, f => f.Random.Int(1, 5));

            return faker.Generate();
        }
    }
}
