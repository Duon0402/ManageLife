using AutoMapper;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Langugage
            CreateMap<LanguageEntity, LanguageModel>().ReverseMap();
            CreateMap<LanguageEntity, CreateLanguageRequest>().ReverseMap();
            CreateMap<LanguageEntity, UpdateLanguageRequest>().ReverseMap();

            // File
            CreateMap<FileEntity, FileModel>().ReverseMap();
        }
    }
}
