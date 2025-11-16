using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.TableSession;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

public class TableSessionService : ITableSessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITableActivityService _activityService;
    private readonly IMapper _mapper;

    public TableSessionService(
        IUnitOfWork unitOfWork,
        ITableActivityService activityService , IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _activityService = activityService;
        _mapper = mapper;
    }

    public async Task<TableSession?> GetActiveSessionForDeviceAsync(string deviceId)
    {
        var spec = new BaseSpecification<TableSession>(
            x => x.DeviceId == deviceId && x.Status == TableSessionStatus.Active);

        spec.ApplyInclude(q => q
                                .Include(o => o.Table)
                                .Include(o => o.Customer)
                                .Include(o => o.Orders));


        return await _unitOfWork.Repository<TableSession, Guid>()
            .GetWithSpecAsync(spec);
    }

    public async Task<TableSession?> GetActiveSessionByTokenAsync(string sessionToken)
    {
        var spec = new BaseSpecification<TableSession>(
            x => x.SessionToken == sessionToken && x.Status == TableSessionStatus.Active);

        spec.ApplyInclude(q => q
                                  .Include(o => o.Table)
                                  .Include(o => o.Customer)
                                  .Include(o => o.Orders));

        return await _unitOfWork.Repository<TableSession, Guid>()
            .GetWithSpecAsync(spec);
    }

    public async Task<TableSession> CreateSessionAsync(Table table, string deviceId)
    {
        var now = DateTime.UtcNow;

        var session = new TableSession
        {
            Id = Guid.NewGuid(),
            TableId = table.Id,
            DeviceId = deviceId,
            SessionToken = Guid.NewGuid().ToString("N"),
            Status = TableSessionStatus.Active,
            CheckIn = now,
            CheckOut = null,
            LastActivityAt = now
        };

        await _unitOfWork.Repository<TableSession, Guid>().AddAsync(session);

        // Update Table entity with full locking mechanism
        table.Status = TableEnums.Occupied;
        table.DeviceId = deviceId;           // Set device owner
        table.IsQrLocked = true;             // Lock QR to prevent other devices
        table.LockedAt = now;                // Track when QR was first locked
        table.LastAccessedAt = now;

        await _activityService.LogAsync(session, deviceId, TableActivityType.CheckIn, new
        {
            tableId = table.Id,
            tableName = table.Name
        });
        await _unitOfWork.SaveChangesAsync();

        return session;
    }

    public Task TouchSessionAsync(TableSession session)
    {
        var now = DateTime.UtcNow;
        session.LastActivityAt = now;
        session.Table.LastAccessedAt = now;
        return Task.CompletedTask;
    }

    public async Task CloseSessionAsync(
        TableSession session,
        string reason,
        Guid? invoiceId,
        string? actorDeviceId)
    {
        if (session.Status != TableSessionStatus.Active)
            return;

        var now = DateTime.UtcNow;

        session.Status = TableSessionStatus.Closed;
        session.CheckOut = now;
        session.LastActivityAt = now;

        // Reset table to available state with all fields
        session.Table.Status = TableEnums.Available;
        session.Table.DeviceId = null;
        session.Table.IsQrLocked = false;        // Unlock QR for next customer
        session.Table.LockedAt = null;           // Clear lock timestamp
        session.Table.isShared = false;
        session.Table.ShareToken = null;
        session.Table.LastAccessedAt = now;

        await _activityService.LogAsync(session, actorDeviceId, TableActivityType.CloseSession, new
        {
            reason,
            tableId = session.TableId,
            invoiceId
        });
    }

    public async Task MoveTableAsync(TableSession session, Table newTable, string? actorDeviceId)
    {
        var oldTable = session.Table;
        var now = DateTime.UtcNow;

        // update session
        session.TableId = newTable.Id;
        session.LastActivityAt = now;

        // old table
        oldTable.Status = TableEnums.Available;
        oldTable.LastAccessedAt = now;

        // new table
        newTable.Status = TableEnums.Occupied;
        newTable.LastAccessedAt = now;

        // cập nhật các order đang mở
        var openOrders = session.Orders
            .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .ToList();

        foreach (var order in openOrders)
        {
            order.TableId = newTable.Id;
        }

        await _activityService.LogAsync(session, actorDeviceId, TableActivityType.MoveTable, new
        {
            fromTableId = oldTable.Id,
            fromTableName = oldTable.Name,
            toTableId = newTable.Id,
            toTableName = newTable.Name
        });
    }

    public async Task<PaginatedList<TableSessionResponse>> GetSessionByTableId(Guid tableId, PagingRequestModel request)
    {
        var existedSpec = new BaseSpecification<TableSession>(x => x.TableId == tableId);
        var query = await _unitOfWork.Repository<TableSession, Guid>()
            .GetAllWithSpecWithInclueAsync(existedSpec,  true , ts=> ts.Table , ts => ts.Customer , ts => ts.Activities);
        var response =  _mapper.Map<List<TableSessionResponse>>(query);
        return PaginatedList<TableSessionResponse>.Create(response, request.PageNumber, request.PageSize);

    }
}
