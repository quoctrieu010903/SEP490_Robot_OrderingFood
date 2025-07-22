
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Category;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponseModel> Create(CreateCategoryRequest request)
        {

            // mốt có login thêm user vào đây
            var entity = _mapper.Map<Category>(request);

            entity.CreatedBy = "";
            entity.CreatedTime = DateTime.UtcNow;
            entity.LastUpdatedBy = "";
            entity.LastUpdatedTime = DateTime.UtcNow;

            await _unitOfWork.Repository<Category, bool>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, entity);


        }

        public async Task<BaseResponseModel> Delete(Guid id)
        {
            var existedCategory = await _unitOfWork.Repository<Category, Guid>().GetByIdAsync(id);
            if (existedCategory != null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Category không tìm thấy ");
            }
            existedCategory.LastUpdatedBy = ""; // mốt có login thêm user vào đây
            existedCategory.LastUpdatedTime = DateTime.UtcNow;
            existedCategory.DeletedBy = "";
            existedCategory.DeletedTime = DateTime.UtcNow; // đánh dấu là đã xoá
            _unitOfWork.Repository<Category, Category>().Update(existedCategory);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Xoá thành công");


        }

        public async Task<PaginatedList<CategoryResponse>> GetAllCategory(PagingRequestModel model)
        {
            var category = await _unitOfWork.Repository<Category, Category>().GetAllWithSpecWithInclueAsync(new BaseSpecification<Category>(x => !x.DeletedTime.HasValue), true , c=>c.ProductCategories);
            if (category == null || !category.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không có danh mục nào");
            }
            var categoryResponse = _mapper.Map<List<CategoryResponse>>(category);

            return PaginatedList<CategoryResponse>.Create(categoryResponse, model.PageNumber, model.PageSize);
        }
        public async Task<BaseResponseModel<CategoryResponse>> GetCategoryById(Guid id)
        {
            var existedCategory = await _unitOfWork.Repository<Category, Guid>().GetByIdAsync(id);
            if (existedCategory == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Category không tìm thấy");
            }
            var categoryResponse = _mapper.Map<CategoryResponse>(existedCategory);
            return new BaseResponseModel<CategoryResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, categoryResponse);
        }

        public async Task<BaseResponseModel> Update(CreateCategoryRequest request, Guid id)
        {
            var existedCategory = await _unitOfWork.Repository<Category, Guid>().GetByIdAsync(id);
            if (existedCategory == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Category không tìm thấy");
            }
            existedCategory.Name = request.Name;
            existedCategory.LastUpdatedBy = ""; // mốt có login thêm user vào đây
            existedCategory.LastUpdatedTime = DateTime.UtcNow;
             await _unitOfWork.Repository<Category, Category>().UpdateAsync(existedCategory);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Cặp nhật thành công");

        }
    }
}
