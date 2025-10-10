using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class CancelledItemService : ICancelledItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CancelledItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

     

        public async Task<bool> CreateCancelledItemAsync(Guid orderItemId, string? cancelNote, Guid cancelledByUserId)
        {
            var orderItem = await _unitOfWork.Repository<OrderItem,Guid>().GetByIdWithIncludeAsync(x=>x.Id == orderItemId,true,o => o.Order, o=>o.ProductSize , o=> o.OrderItemTopping);
            if (orderItem == null) return false;
            if (orderItem.Status != Domain.Enums.OrderItemStatus.Preparing && orderItem.Status != Domain.Enums.OrderItemStatus.Ready)
                return false;
            // ✅ Tính giá món (bao gồm topping)
            decimal itemPrice = (orderItem.ProductSize?.Price ?? 0)
                            + orderItem.OrderItemTopping.Sum(t => t.Price); 
            if (itemPrice <= 0)
            {
                itemPrice = (orderItem.ProductSize?.Price ?? 0)
                            + orderItem.OrderItemTopping.Sum(t => t.Price);
            }
            // ✅ Lưu lại tổng trước khi hủy
            decimal orderTotalBefore = orderItem.Order.TotalPrice;
            decimal orderTotalAfter = Math.Max(0, orderTotalBefore - itemPrice);

            var cancelledItem = new CancelledItem
            {
                OrderItemId = orderItemId,
                Reason = cancelNote ?? "Không ghi chú", 
                Note = cancelNote ,
                CancelledByUserId = cancelledByUserId,
                OrderTotalAfter = orderTotalAfter,
                ItemPrice = itemPrice,
                OrderTotalBefore = orderTotalBefore,
                CreatedBy =cancelledByUserId.ToString(),
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = cancelledByUserId.ToString(),

            };
            orderItem.Status = Domain.Enums.OrderItemStatus.Cancelled;
            orderItem.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<CancelledItem, Guid>().AddAsync(cancelledItem);
            await _unitOfWork.SaveChangesAsync();

            return true;


        }

     
    }
}
