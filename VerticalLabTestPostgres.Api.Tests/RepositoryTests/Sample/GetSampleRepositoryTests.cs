
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

    public class GetSampleRepositoryTests
    { 
        
        [Fact]
        public void GetSample_ParametersMatchExpectedValues()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSample = new FakeSample { }.Generate();

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSample);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                //Assert
                var sampleById = service.GetSample(fakeSample.SampleId);
                                sampleById.SampleId.Should().Be(fakeSample.SampleId);
                sampleById.ExternalId.Should().Be(fakeSample.ExternalId);
                sampleById.InternalId.Should().Be(fakeSample.InternalId);
                sampleById.SampleType.Should().Be(fakeSample.SampleType);
                sampleById.ContainerType.Should().Be(fakeSample.ContainerType);
                sampleById.CollectionDate.Should().Be(fakeSample.CollectionDate);
                sampleById.ArrivalDate.Should().Be(fakeSample.ArrivalDate);
                sampleById.Amount.Should().Be(fakeSample.Amount);
                sampleById.AmountUnits.Should().Be(fakeSample.AmountUnits);
            }
        }
        
        [Fact]
        public async void GetSamplesAsync_CountMatchesAndContainsEquivalentObjects()
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
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto());

                //Assert
                sampleRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(3);

                sampleRepo.Should().ContainEquivalentOf(fakeSampleOne);
                sampleRepo.Should().ContainEquivalentOf(fakeSampleTwo);
                sampleRepo.Should().ContainEquivalentOf(fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetSamplesAsync_ReturnExpectedPageSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            var fakeSampleTwo = new FakeSample { }.Generate();
            var fakeSampleThree = new FakeSample { }.Generate();
            
            // need id's due to default sorting
            fakeSampleOne.SampleId = Guid.Parse("547ee3d9-5241-4ce3-93f6-a65700bd36ca");
            fakeSampleTwo.SampleId = Guid.Parse("621fab6d-2487-43f4-aec2-354fa54089da");
            fakeSampleThree.SampleId = Guid.Parse("f9335b96-63dd-412e-935b-102463b9f245");

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { PageSize = 2 });

                //Assert
                sampleRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                sampleRepo.Should().ContainEquivalentOf(fakeSampleOne);
                sampleRepo.Should().ContainEquivalentOf(fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetSamplesAsync_ReturnExpectedPageNumberAndSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            var fakeSampleTwo = new FakeSample { }.Generate();
            var fakeSampleThree = new FakeSample { }.Generate();
            
            // need id's due to default sorting
            fakeSampleOne.SampleId = Guid.Parse("547ee3d9-5241-4ce3-93f6-a65700bd36ca");
            fakeSampleTwo.SampleId = Guid.Parse("621fab6d-2487-43f4-aec2-354fa54089da");
            fakeSampleThree.SampleId = Guid.Parse("f9335b96-63dd-412e-935b-102463b9f245");

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { PageSize = 1, PageNumber = 2 });

                //Assert
                sampleRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(1);

                sampleRepo.Should().ContainEquivalentOf(fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetSamplesAsync_ListExternalIdSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ExternalId = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ExternalId = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ExternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "ExternalId" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleTwo, fakeSampleOne, fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListExternalIdSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ExternalId = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ExternalId = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ExternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "-ExternalId" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleThree, fakeSampleOne, fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListInternalIdSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.InternalId = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.InternalId = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.InternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "InternalId" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleTwo, fakeSampleOne, fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListInternalIdSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.InternalId = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.InternalId = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.InternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "-InternalId" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleThree, fakeSampleOne, fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListSampleTypeSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.SampleType = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.SampleType = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.SampleType = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "SampleType" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleTwo, fakeSampleOne, fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListSampleTypeSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.SampleType = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.SampleType = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.SampleType = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "-SampleType" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleThree, fakeSampleOne, fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListContainerTypeSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ContainerType = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ContainerType = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ContainerType = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "ContainerType" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleTwo, fakeSampleOne, fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListContainerTypeSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ContainerType = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ContainerType = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ContainerType = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "-ContainerType" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleThree, fakeSampleOne, fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListCollectionDateSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.CollectionDate = DateTime.Now.AddDays(2);

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.CollectionDate = DateTime.Now.AddDays(1);

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.CollectionDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "CollectionDate" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleTwo, fakeSampleOne, fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListCollectionDateSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.CollectionDate = DateTime.Now.AddDays(2);

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.CollectionDate = DateTime.Now.AddDays(1);

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.CollectionDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "-CollectionDate" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleThree, fakeSampleOne, fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListArrivalDateSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ArrivalDate = DateTime.Now.AddDays(2);

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ArrivalDate = DateTime.Now.AddDays(1);

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ArrivalDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "ArrivalDate" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleTwo, fakeSampleOne, fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListArrivalDateSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ArrivalDate = DateTime.Now.AddDays(2);

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ArrivalDate = DateTime.Now.AddDays(1);

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ArrivalDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "-ArrivalDate" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleThree, fakeSampleOne, fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListAmountSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.Amount = 2;

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.Amount = 1;

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.Amount = 3;

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "Amount" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleTwo, fakeSampleOne, fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListAmountSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.Amount = 2;

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.Amount = 1;

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.Amount = 3;

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "-Amount" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleThree, fakeSampleOne, fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListAmountUnitsSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.AmountUnits = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.AmountUnits = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.AmountUnits = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "AmountUnits" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleTwo, fakeSampleOne, fakeSampleThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_ListAmountUnitsSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.AmountUnits = "bravo";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.AmountUnits = "alpha";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.AmountUnits = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { SortOrder = "-AmountUnits" });

                //Assert
                sampleRepo.Should()
                    .ContainInOrder(fakeSampleThree, fakeSampleOne, fakeSampleTwo);

                context.Database.EnsureDeleted();
            }
        }

        
        [Fact]
        public async void GetSamplesAsync_FilterSampleIdListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.SampleId = Guid.NewGuid();

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.SampleId = Guid.NewGuid();

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.SampleId = Guid.NewGuid();

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"SampleId == {fakeSampleTwo.SampleId}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_FilterExternalIdListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ExternalId = "alpha";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ExternalId = "bravo";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ExternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"ExternalId == {fakeSampleTwo.ExternalId}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_FilterInternalIdListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.InternalId = "alpha";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.InternalId = "bravo";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.InternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"InternalId == {fakeSampleTwo.InternalId}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_FilterSampleTypeListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.SampleType = "alpha";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.SampleType = "bravo";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.SampleType = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"SampleType == {fakeSampleTwo.SampleType}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_FilterContainerTypeListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ContainerType = "alpha";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ContainerType = "bravo";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ContainerType = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"ContainerType == {fakeSampleTwo.ContainerType}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_FilterCollectionDateListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.CollectionDate = DateTime.Now.AddDays(1);

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.CollectionDate = DateTime.Parse(DateTime.Now.AddDays(2).ToString("MM/dd/yyyy"));

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.CollectionDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"CollectionDate == {fakeSampleTwo.CollectionDate}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_FilterArrivalDateListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.ArrivalDate = DateTime.Now.AddDays(1);

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.ArrivalDate = DateTime.Parse(DateTime.Now.AddDays(2).ToString("MM/dd/yyyy"));

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.ArrivalDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"ArrivalDate == {fakeSampleTwo.ArrivalDate}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_FilterAmountListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.Amount = 1;

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.Amount = 2;

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.Amount = 3;

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"Amount == {fakeSampleTwo.Amount}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetSamplesAsync_FilterAmountUnitsListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"SampleDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeSampleOne = new FakeSample { }.Generate();
            fakeSampleOne.AmountUnits = "alpha";

            var fakeSampleTwo = new FakeSample { }.Generate();
            fakeSampleTwo.AmountUnits = "bravo";

            var fakeSampleThree = new FakeSample { }.Generate();
            fakeSampleThree.AmountUnits = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo, fakeSampleThree);
                context.SaveChanges();

                var service = new SampleRepository(context, new SieveProcessor(sieveOptions));

                var sampleRepo = await service.GetSamplesAsync(new SampleParametersDto { Filters = $"AmountUnits == {fakeSampleTwo.AmountUnits}" });

                //Assert
                sampleRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

    } 
}