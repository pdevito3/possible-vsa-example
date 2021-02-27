namespace Application.Validation.Sample
{
    using Application.Dtos.Sample;
    using FluentValidation;

    public class SampleForUpdateDtoValidator: SampleForManipulationDtoValidator<SampleForUpdateDto>
    {
        public SampleForUpdateDtoValidator()
        {
            // add fluent validation rules that should only be run on update operations here
            //https://fluentvalidation.net/
        }
    }
}