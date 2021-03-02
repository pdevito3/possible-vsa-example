namespace WebApi.Features.Patients.Mappings
{
    using Application.Dtos.Patient;
    using Application.Wrappers;
    using AutoMapper;
    using Domain.Entities;
    using System.Linq;

    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            CreateMap<IQueryable<Patient>, IQueryable<PatientDto>>()
                .ReverseMap();
            //createmap<to this, from this>
            CreateMap<Patient, PatientDto>()
                .ReverseMap();
            CreateMap<PatientForCreationDto, Patient>();
            CreateMap<PatientForUpdateDto, Patient>()
                .ReverseMap();
        }
    }
}