namespace Infrastructure.Persistence.Seeders
{

    using AutoBogus;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using System.Linq;

    public static class SampleSeeder
    {
        public static void SeedSampleSampleData(VerticalLabTestPostgresDbContext context)
        {
            if (!context.Samples.Any())
            {
                context.Samples.Add(new AutoFaker<Sample>());
                context.Samples.Add(new AutoFaker<Sample>());
                context.Samples.Add(new AutoFaker<Sample>());

                context.SaveChanges();
            }
        }
    }
}