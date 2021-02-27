namespace Application.Interfaces.Sample
{
    using System;
    using Application.Dtos.Sample;
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;

    public interface ISampleRepository
    {
        Task<PagedList<Sample>> GetSamplesAsync(SampleParametersDto SampleParameters);
        Task<Sample> GetSampleAsync(Guid SampleId);
        Sample GetSample(Guid SampleId);
        Task AddSample(Sample sample);
        void DeleteSample(Sample sample);
        void UpdateSample(Sample sample);
        bool Save();
        Task<bool> SaveAsync();
    }
}