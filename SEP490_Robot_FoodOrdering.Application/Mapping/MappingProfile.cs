

using AutoMapper;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Category;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryResponse>()
                .ReverseMap();
            CreateMap<CreateCategoryRequest, Category>()
                .ReverseMap();

        }
    }
}
