

using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class RemakeService : IRemakeItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RemakeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Task<bool> CanRemakeItemAsync(Guid orderItemId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CanUndoRemakeItemAsync(Guid orderItemId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CreateRemakeItemAsync(Guid orderItemId, string? remakeNote, Guid remakedByUserId)
        {
            var orderitem = await _unitOfWork.Repository<OrderItem , Guid>().GetByIdAsync(orderItemId);
            if (orderitem == null) return false;

            if (orderitem.Status != OrderItemStatus.Served || orderitem.Status != OrderItemStatus.Ready)
                return false;

            var remakeItem = new RemakeItem
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
          await  _unitOfWork.Repository<RemakeItem, Guid>().AddAsync(remakeItem);
            await _unitOfWork.Repository<OrderItem, Guid>().UpdateAsync(orderitem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public Task<bool> RedoRemakeItemAsync(Guid orderItemId, Guid redoneByUserId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UndoRemakeItemAsync(Guid orderItemId, Guid undoneByUserId)
        {
            throw new NotImplementedException();
        }
    }
}
