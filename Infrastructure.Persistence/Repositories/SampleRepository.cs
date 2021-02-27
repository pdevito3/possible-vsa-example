namespace Infrastructure.Persistence.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Dtos.Sample;
    using Application.Interfaces.Sample;
    using Application.Wrappers;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;

    public class SampleRepository : ISampleRepository
    {
        private VerticalLabTestPostgresDbContext _context;
        private readonly SieveProcessor _sieveProcessor;

        public SampleRepository(VerticalLabTestPostgresDbContext context,
            SieveProcessor sieveProcessor)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _sieveProcessor = sieveProcessor ??
                throw new ArgumentNullException(nameof(sieveProcessor));
        }

        public async Task<PagedList<Sample>> GetSamplesAsync(SampleParametersDto sampleParameters)
        {
            if (sampleParameters == null)
            {
                throw new ArgumentNullException(nameof(sampleParameters));
            }

            var collection = _context.Samples
                as IQueryable<Sample>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            var sieveModel = new SieveModel
            {
                Sorts = sampleParameters.SortOrder ?? "SampleId",
                Filters = sampleParameters.Filters
            };

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return await PagedList<Sample>.CreateAsync(collection,
                sampleParameters.PageNumber,
                sampleParameters.PageSize);
        }

        public async Task<Sample> GetSampleAsync(Guid sampleId)
        {
            // include marker -- requires return _context.Samples as it's own line with no extra text -- do not delete this comment
            return await _context.Samples
                .FirstOrDefaultAsync(s => s.SampleId == sampleId);
        }

        public Sample GetSample(Guid sampleId)
        {
            // include marker -- requires return _context.Samples as it's own line with no extra text -- do not delete this comment
            return _context.Samples
                .FirstOrDefault(s => s.SampleId == sampleId);
        }

        public async Task AddSample(Sample sample)
        {
            if (sample == null)
            {
                throw new ArgumentNullException(nameof(Sample));
            }

            await _context.Samples.AddAsync(sample);
        }

        public void DeleteSample(Sample sample)
        {
            if (sample == null)
            {
                throw new ArgumentNullException(nameof(Sample));
            }

            _context.Samples.Remove(sample);
        }

        public void UpdateSample(Sample sample)
        {
            // no implementation for now
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}