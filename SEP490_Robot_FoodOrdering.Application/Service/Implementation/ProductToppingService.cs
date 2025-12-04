using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ProductToppingService : IProductToppingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductToppingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<BaseResponseModel> Create(CreateProductToppingRequest request)
        {
            var entity = _mapper.Map<ProductTopping>(request);
            entity.CreatedBy = "";
            entity.CreatedTime = DateTime.UtcNow;
            entity.LastUpdatedBy = "";
            entity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<ProductTopping, bool>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, entity);
        }
        public async Task<BaseResponseModel> Delete(Guid id)
        {
            var existed = await _unitOfWork.Repository<ProductTopping, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "ProductTopping không tìm thấy");
            existed.LastUpdatedBy = "";
            existed.LastUpdatedTime = DateTime.UtcNow;
            existed.DeletedBy = "";
            existed.DeletedTime = DateTime.UtcNow;
            _unitOfWork.Repository<ProductTopping, Guid>().Update(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Xoá thành công");
        }
            public async Task<PaginatedList<ProductToppingResponse>> GetAll(PagingRequestModel paging)
            {
                var list = await _unitOfWork.Repository<ProductTopping, ProductTopping>().GetAllWithSpecAsync( new ProductToppingSpecification() , true);
                var mapped = _mapper.Map<List<ProductToppingResponse>>(list);
                return PaginatedList<ProductToppingResponse>.Create(mapped, paging.PageNumber, paging.PageSize);
            }
        public async Task<ProductToppingResponse> GetById(Guid id)
        {
            var existed = await _unitOfWork.Repository<ProductTopping, Guid>().GetWithSpecAsync(new ProductToppingSpecification(id),true);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "ProductTopping không tìm thấy");
            return _mapper.Map<ProductToppingResponse>(existed);
        }

        public async Task<BaseResponseModel<ProductWithToppingsResponse>> GetProductWithToppingsAsync(Guid ProductId)
        {
            var specification = new ProductToppingSpecification(ProductId, true);

            var product = await _unitOfWork.Repository<ProductTopping, ProductTopping>().GetWithSpecAsync(specification, true);

            if (product == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Product with ID {ProductId} not found");
            }

            var response = _mapper.Map<ProductWithToppingsResponse>(product); 

            return new BaseResponseModel<ProductWithToppingsResponse>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                response
            );
        }
        public async Task<BaseResponseModel> Update(CreateProductToppingRequest request, Guid id)
        {
            var existed = await _unitOfWork.Repository<ProductTopping, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "ProductTopping không tìm thấy");
            existed.ProductId = request.ProductId;
            existed.ToppingId = request.ToppingId;
            existed.LastUpdatedBy = "";
            existed.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<ProductTopping, ProductTopping>().UpdateAsync(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Cập nhật thành công");
        }
    }
} 