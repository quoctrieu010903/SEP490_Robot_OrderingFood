
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Domain.Specifications;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Cloudinary;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<BaseResponseModel> Create(CreateProductRequest request)
        {
            var entity = _mapper.Map<Product>(request);

            entity.CreatedBy = "";
            entity.CreatedTime = DateTime.UtcNow;
            entity.LastUpdatedBy = "";
            entity.LastUpdatedTime = DateTime.UtcNow;
            // TODO: Handle file upload for ImageUrl if needed
            if (request.ImageFile is not null && request.ImageFile.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(
                    request.ImageFile,
                    "products", // tên folder trên Cloudinary
                    null        // vì đang tạo mới nên không có ảnh cũ để xóa
                );
                entity.ImageUrl = imageUrl;
            }


            await _unitOfWork.Repository<Product, bool>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, entity);
        }

        public async Task<BaseResponseModel> Delete(Guid id)
        {
            var existedProduct = await _unitOfWork.Repository<Product, Guid>().GetByIdAsync(id);
            if (existedProduct == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Product không tìm thấy");
            }
            existedProduct.LastUpdatedBy = "";
            existedProduct.LastUpdatedTime = DateTime.UtcNow;
            existedProduct.DeletedBy = "";
            existedProduct.DeletedTime = DateTime.UtcNow;
            _unitOfWork.Repository<Product, Product>().Update(existedProduct);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Xoá thành công");
        }

        public async Task<PaginatedList<ProductResponse>> GetAll(PagingRequestModel paging, ProductSpecParams fillter)
        {
            var products = await _unitOfWork.Repository<Product, Product>()
                .GetAllWithSpecAsync(
                 new ProductSpecification(fillter,paging.PageNumber, paging.PageSize),
                    true
                );
            var productResponses = _mapper.Map<List<ProductResponse>>(products);

            return PaginatedList<ProductResponse>.Create(productResponses, paging.PageNumber, paging.PageSize);
        }

        public async Task<BaseResponseModel<ProductDetailResponse>> GetById(Guid id)
        {
            var existedProduct = await _unitOfWork.Repository<Product, Guid>().GetByIdWithIncludeAsync(x=>x.Id == id , true , p=> p.Sizes , p => p.ProductCategories);
            if (existedProduct == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Product không tìm thấy");
            }
            var productDetail = _mapper.Map<ProductDetailResponse>(existedProduct);
            return new BaseResponseModel<ProductDetailResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, productDetail);
        }

        public async Task<BaseResponseModel<ProductResponse>> Update(CreateProductRequest request, Guid id)
        {
            var existedProduct = await _unitOfWork.Repository<Product, Guid>().GetByIdAsync(id);
            if (existedProduct == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Product không tìm thấy");
            }
            existedProduct.Name = request.ProductName;
            existedProduct.Description = request.Description;
            existedProduct.DurationTime = request.DurationTime;
            existedProduct.LastUpdatedBy = "";
            existedProduct.LastUpdatedTime = DateTime.UtcNow;
            // TODO: Handle file upload for ImageUrl if needed
            if (request.ImageFile is not null && request.ImageFile.Length > 0)
            {
                // Xoá ảnh cũ nếu có
                if (!string.IsNullOrEmpty(existedProduct.ImageUrl))
                {
                    await _cloudinaryService.DeleteImageAsync(existedProduct.ImageUrl);
                }

                var imageUrl = await _cloudinaryService.UploadImageAsync(
                    request.ImageFile,
                    "products", // tên folder trên Cloudinary
                    existedProduct.ImageUrl // xóa ảnh cũ nếu có
                );
                existedProduct.ImageUrl = imageUrl;
            }
            await _unitOfWork.Repository<Product, Product>().UpdateAsync(existedProduct);
            await _unitOfWork.SaveChangesAsync();
            var productResponse = _mapper.Map<ProductResponse>(existedProduct);
            return new BaseResponseModel<ProductResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, productResponse);
        }
    }
}
