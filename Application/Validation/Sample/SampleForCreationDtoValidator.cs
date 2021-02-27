namespace Application.Validation.Sample
{
    using Application.Dtos.Sample;
    using FluentValidation;

    public class SampleForCreationDtoValidator: SampleForManipulationDtoValidator<SampleForCreationDto>
    {
        public SampleForCreationDtoValidator()
        {
            // add fluent validation rules that should only be run on creation operations here
            //https://fluentvalidation.net/
        }
    }
}