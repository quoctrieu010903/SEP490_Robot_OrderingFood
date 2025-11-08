using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet.Actions;
using SEP490_Robot_FoodOrdering.Application.DTO.Fillter;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.CancelledItem;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;
using SEP490_Robot_FoodOrdering.Infrastructure.Specifications.CancelledItems;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class CancelledItemService : ICancelledItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CancelledItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }



        public async Task<bool> CreateCancelledItemAsync(
     Guid orderItemId,
     string? cancelNote,
     Guid cancelledByUserId)
        {
            var orderItemRepo = _unitOfWork.Repository<OrderItem, Guid>();

            var orderItem = await orderItemRepo.GetByIdWithIncludeAsync(
                x => x.Id == orderItemId,
                true,
                o => o.Order,
                o => o.ProductSize,
                o => o.OrderItemTopping);

            if (orderItem == null) return false;

            // Cho phép huỷ: Pending + Preparing + Ready
            var canCancelStatuses = new[]
            {
        Domain.Enums.OrderItemStatus.Pending,
        Domain.Enums.OrderItemStatus.Preparing,
        Domain.Enums.OrderItemStatus.Ready
    };

            if (!canCancelStatuses.Contains(orderItem.Status))
                return false;

            // ✅ Tính giá món (base + topping)
            var basePrice = orderItem.ProductSize?.Price ?? 0m;
            var toppingPrice = orderItem.OrderItemTopping?.Sum(t => t.Price) ?? 0m;
            var itemPrice = basePrice + toppingPrice;

            // Fallback: nếu vì lý do gì đó <= 0 thì dùng TotalPrice hiện tại
            if (itemPrice <= 0m)
                itemPrice = (decimal)orderItem.TotalPrice;

            var order = orderItem.Order!;
            var orderTotalBefore = order.TotalPrice;
            var orderTotalAfter = Math.Max(0m, orderTotalBefore - itemPrice);

            var now = DateTime.UtcNow;

            var cancelledItem = new CancelledOrderItem
            {
                OrderItemId = orderItemId,
                Reason = string.IsNullOrWhiteSpace(cancelNote) ? "Không ghi chú" : cancelNote,
                Note = cancelNote,
                CancelledByUserId = cancelledByUserId,
                ItemPrice = itemPrice,
                OrderTotalBefore = orderTotalBefore,
                OrderTotalAfter = orderTotalAfter,
                CreatedBy = cancelledByUserId.ToString(),
                CreatedTime = now,
                LastUpdatedTime = now,
                LastUpdatedBy = cancelledByUserId.ToString(),
            };

            // ✅ Cập nhật OrderItem
            orderItem.Status = Domain.Enums.OrderItemStatus.Cancelled;
            orderItem.LastUpdatedTime = now;
            orderItem.TotalPrice = 0m; // để tránh bị tính lại ở chỗ khác

            // ✅ Cập nhật tổng tiền Order
            order.TotalPrice = orderTotalAfter;

            await _unitOfWork.Repository<CancelledOrderItem, Guid>()
                .AddAsync(cancelledItem);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PaginatedList<CancelledItemResponse>> getAllCancelledItems(CancelledItemFilterRequestParam request)
        {
            var specification = new CancelledItemSpecification(request);
            var response = await _unitOfWork.Repository<CancelledOrderItem, Guid>().GetAllWithSpecAsync(specification);
            var CancelledResponses = _mapper.Map<List<CancelledItemResponse>>(response);
            return PaginatedList<CancelledItemResponse>.Create(CancelledResponses, request.PageIndex, request.PageSize);

        }
    }
}
