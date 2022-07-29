using System.Linq;
using Application.Activities;
using Application.Comments;
using AutoMapper;
using Domain;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            string currentUsername = null;

            CreateMap<Activity, Activity>();
            CreateMap<Activity, ActivityDto>()
                .ForMember(dest => dest.HostUsername, options => options.MapFrom(src => src.Attendees.FirstOrDefault(x => x.IsHost).AppUser.UserName));
            //.ForMember(dest => dest.Attendees, opt => opt.MapFrom(src => src.Attendees.Select(x => x.AppUser)));

            CreateMap<ActivityAttendee, AttendeeDto>()
                .ForMember(dest => dest.DisplayName, options => options.MapFrom(src => src.AppUser.DisplayName))
                .ForMember(dest => dest.Username, options => options.MapFrom(src => src.AppUser.UserName))
                .ForMember(dest => dest.Image, options => options.MapFrom(src => src.AppUser.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Bio, options => options.MapFrom(src => src.AppUser.Bio))
                .ForMember(d => d.FollowersCount, o => o.MapFrom(s => s.AppUser.Followers.Count))
                .ForMember(d => d.FollowingCount, o => o.MapFrom(s => s.AppUser.Followings.Count))
                .ForMember(d => d.Following, o => o.MapFrom(s => s.AppUser.Followings.Any(x => x.Observer.UserName == currentUsername)));

            CreateMap<AppUser, Profiles.Profile>().ForMember(dest => dest.Image, options => options.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
            .ForMember(d => d.FollowersCount, o => o.MapFrom(s => s.Followers.Count))
            .ForMember(d => d.FollowingCount, o => o.MapFrom(s => s.Followings.Count))
            .ForMember(d => d.Following, o => o.MapFrom(s => s.Followings.Any(x => x.Observer.UserName == currentUsername)));

            CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.DisplayName, options => options.MapFrom(src => src.Author.DisplayName))
            .ForMember(dest => dest.Username, options => options.MapFrom(src => src.Author.UserName))
            .ForMember(dest => dest.Image, options => options.MapFrom(src => src.Author.Photos.FirstOrDefault(x => x.IsMain).Url));

        }
    }
}