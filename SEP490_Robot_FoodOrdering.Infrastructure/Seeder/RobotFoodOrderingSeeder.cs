
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Seeder
{
    public class RobotFoodOrderingSeeder(RobotFoodOrderingDBContext _dbContext) : IRobotFoodOrderingSeeder
    {
        public async Task Seed()
        {
            if (await _dbContext.Database.CanConnectAsync())
            {
                if (!_dbContext.Categories.Any())
                    _dbContext.Categories.AddRange(GetCategories());

                if (!_dbContext.Products.Any())
                    _dbContext.Products.AddRange(GetProducts());

                if (!_dbContext.ProductSizes.Any())
                    _dbContext.ProductSizes.AddRange(GetProductSizes());

                if (!_dbContext.Toppings.Any())
                    _dbContext.Toppings.AddRange(GetDrinkToppings());

                if (!_dbContext.ProductCategories.Any())
                    _dbContext.ProductCategories.AddRange(GetProductCategories());
                if (!_dbContext.ProductToppings.Any())
                    _dbContext.ProductToppings.AddRange(GetProductToppings());


                await _dbContext.SaveChangesAsync();
            }
        }


        public static List<Category> GetCategories()
        {
            return new List<Category>
                {
                    new() { Id = Guid.Parse("522133E8-07F5-46D5-B6BB-978D761668F5"), Name = "Đồ uống" },
                    new() { Id =Guid.Parse("AB99E70E-6B43-4974-8CD2-4D3E47EE0CDA"), Name = "Món chính" },
                    new() { Id = Guid.Parse("AE9AEC4B-3519-4E93-95E0-A98BC435DA8A"), Name = "Tráng miệng" }
                };
        }
        public static List<Product> GetProducts()
        {
            return new List<Product>
            {
                new()
                {
                    Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0"),
                    Name = "Lục Trà Sữa Song Hỷ Khoai Môn",
                    Description = "Khoai Môn Tươi nghiền mịn thủ công, vị ngon hơn đợi mong. Kết hợp Trân Châu Khoai Môn dẻo dai, quyện cùng trà sữa lài, thơm ngon khoái khoái.",
                    DurationTime = 15,
                    ImageUrl = "https://maycha.com.vn/wp-content/uploads/2025/01/Tra-Sua-Song-Hy-Khoai-Mon-768x768.png",
                    CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now
                },
                new()
                {
                    Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF1"),
                    Name = "Trà sữa olong thúy ngọc",
                    Description = "Trà đậm đà hương hoa cỏ tươi, pha chút chát nhẹ, hậu ngọt dịu bền lâu, quyện cùng sữa mang lại cảm giác nhẹ nhàng khi thưởng thức.",
                    DurationTime = 10,
                    ImageUrl = "https://maycha.com.vn/wp-content/uploads/2023/10/TS-OLONG-THUY-NGOC-600x600.png",
                    CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now

                },
                new()
                {
                    Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF2"),
                    Name = " Trà lựu đỏ thạch dừa",
                    Description =  "Mix giữa trà lài lựu đỏ chua ngọt và topping thạch dừa dai dai giòn giòn. ",
                    DurationTime = 15,
                    ImageUrl = "https://maycha.com.vn/wp-content/uploads/2023/10/LU7U8-DO-THACH-DUA-768x768.png",
                    CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now

                },
                new()
                {
                    Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF3"),
                   Name= "Trà sữa Double Cheese",
                      Description= "Nền trà đen thơm thanh hòa quyện cùng phô mai tươi và viên cheese ball thơm béo.",
                    DurationTime = 12,
                    ImageUrl = "https://cdn.tgdd.vn/Files/2021/07/30/1373823/cach-lam-burger-bo-pho-mai-ngon-khong-kem-nha-hang-202107301101536855.jpg",
                     CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now
                },
                new()
                {
                    Id = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF3"),
                    Name= "Trà lựu trân châu lộc đỏ",
                    Description= "Trà Lựu thơm thanh, chua chua ngọt ngọt, quyện cùng Trân Châu Lộc Đỏ như một lời ước nguyện may mắn và phát tài phát lộc.",

                    DurationTime = 12,
                    ImageUrl = "https://cdn.tgdd.vn/Files/2021/07/30/1373823/cach-lam-burger-bo-pho-mai-ngon-khong-kem-nha-hang-202107301101536855.jpg",
                    CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now
                }

                    };
        }


        public static List<ProductCategory> GetProductCategories()
        {
              return new List<ProductCategory>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0"), // Lục Trà Sữa Song Hỷ Khoai Môn
                        CategoryId = Guid.Parse("522133E8-07F5-46D5-B6BB-978D761668F5"), // Đồ uống
                        CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF1"), // Trà sữa olong thúy ngọc
                        CategoryId = Guid.Parse("522133E8-07F5-46D5-B6BB-978D761668F5"),
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF2"), // Trà lựu đỏ thạch dừa
                        CategoryId = Guid.Parse("522133E8-07F5-46D5-B6BB-978D761668F5"),
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF3"), // Trà sữa Double Cheese
                        CategoryId = Guid.Parse("522133E8-07F5-46D5-B6BB-978D761668F5"),
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF4"), // Trà lựu trân châu lộc đỏ
                        CategoryId = Guid.Parse("522133E8-07F5-46D5-B6BB-978D761668F5"),
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    }
                };
        }



        public static List<ProductSize> GetProductSizes()
        {
            return new List<ProductSize>
            {
                new() { Id = Guid.Parse("F09A98D1-E078-4DBB-9E07-B586F29675CB"),ProductId=Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0"), SizeName =SizeEnums.Small, Price = 35000 , CreatedTime = DateTime.Now,LastUpdatedTime = DateTime.Now},
                new() { Id = Guid.Parse("38D005CA-E093-4062-96E1-99C9B02CDB8D"),ProductId=Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0"), SizeName = SizeEnums.Medium, Price = 450000, CreatedTime = DateTime.Now,LastUpdatedTime = DateTime.Now },
                new() { Id = Guid.Parse("936A347E-4EFF-4681-B804-A7F1DD9C6B56"),ProductId=Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0"), SizeName = SizeEnums.Large, Price =55000,  CreatedTime = DateTime.Now,LastUpdatedTime = DateTime.Now }
            };

        }
        public static List<Topping> GetDrinkToppings()
        {
            return new List<Topping>
                {
                    new()
                    {
                        Id =Guid.Parse("896AC235-7804-487B-BA69-22C45B7E49A3"),
                        Name = "Trân châu đen",
                        Price = 5000,
                        CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.Parse("F5969ED7-2918-4BDE-8067-44F96AEA47AC"),
                        Name = "Thạch dừa",
                        Price = 6000,
                        CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now

                    },
                    new()
                    {
                        Id = Guid.Parse("73342532-2D81-421B-85F7-A7788C044054"),
                        Name = "Pudding trứng",
                        Price = 7000,
                         CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.Parse("22971CEE-3B6B-464E-AFAC-F9518968AC77"),
                        Name = "Trân châu trắng",
                        Price = 6000,
                         CreatedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now

                    },
                    new()
                    {
                        Id = Guid.Parse("22971CEE-3B6B-464E-AFAC-F9518968AC77"),
                        Name = "Thạch trái cây",
                        Price = 7000,
                        CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    }
                };
        }


        public static List<ProductTopping> GetProductToppings()
        {
            return new List<ProductTopping>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF0"), // Lục Trà Sữa Song Hỷ Khoai Môn
                        ToppingId = Guid.Parse("896AC235-7804-487B-BA69-22C45B7E49A3"), // Trân châu đen
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF1"), // Trà sữa olong thúy ngọc
                        ToppingId = Guid.Parse("F5969ED7-2918-4BDE-8067-44F96AEA47AC"), // Thạch dừa
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF2"), // Trà lựu đỏ thạch dừa
                        ToppingId = Guid.Parse("73342532-2D81-421B-85F7-A7788C044054"), // Pudding trứng
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF3"), // Trà sữa Double Cheese
                        ToppingId = Guid.Parse("22971CEE-3B6B-464E-AFAC-F9518968AC77"), // Trân châu trắng
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-56789ABCDEF4"), // Trà lựu trân châu lộc đỏ
                        ToppingId = Guid.Parse("A7D17884-1111-4444-A1B2-CFAE6A77FF01"), // Thạch trái cây (fix ID)
                         CreatedTime = DateTime.Now,
                        LastUpdatedTime = DateTime.Now
                    }
                };
                    }


    }
}
