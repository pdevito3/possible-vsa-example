namespace Application.Dtos.Sample
{
    using Application.Dtos.Shared;

    public class SampleParametersDto : BasePaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}