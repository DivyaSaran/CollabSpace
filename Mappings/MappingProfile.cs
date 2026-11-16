using AutoMapper;
using CollabSpace.DTOs;
using CollabSpace.Models;

namespace CollabSpace.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ==========================================
            // 1. User Mappings
            // ==========================================
            // Entity -> DTO (Reading data)
            CreateMap<User, UserDto>();

            // DTO -> Entity (Creating/Updating data)
            CreateMap<UserCreateUpdateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());


            // ==========================================
            // 2. Workspace Mappings
            // ==========================================
            // Entity -> DTO
            CreateMap<Workspace, WorkspaceDto>()
                // OwnerName and CurrentUserRole do not exist directly on the Workspace entity,
                // so we explicitly tell AutoMapper not to throw an error when mapping them.
                // Our repository will populate them dynamically.
                .ForMember(dest => dest.OwnerName, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserRole, opt => opt.Ignore());

            // DTO -> Entity (Creating a new workspace)
            CreateMap<WorkspaceCreateDto, Workspace>();


            // ==========================================
            // 3. WorkspaceMember Mappings
            // ==========================================
            // Flatten nested User properties into the Member DTO
            CreateMap<WorkspaceMember, WorkspaceMemberDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

            // ==========================================
            // 4. WorkspaceAccessRequest Mappings
            // ==========================================

            CreateMap<WorkspaceAccessRequest, WorkspaceAccessRequestDto>()
                .ForMember(dest => dest.WorkspaceName, opt => opt.MapFrom(src => src.Workspace.Name))
                .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => src.RequesterUser.Name))
                .ForMember(dest => dest.RequesterEmail, opt => opt.MapFrom(src => src.RequesterUser.Email));

            // ==========================================
            // 5. WorkspaceInvite Mappings
            // ==========================================

            CreateMap<WorkspaceInvite, WorkspaceInviteDto>()
                .ForMember(dest => dest.WorkspaceName, opt => opt.MapFrom(src => src.Workspace.Name))
                .ForMember(dest => dest.WorkspaceDescription, opt => opt.MapFrom(src => src.Workspace.Description))
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.SenderUser.Name));


        }
    }
}