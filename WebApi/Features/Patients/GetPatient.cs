namespace WebApi.Features.Patients
{
    using Application.Dtos.Patient;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Infrastructure.Persistence.Contexts;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetPatient
    {
        public class PatientQuery : IRequest<PatientDto>
        {
            public Guid PatientId { get; set; }

            public PatientQuery(Guid patientId)
            {
                PatientId = patientId;
            }
        }

        public class Handler : IRequestHandler<PatientQuery, PatientDto>
        {
            private readonly VerticalLabTestPostgresDbContext _db;
            private readonly IMapper _mapper;

            public Handler(VerticalLabTestPostgresDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<PatientDto> Handle(PatientQuery request, CancellationToken cancellationToken)
            {
                // add a try catch with a logger here (and on all features)

                return await _db.Patients
                    .ProjectTo<PatientDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(p => p.PatientId == request.PatientId, cancellationToken);
            }
        }
    }
}