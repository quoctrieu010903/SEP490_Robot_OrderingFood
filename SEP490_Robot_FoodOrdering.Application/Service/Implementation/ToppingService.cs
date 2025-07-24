

using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ToppingService : IToppingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ToppingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<BaseResponseModel> Create(CreateToppingRequest request)
        {
            var entity = _mapper.Map<Topping>(request);
            entity.CreatedBy = "";
            entity.CreatedTime = DateTime.UtcNow;
            entity.LastUpdatedBy = "";
            entity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<Topping, bool>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Create Topping successfully");
        }

     

        public async Task<BaseResponseModel> Delete(Guid id)
        {
          var existedEntity = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(id);
            if (existedEntity == null)
            {
                return new BaseResponseModel(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Topping not found");
            }
            existedEntity.LastUpdatedBy = "";
            existedEntity.LastUpdatedTime = DateTime.UtcNow;
            existedEntity.DeletedBy = "";
            existedEntity.DeletedTime = DateTime.UtcNow;
            _unitOfWork.Repository<Topping, bool>().Update(existedEntity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Delete Topping successfully");
        }

        public async Task<PaginatedList<ToppingResponse>> GetAllToppingsAsync(PagingRequestModel paging)
        {
            var Topping = await _unitOfWork.Repository<Topping, Guid>().GetAllWithSpecWithInclueAsync(new BaseSpecification<Topping>(x => !x.DeletedTime.HasValue), true, c => c.ProductToppings);
            if (Topping == null || !Topping.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không có danh mục nào");
            }
            var ToppingResponse = _mapper.Map<List<ToppingResponse>>(Topping);

            return PaginatedList<ToppingResponse>.Create(ToppingResponse, paging.PageNumber, paging.PageSize);
        }

        public async Task<BaseResponseModel<ToppingResponse>> GetByIdAsync(Guid id)
        {
           var toppingExisted = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(id);
            if (toppingExisted == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Topping not found");
            }
            var toppingResponse = _mapper.Map<ToppingResponse>(toppingExisted);
            return new BaseResponseModel<ToppingResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, toppingResponse);
        }

        public async Task<BaseResponseModel> Update(CreateToppingRequest request, Guid id)
        {
            var existedEntity = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(id);
            if (existedEntity == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Topping not found");
            }
            _mapper.Map(request, existedEntity);
            existedEntity.LastUpdatedBy = "";
            existedEntity.LastUpdatedTime = DateTime.UtcNow;
            _unitOfWork.Repository<Topping, bool>().Update(existedEntity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Update Topping successfully");
        }
    }
}
