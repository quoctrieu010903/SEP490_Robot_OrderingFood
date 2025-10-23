

using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;
using SEP490_Robot_FoodOrdering.Application.DTO.Response;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ComplainService : IComplainService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        public ComplainService(IUnitOfWork unitOfWork, IMapper mapper, IOrderService orderService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderService = orderService;
        }

        public async Task<BaseResponseModel<List<ComplainCreate>>> ComfirmComplain(
                             Guid idTable,
                             List<Guid> IDFeedback,
                             bool isPending,
                             string content)
        {
            // Lấy tất cả feedback (Complain) từ DB theo table
            var feedbackEntities = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<Complain>(f => f.TableId == idTable));

            if (feedbackEntities == null || !feedbackEntities.Any())
                throw new ErrorException(404, "No feedbacks found for this table");

            var updatedFeedbacks = new List<ComplainCreate>();
            bool found = false;

            foreach (var feedback in feedbackEntities.Where(f => IDFeedback.Contains(f.Id)))
            {
                found = true;
                feedback.isPending = isPending;
                feedback.ResolutionNote = content;
                feedback.ResolvedAt = DateTime.UtcNow;

                await _unitOfWork.Repository<Complain, Guid>().UpdateAsync(feedback);

                updatedFeedbacks.Add(new ComplainCreate(
                    feedback.CreatedTime ,
                    feedback.isPending,
                    feedback.Description
                ));
            }

            if (!found)
                throw new ErrorException(404, "No feedbacks found with given IDs");

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponseModel<List<ComplainCreate>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                updatedFeedbacks
            );
        }






        public async Task<BaseResponseModel<ComplainCreate>> CreateComplainAsyns(ComplainRequests request)
        {
            var existedTable = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(request.TableId);
            if (existedTable == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không tìm thất table");
            foreach (var existedComplain in request.OrderItemIds)
            {
                var complain = new Complain()
                {
                    Id = Guid.NewGuid(),
                    TableId = request.TableId,
                    OrderItemId = existedComplain,
                    Title = request.Title,
                    Description = request.ComplainNote,
                    isPending = false,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };
                await _unitOfWork.Repository<Complain, Guid>().AddAsync(complain);
                await _unitOfWork.SaveChangesAsync();

            }
            var response = new ComplainCreate(DateTime.UtcNow, true, "Tạo complain thành công");
            return new BaseResponseModel<ComplainCreate>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, response);

        }

        public async Task<BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>> GetAllComplainIsPending()
        {
            // Lấy tất cả dữ liệu cần thiết
            var tables = await _unitOfWork.Repository<Table, Guid>().GetAllWithIncludeAsync(true , t=> t.Orders);
            var complains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<Complain>(x => x.isPending));

            if (tables == null || !tables.Any())
                throw new ErrorException(404, "No tables found");

            // Lấy toàn bộ thống kê order cho các bàn
            var orderStatsDict = await _orderService.GetOrderStatsByTableIds(tables.Select(x => x.Id));

            // 🔹 Gộp dữ liệu bằng LINQ
            var result = tables
                .Select(table =>
                {
                    int pendingCount = complains.Count(c => c.TableId == table.Id);

                    // Lấy thống kê của bàn này nếu có
                    var stats = orderStatsDict.TryGetValue(table.Id, out var s)
                        ? s
                        : new OrderStaticsResponse();

                    return new
                    {
                        Key = table.Id.ToString(),
                        Value = new ComplainPeedingInfo(

                            TableName: table.Name,
                            tableStatus: table.Status,
                            paymentStatus : stats.PaymentStatus,
                            Counter: pendingCount,
                            DeliveredCount: stats.DeliveredCount,
                            ServeredCount: stats.ServedCount,
                            PaidCount: stats.PaidCount,
                            TotalItems: stats.TotalOrderItems
                        )
                    };
                })
                .ToDictionary(x => x.Key, x => x.Value);

            return new BaseResponseModel<Dictionary<string, ComplainPeedingInfo>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                result
            );
        }

        public async Task<BaseResponseModel<List<ComplainResponse>>> GetComplainByTable(Guid idTable)
        {
            // ✅ Lấy tất cả complain của table, include luôn OrderItem và Product
            var complains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecWithInclueAsync(
                    new BaseSpecification<Complain>(c => c.TableId == idTable),
                    true,
                    o => o.OrderItem,
                    o => o.OrderItem.Product
                );

            // ✅ Nếu không có dữ liệu, ném lỗi 404
            if (complains == null || !complains.Any())
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không tìm thấy complain");

            // ✅ Map thủ công sang List<ComplainResponse>
            var responseList = complains.Select(c => new ComplainResponse
            {
                ComplainId = c.Id,
                IdTable = c.TableId,
                FeedBack = c.Description,
                CreateData = c.CreatedTime,
                IsPending = c.isPending,

                // Nếu OrderItem có include thì map chi tiết luôn
                Dtos = c.OrderItem != null
                    ? new List<OrderItemDTO>
                    {
                new OrderItemDTO(
                    c.OrderItem.Id,
                    c.OrderItem.Product?.Name ?? "N/A",
                    c.OrderItem.Product?.ImageUrl??"N/A",
                    c.OrderItem.Status
                )
                    }
                    : new List<OrderItemDTO>()
            }).ToList();

            // ✅ Trả kết quả về client
            return new BaseResponseModel<List<ComplainResponse>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                responseList
            );
        }


    }
}
