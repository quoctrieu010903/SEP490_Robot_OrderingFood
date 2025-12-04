

using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Cloudinary;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using OfficeOpenXml;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IToppingRepository _toppingRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public AdminService(IUnitOfWork unitOfWork, IToppingRepository toppingRepository, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _toppingRepository = toppingRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<BaseResponseModel<bool>> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new BaseResponseModel<bool>(500, "ERROR", "File is empty");



            var categories = await _unitOfWork.Repository<Category,Category>()
                .GetAllAsync();

            var categoryDict = categories.ToDictionary(
                c => c.Name.Trim().ToLower(),
                c => c.Id
            );

            List<Product> productEntities = new();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0];
                    int rowCount = ws.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        if (string.IsNullOrWhiteSpace(ws.Cells[row, 1].Text))
                            continue;

                        string name = ws.Cells[row, 1].Text;
                        string sizeM = ws.Cells[row, 2].Text;
                        string sizeS = ws.Cells[row, 3].Text;
                        string sizeL = ws.Cells[row, 4].Text;
                        string image = ws.Cells[row, 5].Text;
                        string categoryName = ws.Cells[row, 6].Text.Trim().ToLower();

                        if (!categoryDict.TryGetValue(categoryName, out Guid categoryId))
                        {
                            return new BaseResponseModel<bool>(
                                400, "ERROR",
                                $"Category không tồn tại: {categoryName}"
                            );
                        }

                        var product = new Product
                        {
                            Name = name,
                            Description = "",
                            DurationTime = 0,
                            ImageUrl = image,
                            Sizes = new List<ProductSize>(),
                            ProductCategories = new List<ProductCategory>()
                        };

                        product.ProductCategories.Add(new ProductCategory
                        {
                            CategoryId = categoryId,
                            Product = product
                        });

                        product.Sizes.Add(new ProductSize
                        {
                            SizeName = SizeNameEnum.Medium,
                            Price = decimal.TryParse(sizeM, out var pm) ? pm : 0
                        });

                        product.Sizes.Add(new ProductSize
                        {
                            SizeName = SizeNameEnum.Small,
                            Price = decimal.TryParse(sizeS, out var ps) ? ps : 0
                        });

                        product.Sizes.Add(new ProductSize
                        {
                            SizeName = SizeNameEnum.Large,
                            Price = decimal.TryParse(sizeL, out var pl) ? pl : 0
                        });

                        productEntities.Add(product);
                    }
                }
            }

            await _unitOfWork.Repository<Product, bool>().AddRangeAsync(productEntities);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponseModel<bool>(200, ResponseCodeConstants.SUCCESS, "Import thành công");
        }


        public async Task<BaseResponseModel<bool>> ImportExcelTopping(Guid id, IFormFile file)
        {
            var existedProduct = await _unitOfWork.Repository<Product, Guid>().GetByIdAsync(id);

            if (existedProduct == null)
            {
                throw new ErrorException(
                    StatusCodes.Status404NotFound,
                    ResponseCodeConstants.NOT_FOUND,
                    "Product không tìm thấy"
                );
            }

            if (file == null || file.Length == 0)
            {
                return new BaseResponseModel<bool>(500, "ERROR", "File rỗng hoặc không hợp lệ");
            }

            List<ProductTopping> toppingEntities = new List<ProductTopping>();

            string? imageUrl = null;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0];

                    if (ws.Dimension == null)
                    {
                        return new BaseResponseModel<bool>(400, "ERROR", "File Excel không có dữ liệu");
                    }

                    int rows = ws.Dimension.Rows;

                    for (int r = 2; r <= rows; r++)
                    {
                        string name = ws.Cells[r, 1].Text?.Trim();
                        string priceText = ws.Cells[r, 2].Text?.Trim();
                        string image = ws.Cells[r, 3].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        decimal price = 0;
                        decimal.TryParse(priceText, out price);

                        toppingEntities.Add(new ProductTopping
                        {
                            Product = existedProduct,
                            Topping = new Topping()
                            {
                                Name = name,
                                Price = price,
                                ImageUrl = image
                            }
                        });
                    }
                }
            }

            if (toppingEntities.Count == 0)
            {
                return new BaseResponseModel<bool>(400, "ERROR", "Không có topping nào được import");
            }

            await _unitOfWork.Repository<ProductTopping, ProductTopping>()
                .AddRangeAsync(toppingEntities);

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponseModel<bool>(
                200,
                ResponseCodeConstants.SUCCESS,
                "Import topping thành công"
            );
        }


        public async Task<byte[]> ExportExcel()
        {
            var products = await _unitOfWork.Repository<Product, Product>()
                .GetAllWithSpecWithInclueAsync(
                    null,
                    true,
                    p => p.Sizes,
                    p => p.ProductCategories
                );

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/excel-templates/ProductionItem.xlsx");
            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var ws = package.Workbook.Worksheets[0];
                int row = 2;

                foreach (var p in products)
                {
                    decimal sizeM = p.Sizes?.FirstOrDefault(x => x.SizeName == SizeNameEnum.Medium)?.Price ?? 0;
                    decimal sizeS = p.Sizes?.FirstOrDefault(x => x.SizeName == SizeNameEnum.Small)?.Price ?? 0;
                    decimal sizeL = p.Sizes?.FirstOrDefault(x => x.SizeName == SizeNameEnum.Large)?.Price ?? 0;

                    string categoryName = p.ProductCategories?
                        .Select(pc => pc.Category.Name)
                        .FirstOrDefault() ?? "";

                    ws.Cells[row, 1].Value = p.Name;
                    ws.Cells[row, 2].Value = sizeM;
                    ws.Cells[row, 3].Value = sizeS;
                    ws.Cells[row, 4].Value = sizeL;
                    ws.Cells[row, 5].Value = p.ImageUrl;
                    ws.Cells[row, 6].Value = categoryName;

                    row++;
                }

                return package.GetAsByteArray();
            }
        }

        public async Task<byte[]> ExportExcelTable()
        {
            var list = await _unitOfWork.Repository<Table, Table>().GetAllAsync();

            /// chua co ban luu toa do bang
            throw new NotImplementedException();
        }

        public async Task<byte[]> ExportExcelTopping(Guid id)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var toppings = await _toppingRepository.getToppingbyProductionId(id);

            var templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/excel-templates/ToppingItem.xlsx"
            );

            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var ws = package.Workbook.Worksheets[0];

                int row = 2;

                foreach (var t in toppings)
                {
                    ws.Cells[row, 1].Value = t.Name;
                    ws.Cells[row, 2].Value = t.Price;
                    ws.Cells[row, 3].Value = t.ImageUrl;

                    row++;
                }

                return package.GetAsByteArray();
            }
        }


        public async Task<BaseResponseModel<bool>> ImportExcelTable(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new BaseResponseModel<bool>(500, "ERROR", "File is empty");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            List<Table> tables = new List<Table>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0];

                    int rows = ws.Dimension.Rows;
                    int cols = ws.Dimension.Columns;

                    for (int r = 1; r <= rows; r++)
                    {
                        for (int c = 1; c <= cols; c++)
                        {
                            string cellValue = ws.Cells[r, c].Text?.Trim();

                            if (!string.IsNullOrWhiteSpace(cellValue) &&
                                cellValue.StartsWith("Ban", StringComparison.OrdinalIgnoreCase))
                            {
                                tables.Add(new Table
                                {
                                    Name = cellValue
                                });
                            }
                        }
                    }
                }
            }

            await _unitOfWork.Repository<Table, bool>().AddRangeAsync(tables);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel<bool>(200, "SUCCESS", "Import thành công");
        }
    }
}
