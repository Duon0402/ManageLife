using AutoMapper;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
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
