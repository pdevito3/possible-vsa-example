﻿namespace WebApi.Features.Patients
{
    using Application.Dtos.Patient;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using WebApi.Features.Patients.Validation;

    public class CreatePatient
    {
        public class PatientForCreationCommand : IRequest<PatientDto>
        {
            public PatientForCreationDto CreationCommand { get; set; }

            public PatientForCreationCommand(PatientForCreationDto patientForCreationDto)
            {
                CreationCommand = patientForCreationDto;
            }
        }

        public class CustomCreatePatientValidation : PatientForManipulationDtoValidator<PatientForCreationDto>
        {
            public CustomCreatePatientValidation()
            {
            }
        }

        public class Handler : IRequestHandler<PatientForCreationCommand, PatientDto>
        {
            private readonly VerticalLabTestPostgresDbContext _db;
            private readonly IMapper _mapper;

            public Handler(VerticalLabTestPostgresDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<PatientDto> Handle(PatientForCreationCommand request, CancellationToken cancellationToken)
            {
                var patient = _mapper.Map<Patient>(request.CreationCommand);
                _db.Patients.Add(patient);
                var saveSuccessful = await _db.SaveChangesAsync(cancellationToken) > 0;

                if (saveSuccessful)
                {
                    // change this to a mediatr query? that would couple 'between slices' though which is something to minimize
                    return await _db.Patients
                        .ProjectTo<PatientDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefaultAsync(p => p.PatientId == patient.PatientId, cancellationToken); ;
                }
                else
                {
                    // logger message
                    throw new Exception("Save error. Should throw a 500 from here");
                }
            }
        }
    }
}