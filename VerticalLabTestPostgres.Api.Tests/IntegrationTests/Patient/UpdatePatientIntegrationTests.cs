
namespace VerticalLabTestPostgres.Api.Tests.IntegrationTests.Patient
{
    using Application.Dtos.Patient;
    using FluentAssertions;
    using VerticalLabTestPostgres.Api.Tests.Fakes.Patient;
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using WebApi;
    using System.Collections.Generic;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.Extensions.DependencyInjection;    
    using Microsoft.AspNetCore.JsonPatch;
    using System.Linq;
    using AutoMapper;
    using Bogus;
    using Application.Mappings;
    using System.Text;
    using Application.Wrappers;
    using VerticalLabTestPostgres.Api.Tests.Helpers;

    [Collection("Sequential")]
    public class UpdatePatientIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public UpdatePatientIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task PatchPatient204AndFieldsWereSuccessfullyUpdated_WithAuth()
        {
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PatientProfile>();
            }).CreateMapper();

            var lookupVal = "Easily Identified Value For Test"; // don't know the id at this scope, so need to have another value to lookup
            var fakePatientOne = new FakePatient { }.Generate();
            
            var expectedFinalObject = mapper.Map<PatientDto>(fakePatientOne);
            expectedFinalObject.ExternalId = lookupVal;

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VerticalLabTestPostgresDbContext> ();
                context.Database.EnsureCreated();

                context.Patients.RemoveRange(context.Patients);
                context.Patients.AddRange(fakePatientOne);
                context.SaveChanges();
            }

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            client.AddAuth(new[] {"patients.read", "patients.update"});

            var patchDoc = new JsonPatchDocument<PatientForUpdateDto>();
            patchDoc.Replace(p => p.ExternalId, lookupVal);
            var serializedPatientToUpdate = JsonConvert.SerializeObject(patchDoc);

            // Act
            // get the value i want to update. assumes I can use sieve for this field. if this is not an option, just use something else
            var getResult = await client.GetAsync($"api/Patients/?filters=ExternalId=={fakePatientOne.ExternalId}")
                .ConfigureAwait(false);
            var getResponseContent = await getResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var getResponse = JsonConvert.DeserializeObject<Response<IEnumerable<PatientDto>>>(getResponseContent);
            var id = getResponse.Data.FirstOrDefault().PatientId;

            // patch it
            var method = new HttpMethod("PATCH");
            var patchRequest = new HttpRequestMessage(method, $"api/Patients/{id}")
            {
                Content = new StringContent(serializedPatientToUpdate,
                    Encoding.Unicode, "application/json")
            };
            var patchResult = await client.SendAsync(patchRequest)
                .ConfigureAwait(false);

            // get it again to confirm updates
            var checkResult = await client.GetAsync($"api/Patients/{id}")
                .ConfigureAwait(false);
            var checkResponseContent = await checkResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var checkResponse = JsonConvert.DeserializeObject<Response<PatientDto>>(checkResponseContent);

            // Assert
            patchResult.StatusCode.Should().Be(204);
            checkResponse.Should().BeEquivalentTo(expectedFinalObject, options =>
                options.ExcludingMissingMembers());
        }
        
        [Fact]
        public async Task PutPatientReturnsBodyAndFieldsWereSuccessfullyUpdated_WithAuth()
        {
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PatientProfile>();
            }).CreateMapper();

            var fakePatientOne = new FakePatient { }.Generate();
            var expectedFinalObject = mapper.Map<PatientDto>(fakePatientOne);
            expectedFinalObject.ExternalId = "Easily Identified Value For Test";

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VerticalLabTestPostgresDbContext> ();
                context.Database.EnsureCreated();

                context.Patients.RemoveRange(context.Patients);
                context.Patients.AddRange(fakePatientOne);
                context.SaveChanges();
            }

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            client.AddAuth(new[] {"patients.read", "patients.update"});

            var serializedPatientToUpdate = JsonConvert.SerializeObject(expectedFinalObject);

            // Act
            // get the value i want to update. assumes I can use sieve for this field. if this is not an option, just use something else
            var getResult = await client.GetAsync($"api/Patients/?filters=ExternalId=={fakePatientOne.ExternalId}")
                .ConfigureAwait(false);
            var getResponseContent = await getResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var getResponse = JsonConvert.DeserializeObject<Response<IEnumerable<PatientDto>>>(getResponseContent);
            var id = getResponse?.Data.FirstOrDefault().PatientId;

            // put it
            var putResult = await client.PutAsJsonAsync($"api/Patients/{id}", expectedFinalObject)
                .ConfigureAwait(false);

            // get it again to confirm updates
            var checkResult = await client.GetAsync($"api/Patients/{id}")
                .ConfigureAwait(false);
            var checkResponseContent = await checkResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var checkResponse = JsonConvert.DeserializeObject<Response<PatientDto>>(checkResponseContent);

            // Assert
            putResult.StatusCode.Should().Be(204);
            checkResponse.Should().BeEquivalentTo(expectedFinalObject, options =>
                options.ExcludingMissingMembers());
        }
            
        [Fact]
        public async Task UpdateRecordPatients_Returns_Unauthorized_Without_Valid_Token()
        {
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PatientProfile>();
            }).CreateMapper();

            var fakePatientOne = new FakePatient { }.Generate();
            var expectedFinalObject = mapper.Map<PatientDto>(fakePatientOne);
            var id = fakePatientOne.PatientId;

            var patchDoc = new JsonPatchDocument<PatientForUpdateDto>();
            patchDoc.Replace(p => p.ExternalId, "");
            var serializedPatientToUpdate = JsonConvert.SerializeObject(patchDoc);

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var putResult = await client.PutAsJsonAsync($"api/Patients/{id}", expectedFinalObject)
                .ConfigureAwait(false);

            // Assert
            putResult.StatusCode.Should().Be(401);
        }
            
        [Fact]
        public async Task UpdateRecordPatient_Returns_Forbidden_Without_Proper_Scope()
        {
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PatientProfile>();
            }).CreateMapper();

            var fakePatientOne = new FakePatient { }.Generate();
            var expectedFinalObject = mapper.Map<PatientDto>(fakePatientOne);
            var id = fakePatientOne.PatientId;

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            client.AddAuth(new[] { "" });

            // Act
            var putResult = await client.PutAsJsonAsync($"api/Patients/{id}", expectedFinalObject)
                .ConfigureAwait(false);

            // Assert
            putResult.StatusCode.Should().Be(403);
        }
            
        [Fact]
        public async Task UpdatePartialPatients_Returns_Unauthorized_Without_Valid_Token()
        {
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PatientProfile>();
            }).CreateMapper();

            var fakePatientOne = new FakePatient { }.Generate();
            var expectedFinalObject = mapper.Map<PatientDto>(fakePatientOne);
            var id = fakePatientOne.PatientId;

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var patchDoc = new JsonPatchDocument<PatientForUpdateDto>();
            patchDoc.Replace(p => p.ExternalId, "");
            var serializedPatientToUpdate = JsonConvert.SerializeObject(patchDoc);

            // Act
            var method = new HttpMethod("PATCH");
            var patchRequest = new HttpRequestMessage(method, $"api/Patients/{id}")
            {
                Content = new StringContent(serializedPatientToUpdate,
                    Encoding.Unicode, "application/json")
            };
            var patchResult = await client.SendAsync(patchRequest)
                .ConfigureAwait(false);

            // Assert
            patchResult.StatusCode.Should().Be(401);
        }
            
        [Fact]
        public async Task UpdatePartialPatient_Returns_Forbidden_Without_Proper_Scope()
        {
            //Arrange
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PatientProfile>();
            }).CreateMapper();

            var fakePatientOne = new FakePatient { }.Generate();
            var expectedFinalObject = mapper.Map<PatientDto>(fakePatientOne);
            var id = fakePatientOne.PatientId;

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            client.AddAuth(new[] { "" });

            // Act
            var patchResult = await client.PutAsJsonAsync($"api/Patients/{id}", expectedFinalObject)
                .ConfigureAwait(false);

            // Assert
            patchResult.StatusCode.Should().Be(403);
        }
    } 
}