using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IRemakeItemService
    {
        Task<bool> CreateRemakeItemAsync(Guid orderItemId, string? remakeNote, Guid remakedByUserId);
        Task<bool> UndoRemakeItemAsync(Guid orderItemId, Guid undoneByUserId);
        Task<bool> RedoRemakeItemAsync(Guid orderItemId, Guid redoneByUserId);
        Task<bool> CanRemakeItemAsync(Guid orderItemId);
        Task<bool> CanUndoRemakeItemAsync(Guid orderItemId);

    }
}
