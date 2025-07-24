

using AutoMapper;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;
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

            #region Product Mapping
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
                .ReverseMap();


            CreateMap<ProductSize, ProductSizeResponse>();
                
            CreateMap<Product, ProductDetailResponse>()
       .ForMember(dest => dest.Price, opt =>
           opt.MapFrom(src => src.Sizes != null && src.Sizes.Any()
               ? src.Sizes.Min(size => size.Price)
               : 0)) // fallback nếu không có size
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
           .ForMember(dest => dest.UrlImg, opt => opt.MapFrom(src => src.ImageUrl))
           .ForMember(dest => dest.Sizes, opt => opt.MapFrom(src => src.Sizes));
          




            CreateMap<CreateProductRequest, Product>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); // Handle file upload separately
            #endregion  

            CreateMap<ProductCategory, ProductCategoryResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
                .ForMember(dest => dest.UrlImg, opt => opt.MapFrom(src => src.Product.ImageUrl))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ReverseMap();

            CreateMap<ProductSize, ProductSizeResponse>().ReverseMap();
            CreateMap<CreateProductSizeRequest, ProductSize>().ReverseMap();
            CreateMap<ProductTopping, ProductToppingResponse>().ReverseMap();
            CreateMap<CreateProductToppingRequest, ProductTopping>().ReverseMap();
            CreateMap<Table, TableResponse>().ReverseMap();
            CreateMap<CreateTableRequest, Table>().ReverseMap();


            CreateMap<Topping,ToppingResponse>().ReverseMap();
            CreateMap<CreateToppingRequest, Topping>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) 
                .ReverseMap();
            
            // Order Mappings
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore()) // handled in service
                .ForMember(dest => dest.TableId, opt => opt.MapFrom(src => src.TableId));
            CreateMap<CreateOrderItemRequest, OrderItem>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => SEP490_Robot_FoodOrdering.Domain.Enums.OrderItemStatus.Pending));
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.ProductSize.SizeName.ToString()));
            
        }
    }
}
