
namespace VerticalLabTestPostgres.Api.Tests.RepositoryTests.Patient
{
    using Application.Dtos.Patient;
    using FluentAssertions;
    using VerticalLabTestPostgres.Api.Tests.Fakes.Patient;
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

    public class GetPatientRepositoryTests
    { 
        
        [Fact]
        public void GetPatient_ParametersMatchExpectedValues()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatient = new FakePatient { }.Generate();

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatient);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                //Assert
                var patientById = service.GetPatient(fakePatient.PatientId);
                                patientById.PatientId.Should().Be(fakePatient.PatientId);
                patientById.ExternalId.Should().Be(fakePatient.ExternalId);
                patientById.InternalId.Should().Be(fakePatient.InternalId);
                patientById.FirstName.Should().Be(fakePatient.FirstName);
                patientById.LastName.Should().Be(fakePatient.LastName);
                patientById.Dob.Should().Be(fakePatient.Dob);
                patientById.Sex.Should().Be(fakePatient.Sex);
                patientById.Gender.Should().Be(fakePatient.Gender);
                patientById.Race.Should().Be(fakePatient.Race);
                patientById.Ethnicity.Should().Be(fakePatient.Ethnicity);
            }
        }
        
        [Fact]
        public async void GetPatientsAsync_CountMatchesAndContainsEquivalentObjects()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            var fakePatientTwo = new FakePatient { }.Generate();
            var fakePatientThree = new FakePatient { }.Generate();

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto());

                //Assert
                patientRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(3);

                patientRepo.Should().ContainEquivalentOf(fakePatientOne);
                patientRepo.Should().ContainEquivalentOf(fakePatientTwo);
                patientRepo.Should().ContainEquivalentOf(fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetPatientsAsync_ReturnExpectedPageSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            var fakePatientTwo = new FakePatient { }.Generate();
            var fakePatientThree = new FakePatient { }.Generate();
            
            // need id's due to default sorting
            fakePatientOne.PatientId = Guid.Parse("547ee3d9-5241-4ce3-93f6-a65700bd36ca");
            fakePatientTwo.PatientId = Guid.Parse("621fab6d-2487-43f4-aec2-354fa54089da");
            fakePatientThree.PatientId = Guid.Parse("f9335b96-63dd-412e-935b-102463b9f245");

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { PageSize = 2 });

                //Assert
                patientRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                patientRepo.Should().ContainEquivalentOf(fakePatientOne);
                patientRepo.Should().ContainEquivalentOf(fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetPatientsAsync_ReturnExpectedPageNumberAndSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            var fakePatientTwo = new FakePatient { }.Generate();
            var fakePatientThree = new FakePatient { }.Generate();
            
            // need id's due to default sorting
            fakePatientOne.PatientId = Guid.Parse("547ee3d9-5241-4ce3-93f6-a65700bd36ca");
            fakePatientTwo.PatientId = Guid.Parse("621fab6d-2487-43f4-aec2-354fa54089da");
            fakePatientThree.PatientId = Guid.Parse("f9335b96-63dd-412e-935b-102463b9f245");

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { PageSize = 1, PageNumber = 2 });

                //Assert
                patientRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(1);

                patientRepo.Should().ContainEquivalentOf(fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetPatientsAsync_ListExternalIdSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.ExternalId = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.ExternalId = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.ExternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "ExternalId" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListExternalIdSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.ExternalId = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.ExternalId = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.ExternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-ExternalId" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListInternalIdSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.InternalId = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.InternalId = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.InternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "InternalId" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListInternalIdSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.InternalId = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.InternalId = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.InternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-InternalId" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListFirstNameSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.FirstName = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.FirstName = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.FirstName = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "FirstName" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListFirstNameSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.FirstName = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.FirstName = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.FirstName = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-FirstName" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListLastNameSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.LastName = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.LastName = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.LastName = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "LastName" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListLastNameSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.LastName = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.LastName = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.LastName = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-LastName" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListDobSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Dob = DateTime.Now.AddDays(2);

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Dob = DateTime.Now.AddDays(1);

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Dob = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "Dob" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListDobSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Dob = DateTime.Now.AddDays(2);

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Dob = DateTime.Now.AddDays(1);

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Dob = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-Dob" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListSexSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Sex = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Sex = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Sex = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "Sex" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListSexSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Sex = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Sex = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Sex = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-Sex" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListGenderSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Gender = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Gender = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Gender = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "Gender" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListGenderSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Gender = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Gender = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Gender = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-Gender" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListRaceSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Race = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Race = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Race = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "Race" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListRaceSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Race = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Race = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Race = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-Race" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListEthnicitySortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Ethnicity = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Ethnicity = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Ethnicity = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "Ethnicity" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientTwo, fakePatientOne, fakePatientThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_ListEthnicitySortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Ethnicity = "bravo";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Ethnicity = "alpha";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Ethnicity = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { SortOrder = "-Ethnicity" });

                //Assert
                patientRepo.Should()
                    .ContainInOrder(fakePatientThree, fakePatientOne, fakePatientTwo);

                context.Database.EnsureDeleted();
            }
        }

        
        [Fact]
        public async void GetPatientsAsync_FilterPatientIdListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.PatientId = Guid.NewGuid();

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.PatientId = Guid.NewGuid();

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.PatientId = Guid.NewGuid();

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"PatientId == {fakePatientTwo.PatientId}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterExternalIdListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.ExternalId = "alpha";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.ExternalId = "bravo";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.ExternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"ExternalId == {fakePatientTwo.ExternalId}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterInternalIdListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.InternalId = "alpha";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.InternalId = "bravo";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.InternalId = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"InternalId == {fakePatientTwo.InternalId}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterFirstNameListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.FirstName = "alpha";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.FirstName = "bravo";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.FirstName = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"FirstName == {fakePatientTwo.FirstName}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterLastNameListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.LastName = "alpha";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.LastName = "bravo";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.LastName = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"LastName == {fakePatientTwo.LastName}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterDobListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Dob = DateTime.Now.AddDays(1);

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Dob = DateTime.Parse(DateTime.Now.AddDays(2).ToString("MM/dd/yyyy"));

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Dob = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"Dob == {fakePatientTwo.Dob}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterSexListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Sex = "alpha";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Sex = "bravo";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Sex = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"Sex == {fakePatientTwo.Sex}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterGenderListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Gender = "alpha";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Gender = "bravo";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Gender = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"Gender == {fakePatientTwo.Gender}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterRaceListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Race = "alpha";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Race = "bravo";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Race = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"Race == {fakePatientTwo.Race}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetPatientsAsync_FilterEthnicityListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VerticalLabTestPostgresDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatientDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePatientOne = new FakePatient { }.Generate();
            fakePatientOne.Ethnicity = "alpha";

            var fakePatientTwo = new FakePatient { }.Generate();
            fakePatientTwo.Ethnicity = "bravo";

            var fakePatientThree = new FakePatient { }.Generate();
            fakePatientThree.Ethnicity = "charlie";

            //Act
            using (var context = new VerticalLabTestPostgresDbContext(dbOptions))
            {
                context.Patients.AddRange(fakePatientOne, fakePatientTwo, fakePatientThree);
                context.SaveChanges();

                var service = new PatientRepository(context, new SieveProcessor(sieveOptions));

                var patientRepo = await service.GetPatientsAsync(new PatientParametersDto { Filters = $"Ethnicity == {fakePatientTwo.Ethnicity}" });

                //Assert
                patientRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

    } 
}