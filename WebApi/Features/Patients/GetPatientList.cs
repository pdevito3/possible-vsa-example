namespace WebApi.Features.Patients
{
    using Application.Dtos.Patient;
    using Application.Exceptions;
    using Application.Wrappers;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetPatientList
    {
        public class PatientListQuery : IRequest<PagedList<PatientDto>>
        {
            public PatientParametersDto PatientParametersDto { get; set; }

            public PatientListQuery(PatientParametersDto patientParametersDto)
            {
                PatientParametersDto = patientParametersDto;
            }
        }

        public class Handler : IRequestHandler<PatientListQuery, PagedList<PatientDto>>
        {
            private readonly VerticalLabTestPostgresDbContext _db;
            private readonly SieveProcessor _sieveProcessor;
            private readonly IMapper _mapper;

            public Handler(VerticalLabTestPostgresDbContext db, IMapper mapper, SieveProcessor sieveProcessor)
            {
                _mapper = mapper;
                _db = db;
                _sieveProcessor = sieveProcessor;
            }

            public async Task<PagedList<PatientDto>> Handle(PatientListQuery request, CancellationToken cancellationToken)
            {
                if (request.PatientParametersDto == null)
                {
                    // log error
                    throw new ApiException("Invalid query parameters.");
                }

                var collection = _db.Patients
                    .ProjectTo<PatientDto>(_mapper.ConfigurationProvider);

                var sieveModel = new SieveModel
                {
                    Sorts = request.PatientParametersDto.SortOrder ?? "PatientId",
                    Filters = request.PatientParametersDto.Filters
                };

                collection = _sieveProcessor.Apply(sieveModel, collection);

                return await PagedList<PatientDto>.CreateAsync(collection,
                    request.PatientParametersDto.PageNumber,
                    request.PatientParametersDto.PageSize,
                    cancellationToken);
            }
        }
    }
}