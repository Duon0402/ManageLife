using AutoMapper;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
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
            CreateMap<LanguageEntity, LanguageModel>().ReverseMap();
            CreateMap<LanguageEntity, CreateLanguageRequest>().ReverseMap();
            CreateMap<LanguageEntity, UpdateLanguageRequest>().ReverseMap();

            // File
            CreateMap<FileEntity, FileModel>().ReverseMap();
        }
    }
}
