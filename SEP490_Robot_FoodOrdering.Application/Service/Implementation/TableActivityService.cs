

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.TableActivities;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class TableActivityService : ITableActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TableActivityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedList<TableActivityLogResponse>> GetLogAsync(Guid sessionId , PagingRequestModel requestModel)
        {
            var spec = new BaseSpecification<TableActivity>(
                x => x.TableSessionId == sessionId);

            // sắp xếp theo thời gian nếu muốn
            spec.AddOrderByDescending(x => x.CreatedTime);

            var activities = await _unitOfWork.Repository<TableActivity, Guid>()
                .GetAllWithSpecAsync(spec);

            var result = activities
                .Select(a => new TableActivityLogResponse
                {

                    TableSessionId = a.TableSessionId,
                    DeviceId = a.DeviceId,
                    Type = a.Type.ToString(),
                    CreatedTime = a.CreatedTime,

                    Data = string.IsNullOrEmpty(a.Data)
                        ? null
                        : JsonSerializer.Deserialize<object>(a.Data)
                })
                .ToList();

            return new PaginatedList<TableActivityLogResponse>(
                result,result.Count(), requestModel.PageNumber,requestModel.PageSize);

        }
        

        public async Task LogAsync(TableSession session, string? deviceId, TableActivityType type, object? data = null)
        {
            var activity = new TableActivity
            {
                TableSessionId = session.Id,
                DeviceId = deviceId,
                Type = type,
                Data = data != null ? JsonSerializer.Serialize(data) : null,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow
            };

            await _unitOfWork.Repository<TableActivity, Guid>().AddAsync(activity);
            await _unitOfWork.SaveChangesAsync();
        }

        public Task LogPaymentActivityAsync(Order order, Payment currentPayment, TableSession session)
        {
            throw new NotImplementedException();
        }
    }
}
