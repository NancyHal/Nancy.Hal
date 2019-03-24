using AutoMapper;
using Nancy.Hal.Example.Model.Users.ViewModels;

namespace Nancy.Hal.Example.Mapping
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<UserDetails, UserSummary>();
            CreateMap<RoleDetails, Role>();
        }
    }
}
