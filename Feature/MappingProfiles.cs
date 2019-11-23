using AutoMapper;
using HAS.Registration.Models;
using MongoDB.Bson;
using static HAS.Registration.Data.GatedRegistrationContext;

namespace HAS.Registration.Feature
{
    public class InvitedUserDAOProfile : Profile
    {
        public InvitedUserDAOProfile()
        {
            CreateMap<InvitedUser, InvitedUserDAO>()
                .ForMember(m => m.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(src.Id)));
            CreateMap<InvitedUserLogEntry, InvitedUserLogEntryDAO>();
        }
    }

    public class InvitedUserProfile : Profile
    {
        public InvitedUserProfile()
        {
            CreateMap<InvitedUserDAO, InvitedUser>();
            CreateMap<InvitedUserLogEntryDAO, InvitedUserLogEntry>();
        }
    }
}
