using System.Linq;
using Application.Activities;
using AutoMapper;
using Domain;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Activity, Activity>();
            CreateMap<Activity, ActivityDto>()
                .ForMember(dest => dest.HostUsername, options => options.MapFrom(src => src.Attendees.FirstOrDefault(x => x.IsHost).AppUser.UserName));
                //.ForMember(dest => dest.Attendees, opt => opt.MapFrom(src => src.Attendees.Select(x => x.AppUser)));

            CreateMap<ActivityAttendee, AttendeeDto>()
                .ForMember(dest => dest.DisplayName, options => options.MapFrom(src => src.AppUser.DisplayName))
                .ForMember(dest => dest.Username, options => options.MapFrom(src => src.AppUser.UserName))
                .ForMember(dest => dest.Image, options => options.MapFrom(src => src.AppUser.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Bio, options => options.MapFrom(src => src.AppUser.Bio));

                CreateMap<AppUser, Profiles.Profile>().ForMember(dest => dest.Image, options => options.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url));

        }
    }
}