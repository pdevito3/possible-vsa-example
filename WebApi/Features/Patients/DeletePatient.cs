namespace WebApi.Features.Patients
{
    using AutoMapper;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using MediatR;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    using FluentValidation;
    using System.Threading;
    using Application.Dtos.Patient;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;

    public class DeletePatient
    {
        public class PatientForDeleteCommand : IRequest<bool>
        {
            public Guid PatientId { get; set; }

            public PatientForDeleteCommand(Guid patientId)
            {
                PatientId = patientId;
            }
        }

        public class Handler : IRequestHandler<PatientForDeleteCommand, bool>
        {
            private readonly VerticalLabTestPostgresDbContext _db;

            public Handler(VerticalLabTestPostgresDbContext db)
            {
                _db = db;
            }

            public async Task<bool> Handle(PatientForDeleteCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var patient = await _db.Patients
                        .FirstOrDefaultAsync(p => p.PatientId == request.PatientId, cancellationToken);

                    if (patient == null)
                    {
                        // log error
                        throw new KeyNotFoundException();
                    }

                    _db.Patients.Remove(patient);
                    return await _db.SaveChangesAsync(cancellationToken) > 0;

                }
                catch(Exception e)
                {
                    // logger message
                    throw new Exception("Save error. Should throw a 500 from here");
                }

            }
        }
    }
}
