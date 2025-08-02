using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class TableService : ITableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TableService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<BaseResponseModel> Create(CreateTableRequest request)
        {
            var entity = _mapper.Map<Table>(request);
            entity.CreatedBy = "";
            entity.CreatedTime = DateTime.UtcNow;
            entity.LastUpdatedBy = "";
            entity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<Table, bool>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, entity);
        }
        public async Task<BaseResponseModel> Delete(Guid id)
        {
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");
            existed.LastUpdatedBy = "";
            existed.LastUpdatedTime = DateTime.UtcNow;
            existed.DeletedBy = "";
            existed.DeletedTime = DateTime.UtcNow;
            _unitOfWork.Repository<Table, Table>().Update(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Xoá thành công");
        }
        public async Task<PaginatedList<TableResponse>> GetAll(PagingRequestModel paging , TableEnums? status)
            {
            var list = await _unitOfWork.Repository<Table, Table>().GetAllWithSpecAsync( new TableSpecification(paging.PageNumber , paging.PageSize,status));
            var mapped = _mapper.Map<List<TableResponse>>(list);
            return PaginatedList<TableResponse>.Create(mapped, paging.PageNumber, paging.PageSize);
        }
        public async Task<TableResponse> GetById(Guid id)
        {
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");
            return _mapper.Map<TableResponse>(existed);
        }
        public async Task<BaseResponseModel> Update(CreateTableRequest request, Guid id)
        {
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");
            existed.Name = request.Name;
            existed.Status = request.Status;
            existed.LastUpdatedBy = "";
            existed.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<Table, Table>().UpdateAsync(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Cập nhật thành công");
        }
    }
} 