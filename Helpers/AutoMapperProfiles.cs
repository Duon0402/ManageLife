using AutoMapper;
using ManageLife.Entities;
using ManageLife.Entities.ManageFinance;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<WalletEntity, WalletModel>().ReverseMap();
            CreateMap<TransactionEntity, TransactionModel>().ReverseMap();
            CreateMap<TransactionCategoryEntity, TransactionCategoryModel>().ReverseMap();
        }
    }
}
