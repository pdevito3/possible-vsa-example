namespace WebApi.Features.Patients
{
    using Application.Dtos.Patient;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Domain.Entities;
    using FluentValidation;
    using Infrastructure.Persistence.Contexts;
    using MediatR;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using WebApi.Features.Patients.Validators;

    public class UpdatePatient
    {
        public class PatientForUpdateCommand : IRequest<bool>
        {
            public Guid PatientId { get; set; }
            public PatientForUpdateDto PatientForUpdateDto { get; }

            public PatientForUpdateCommand(Guid patientId, PatientForUpdateDto patientForUpdateDto)
            {
                PatientId = patientId;
                PatientForUpdateDto = patientForUpdateDto;
            }
        }

        public class CustomPatchPatientValidation : PatientForManipulationDtoValidator<PatientForUpdateDto>
        {
            public CustomPatchPatientValidation()
            {
            }
        }

        public class Handler : IRequestHandler<PatientForUpdateCommand, bool>
        {
            private readonly VerticalLabTestPostgresDbContext _db;
            private readonly IMapper _mapper;

            public Handler(VerticalLabTestPostgresDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<bool> Handle(PatientForUpdateCommand updateCommand, CancellationToken cancellationToken)
            {
                var patientToUpdate = await _db.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == updateCommand.PatientId, cancellationToken);

                if (patientToUpdate == null)
                {
                    // log error
                    throw new KeyNotFoundException();
                }

                _mapper.Map(updateCommand.PatientForUpdateDto, patientToUpdate);
                return await _db.SaveChangesAsync(cancellationToken) > 0;
                //change this to return a Task.CompletedTask or something other than bool?
            }
        }
    }
}