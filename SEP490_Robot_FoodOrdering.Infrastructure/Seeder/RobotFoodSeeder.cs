using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Seeder
{
    public class RobotFoodSeeder(RobotFoodOrderingDBContext _dbContext) : IRobotFoodSeeder
    {
        public async Task Seed()
        {
            if (await _dbContext.Database.CanConnectAsync())
            {
                if (!_dbContext.Categories.Any())
                {
                    var categories = GetCategories();
                    await _dbContext.Categories.AddRangeAsync(categories);
                }
                if (!_dbContext.Toppings.Any())
                {
                    var toppings = GetToppings();
                    await _dbContext.Toppings.AddRangeAsync(toppings);
                }
                if (!_dbContext.Tables.Any())
                {
                    var tables = GetTables();
                    await _dbContext.Tables.AddRangeAsync(tables);
                }
                if (!_dbContext.Products.Any())
                {
                    var products = GetProducts();
                    await _dbContext.Products.AddRangeAsync(products);
                }
                if (!_dbContext.ProductSizes.Any())
                {
                    var productSizes = GetProductSizes();
                    await _dbContext.ProductSizes.AddRangeAsync(productSizes);
                }

                if(!_dbContext.ProductToppings.Any())
                {
                    var productToppings = GetProductToppings();
                    await _dbContext.ProductToppings.AddRangeAsync(productToppings);
                }
                if (!_dbContext.ProductCategories.Any())
                {
                    var productCategory = GetProductCategories();   
                     await _dbContext.ProductCategories.AddRangeAsync(productCategory);
                }


                await _dbContext.SaveChangesAsync();

            }
        }
        public static List<Category> GetCategories()
        {
            return new List<Category>
            {
                   new Category
                   {
                       Id = Guid.Parse("DD213636-83A7-4377-8E70-B9D7CEA3A94B"),
                       Name = "Đồ Uống",
                       CreatedTime = DateTime.UtcNow,
                       LastUpdatedTime = DateTime.UtcNow
                   },
                     new Category
                     {
                          Id = Guid.Parse("46366679-358A-4C11-9FF6-20635580E07E"),
                          Name = "Món Chính",
                          CreatedTime = DateTime.UtcNow,
                          LastUpdatedTime = DateTime.UtcNow
                     },
                     new Category
                     {
                          Id = Guid.Parse("197D242F-86B0-4772-9FD8-BCFAD8606DF6"),
                          Name = "Tráng Miệng",
                          CreatedTime = DateTime.UtcNow,
                          LastUpdatedTime = DateTime.UtcNow
                     },
             };
        }

        public static List<Topping> GetToppings()
        {
            return new List<Topping>
            {
                new Topping
                {
                    Id = Guid.Parse("BBFBC305-2508-47F2-807F-2037BBE656A7"),
                    Name = "Thêm Đá",
                    Price = 0, // Giá thêm đá là miễn phí
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Topping
                {
                    Id = Guid.Parse("6039B485-D82A-45B1-AA06-37F53FEF7C19"),
                    Name = "Thêm Sữa",
                    Price = 15000,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Topping
                {
                    Id = Guid.Parse("F7ED898C-02AC-462C-AD46-314223800993"),
                    Name = "Thêm Đường",
                    Price = 5000,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                 new Topping
                    {
                        Id = Guid.Parse("E1A7BCF1-9873-4E64-A9EF-5C9C154097A1"),
                        Name = "Thêm Trân Châu",
                        Price = 10000,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                    }
            };
        }

        public static List<Table> GetTables()
        {
            return new List<Table>
            {
                        new Table
                        {
                            Id = Guid.NewGuid(),
                            Name= "Bàn 1",
                            Status = Domain.Enums.TableEnums.Available,
                            CreatedTime = DateTime.UtcNow,
                            LastUpdatedTime = DateTime.UtcNow
                        },
                        new Table
                        {
                            Id = Guid.NewGuid(),
                            Name= "Bàn 2",
                            Status = Domain.Enums.TableEnums.Available,
                            CreatedTime = DateTime.UtcNow,
                            LastUpdatedTime = DateTime.UtcNow
                        },
                        new Table
                        {
                            Id = Guid.NewGuid(),
                          Name= "Bàn 3",
                               Status = Domain.Enums.TableEnums.Available,
                            CreatedTime = DateTime.UtcNow,
                            LastUpdatedTime = DateTime.UtcNow
                        }
             };
        }

        public static List<Product> GetProducts()
        {
            return new List<Product>
            {
                // đô uống
                new Product
                {
                    Id = Guid.Parse("4AE13A6B-EEB1-4089-BA41-CC661DA91D4A"),
                    Name = "Cà Phê Sữa Đá",
                    Description = "Thức uống phổ biến tại Việt Nam",
                    DurationTime = 5,
                    ImageUrl = "https://bizweb.dktcdn.net/thumb/1024x1024/100/487/455/products/phin-sua-da-1698982829291.jpg?v=1724205217697",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("20513572-9DB1-4325-A67E-0778B683B9F9"),
                    Name = "Trà Sữa Trân Châu",
                    Description = "Thức uống ngọt ngào với trân châu dai ngon",
                    DurationTime = 3,
                    ImageUrl = "https://mixuediemdien.com/wp-content/uploads/2023/07/Tra-Sua-Tran-Chau-768x768.jpeg",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("AEDCCCA1-7419-4884-B3D1-E3FBDC87C97F"),
                    Name = "Trà Sữa Thạch Dừa",
                    Description = "Trà sữa thạch dừa là một loại thức uống giải khát phổ biến, kết hợp giữa vị ngọt của trà sữa và vị thanh mát, giòn sần sật của thạch dừa",
                    DurationTime = 10,
                    ImageUrl = "https://trungnguyenecoffee.com/wp-content/uploads/2020/08/H%C3%ACnh-App_3006021-Tr%C3%A0-S%E1%BB%AFa-Th%E1%BA%A1ch-C%C3%A0-Ph%C3%AA-768x768.jpg",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("E6E394C0-2226-4F4F-8DD2-A623975C13FB"),
                    Name = "Nước Ép Cam",
                    Description = "Nước ép cam tươi mát, giàu vitamin C",
                    DurationTime = 2,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSkOIGYOFkLj9jhstW6wP8knoI1zSFp_jRwoQ&s",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("56B31EDA-6E96-41B4-A7D6-0C1284A13E8B"),
                    Name = "Nước Ép Dứa",
                    Description = "Nước ép dứa thơm ngon, bổ dưỡng",
                    DurationTime = 2,
                    ImageUrl = "https://png.pngtree.com/png-vector/20241227/ourmid/pngtree-enjoy-the-tropical-flavor-of-pineapple-juice-png-image_14892986.png",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },

                // món chính
                 new Product
                {
                    Id = Guid.Parse("9757A503-E801-469E-980C-5FBEE99AACCE"),
                    Name = "Cơm lỏi vai bò sốt Teriyaki",
                    Description = "Thịt bò Nhật mềm, mọng nước quyện sốt Teriyaki đậm đà, thêm mè rang thơm ngọt.",
                    DurationTime = 0,
                    ImageUrl = "https://mrecohealthy.com/wp-content/uploads/2022/10/FRAME-07.jpg",
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                        Id = Guid.Parse("3198FC44-CE8C-49B9-BC54-86915BED805C"),
                        Name = "Bún bò Huế",
                        Description = "Bún bò Huế thơm ngon đậm đà với nước dùng cay nồng, chả lụa, thịt bò mềm và rau thơm ăn kèm.",
                        DurationTime = 0,
                        ImageUrl = "https://tourhue.vn/wp-content/uploads/2024/08/quan-bun-bo-hue-5.jpg",
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("BA3DE89B-1FC4-4156-A259-79B977731CF2"),
                    Name = "Cơm bò lúc lắc",
                    Description = "Rau củ thanh mát hòa quyện cùng bò áp chảo sốt đậm vị sốt vang đỏ.",
                    DurationTime = 0,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRDdE3Nc13Yevaw1BuVPN5ehkeTkzBuZAocDw&s",
                     CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("1DEBE98E-C358-4729-A44E-A3442455733B"),
                    Name = "Cơm gà sốt Teriyaki",
                    Description = "Gà áp chảo thơm lừng, sốt Teriyaki ngọt nhẹ, cơm mềm, rau củ giòn mát.",
                    DurationTime = 0,
                    ImageUrl = "https://storage.googleapis.com/onelife-public/blog.onelife.vn/2021/10/cach-lam-com-ga-sot-teriyaki-mon-an-sang-213341071443.jpg",
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("3B8EBE7C-8948-4D9A-AC47-BE85EE7CF085"),
                    Name = "Cơm sườn sốt chua ngọt",
                    Description = "Sườn được chiên vàng giòn rụm thấm đẫm nước sốt chua ngọt đậm đà.",
                    DurationTime = 0,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSoQ-Oayp9kgwx1VgrgaezJtYSVANCQcid5vQ&s",
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                // Tráng miệng 

                new Product
                {
                    Id = Guid.Parse("8F69CBB1-817D-4192-8ED0-B284FE4B1297"),
                    Name = "Chè đậu đỏ",
                    Description = "Món chè truyền thống với đậu đỏ mềm, bùi, kết hợp với nước cốt dừa béo ngậy.",
                    DurationTime = 0,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTJFX2VjeCkDYQ48NY7b9s_HXJQ3XkETTxPQQ&s",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("F3E3697B-757D-4E64-BB82-3F5EBC7F3E6F"),
                    Name = "Bánh flan",
                    Description = "Bánh trứng mềm mịn, thơm mùi vani, thường được ăn kèm với nước đường và đá.",
                    DurationTime = 0,
                    ImageUrl = "https://cdn.tgdd.vn/2021/10/CookRecipe/Avatar/banh-flan-sua-dac-thumbnail-1.jpg",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("CF21F6AA-1215-4EC2-BACD-C601FF76D26B"),
                    Name = "Chè sương sáo",
                    Description = "Món chè với sương sáo dai dai, kết hợp với nước cốt dừa và các loại topping khác như thạch, trân châu, đậu xanh.",
                    DurationTime = 0,
                    ImageUrl = "https://thachan.vn/theme/wbthachan/wp-content/uploads/2023/10/thanh-pham-suong-sao-cot-dua.png",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("25084817-29F4-42FD-BD60-081ECC90931C"),
                    Name = "Chè bưởi",
                    Description = "Món chè thanh mát với vị chua ngọt của bưởi, kết hợp cùng các loại topping như đậu xanh, nước cốt dừa.",
                    DurationTime = 0,
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSjjqjOhf06ny_DsvA3_9oer-YZYtym8zqF-g&s",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.Parse("35014A99-5664-4063-8BEB-83B80B2EEADB"),
                    Name = "Rau câu dừa",
                    Description = "Món tráng miệng với lớp rau câu giòn dai, vị ngọt của dừa và nước cốt dừa béo ngậy.",
                    DurationTime = 0,
                    ImageUrl = "https://cdn.tgdd.vn/2021/11/CookRecipe/Avatar/rau-cau-nuoc-cot-dua-thumbnail-1.jpg",
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                }
            };

        }

        public static List<ProductSize> GetProductSizes()
        {
            return new List<ProductSize>
            {
                // Cà Phê Sữa Đá
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("4AE13A6B-EEB1-4089-BA41-CC661DA91D4A"),
                    SizeName = SizeNameEnum.Small,
                    Price = 15000,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("4AE13A6B-EEB1-4089-BA41-CC661DA91D4A"),
                    SizeName = SizeNameEnum.Medium,
                    Price = 20000,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("4AE13A6B-EEB1-4089-BA41-CC661DA91D4A"),
                    SizeName = SizeNameEnum.Large,
                    Price = 25000,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },

                // Trà Sữa Trân Châu
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("20513572-9DB1-4325-A67E-0778B683B9F9"),
                    SizeName = SizeNameEnum.Small,
                    Price = 16000,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("20513572-9DB1-4325-A67E-0778B683B9F9"),
                    SizeName = SizeNameEnum.Medium,
                    Price = 21000,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("20513572-9DB1-4325-A67E-0778B683B9F9"),
                    SizeName = SizeNameEnum.Large,
                    Price = 26000,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },

                // Trà Sữa Thạch Dừa
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("AEDCCCA1-7419-4884-B3D1-E3FBDC87C97F"),
                    SizeName = SizeNameEnum.Medium,
                    Price = 22000,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("AEDCCCA1-7419-4884-B3D1-E3FBDC87C97F"),
                    SizeName = SizeNameEnum.Large,
                    Price = 27000,
                     CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },

                // Nước Ép Cam
                new ProductSize
                {
                    Id =Guid.NewGuid(),
                    ProductId = Guid.Parse("E6E394C0-2226-4F4F-8DD2-A623975C13FB"),
                    SizeName = SizeNameEnum.Small,
                    Price = 18000,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("E6E394C0-2226-4F4F-8DD2-A623975C13FB"),
                    SizeName = SizeNameEnum.Large,
                    Price = 23000,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },

                // Nước Ép Dứa
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("56B31EDA-6E96-41B4-A7D6-0C1284A13E8B"),
                    SizeName = SizeNameEnum.Medium,
                    Price = 19000
                },
                new ProductSize
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("56B31EDA-6E96-41B4-A7D6-0C1284A13E8B"),
                    SizeName = SizeNameEnum.Large,
                    Price = 24000
                }
            };
        }
        public static List<ProductTopping> GetProductToppings()
        {
            return new List<ProductTopping>
            {
                new ProductTopping
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("4AE13A6B-EEB1-4089-BA41-CC661DA91D4A"),
                    ToppingId = Guid.Parse("6039B485-D82A-45B1-AA06-37F53FEF7C19"),
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductTopping
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("20513572-9DB1-4325-A67E-0778B683B9F9"),
                    ToppingId = Guid.Parse("6039B485-D82A-45B1-AA06-37F53FEF7C19"),
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                }
            };
        }

        public static List<ProductCategory> GetProductCategories()
        {
            var categoryId = Guid.Parse("DD213636-83A7-4377-8E70-B9D7CEA3A94B"); // Category: Đồ Uống
            var categoryId2 = Guid.Parse("46366679-358A-4C11-9FF6-20635580E07E"); // Category: Món Chính
            var categoryId3 = Guid.Parse("197D242F-86B0-4772-9FD8-BCFAD8606DF6"); // tráng miệng

            return new List<ProductCategory>
            {
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("4AE13A6B-EEB1-4089-BA41-CC661DA91D4A"), // Cà Phê Sữa Đá
                    CategoryId = categoryId,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("20513572-9DB1-4325-A67E-0778B683B9F9"), // Trà Sữa Trân Châu
                    CategoryId = categoryId,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("AEDCCCA1-7419-4884-B3D1-E3FBDC87C97F"), // Trà Sữa Thạch Dừa
                    CategoryId = categoryId
                },
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("E6E394C0-2226-4F4F-8DD2-A623975C13FB"), // Nước Ép Cam
                    CategoryId = categoryId,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("56B31EDA-6E96-41B4-A7D6-0C1284A13E8B"), // Nước Ép Dứa
                    CategoryId = categoryId,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                 // Món chính
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("9757A503-E801-469E-980C-5FBEE99AACCE"), // Cơm lõi vai bò sốt Teriyaki
                    CategoryId = categoryId2,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("3198FC44-CE8C-49B9-BC54-86915BED805C"), // Bún bò Huế
                    CategoryId = categoryId2,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("BA3DE89B-1FC4-4156-A259-79B977731CF2"), // Cơm bò lúc lắc
                    CategoryId = categoryId2,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("1DEBE98E-C358-4729-A44E-A3442455733B"), // Cơm gà sốt Teriyaki
                    CategoryId = categoryId2,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                new ProductCategory
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.Parse("3B8EBE7C-8948-4D9A-AC47-BE85EE7CF085"), // Cơm sườn sốt chua ngọt
                    CategoryId = categoryId2,
                      CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                },
                     // Tráng Miệng
                new ProductCategory { Id = Guid.NewGuid(), ProductId = Guid.Parse("8F69CBB1-817D-4192-8ED0-B284FE4B1297"), CategoryId = categoryId3 ,  CreatedTime = DateTime.UtcNow,LastUpdatedTime = DateTime.UtcNow}, // Chè đậu đỏ
                new ProductCategory { Id = Guid.NewGuid(), ProductId = Guid.Parse("F3E3697B-757D-4E64-BB82-3F5EBC7F3E6F"), CategoryId = categoryId3 ,CreatedTime = DateTime.UtcNow,LastUpdatedTime = DateTime.UtcNow}, // Bánh flan
                new ProductCategory { Id = Guid.NewGuid(), ProductId = Guid.Parse("CF21F6AA-1215-4EC2-BACD-C601FF76D26B"), CategoryId = categoryId3,CreatedTime = DateTime.UtcNow,LastUpdatedTime = DateTime.UtcNow }, // Chè sương sáo
                new ProductCategory { Id = Guid.NewGuid(), ProductId = Guid.Parse("25084817-29F4-42FD-BD60-081ECC90931C"), CategoryId = categoryId3,CreatedTime = DateTime.UtcNow,LastUpdatedTime = DateTime.UtcNow }, // Chè bưởi
                new ProductCategory { Id = Guid.NewGuid(), ProductId = Guid.Parse("35014A99-5664-4063-8BEB-83B80B2EEADB"), CategoryId = categoryId3,CreatedTime = DateTime.UtcNow,LastUpdatedTime = DateTime.UtcNow }  // Rau câu dừa
   
            };
        }

    }
}
