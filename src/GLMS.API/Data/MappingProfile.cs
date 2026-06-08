using AutoMapper;
using GLMS.API.Models;
using GLMS.API.Models.DTOs;

namespace GLMS.API.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Contract mappings
            CreateMap<Contract, ContractDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.ServiceRequestCount, opt => opt.MapFrom(src => src.ServiceRequests.Count));

            CreateMap<CreateContractDto, Contract>();
            CreateMap<UpdateContractDto, Contract>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ServiceRequest mappings
            CreateMap<ServiceRequest, ServiceRequestDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ContractNumber, opt => opt.MapFrom(src => src.Contract.ContractNumber))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Contract.ClientName));

            CreateMap<CreateServiceRequestDto, ServiceRequest>();
        }
    }
}