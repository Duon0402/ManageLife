using AutoMapper;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // UserTelegramConnection
            CreateMap<UserTelegramConnectionEntity, UserTelegramConnectionModel>().ReverseMap();
            CreateMap<UserTelegramConnectionEntity, CreateUserTelegramConnectionRequest>().ReverseMap();
            CreateMap<UserTelegramConnectionEntity, UpdateUserTelegramConnectionRequest>().ReverseMap();

            // Role
            CreateMap<RoleEntity, RoleModel>().ReverseMap();
            CreateMap<RoleEntity, CreateRoleRequest>().ReverseMap();

            // User
            CreateMap<UserEntity, UserModel>().ReverseMap();

            // Exception
            CreateMap<ExceptionItemEntity, ExceptionItemModel>().ReverseMap();

            // TodoList
            CreateMap<TodoListEntity, TodoListModel>().ReverseMap();
            CreateMap<TodoListEntity, CreateToDoListRequest>().ReverseMap();
            CreateMap<TodoListEntity, UpdateToDoListRequest>().ReverseMap();

            // Permission
            CreateMap<PermissionEntity, PermissionModel>().ReverseMap();

            // Translation
            CreateMap<TranslationEntity, TranslationModel>().ReverseMap();
            CreateMap<TranslationEntity, CreateTranslationRequest>().ReverseMap();
            CreateMap<TranslationEntity, UpdateLanguageRequest>().ReverseMap();

            // Langugage
            CreateMap<ChangeLanguageRequest, ChangeLanguageResult>().ReverseMap();
            CreateMap<LanguageEntity, LanguageModel>().ReverseMap();
            CreateMap<LanguageEntity, CreateLanguageRequest>().ReverseMap();
            CreateMap<LanguageEntity, UpdateLanguageRequest>().ReverseMap();

            // File
            CreateMap<FileEntity, FileModel>().ReverseMap();

            // Chat
            CreateMap<ChatMessageEntity, ChatMessageModel>().ReverseMap();
        }
    }
}
