using AutoMapper;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<FileEntity, FileModel>().ReverseMap();
            CreateMap<WalletEntity, WalletModel>().ReverseMap();
            CreateMap<TransactionEntity, TransactionModel>().ReverseMap();
            CreateMap<TransactionCategoryEntity, TransactionCategoryModel>().ReverseMap();
        }
    }
}
