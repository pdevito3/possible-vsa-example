
namespace VerticalLabTestPostgres.Api.Tests.RepositoryTests.Sample
{
    using Application.Dtos.Sample;
    using FluentAssertions;
    using VerticalLabTestPostgres.Api.Tests.Fakes.Sample;
    using Infrastructure.Persistence.Contexts;
    using Infrastructure.Persistence.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Linq;
    using Xunit;
    using Application.Interfaces;
    using Moq;

    public class DeleteSampleRepositoryTests
    { 
        
        [Fact]
        public void DeleteSample_ReturnsProperCount()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            var fakeSampleTwo = new FakeSample { }.Generate();
            var fakeSampleThree = new FakeSample { }.Generate();

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));
                service.DeleteSample(fakeSampleTwo);

                context.SaveChanges();

                //Assert
                var sampleList = context.Samples.ToList();

                sampleList.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                sampleList.Should().ContainEquivalentOf(fakeSampleOne);
                sampleList.Should().ContainEquivalentOf(fakeSampleThree);
                Assert.DoesNotContain(sampleList, s => s == fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }
    } 
}