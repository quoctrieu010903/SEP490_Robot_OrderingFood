

using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class RemakeOrderItemService : IRemakeItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RemakeOrderItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        

        public async Task<bool> CreateRemakeItemAsync(Guid orderItemId, string? remakeNote, Guid remakedByUserId)
        {
            var orderitem = await _unitOfWork.Repository<OrderItem , Guid>().GetByIdAsync(orderItemId);
            if (orderitem == null) return false;

            if (orderitem.Status != OrderItemStatus.Served || orderitem.Status != OrderItemStatus.Ready)
                return false;

            var remakeItem = new RemakeOrderItem
            {
                OrderItemId = orderItemId,
                RemakeNote = remakeNote,
                RemakedByUserId = remakedByUserId,
                PreviousStatus = orderitem.Status,
                AfterStatus = OrderItemStatus.Preparing,
                CreatedBy = remakedByUserId.ToString(),
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow,
                LastUpdatedBy = remakedByUserId.ToString(),
            };
            orderitem.Status =OrderItemStatus.Preparing;
            orderitem.LastUpdatedTime = DateTime.UtcNow;
            orderitem.IsUrgent = true;
            await  _unitOfWork.Repository<RemakeOrderItem, Guid>().AddAsync(remakeItem);
            await _unitOfWork.Repository<OrderItem, Guid>().UpdateAsync(orderitem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public Task<PaginatedList<RemakeOrderItemsResponse>> GetAllRemakeItemsAsync()
        {
           throw new Exception("Not Implemented");
        }

        
    }
}
