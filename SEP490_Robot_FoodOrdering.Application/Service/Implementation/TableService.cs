
using AutoMapper;
using Microsoft.AspNetCore.Http;
using QRCoder;
using System.Drawing; 
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
using ZXing.QrCode.Internal;

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
            foreach (var table in mapped)
            {
                // Tạo URL chứa id của bàn
                string url = $"https://mobile-production-1431.up.railway.app/{table.Id}";

                // Sinh QR code dạng Base64
                table.QRCode = GenerateQrCodeBase64_NoDrawing(url);

            }

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
            existed.Status = request.Status;
            existed.LastUpdatedBy = "";
            existed.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<Table, Guid>().UpdateAsync(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Cập nhật thành công");
        }
        private string GenerateQrCodeBase64_NoDrawing(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrBytes = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";
        }

    }
} 