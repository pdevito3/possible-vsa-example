
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
    using Infrastructure.Persistence.Contexts;
    using Microsoft.Extensions.DependencyInjection;
    using Application.Wrappers;

    [Collection("Sequential")]
    public class GetSampleIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public GetSampleIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task GetSamples_ReturnsSuccessCodeAndResourceWithAccurateFields()
        {
            var fakeSampleOne = new FakeSample { }.Generate();
            var fakeSampleTwo = new FakeSample { }.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VerticalLabTestPostgresDbContext>();
                context.Database.EnsureCreated();

                //context.Samples.RemoveRange(context.Samples);
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo);
                context.SaveChanges();
            }

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var result = await client.GetAsync("api/Samples")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<Response<IEnumerable<SampleDto>>>(responseContent)?.Data;

            // Assert
            result.StatusCode.Should().Be(200);
            response.Should().ContainEquivalentOf(fakeSampleOne, options =>
                options.ExcludingMissingMembers());
            response.Should().ContainEquivalentOf(fakeSampleTwo, options =>
                options.ExcludingMissingMembers());
        }
        
        [Fact]
        public async Task GetSample_ReturnsSuccessCodeAndResourceWithAccurateFields()
        {
            var fakeSampleOne = new FakeSample { }.Generate();
            var fakeSampleTwo = new FakeSample { }.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VerticalLabTestPostgresDbContext>();
                context.Database.EnsureCreated();

                //context.Samples.RemoveRange(context.Samples);
                context.Samples.AddRange(fakeSampleOne, fakeSampleTwo);
                context.SaveChanges();
            }

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var result = await client.GetAsync($"api/Samples/{fakeSampleOne.SampleId}")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<Response<SampleDto>>(responseContent)?.Data;

            // Assert
            result.StatusCode.Should().Be(200);
            response.Should().BeEquivalentTo(fakeSampleOne, options =>
                options.ExcludingMissingMembers());
        }
    } 
}