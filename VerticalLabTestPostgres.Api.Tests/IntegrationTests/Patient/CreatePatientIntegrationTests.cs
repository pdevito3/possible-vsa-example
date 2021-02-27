
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
    using Application.Wrappers;
    using VerticalLabTestPostgres.Api.Tests.Helpers;

    [Collection("Sequential")]
    public class CreatePatientIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public CreatePatientIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task PostPatientReturnsSuccessCodeAndResourceWithAccurateFields_WithAuth()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var fakePatient = new FakePatientDto().Generate();

            client.AddAuth(new[] {"patients.add"});

            // Act
            var httpResponse = await client.PostAsJsonAsync("api/Patients", fakePatient)
                .ConfigureAwait(false);

            // Assert
            httpResponse.EnsureSuccessStatusCode();

            var resultDto = JsonConvert.DeserializeObject<Response<PatientDto>>(await httpResponse.Content.ReadAsStringAsync()
                .ConfigureAwait(false));

            httpResponse.StatusCode.Should().Be(201);
            resultDto.Data.ExternalId.Should().Be(fakePatient.ExternalId);
            resultDto.Data.InternalId.Should().Be(fakePatient.InternalId);
            resultDto.Data.FirstName.Should().Be(fakePatient.FirstName);
            resultDto.Data.LastName.Should().Be(fakePatient.LastName);
            resultDto.Data.Dob.Should().Be(fakePatient.Dob);
            resultDto.Data.Sex.Should().Be(fakePatient.Sex);
            resultDto.Data.Gender.Should().Be(fakePatient.Gender);
            resultDto.Data.Race.Should().Be(fakePatient.Race);
            resultDto.Data.Ethnicity.Should().Be(fakePatient.Ethnicity);
        }
            
        [Fact]
        public async Task PostPatients_Returns_Unauthorized_Without_Valid_Token()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var fakePatient = new FakePatientDto().Generate();

            // Act
            var httpResponse = await client.PostAsJsonAsync("api/Patients", fakePatient)
                .ConfigureAwait(false);

            // Assert
            httpResponse.StatusCode.Should().Be(401);
        }
            
        [Fact]
        public async Task PostPatient_Returns_Forbidden_Without_Proper_Scope()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var fakePatient = new FakePatientDto().Generate();

            client.AddAuth(new[] { "" });

            // Act
            var httpResponse = await client.PostAsJsonAsync("api/Patients", fakePatient)
                .ConfigureAwait(false);

            // Assert
            httpResponse.StatusCode.Should().Be(403);
        }
    } 
}