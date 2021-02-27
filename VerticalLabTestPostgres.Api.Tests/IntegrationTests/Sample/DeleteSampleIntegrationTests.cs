
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
    using Microsoft.AspNetCore.JsonPatch;
    using System.Linq;
    using AutoMapper;
    using Bogus;
    using Application.Mappings;
    using System.Text;
    using Application.Wrappers;

    [Collection("Sequential")]
    public class DeleteSampleIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public DeleteSampleIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task DeleteSample204AndFieldsWereSuccessfullyUpdated()
        {
            //Arrange
            var fakeSampleOne = new FakeSample { }.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VerticalLabTestPostgresDbContext> ();
                context.Database.EnsureCreated();

                context.Samples.RemoveRange(context.Samples);
                context.Samples.AddRange(fakeSampleOne);
                context.SaveChanges();
            }

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            // get the value i want to update. assumes I can use sieve for this field. if this is not an option, just use something else
            var getResult = await client.GetAsync($"api/Samples/?filters=ExternalId=={fakeSampleOne.ExternalId}")
                .ConfigureAwait(false);
            var getResponseContent = await getResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var getResponse = JsonConvert.DeserializeObject<Response<IEnumerable<SampleDto>>>(getResponseContent);
            var id = getResponse.Data.FirstOrDefault().SampleId;

            // delete it
            var method = new HttpMethod("DELETE");
            var deleteRequest = new HttpRequestMessage(method, $"api/Samples/{id}");
            var deleteResult = await client.SendAsync(deleteRequest)
                .ConfigureAwait(false);

            // get it again to confirm updates
            var checkResult = await client.GetAsync($"api/Samples/{id}")
                .ConfigureAwait(false);
            var checkResponseContent = await checkResult.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var checkResponse = JsonConvert.DeserializeObject<Response<SampleDto>>(checkResponseContent);

            // Assert
            deleteResult.StatusCode.Should().Be(204);
            checkResponse.Data.Should().Be(null);
        }
    } 
}