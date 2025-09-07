using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Cloudinary;
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
        private readonly IToppingRepository _toppingRepository;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;

        public ToppingService(IUnitOfWork unitOfWork, IMapper mapper, IToppingRepository toppingRepository , ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _toppingRepository = toppingRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<BaseResponseModel> Create(CreateToppingRequest request)
        {
            var entity = _mapper.Map<Topping>(request);
            entity.CreatedBy = "";
            entity.CreatedTime = DateTime.UtcNow;
            entity.LastUpdatedBy = "";
            entity.LastUpdatedTime = DateTime.UtcNow;
            // Handle file upload for ImageUrl if needed

            if (request.ImageFile is not null && request.ImageFile.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(
                    request.ImageFile,
                    "toppings", // tên folder trên Cloudinary
                    null        // vì đang tạo mới nên không có ảnh cũ để xóa
                );
                entity.ImageUrl = imageUrl;
            }

            await _unitOfWork.Repository<Topping, bool>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
                "Create Topping successfully");
        }


        public async Task<BaseResponseModel> Delete(Guid id)
        {
            var existedEntity = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(id);
            if (existedEntity == null)
            {
                return new BaseResponseModel(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND,
                    "Topping not found");
            }

            existedEntity.LastUpdatedBy = "";
            existedEntity.LastUpdatedTime = DateTime.UtcNow;
            existedEntity.DeletedBy = "";
            existedEntity.DeletedTime = DateTime.UtcNow;
            _unitOfWork.Repository<Topping, bool>().Update(existedEntity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
                "Delete Topping successfully");
        }

        public async Task<PaginatedList<ToppingResponse>> GetAllToppingsAsync(PagingRequestModel paging)
        {
            var Topping = await _unitOfWork.Repository<Topping, Guid>()
                .GetAllWithSpecWithInclueAsync(new BaseSpecification<Topping>(x => !x.DeletedTime.HasValue), true,
                    c => c.ProductToppings);
            if (Topping == null || !Topping.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND,
                    "Không có danh mục nào");
            }

            var ToppingResponse = _mapper.Map<List<ToppingResponse>>(Topping);

            return PaginatedList<ToppingResponse>.Create(ToppingResponse, paging.PageNumber, paging.PageSize);
        }

        public async Task<BaseResponseModel<ToppingResponse>> GetByIdAsync(Guid id)
        {
            var toppingExisted = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(id);
            if (toppingExisted == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND,
                    "Topping not found");
            }

            var toppingResponse = _mapper.Map<ToppingResponse>(toppingExisted);
            return new BaseResponseModel<ToppingResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
                toppingResponse);
        }

        public async Task<BaseResponseModel> Update(CreateToppingRequest request, Guid id)
        {
            var existedEntity = await _unitOfWork.Repository<Topping, Guid>().GetByIdAsync(id);
            if (existedEntity == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND,
                    "Topping not found");
            }

            _mapper.Map(request, existedEntity);
            existedEntity.LastUpdatedBy = "";
            existedEntity.LastUpdatedTime = DateTime.UtcNow;
            // Handle file upload for ImageUrl if needed
            if (request.ImageFile is not null && request.ImageFile.Length > 0)
            {
                // If there is an existing image, delete it first
                if (!string.IsNullOrEmpty(existedEntity.ImageUrl))
                {
                    await _cloudinaryService.DeleteImageAsync(existedEntity.ImageUrl);
                }
                // Upload the new image
                var imageUrl = await _cloudinaryService.UploadImageAsync(
                    request.ImageFile,
                    "toppings", // tên folder trên Cloudinary
                    null        // vì đang cập nhật nên không có ảnh cũ để xóa
                );
                existedEntity.ImageUrl = imageUrl;
            }
            _unitOfWork.Repository<Topping, bool>().Update(existedEntity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
                "Update Topping successfully");
        }

        public async Task<BaseResponseModel<List<ToppingResponse>>> GetByIdProduction(Guid id)
        {
            var temp = await _toppingRepository.getToppingbyProductionId(id);
            var toppingResponse = _mapper.Map<List<ToppingResponse>>(temp);
            return new BaseResponseModel<List<ToppingResponse>>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
                toppingResponse);
        }
    }
    
}