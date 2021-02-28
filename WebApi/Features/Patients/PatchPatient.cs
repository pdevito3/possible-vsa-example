namespace WebApi.Features.Patients
{
    using Application.Dtos.Patient;
    using Application.Exceptions;
    using Application.Validation.Patient;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Domain.Entities;
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using Infrastructure.Persistence.Contexts;
    using MediatR;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class PatchPatient
    {
        public class PatientForPatchCommand : IRequest<bool>
        {
            public Guid PatientId { get; set; }
            public JsonPatchDocument<PatientForUpdateDto> PatchDoc { get; set; }

            public PatientForPatchCommand(Guid patientId, JsonPatchDocument<PatientForUpdateDto> patchDoc)
            {
                PatientId = patientId;
                PatchDoc = patchDoc;
            }
        }

        public class Validator : AbstractValidator<PatientForUpdateDto>
        {
            // move this to a centralized location???
            public Validator()
            {
                RuleFor(p => p.LastName).NotNull().Length(1, 3);
                RuleFor(p => p.FirstName).NotNull().Length(1, 50);
                RuleFor(p => p.Dob).NotNull();
            }
        }

        public class Handler : IRequestHandler<PatientForPatchCommand, bool>
        {
            private readonly VerticalLabTestPostgresDbContext _db;
            private readonly IMapper _mapper;

            public Handler(VerticalLabTestPostgresDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<bool> Handle(PatientForPatchCommand patchCommand, CancellationToken cancellationToken)
            {
                if (patchCommand.PatchDoc == null)
                {
                    // log error
                    throw new ApiException("Invalid patch document.");
                }

                var patientToUpdate = await _db.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == patchCommand.PatientId, cancellationToken);

                if (patientToUpdate == null)
                {
                    // log error
                    throw new KeyNotFoundException();
                }

                var patientToPatch = _mapper.Map<PatientForUpdateDto>(patientToUpdate); // map the patient we got from the database to an updatable patient model
                patchCommand.PatchDoc.ApplyTo(patientToPatch); // apply patchdoc updates to the updatable patient

                var validationResults = new Validator().Validate(patientToPatch);
                if (!validationResults.IsValid)
                {
                    throw new Application.Exceptions.ValidationException(validationResults.Errors);
                }

                _mapper.Map(patientToPatch, patientToUpdate); // apply updates from the updatable patient to the db entity so we can apply the updates to the database

                return await _db.SaveChangesAsync(cancellationToken) > 0;
            }
        }
    }
}