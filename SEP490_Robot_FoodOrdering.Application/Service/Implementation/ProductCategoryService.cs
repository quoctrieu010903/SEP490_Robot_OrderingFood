
using AutoMapper;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Specifications;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponseModel> CreateProductCategoryAsync(Guid productId, Guid categoryId)
        {
            // Check if already exists
            var exists = await _unitOfWork.Repository<ProductCategory, ProductCategory>()
                .GetWithSpecAsync(new BaseSpecification<ProductCategory>( x => x.ProductId == productId && x.CategoryId == categoryId && !x.DeletedTime.HasValue));
            if (exists != null)
            {
                return new BaseResponseModel(StatusCodes.Status409Conflict, "ALREADY_EXISTS", "ProductCategory already exists");
            }
            var entity = new ProductCategory
            {
                ProductId = productId,
                CategoryId = categoryId,
                CreatedBy = "",
                CreatedTime = DateTime.UtcNow,
                LastUpdatedBy = "",
                LastUpdatedTime = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ProductCategory, ProductCategory>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, "SUCCESS", entity);
        }

        public async Task<PaginatedList<ProductCategoryResponse>> GetAllProductCategoriesAsync(PagingRequestModel paging)
        {
            var list = await _unitOfWork.Repository<ProductCategory, ProductCategory>()
                .GetAllWithSpecAsync(new ProductCategorySpecification(paging.PageNumber , paging.PageSize),true);
            var mapped = _mapper.Map<List<ProductCategoryResponse>>(list);
            return PaginatedList<ProductCategoryResponse>.Create(mapped, paging.PageNumber, paging.PageSize);
        }

        public async Task<BaseResponseModel> DeleteProductCategoryAsync(Guid id)
        {
            var existed = await _unitOfWork.Repository<ProductCategory, Guid>().GetByIdAsync(id);
            if (existed == null)
            {
                return new BaseResponseModel(StatusCodes.Status404NotFound, "NOT_FOUND", "ProductCategory not found");
            }
            existed.DeletedBy = "";
            existed.DeletedTime = DateTime.UtcNow;
            existed.LastUpdatedBy = "";
            existed.LastUpdatedTime = DateTime.UtcNow;
            _unitOfWork.Repository<ProductCategory, ProductCategory>().Update(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, "SUCCESS", "Deleted successfully");
        }

    }
}
