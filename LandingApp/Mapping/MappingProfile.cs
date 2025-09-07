using AutoMapper;
using LandingApp.Dto;
using LandingApp.Models;

namespace LandingApp.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LeadDto, LeadModel>().ReverseMap();
            CreateMap<TariffDto, TariffModel>().ReverseMap();
        }
    }
}
