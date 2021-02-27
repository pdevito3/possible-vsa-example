
namespace VerticalLabTestPostgres.Api.Tests.IntegrationTests.Sample
{
    using Application.Dtos.Sample;
    using FluentAssertions;
    using VerticalLabTestPostgres.Api.Tests.Fakes.Sample;
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using WebApi;
    using System.Collections.Generic;
    using Application.Wrappers;

    [Collection("Sequential")]
    public class CreateSampleIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public CreateSampleIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task PostSampleReturnsSuccessCodeAndResourceWithAccurateFields()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var fakeSample = new FakeSampleDto().Generate();

            // Act
            var httpResponse = await client.PostAsJsonAsync("api/Samples", fakeSample)
                .ConfigureAwait(false);

            // Assert
            httpResponse.EnsureSuccessStatusCode();

            var resultDto = JsonConvert.DeserializeObject<Response<SampleDto>>(await httpResponse.Content.ReadAsStringAsync()
                .ConfigureAwait(false));

            httpResponse.StatusCode.Should().Be(201);
            resultDto.Data.ExternalId.Should().Be(fakeSample.ExternalId);
            resultDto.Data.InternalId.Should().Be(fakeSample.InternalId);
            resultDto.Data.SampleType.Should().Be(fakeSample.SampleType);
            resultDto.Data.ContainerType.Should().Be(fakeSample.ContainerType);
            resultDto.Data.CollectionDate.Should().Be(fakeSample.CollectionDate);
            resultDto.Data.ArrivalDate.Should().Be(fakeSample.ArrivalDate);
            resultDto.Data.Amount.Should().Be(fakeSample.Amount);
            resultDto.Data.AmountUnits.Should().Be(fakeSample.AmountUnits);
        }
    } 
}