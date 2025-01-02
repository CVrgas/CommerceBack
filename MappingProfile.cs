using AutoMapper;
using CommerceBack.Common;
using CommerceBack.DTOs.Product;
using CommerceBack.Entities;

namespace CommerceBack;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product
        CreateMap<ProductDto, Product>();
        CreateMap<Product, ProductDto>();
        CreateMap<PaginatedResponse<ProductDto>, PaginatedResponse<Product>>();
        CreateMap<PaginatedResponse<Product>, PaginatedResponse<ProductDto>>();
    }
    
}