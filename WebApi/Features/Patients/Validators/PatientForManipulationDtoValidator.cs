namespace WebApi.Features.Patients.Validators
{
    using Application.Dtos.Patient;
    using FluentValidation;
    using System;

    public class PatientForManipulationDtoValidator<T> : AbstractValidator<T> where T : PatientForManipulationDto
    {
        public PatientForManipulationDtoValidator()
        {
            RuleFor(p => p.LastName).NotNull().Length(1, 3);
            RuleFor(p => p.FirstName).NotNull().Length(1, 50);
            RuleFor(p => p.Dob).NotNull();
        }
    }
}