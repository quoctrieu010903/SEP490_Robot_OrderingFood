
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.TableSession;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ITableSessionService
    {
        Task<PaginatedList<TableSessionResponse>> GetSessionByTableId(Guid tableId, PagingRequestModel request);
        Task<TableSession?> GetActiveSessionForDeviceAsync(string deviceId);
        Task<TableSession?> GetActiveSessionByTokenAsync(string sessionToken);

        Task<TableSession> CreateSessionAsync(Table table, string deviceId);

        Task TouchSessionAsync(TableSession session); // update LastActivityAt + Table.LastAccessedAt

        Task CloseSessionAsync(TableSession session, string reason, Guid? invoiceId , string? invoiceCode, string? actorDeviceId);

        Task MoveTableAsync(TableSession session, Table newTable, string? actorDeviceId);
    }

}
