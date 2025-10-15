using AutoMapper;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.CancelledItem;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.User;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

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
            CreateMap<ProductTopping, ProductToppingResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ToppingName, opt => opt.MapFrom(src => src.Topping.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Topping.Price))
                .ReverseMap();
            CreateMap<CreateProductToppingRequest, ProductTopping>().ReverseMap();
            CreateMap<Table, TableResponse>().ReverseMap();
            CreateMap<CreateTableRequest, Table>().ReverseMap();


            CreateMap<Topping, ToppingResponse>().ReverseMap();
            CreateMap<CreateToppingRequest, Topping>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ReverseMap();

            //CreateMap<Product, ProductWithToppingsResponse>()
            //    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
            //    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
            //    .ForMember(dest => dest.Toppings, opt => opt.MapFrom(src =>
            //        src.AvailableToppings.Select(pt => pt.Topping)
            //    )).ReverseMap();
            CreateMap<ProductTopping, ProductWithToppingsResponse>()
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.Id))
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                    .ForMember(dest => dest.Toppings, opt => opt.MapFrom(src =>
                        src.Product != null && src.Product.AvailableToppings != null
                            ? src.Product.AvailableToppings
                                .Where(pt => pt.Topping != null)
                                .Select(pt => pt.Topping)
                            : new List<Topping>()
                    ));


            // Order Mappings
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore()) // handled in service
                .ForMember(dest => dest.TableId, opt => opt.MapFrom(src => src.TableId));
            CreateMap<CreateOrderItemRequest, OrderItem>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => SEP490_Robot_FoodOrdering.Domain.Enums.OrderItemStatus.Pending));

            CreateMap<Order, OrderResponse>()
             .ForMember(dest => dest.TableName, opt => opt.MapFrom(src => src.Table.Name))
             .ForMember(dest => dest.deviderId, opt => opt.MapFrom(src => src.Table.DeviceId.ToString()))
             .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems)); // Items instead of OrderItems

            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.ProductSize.SizeName.ToString()))
                //.ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.ProductSize != null ? src.ProductSize.Price : 0)) // Map price from ProductSize
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                src.Status == OrderItemStatus.Cancelled
                    ? 0
                    : (src.ProductSize != null ? src.ProductSize.Price +
                        (src.OrderItemTopping != null ? src.OrderItemTopping.Sum(t => t.Price) : 0)
                      : 0)
                 ))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : null))

                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.Toppings, opt => opt.MapFrom(src =>
                    src.OrderItemTopping != null && src.OrderItemTopping.Count > 0
                        ? src.OrderItemTopping.Select(oit => oit.Topping).ToList()
                        : new List<Topping>())); // Direct Top  ping entities

            CreateMap<CancelledOrderItem, CancelledItemResponse>()

        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.OrderItem.ProductSize.Product.Name))
        .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.OrderItem.ProductSize.SizeName))
        .ForMember(
                    dest => dest.ToppingName,
                    opt => opt.MapFrom(src =>
                        src.OrderItem.Product.AvailableToppings != null
                            ? string.Join(", ", src.OrderItem.Product.AvailableToppings
                                .Select(t => t.Topping.Name))
                            : string.Empty
                    )
)

                .ForMember(dest => dest.CancelledByUserName, opt => opt.MapFrom(src => src.CancelledByUser.FullName));

            CreateMap<RemakeOrderItem, RemakeOrderItemsResponse>()
            .ForMember(dest => dest.RemakeOrderItemId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OriginalOrderItemId, opt => opt.MapFrom(src => src.OrderItemId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.OrderItem.Product.Name))
            .ForMember(dest => dest.ProductSize, opt => opt.MapFrom(src => src.OrderItem.ProductSize.Price))
            .ForMember(dest => dest.Toppings,
                            opt => opt.MapFrom(src =>
                                string.Join(", ",
                                    src.OrderItem.Product.AvailableToppings
                                        .Select(t => t.Topping.Name)
                                )))
        .ForMember(dest => dest.RemakeCount, opt => opt.MapFrom(src => src.OrderItem.RemakeOrderItems.Count()))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.RemakeNote))
            .ForMember(dest => dest.PreviousStatus, opt => opt.MapFrom(src => src.PreviousStatus))
            .ForMember(dest => dest.CurrentStatus, opt => opt.MapFrom(src => src.AfterStatus))
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))

            .ForMember(dest => dest.LastUpdatedTime, opt => opt.MapFrom(src => src.LastUpdatedTime))
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderItem.OrderId))
            .ForMember(dest => dest.TableName, opt => opt.MapFrom(src => src.OrderItem.Order.Table.Name))
            .ForMember(dest => dest.OrderCreatedTime, opt => opt.MapFrom(src => src.OrderItem.Order.CreatedTime));



            CreateMap<Invoice, InvoiceResponse>()
            .ForMember(dest => dest.TableName, opt => opt.MapFrom(src => src.Table != null ? src.Table.Name : string.Empty))
            .ForMember(dest => dest.TotalMoney, opt => opt.MapFrom(src => src.totalMoney))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.status.ToString()))

            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime));

            CreateMap<InvoiceDetail, InvoiceDetailResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.OrderItem.Product.Name))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.OrderItem.ProductSize.Price))
                .ForMember(dest => dest.TotalMoney, opt => opt.MapFrom(src => src.totalMoney));

            #region user - authentication mapping
            CreateMap<User  , UserProfileResponse>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name.ToString()))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avartar))
                .ReverseMap()
                .ForMember(dest => dest.Role, opt => opt.Ignore()); // Prevent circular reference

            #endregion


        }


    }
}
