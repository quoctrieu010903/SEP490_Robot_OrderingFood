using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.TableSession;
using SEP490_Robot_FoodOrdering.Application.DTO.TableActivity;
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
    private readonly IUtilsService _utilsService;

    public TableSessionService(
        IUnitOfWork unitOfWork,
        ITableActivityService activityService, IMapper mapper, IUtilsService utilsService)
    {
        _unitOfWork = unitOfWork;
        _activityService = activityService;
        _mapper = mapper;
        _utilsService = utilsService;
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
            SessionCode = _utilsService.GenerateCode("SC", 6),
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
     string? invoiceCode,
     string? actorDeviceId)
    {
        if (session.Status != TableSessionStatus.Active)
            return;

        var now = DateTime.UtcNow;

        session.Status = TableSessionStatus.Closed;
        session.CheckOut = now;
        session.LastActivityAt = now;

        session.Table.Status = TableEnums.Available;
        session.Table.DeviceId = null;
        session.Table.IsQrLocked = false;
        session.Table.LockedAt = null;
        session.Table.isShared = false;
        session.Table.ShareToken = null;
        session.Table.LastAccessedAt = null;

        // Auto-resolve all pending complaints for this table
        var pendingComplains = await _unitOfWork.Repository<Complain, Guid>()
            .GetAllWithSpecAsync(new BaseSpecification<Complain>(c => c.TableId == session.TableId && c.isPending));
        
        foreach (var comp in pendingComplains)
        {
            comp.isPending = false;
            comp.ResolvedAt = now;
            
            if (string.IsNullOrEmpty(comp.ResolutionNote))
            {
                comp.ResolutionNote = "System: Session Closed";
            }
            else
            {
                 comp.ResolutionNote += " (System: Session Closed)";
            }
            
            _unitOfWork.Repository<Complain, Guid>().Update(comp);
        }

        var actorType = string.IsNullOrWhiteSpace(actorDeviceId) ? "System" : "Customer";

        var payload = TableActivityPayloadFactory.Build(
            action: TableActivityType.CloseSession.ToString(),     // "SESSION_CLOSED"
            actorType: actorType,
            actorUserId: null,
            actorDeviceId: actorDeviceId,
            reasonCode: "CHECKOUT",
            reasonText: reason,
            snapshot: new { tablename = session.Table.Name, invoiceId , invoiceCode}
        );

        await _activityService.LogAsync(
            session,
            actorDeviceId,
            TableActivityType.CloseSession,
            payload
            
        );
        // ✅ Save changes after logging activity
        // Note: If called within a transaction, this will be part of the transaction
        await _unitOfWork.SaveChangesAsync();
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
        // ✅ Save changes after logging activity
        // Note: If called within a transaction, this will be part of the transaction
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginatedList<TableSessionResponse>> GetSessionByTableId(
      Guid tableId, PagingRequestModel request)
    {
        // 1) Lấy sessions
        var sessionSpec = new BaseSpecification<TableSession>(x => x.TableId == tableId);
        sessionSpec.AddOrderByDescending(x => x.CheckIn);

        var sessions = await _unitOfWork.Repository<TableSession, Guid>()
            .GetAllWithSpecWithInclueAsync(
                sessionSpec,
                true,
                ts => ts.Table,
                ts => ts.Customer,
                ts => ts.Activities
            );

        var response = _mapper.Map<List<TableSessionResponse>>(sessions);

        var sessionIds = sessions.Select(s => s.Id).ToList();
        if (sessionIds.Count == 0)
            return PaginatedList<TableSessionResponse>.Create(response, request.PageNumber, request.PageSize);

        // 2) Query orders đúng sessionIds (như bạn đang làm)
        var orderSpec = new BaseSpecification<Order>(o =>
            o.TableId == tableId
            && o.TableSessionId.HasValue
            && sessionIds.Contains(o.TableSessionId.Value)
        );

        var orders = await _unitOfWork.Repository<Order, Guid>()
            .GetAllWithSpecWithInclueAsync(orderSpec, true, o => o.Invoices);

        // 3) Map theo Order -> Invoice (chuẩn nếu data đúng)
        var invoiceMap = orders
            .Where(o => o.TableSessionId.HasValue)
            .GroupBy(o => o.TableSessionId!.Value)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    Guid? invoiceId = g
                        .Select(o => (Guid?)o.Invoices?.Id)
                        .FirstOrDefault(id => id.HasValue && id.Value != Guid.Empty);

                    return invoiceId; // trả thẳng Guid?
                }
            );

        // 4) Fallback: nếu mismatch (không có invoice theo order) thì map theo thời gian invoice nằm trong session
        // Lưu ý: đổi "CreatedAt" đúng tên field của Invoice entity của bạn
        var minTime = sessions.Min(s => s.CheckIn).AddMinutes(-5);
        var maxTime = (sessions.Max(s => s.CheckOut ?? DateTime.UtcNow)).AddMinutes(5);

        var invoiceSpec = new BaseSpecification<Invoice>(i =>
            i.TableId == tableId
            && i.CreatedTime >= minTime
            && i.CreatedTime <= maxTime
            && i.Id != Guid.Empty
        );

        var invoices = await _unitOfWork.Repository<Invoice, Guid>()
            .GetAllWithSpecAsync(invoiceSpec);

        foreach (var r in response)
        {
            Guid? invId = null;

            // ưu tiên map chuẩn theo order
            if (invoiceMap.TryGetValue(r.Id, out var mappedId) && mappedId.HasValue)
            {
                invId = mappedId.Value;
            }
            else
            {
                // fallback theo khoảng thời gian session
                var session = sessions.First(s => s.Id == r.Id);

                var start = session.CheckIn.AddMinutes(-1);
                var end = (session.CheckOut ?? DateTime.UtcNow).AddMinutes(1);

                invId = invoices
                    .Where(i => i.CreatedTime >= start && i.CreatedTime <= end)
                    .OrderByDescending(i => i.CreatedTime)
                    .Select(i => (Guid?)i.Id)
                    .FirstOrDefault();
            }

            r.InvoiceId = invId;
            r.HasInvoice = invId.HasValue;
        }

        return PaginatedList<TableSessionResponse>.Create(response, request.PageNumber, request.PageSize);
    }

}
