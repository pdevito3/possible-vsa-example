namespace Application.Mappings
{
    using Application.Dtos.Sample;
    using AutoMapper;
    using Domain.Entities;

    public class SampleProfile : Profile
    {
        public SampleProfile()
        {
            //createmap<to this, from this>
            CreateMap<Sample, SampleDto>()
                .ReverseMap();
            CreateMap<SampleForCreationDto, Sample>();
            CreateMap<SampleForUpdateDto, Sample>()
                .ReverseMap();
        }
    }
}