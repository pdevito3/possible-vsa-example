namespace Application.Validation.Sample
{
    using Application.Dtos.Sample;
    using FluentValidation;
    using System;

    public class SampleForManipulationDtoValidator<T> : AbstractValidator<T> where T : SampleForManipulationDto
    {
        public SampleForManipulationDtoValidator()
        {
            // add fluent validation rules that should be shared between creation and update operations here
            //https://fluentvalidation.net/
        }
    }
}