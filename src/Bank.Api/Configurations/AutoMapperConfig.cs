using AutoMapper;
using Bank.Domain.Entities;
using Bank.Api.Models.Transaction.Commands;
using Bank.Api.Models.Transaction.Queries;
using Bank.Domain.Enums;
using static Bank.Domain.Enums.TransactionStatusEnum;

namespace Bank.Api.Configurations
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Transaction, CreateTransactionCommand>().ReverseMap();
            CreateMap<Transaction, ResponseTransactionQuery>()
                .ForMember(
                    dest => dest.TransactionId,
                    opt => opt.MapFrom(src => src.Id)
                    );
            CreateMap<Transaction, ResponseTransactionStatusQuery>()
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => Enumerations.GetEnumDescription((TransactionStatus)src.Status))
                    );
            CreateMap<Transaction, ResponseTransactionErrorQuery>()
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => Enumerations.GetEnumDescription((TransactionStatus)src.Status))
                    )
                .ForMember(
                    dest => dest.Message,
                    opt => opt.MapFrom(src => src.Message??"")
                    );
        }
    }
}
