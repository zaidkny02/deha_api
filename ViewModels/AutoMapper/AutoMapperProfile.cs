using AutoMapper;
using deha_api_exam.Models;

namespace deha_api_exam.ViewModels.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<LoginViewModel, User>();
            CreateMap<RegisterViewModel, User>();
            CreateMap<User, RegisterViewModel>();
            CreateMap<Attachment, AttachmentViewModel>();
            CreateMap<AttachmentViewModel, Attachment>();
            CreateMap<AttachmentRequest, AttachmentViewModel>();
            CreateMap<AttachmentUpdateRequest, AttachmentViewModel>();
            CreateMap<Post, PostViewModel>();
            CreateMap<PostViewModel, Post>();
            CreateMap<PostRequest, Post>();
            CreateMap<PostUpdateRequest, PostViewModel>();
            CreateMap<Comment, CommentViewModel>();
            CreateMap<CommentViewModel, Comment>();
            CreateMap<CommentRequest, Comment>();
            CreateMap<CommentUpdateRequest, CommentViewModel>();
            CreateMap<User, UserViewModel>();
            CreateMap<UserViewModel, User>();
            CreateMap<VoteViewModel, Vote>();
            CreateMap<Vote, VoteViewModel>();
            CreateMap<PostViewModel, PostwithComment>();
            CreateMap<CommentViewModel, CommentView>();
            CreateMap<PostViewModel, PostwithComment>();

            //Map với trường không trùng
            /*
             CreateMap<PostViewModel, PostwithComment>()
             .ForMember(dest => dest.PostUserName, opt => opt.MapFrom(src => src.User.UserName));
             */

        }
    }
}
