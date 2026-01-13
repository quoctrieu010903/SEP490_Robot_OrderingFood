
using AutoMapper;
using Microsoft.AspNetCore.Http;
using QRCoder;
using System.Drawing;
using System.Linq;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;
using ZXing.QrCode.Internal;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.Abstractions.ServerEndPoint;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using ZXing;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.User;
using CloudinaryDotNet.Core;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class TableService : ITableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IUtilsService _utill;
        private readonly IServerEndpointService _enpointService;
        private readonly ITableSessionService _tableSessionService;
        private readonly ITableActivityService _tableActivityService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerPointService _customerPointService;
        private readonly ILogger<TableService> _logger;
        private readonly IModeratorDashboardRefresher _moderatorDashboardRefresher;
        private readonly IOrderService _orderService;
        private readonly ISettingsService _settingsService;

        public TableService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IUtilsService utils, IServerEndpointService endpointService, ILogger<TableService> logger, ITableSessionService tableSessionService, ITableActivityService tableActivityService, IInvoiceService invoiceService, ICustomerPointService customerPointService, IModeratorDashboardRefresher moderatorDashboardRefresher, IOrderService orderService, ISettingsService settingsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _logger = logger;

            _utill = utils;
            _enpointService = endpointService;
            _tableSessionService = tableSessionService;
            _tableActivityService = tableActivityService;
            _invoiceService = invoiceService;
            _customerPointService = customerPointService;
            _moderatorDashboardRefresher = moderatorDashboardRefresher;
            _orderService = orderService;
            _settingsService = settingsService;
        }
        public async Task<BaseResponseModel> Create(CreateTableRequest request)
        {
            var entity = _mapper.Map<Table>(request);




            entity.Name = request.Name;
            entity.Status = TableEnums.Available; // Mặc định trạng thái là Available
            entity.IsQrLocked = false;
            entity.LockedAt = null;

            entity.CreatedBy = "";

            entity.CreatedTime = DateTime.UtcNow;
            entity.LastUpdatedBy = "";
            entity.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<Table, bool>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, entity);
        }
        public async Task<BaseResponseModel> Delete(Guid id)
        {
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");
            existed.LastUpdatedBy = "";
            existed.LastUpdatedTime = DateTime.UtcNow;
            existed.DeletedBy = "";
            existed.DeletedTime = DateTime.UtcNow;
            _unitOfWork.Repository<Table, Table>().Update(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Xoá thành công");
        }
        public async Task<PaginatedList<TableResponse>> GetAll(PagingRequestModel paging, TableEnums? status, string? tableName)
        {
            var list = await _unitOfWork.Repository<Table, Table>().GetAllWithSpecWithInclueAsync(new TableSpecification(paging.PageNumber, paging.PageSize, status, tableName), true, t => t.Sessions, t => t.Orders);
            var mapped = _mapper.Map<List<TableResponse>>(list);
            mapped = mapped
                        .OrderBy(t =>
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(t.Name, @"\d+");
                            return match.Success ? Convert.ToInt32(match.Value) : int.MaxValue;
                        })
                        .ToList();



            return PaginatedList<TableResponse>.Create(mapped, paging.PageNumber, paging.PageSize);
        }
        public async Task<TableResponse> GetById(Guid id)
        {
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdWithIncludeAsync(t => t.Id == id, true, t => t.Sessions);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

            string url = _enpointService.GetFrontendUrl() + $"/{existed.Id}";

            // Sinh QR code dạng Base64

            var response = _mapper.Map<TableResponse>(existed);
            response.QRCode = "data:image/png;base64," + _utill.GenerateQrCodeBase64_NoDrawing(url);
            return response;

        }

        public async Task<BaseResponseModel> Update(UpdateStatusTable request, Guid id)
        {
            var existed = await _unitOfWork.Repository<Table, Guid>()
                 .GetByIdWithIncludeAsync(
                     t => t.Id == id,
                     true,
                     t => t.Orders
                 );

            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

            existed.Status = request.Status;



            existed.LastUpdatedBy = "";
            existed.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.Repository<Table, Guid>().UpdateAsync(existed);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponseModel(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, "Cập nhật thành công");
        }


        public async Task<TableResponse> ChangeTableStatus(Guid tableId, TableEnums newStatus, string reason, string updatedBy = "System")
        {
            var table = await _unitOfWork.Repository<Table, Guid>().GetByIdWithIncludeAsync(t => t.Id == tableId, true, t => t.Sessions, t => t.Orders);
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");
            if (String.IsNullOrWhiteSpace(reason))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST,
                    "Lý do thay đổi trạng thái bàn không được để trống");
            }
            // Nếu trạng thái giống nhau thì không cần thay đổi
            if (table.Status == newStatus)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST,
                    $"Bàn đã ở trạng thái {newStatus}");

            var latestSessionId = table.Sessions
                                .Where(s => s.Status == TableSessionStatus.Active)     // chỉ lấy session Active
                                .OrderByDescending(s => s.CheckIn)                     // session nào CheckIn mới nhất
                                .FirstOrDefault();                                     // nếu không có thì = null

            // Load orders + orderItems của bàn
            var orders = await _unitOfWork.Repository<Order, Guid>().GetAllWithSpecAsync(
                new OrdersByTableIdsSpecification(tableId)
            );
            var allItems = orders.SelectMany(o => o.OrderItems).ToList();

            // Lưu trạng thái cũ để log
            var oldStatus = table.Status;

            switch (table.Status, newStatus)
            {
                // 1️⃣ Occupied → Available
                case (TableEnums.Occupied, TableEnums.Available):
                    await HandleOccupiedToAvailable(latestSessionId, table, allItems, orders.ToList(), reason, updatedBy);

                    break;

                // 2️⃣ Available → Occupied  
                case (TableEnums.Available, TableEnums.Occupied):
                    await HandleAvailableToOccupied(table, orders.ToList());
                    break;

                // 4️⃣ Occupied → Reserved
                case (TableEnums.Occupied, TableEnums.Reserved):
                    await HandleOccupiedToReserved(table, allItems);
                    break;



                default:
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST,
                        $"Chuyển từ {table.Status} → {newStatus} không hợp lệ");
            }

            table.LastUpdatedTime = DateTime.UtcNow;
            table.LastUpdatedBy = updatedBy;

            // Cập nhật database
            _unitOfWork.Repository<Table, Guid>().Update(table);
            await _unitOfWork.SaveChangesAsync();


            //Log status change
            //await LogTableStatusChange(tableId, oldStatus, newStatus, reason, updatedBy);

            // Send notification
            await SendTableStatusChangeNotification(table, oldStatus, newStatus, reason, updatedBy);
            await _moderatorDashboardRefresher.PushTableAsync(table.Id);

            return _mapper.Map<TableResponse>(table);
        }
            
        public async Task<BaseResponseModel<TableResponse>> ScanQrCode(Guid id, string deviceId)
        {
            // 0. Lấy thông tin bàn
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

            _logger.LogInformation(
                "ScanQrCode: tableId={TableId}, deviceId={DeviceId}, tableStatus={Status}, tableDeviceId={TableDeviceId}",
                id, deviceId, existed.Status, existed.DeviceId);

            // 1. Bàn Reserved -> luôn chặn
            if (existed.Status == TableEnums.Reserved)
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN, "Bàn không khả dụng");

            // 2. Check xem THIẾT BỊ này đang giữ bàn khác trong ngày chưa
            //    (nếu có và còn hóa đơn pending thì chặn đổi bàn)
            var currentTable = await _unitOfWork.Repository<Table, Guid>()
                .GetWithSpecAsync(new BaseSpecification<Table>(x =>
                    x.DeviceId == deviceId &&
                    x.Status == TableEnums.Occupied &&
                    x.CreatedTime.Date == DateTime.UtcNow.Date));

            if (currentTable != null && currentTable.Id != id)
            {
                var unpaidInvoices = await _unitOfWork.Repository<Payment, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<Payment>(
                        i => i.Order.TableId == currentTable.Id &&
                            i.Order.OrderItems.Any(x => x.PaymentStatus != PaymentStatusEnums.Paid) &&
                             i.PaymentStatus == PaymentStatusEnums.Pending));

                if (unpaidInvoices != null)
                {
                    _logger.LogWarning("ScanQrCode: device {DeviceId} còn hóa đơn pending ở {TableName}",
                        deviceId, currentTable.Name);

                    throw new ErrorException(StatusCodes.Status403Forbidden,
                        ResponseCodeConstants.FORBIDDEN,
                        $"Bạn đang có hóa đơn chưa thanh toán ở {currentTable.Name}, vui lòng thanh toán trước khi đổi bàn.");
                }
                else
                {
                    // Không còn hóa đơn pending -> release bàn cũ cho thiết bị này
                    currentTable.Status = TableEnums.Available;
                    currentTable.DeviceId = null;
                    currentTable.IsQrLocked = false;
                    currentTable.LockedAt = null;
                    currentTable.LastUpdatedTime = DateTime.UtcNow;

                    _unitOfWork.Repository<Table, Guid>().Update(currentTable);
                }
            }

            // 3. BÀN HIỆN TẠI đang Occupied bởi THIẾT BỊ KHÁC
            //    → chỉ chặn nếu bàn này còn hóa đơn pending
            if (existed.Status == TableEnums.Occupied && existed.DeviceId != deviceId)
            {
                var unpaidInvoicesForThisTable = await _unitOfWork.Repository<Payment, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<Payment>(
                        i => i.Order.TableId == existed.Id &&
                             i.PaymentStatus == PaymentStatusEnums.Pending));

                if (unpaidInvoicesForThisTable != null)
                {
                    // Vẫn còn bill pending -> block
                    _logger.LogWarning("ScanQrCode: table {TableName} đang occupied bởi device khác và còn bill pending",
                        existed.Name);

                    throw new ErrorException(StatusCodes.Status403Forbidden,
                        ResponseCodeConstants.FORBIDDEN,
                        "Bàn đã có người sử dụng, vui lòng liên hệ nhân viên hỗ trợ.");
                }

                // 👉 KHÔNG còn bill pending -> cho phép device mới chiếm bàn này
                _logger.LogInformation(
                    "ScanQrCode: table {TableName} không còn bill pending, cho phép device {DeviceId} override",
                    existed.Name, deviceId);

                existed.Status = TableEnums.Occupied;
                existed.DeviceId = deviceId;
                existed.IsQrLocked = true;
                existed.LockedAt = DateTime.UtcNow;
                existed.LastAccessedAt = DateTime.UtcNow;
                existed.LastUpdatedTime = DateTime.UtcNow;

                _unitOfWork.Repository<Table, Guid>().Update(existed);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponseModel<TableResponse>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    _mapper.Map<TableResponse>(existed),
                    null,
                    "Đã checkin vào bàn thành công");
            }

            // 4. Nếu cùng thiết bị scan lại -> chỉ refresh
            if (existed.Status == TableEnums.Occupied && existed.DeviceId == deviceId)
            {
                existed.LastAccessedAt = DateTime.UtcNow;
                existed.LastUpdatedTime = DateTime.UtcNow;

                _unitOfWork.Repository<Table, Guid>().Update(existed);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponseModel<TableResponse>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    _mapper.Map<TableResponse>(existed),
                    null,
                    "Tiếp tục sử dụng bàn");
            }

            // 5. Bàn Available -> thiết bị mới checkin
            if (existed.Status == TableEnums.Available)
            {
                existed.Status = TableEnums.Occupied;
                existed.DeviceId = deviceId;
                existed.IsQrLocked = true;
                existed.LockedAt = DateTime.UtcNow;
                existed.LastAccessedAt = DateTime.UtcNow;
                existed.LastUpdatedTime = DateTime.UtcNow;

                _unitOfWork.Repository<Table, Guid>().Update(existed);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponseModel<TableResponse>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    _mapper.Map<TableResponse>(existed),
                    null,
                    "Đã checkin vào bàn thành công");
            }

            // 6. Trường hợp còn lại
            throw new ErrorException(StatusCodes.Status400BadRequest,
                ResponseCodeConstants.BADREQUEST,
                "Trạng thái bàn không hợp lệ");
        }




        // ===== HELPER METHODS =====
        private async Task HandleOccupiedToAvailable(TableSession tableSession, Table table, List<OrderItem> allItems, List<Order> orders, string reason, string updatedBy)
        {

            // 🧩 1️⃣ Xử lý từng OrderItem
            foreach (var item in allItems)
            {
                var oldStatus = item.Status;

                switch (item.Status)
                {
                    case OrderItemStatus.Pending:
                        item.Status = OrderItemStatus.Abandoned;
                        break;

                    // ❗ ĐÃ VÀO BẾP / ĐÃ HOÀN THÀNH → GIỮ NGUYÊN
                    case OrderItemStatus.Preparing:
                    case OrderItemStatus.Ready:
                    case OrderItemStatus.Served:
                    case OrderItemStatus.Completed:
                    case OrderItemStatus.Cancelled:
                    case OrderItemStatus.Remark:
                        break;
                }


                if (item.Status != oldStatus)
                {
                    item.LastUpdatedTime = DateTime.UtcNow;
                    item.LastUpdatedBy = updatedBy;
                    _unitOfWork.Repository<OrderItem, Guid>().Update(item);

                    // TODO: Gửi thông báo real-time nếu cần (ví dụ tới bếp / waiter)
                }
            }
            var paymentPolicyResponse = await _settingsService.GetPaymentPolicyAsync();

            var paymentPolicy = paymentPolicyResponse.Data; // ✅ enum
            foreach (var order in orders)
            {
                var relatedItems = allItems.Where(i => i.OrderId == order.Id).ToList();
                if (!relatedItems.Any()) continue;

                // Tính lại trạng thái order và payment
                var (newOrderStatus, newPaymentStatus) = CalculateOrderAndPaymentStatus(relatedItems, order, paymentPolicy);

                var changed = false;

                // 🔹 Nếu khách chưa thanh toán mà bàn bị chuyển trống → đánh dấu Failed
                if (newPaymentStatus == PaymentStatusEnums.Pending)
                    newPaymentStatus = PaymentStatusEnums.Failed;

                if (order.Status != newOrderStatus)
                {
                    order.Status = newOrderStatus;
                    changed = true;
                }

                if (order.PaymentStatus != newPaymentStatus)
                {
                    order.PaymentStatus = newPaymentStatus;
                    changed = true;
                }

                // 🔹 Tính lại tổng tiền
                var newTotal = CalculateOrderTotal(relatedItems);
                if (order.TotalPrice != newTotal)
                {
                    order.TotalPrice = newTotal;
                    changed = true;
                }

                if (changed)
                {
                    order.LastUpdatedTime = DateTime.UtcNow;
                    order.LastUpdatedBy = updatedBy;
                    _unitOfWork.Repository<Order, Guid>().Update(order);
                }
                order.LastUpdatedBy = "";
                order.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<Order, Guid>().Update(order);
               
            }

            await _unitOfWork.SaveChangesAsync();
            foreach (var order in orders
           .Where(o => o.TableSession.Status == TableSessionStatus.Active))
            {
                var existedInvoice = await _unitOfWork
                    .Repository<Invoice, Guid>()
                    .AnyAsync(i => i.OrderId == order.Id);

                if (existedInvoice)
                    continue;

                // ✅ Create invoice và NHẬN LẠI invoice
                var invoice = await _invoiceService.CreateInvoice(
                    new InvoiceCreatRequest(table.Id, order.Id)
                );

                await _unitOfWork.SaveChangesAsync();
                // ✅ Log activity GẮN VỚI INVOICE VỪA TẠO
                await _tableActivityService.LogAsync(
                    tableSession,
                    table.DeviceId,
                    TableActivityType.CreateInvoice,
                    new
                    {
                        invoiceId = invoice.Id.ToString(),
                        invoiceCode = invoice.InvoiceCode,

                        orderId = order.Id.ToString(),
                        orderCode = order.OrderCode,
                        totalAmount = invoice.TotalAmount,
                        paymentMethod = invoice.PaymentMethod,
                        paymentStatus = invoice.PaymentStatus,
                        createdAtUtc = invoice.CreatedTime,
                        tableSessionId = tableSession.Id.ToString(),
                        tableId = table.Id.ToString(),
                        tableName = table.Name
                    }
                );
            }




            await _tableSessionService.CloseSessionAsync(
                                 tableSession,
                                 "Người điều phối trưởng muốn huỷ bàn vì lý do sau :  " + reason,
                                  null,
                                  null,
                                 table.DeviceId
                             );


            _unitOfWork.Repository<Table, Guid>().Update(table);
        }
        private decimal CalculateOrderTotal(List<OrderItem> orderItems)
        {
            // Lọc bỏ các món bị hủy
            var validItems = orderItems.Where(i => i.Status != OrderItemStatus.Cancelled);

            if (!validItems.Any())
                return 0;

            // Tính tổng giá từng món (base + topping)
            return validItems.Sum(i =>
                i.ProductSize.Price + i.OrderItemTopping.Sum(t => t.Topping.Price)
            );
        }






        private async Task HandleAvailableToOccupied(Table table, List<Order> orders)
        {
            // Kiểm tra xem bàn có order đang active không
            if (orders.Any(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled))
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST,
                    "Bàn đã có order đang hoạt động, không thể chuyển sang Occupied");

            table.Status = TableEnums.Occupied;
            table.IsQrLocked = true;
            table.LockedAt = DateTime.UtcNow;
            table.LastAccessedAt = DateTime.UtcNow;
        }
        private async Task HandleOccupiedToReserved(Table table, List<OrderItem> allItems)
        {
            // Kiểm tra có món đang active không
            var activeItems = allItems.Where(i => i.Status == OrderItemStatus.Pending ||
                                                i.Status == OrderItemStatus.Preparing ||
                                                i.Status == OrderItemStatus.Ready ||
                                                i.Status == OrderItemStatus.Served).ToList();

            if (activeItems.Any())
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST,
                    "Không thể chuyển bàn sang Reserved vì vẫn còn món đang hoạt động");

            // Cancel tất cả pending items
            foreach (var item in allItems.Where(i => i.Status == OrderItemStatus.Pending))
            {
                item.Status = OrderItemStatus.Cancelled;
                item.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<OrderItem, Guid>().Update(item);
            }

            table.Status = TableEnums.Reserved;
            table.IsQrLocked = false;
            table.LockedAt = null;
        }

        private async Task SendTableStatusChangeNotification(Table table, TableEnums oldStatus, TableEnums newStatus,
            string? reason, string updatedBy)
        {
            var notification = new TableStatusChangeNotification
            {
                TableId = table.Id,
                TableName = table.Name,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Reason = reason,
                UpdatedBy = updatedBy,
                UpdatedAt = DateTime.UtcNow,
                NotificationType = "TableStatusChanged"
            };

            //await _notificationService.SendKitchenNotificationAsync(notification);
        }

        private (OrderStatus orderStatus, PaymentStatusEnums paymentStatus)
 CalculateOrderAndPaymentStatus(
     List<OrderItem> items,
     Order currentOrder,
     PaymentPolicy paymentPolicys)
        {
            // 1️⃣ Có món đã phục vụ hay chưa
            var hasChargeableItem = items.Any(i =>
                i.Status == OrderItemStatus.Served ||
                i.Status == OrderItemStatus.Completed);

            // 2️⃣ ORDER STATUS
            var orderStatus = hasChargeableItem
                ? OrderStatus.Completed
                : OrderStatus.Cancelled;

            // 3️⃣ PAYMENT STATUS (PHỤ THUỘC SYSTEM SETTING)
            PaymentStatusEnums paymentStatus;

            if (!hasChargeableItem)
            {
                // ❌ Chưa phục vụ món nào
                if (paymentPolicys == PaymentPolicy.Prepay)
                {
                    // PREPAY: giữ tiền, refund là flow riêng
                    paymentStatus = PaymentStatusEnums.Paid;
                }
                else
                {
                    // POSTPAY: chưa ăn gì → không thu tiền
                    paymentStatus = PaymentStatusEnums.None;
                }
            }
            else
            {
                // ✅ Có món đã phục vụ
                paymentStatus = currentOrder.PaymentStatus == PaymentStatusEnums.Paid
                    ? PaymentStatusEnums.Paid
                    : PaymentStatusEnums.Pending;
            }

            return (orderStatus, paymentStatus);
        }

        public async Task<BaseResponseModel<QrShareResponse>> ShareTableAsync(Guid tableId, string CurrentDevideId)
        {
            var table = await _unitOfWork.Repository<Table, Guid>().GetWithSpecAsync(new BaseSpecification<Table>(x => x.Id == tableId && x.DeviceId == CurrentDevideId));
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Không tìm thấy người dùng hiện tại ở {table.Name} ");
            var sharetoken = Guid.NewGuid().ToString("N");

            table.ShareToken = sharetoken;
            table.isShared = true;
            table.LockedAt = DateTime.UtcNow;
            table.LastAccessedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            var newdevidedtoken = Guid.NewGuid();
            var shareUrl = _enpointService.GetBackendUrl() + $"/Table/{tableId}/accept-share?shareToken={sharetoken}&newDeviceId=";
            string qrCodeBase64 = "data:image/png;base64," + _utill.GenerateQrCodeBase64_NoDrawing(shareUrl);

            var data = new QrShareResponse
            {
                QrCodeBase64 = "qrCodeBase64",
                ShareToken = sharetoken,
                ShareUrl = shareUrl,
                ExpireAt = DateTime.UtcNow.AddMinutes(15)
            };

            return new BaseResponseModel<QrShareResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, data, null, "Chia sẻ bàn thành công,");
        }

        public Task<BaseResponseModel<TableResponse>> TransferTableAsync(Guid tableId, Guid transferToUserId, string? reason = null, string transferredBy = "System")
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponseModel<TableResponse>> AcceptSharedTableAsync(Guid tableId, string shareToken, string newDeviceId)
        {
            var table = _unitOfWork.Repository<Table, Guid>().GetWithSpecAsync(new BaseSpecification<Table>(x => x.Id == tableId && x.ShareToken == shareToken && x.isShared == true));
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Không tìm thấy bàn hoặc token không hợp lệ");
            if (table.Result.LockedAt == null || table.Result.LockedAt.Value.AddMinutes(15) < DateTime.UtcNow)
            {
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN, "Token đã hết hạn");
            }
            else
            {
                table.Result.DeviceId = newDeviceId;
                table.Result.isShared = false;
                table.Result.ShareToken = null;
                table.Result.LastAccessedAt = DateTime.UtcNow;
                table.Result.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<Table, Guid>().Update(table.Result);
                await _unitOfWork.SaveChangesAsync();
                return (new BaseResponseModel<TableResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, _mapper.Map<TableResponse>(table.Result), null, "Chấp nhận chia sẻ bàn thành công"));
            }
        }

        //public async Task<BaseResponseModel<TableResponse>> CheckoutTable(Guid id , CheckoutTableRequest req)
        //{
        //    var now = DateTime.UtcNow;

        //    var existedTable = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
        //    if (existedTable == null)
        //        throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

        //    if (existedTable.Status != TableEnums.Occupied)
        //        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.VALIDATION_ERROR,
        //            "Bàn không ở trạng thái đang sử dụng, không thể checkout");

        //    // ✅ Lấy order đang mở (chưa Completed/Cancelled)
        //    var order = await _unitOfWork.Repository<Order, Guid>()
        //        .GetWithSpecAsync(new BaseSpecification<Order>(o =>
        //            o.TableId == id &&
        //            o.Status != OrderStatus.Completed &&
        //            o.Status != OrderStatus.Cancelled
        //        ));

        //    if (order == null)
        //        throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND,
        //            "Không tìm thấy order đang hoạt động của bàn");

        //    if (order.PaymentStatus != PaymentStatusEnums.Paid)
        //        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_OPERATION,
        //            "Không thể checkout khi order vẫn đang mở hoặc chưa thanh toán");

        //    // ✅ Lấy session Active mới nhất (vì AddOrderByDescending thường return void)
        //    var sessionSpec = new BaseSpecification<TableSession>(s =>
        //        s.TableId == id && s.Status == TableSessionStatus.Active
        //    );
        //    sessionSpec.AddOrderByDescending(s => s.CheckIn);

        //    var tableSession = await _unitOfWork.Repository<TableSession, Guid>()
        //        .GetWithSpecAsync(sessionSpec);

        //    if (tableSession == null)
        //        throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND,
        //            "Bàn hiện không có phiên hoạt động (Active session).");

        //    // ✅ Đóng order
        //    order.Status = OrderStatus.Completed;
        //    order.LastUpdatedTime = now;
        //    _unitOfWork.Repository<Order, Guid>().Update(order);

        //    // ✅ Tạo invoice theo kiểu idempotent + graph (Invoice + InvoiceDetails)
        //    var requestInvoice = new InvoiceCreatRequest(existedTable.Id, order.Id);

        //    var invoice = await _invoiceService.CreateInvoice(requestInvoice); // ❗ KHÔNG SaveChanges trong service

        //    // ✅ Award point (cũng không SaveChanges trong service)
        //    await _customerPointService.AwardPointsForInvoiceAsync(invoice.Id);

        //    // ✅ Log activity (dùng invoice vừa tạo, không dùng order.Invoices)
        //    await _tableActivityService.LogAsync(
        //        tableSession,
        //        existedTable.DeviceId,
        //        TableActivityType.CreateInvoice,
        //        new
        //        {
        //            invoiceId = invoice.Id.ToString(),
        //            invoiceCode = invoice.InvoiceCode,

        //            orderId = order.Id.ToString(),
        //            orderCode = order.OrderCode,

        //            totalAmount = invoice.TotalAmount,
        //            paymentMethod = invoice.PaymentMethod,   // int/enum/string đều được, nhưng phải thống nhất
        //            paymentStatus = order.PaymentStatus,     // idem

        //            createdAtUtc = DateTime.UtcNow,          // rất nên có
        //            tableSessionId = tableSession.Id.ToString(),
        //            tableId = existedTable.Id.ToString(),
        //            tableName = existedTable.Name
        //        });

        //    // ✅ Close session (không SaveChanges bên trong)
        //    await _tableSessionService.CloseSessionAsync(
        //        tableSession,
        //        "Checkout table",
        //        invoice.Id,
        //        invoice.InvoiceCode,
        //        existedTable.DeviceId
        //    );

        //    // ✅ CHỈ COMMIT 1 LẦN Ở CUỐI
        //    await _unitOfWork.SaveChangesAsync();

        //    await _moderatorDashboardRefresher.PushTableAsync(existedTable.Id);
        //    var resp = new BaseResponseModel<TableResponse>(
        //        StatusCodes.Status200OK,
        //        ResponseCodeConstants.SUCCESS,
        //        _mapper.Map<TableResponse>(existedTable),
        //        "Checkout thành công"
        //    );

        //    return resp;
        //    // return new BaseResponseModel<TableResponse>(
        //    //     StatusCodes.Status200OK,
        //    //     ResponseCodeConstants.SUCCESS,
        //    //     _mapper.Map<TableResponse>(existedTable),
        //    //     "Checkout thành công"
        //    // );
        //}

        public async Task<BaseResponseModel<TableResponse>> CheckoutTable(Guid id, CheckoutTableRequest req)
        {
            var now = DateTime.UtcNow;

            var existedTable = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existedTable == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

            if (existedTable.Status != TableEnums.Occupied)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.VALIDATION_ERROR,
                    "Bàn không ở trạng thái đang sử dụng, không thể checkout");

            var order = await _unitOfWork.Repository<Order, Guid>()
                .GetWithSpecAsync(new BaseSpecification<Order>(o =>
                    o.TableId == id &&
                    o.Status != OrderStatus.Completed &&
                    o.Status != OrderStatus.Cancelled
                ));

            if (order == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND,
                    "Không tìm thấy order đang hoạt động của bàn");

            if (order.PaymentStatus != PaymentStatusEnums.Paid)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_OPERATION,
                    "Không thể checkout khi order vẫn đang mở hoặc chưa thanh toán");

            var sessionSpec = new BaseSpecification<TableSession>(s =>
                s.TableId == id && s.Status == TableSessionStatus.Active
            );
            sessionSpec.AddOrderByDescending(s => s.CheckIn);

            var tableSession = await _unitOfWork.Repository<TableSession, Guid>()
                .GetWithSpecAsync(sessionSpec);

            if (tableSession == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND,
                    "Bàn hiện không có phiên hoạt động (Active session).");

            // =========================
            // ✅ OPTIONAL: Nhập tên/SĐT để gán Customer cho Invoice/Order
            // =========================
            Customer? customer = null;
            string? normalizedPhone = null;
            var rawPhone = req?.CustomerPhone;

            if (!string.IsNullOrWhiteSpace(rawPhone))
            {
                normalizedPhone = NormalizeVnPhone(rawPhone);

                // Sau normalize mà rỗng => coi như không nhập
                if (string.IsNullOrWhiteSpace(normalizedPhone))
                {
                    normalizedPhone = null;
                }
                // Nếu không đúng format => coi như không nhập (KHÔNG throw)
                else if (normalizedPhone.Length < 9 || normalizedPhone.Length > 11)
                {
                    normalizedPhone = null;
                }
                else
                {
                    var customerRepo = _unitOfWork.Repository<Customer, Guid>();

                    customer = await customerRepo.GetWithSpecAsync(
                        new BaseSpecification<Customer>(c => c.PhoneNumber == normalizedPhone)
                    );

                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            Id = Guid.NewGuid(),
                            PhoneNumber = normalizedPhone,
                            Name = string.IsNullOrWhiteSpace(req?.CustomerName) ? "Khách lẻ" : req.CustomerName.Trim(),
                            CreatedTime = now,
                            LastUpdatedTime = now
                        };
                        await customerRepo.AddAsync(customer);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(req?.CustomerName) &&
                            (string.IsNullOrWhiteSpace(customer.Name) || customer.Name == "Khách lẻ"))
                        {
                            customer.Name = req.CustomerName.Trim();
                            customer.LastUpdatedTime = now;
                            customerRepo.Update(customer);
                        }
                    }

                    if (!order.CustomerId.HasValue || order.CustomerId.Value == Guid.Empty)
                        order.CustomerId = customer.Id;
                }
            }

            // ✅ Đóng order
            order.Status = OrderStatus.Completed;
            order.LastUpdatedTime = now;
            _unitOfWork.Repository<Order, Guid>().Update(order);

            // ✅ Tạo invoice
            var requestInvoice = new InvoiceCreatRequest(existedTable.Id, order.Id)
            {
                CustomerId = customer?.Id ?? order.CustomerId, // có customer thì ưu tiên, không có thì lấy từ order (nếu có)
                CustomerPhone = customer?.PhoneNumber ?? normalizedPhone,
                CustomerName = customer?.Name
          ?? (string.IsNullOrWhiteSpace(req.CustomerName) ? null : req.CustomerName.Trim())
            };

            var invoice = await _invoiceService.CreateInvoice(requestInvoice);
          
            await _unitOfWork.SaveChangesAsync();

            // ✅ Award point (nếu invoice/customer có thì tự cộng, không có thì return như bạn đang làm)
            await _customerPointService.AwardPointsForInvoiceAsync(invoice.Id);

            await _tableActivityService.LogAsync(
                tableSession,
                existedTable.DeviceId,
                TableActivityType.CreateInvoice,
                new
                {
                    invoiceId = invoice.Id.ToString(),
                    invoiceCode = invoice.InvoiceCode,
                    orderId = order.Id.ToString(),
                    orderCode = order.OrderCode,
                    totalAmount = invoice.TotalAmount,
                    paymentMethod = invoice.PaymentMethod,
                    paymentStatus = order.PaymentStatus,
                    createdAtUtc = now,
                    tableSessionId = tableSession.Id.ToString(),
                    tableId = existedTable.Id.ToString(),
                    tableName = existedTable.Name,
                    customerPhone = normalizedPhone,      // log cho dễ trace
                    customerName = req?.CustomerName
                });

            await _tableSessionService.CloseSessionAsync(
                tableSession,
                "Checkout table",
                invoice.Id,
                invoice.InvoiceCode,
                existedTable.DeviceId
            );

            await _unitOfWork.SaveChangesAsync();

            await _moderatorDashboardRefresher.PushTableAsync(existedTable.Id);

            return new BaseResponseModel<TableResponse>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                _mapper.Map<TableResponse>(existedTable),
                "Checkout thành công"
            );
        }

        private static string NormalizeVnPhone(string raw)
        {
            var p = raw.Trim()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace(".", "");

            if (p.StartsWith("+84")) p = "0" + p.Substring(3);
            if (p.StartsWith("84") && p.Length >= 10) p = "0" + p.Substring(2);

            return p;
        }



        public async Task<BaseResponseModel<TableResponse>> ScanQrCode01(Guid tableId, string deviceId)
        {
            var now = DateTime.UtcNow;

            // ======================================================
            // STEP 0: Validate table
            // ======================================================
            var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(tableId);
            if (table == null)
                throw new ErrorException(
                    StatusCodes.Status404NotFound,
                    ResponseCodeConstants.NOT_FOUND,
                    "Không tìm thấy bàn");

            if (table.Status == TableEnums.Reserved)
                throw new ErrorException(
                    StatusCodes.Status403Forbidden,
                    ResponseCodeConstants.FORBIDDEN,
                    "Bàn không khả dụng");

            // ======================================================
            // STEP 1: Lấy session ACTIVE của bàn đang scan
            // ======================================================
            var tableSession = await _unitOfWork.Repository<TableSession, Guid>()
                .GetWithSpecAsync(new BaseSpecification<TableSession>(
                    s => s.TableId == tableId && s.Status == TableSessionStatus.Active));

            // ======================================================
            // CASE 1: Bàn đã có khách – thiết bị khác quét vào
            // ======================================================
            if (tableSession != null
                && !string.IsNullOrEmpty(tableSession.DeviceId)
                && tableSession.DeviceId != deviceId)
            {
                throw new ErrorException(
                    StatusCodes.Status403Forbidden,
                    ResponseCodeConstants.FORBIDDEN,
                    $"{table.Name} đã có khách sử dụng , Vui lòng quét bàn khác. ");
            }

            // ======================================================
            // CASE 2: Re-enter – cùng thiết bị, cùng bàn
            // ======================================================
            if (tableSession != null && tableSession.DeviceId == deviceId)
            {
                tableSession.LastActivityAt = now;
                table.LastAccessedAt = now;

                _unitOfWork.Repository<TableSession, Guid>().Update(tableSession);
                _unitOfWork.Repository<Table, Guid>().Update(table);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponseModel<TableResponse>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    _mapper.Map<TableResponse>(table),
                    null,
                    "Tiếp tục sử dụng bàn");
            }

            // ======================================================
            // STEP 2: Lấy session ACTIVE của thiết bị
            // ======================================================
            var deviceSession =
                await _tableSessionService.GetActiveSessionForDeviceAsync(deviceId);

            // ======================================================
            // CASE 3: Thiết bị đang ở bàn khác → BLOCK + redirect
            // ======================================================
            if (deviceSession != null && deviceSession.TableId != tableId)
            {
                var oldTable = deviceSession.Table;
                var oldTableName = oldTable?.Name ?? "bàn cũ";

                var redirectUrl = _enpointService.GetFrontendUrl() + $"/{oldTable.Id}";
                // chỉnh lại theo routing FE của bạn nếu cần

                var redirectResponse = new TableResponse() {
                    RedirectUrl = redirectUrl,
                    RedirectTableId = oldTable.Id.ToString()
                };


                return new BaseResponseModel<TableResponse>(
                      StatusCodes.Status403Forbidden,
                      ResponseCodeConstants.FORBIDDEN,
                      redirectResponse,
                      null,
      $"Bạn đang sử dụng {oldTableName}. Vui lòng quay lại bàn này."
  );
            }

            // ======================================================
            // CASE 4: Moderator mở bàn trước (session có nhưng chưa có device)
            // ======================================================
            if (tableSession != null && string.IsNullOrEmpty(tableSession.DeviceId))
            {
                tableSession.DeviceId = deviceId;
                tableSession.LastActivityAt = now;

                table.Status = TableEnums.Occupied;
                table.DeviceId = deviceId;
                table.IsQrLocked = true;
                table.LockedAt ??= now;
                table.LastAccessedAt = now;

                _unitOfWork.Repository<TableSession, Guid>().Update(tableSession);
                _unitOfWork.Repository<Table, Guid>().Update(table);

                await _unitOfWork.SaveChangesAsync();
                await _moderatorDashboardRefresher.PushTableAsync(tableId);

                return new BaseResponseModel<TableResponse>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    _mapper.Map<TableResponse>(table),
                    null,
                    "Đã gán thiết bị vào bàn");
            }

            // ======================================================
            // CASE 5: Bàn trống + thiết bị trống → CREATE SESSION
            // ======================================================
            var newSession = await _tableSessionService.CreateSessionAsync(table, deviceId);

            table.Status = TableEnums.Occupied;
            table.DeviceId = deviceId;
            table.IsQrLocked = true;
            table.LockedAt ??= now;
            table.LastAccessedAt = now;

            _unitOfWork.Repository<Table, Guid>().Update(table);
            await _unitOfWork.SaveChangesAsync();
            await _moderatorDashboardRefresher.PushTableAsync(tableId);

            return new BaseResponseModel<TableResponse>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                _mapper.Map<TableResponse>(table),
                null,
                "Đã check-in vào bàn thành công");
        }

        /// <summary>
        /// Move the latest order from old table to new table
        /// </summary>
        /// <param name="oldTableId">The ID of the old table</param>
        /// <param name="request">Move table request containing newTableId and reason</param>
        /// <returns>Response with updated table information</returns>
        public async Task<BaseResponseModel<TableResponse>> MoveTable(Guid oldTableId, MoveTableRequest request)
        {
            _logger.LogInformation(
                "MoveTable: Starting move from table {OldTableId} to {NewTableId}. Reason: {Reason}",
                oldTableId, request.NewTableId, request.Reason);

            // ===== VALIDATION 1: Check if trying to move to same table =====
            if (oldTableId == request.NewTableId)
            {
                throw new ErrorException(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.BADREQUEST,
                    "Không thể chuyển bàn sang chính bàn đó");
            }

            // ===== VALIDATION 2: Get and validate old table =====
            var oldTable = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(oldTableId);
            if (oldTable == null)
            {
                throw new ErrorException(
                    StatusCodes.Status404NotFound,
                    ResponseCodeConstants.NOT_FOUND,
                    "Bàn cũ không tìm thấy");
            }

            if (oldTable.Status != TableEnums.Occupied)
            {
                throw new ErrorException(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.BADREQUEST,
                    $"{oldTable.Name} không ở trạng thái Occupied. Trạng thái hiện tại: {oldTable.Status}");
            }

            // ===== VALIDATION 3: Get and validate new table =====
            var newTable = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(request.NewTableId);
            if (newTable == null)
            {
                throw new ErrorException(
                    StatusCodes.Status404NotFound,
                    ResponseCodeConstants.NOT_FOUND,
                    "Bàn mới không tìm thấy");
            }

            if (newTable.Status == TableEnums.Occupied)
            {
                throw new ErrorException(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.BADREQUEST,
                    $"{newTable.Name} đang trong trạng thái Occupied. Vui lòng chọn bàn khác");
            }

            if (newTable.Status == TableEnums.Reserved)
            {
                throw new ErrorException(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.BADREQUEST,
                    $"{newTable.Name} đang trong trạng thái Reserved (đã giữ chỗ). Vui lòng chọn bàn khác");
            }

            // ===== VALIDATION 4: Get the latest order from old table =====
            var latestOrder = await _unitOfWork.Repository<Order, Guid>()
                .GetAllWithSpecAsync(new OrdersByTableIdsSpecification(oldTableId));

            if (latestOrder == null || !latestOrder.Any())
            {
                throw new ErrorException(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.BADREQUEST,
                    $"{oldTable.Name} không có order nào để chuyển");
            }

            // Get the most recent order based on CreatedTime
            var orderToMove = latestOrder
                .OrderByDescending(o => o.CreatedTime)
                .FirstOrDefault();

            if (orderToMove == null)
            {
                throw new ErrorException(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.BADREQUEST,
                    "Không tìm thấy order để chuyển");
            }

            _logger.LogInformation(
                "MoveTable: Found latest order {OrderId} created at {CreatedTime}",
                orderToMove.Id, orderToMove.CreatedTime);

            // ===== VALIDATION 5: Check if there's a new order being created (prevent concurrent operations) =====
            // Check if the order was created very recently (e.g., within last 5 seconds)
            var now = DateTime.UtcNow;
            var timeSinceOrderCreated = now - orderToMove.CreatedTime;
            if (timeSinceOrderCreated.TotalSeconds < 5)
            {
                throw new ErrorException(
                    StatusCodes.Status409Conflict,
                    ResponseCodeConstants.CONFLICT,
                    "Có order mới đang được tạo. Vui lòng đợi vài giây và thử lại");
            }

            // ===== VALIDATION 6: Get active session from old table =====
            var activeSessionSpec = new ActiveSessionByTableSpecification(oldTableId);
            var activeSession = await _unitOfWork.Repository<TableSession, Guid>()
                .GetWithSpecAsync(activeSessionSpec);

            if (activeSession == null)
            {
                throw new ErrorException(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.BADREQUEST,
                    $"{oldTable.Name} không có session hoạt động");
            }

            _logger.LogInformation(
                "MoveTable: Found active session {SessionId} with DeviceId {DeviceId}",
                activeSession.Id, activeSession.DeviceId);

            // ===== START TRANSACTION =====
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ===== STEP 1: Update Order's TableId =====
                orderToMove.TableId = request.NewTableId;
                orderToMove.LastUpdatedTime = now;
                orderToMove.LastUpdatedBy = "Moderator";
                _unitOfWork.Repository<Order, Guid>().Update(orderToMove);

                _logger.LogInformation(
                    "MoveTable: Updated order {OrderId} TableId to {NewTableId}",
                    orderToMove.Id, request.NewTableId);

                // ===== STEP 2: Update Invoice's TableId if exists =====
                var invoice = await _unitOfWork.Repository<Invoice, Guid>()
                    .GetAllWithSpecAsync(new InvoiceByOrderIdSpecification(orderToMove.Id));

                if (invoice != null && invoice.Any())
                {
                    foreach (var inv in invoice)
                    {
                        inv.TableId = request.NewTableId;
                        inv.LastUpdatedTime = now;
                        inv.LastUpdatedBy = "Moderator";
                        _unitOfWork.Repository<Invoice, Guid>().Update(inv);

                        _logger.LogInformation(
                            "MoveTable: Updated invoice {InvoiceId} TableId to {NewTableId}",
                            inv.Id, request.NewTableId);
                    }
                }

                // ===== STEP 3: Update TableSession =====
                activeSession.TableId = request.NewTableId;
                activeSession.LastActivityAt = now;
                _unitOfWork.Repository<TableSession, Guid>().Update(activeSession);

                _logger.LogInformation(
                    "MoveTable: Updated session {SessionId} to new table {NewTableId}",
                    activeSession.Id, request.NewTableId);

                // ===== STEP 4: Transfer DeviceId from old table to new table =====
                var deviceIdToTransfer = oldTable.DeviceId;
                var shareTokenToTransfer = oldTable.ShareToken;
                var isSharedToTransfer = oldTable.isShared;

                newTable.DeviceId = deviceIdToTransfer;
                newTable.ShareToken = shareTokenToTransfer;
                newTable.isShared = isSharedToTransfer;
                newTable.Status = TableEnums.Occupied;
                newTable.LastAccessedAt = now;
                newTable.LastUpdatedTime = now;
                newTable.LastUpdatedBy = "Moderator";
                _unitOfWork.Repository<Table, Guid>().Update(newTable);

                _logger.LogInformation(
                    "MoveTable: Transferred DeviceId {DeviceId} to new table {NewTableName}",
                    deviceIdToTransfer, newTable.Name);

                // ===== STEP 5: Reset old table to Available =====
                oldTable.Status = TableEnums.Available;
                oldTable.DeviceId = null;
                oldTable.ShareToken = null;
                oldTable.isShared = false;
                oldTable.LastAccessedAt = now;
                oldTable.LastUpdatedTime = now;
                oldTable.LastUpdatedBy = "Moderator";
                _unitOfWork.Repository<Table, Guid>().Update(oldTable);

                _logger.LogInformation(
                    "MoveTable: Reset old table {OldTableName} to Available",
                    oldTable.Name);

                // ===== STEP 6: Log activity =====
                await _tableActivityService.LogAsync(
                    activeSession,
                    deviceIdToTransfer,
                    TableActivityType.MoveTable,
                    new
                    {
                        fromTableId = oldTableId,
                        fromTableName = oldTable.Name,
                        toTableId = request.NewTableId,
                        toTableName = newTable.Name,
                        orderId = orderToMove.Id,
                        reason = request.Reason,
                        movedBy = "Moderator",
                        movedAt = now
                    });

                _logger.LogInformation(
                    "MoveTable: Logged activity for session {SessionId}",
                    activeSession.Id);

                // ===== COMMIT TRANSACTION =====
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "MoveTable: Successfully moved table from {OldTableName} to {NewTableName}",
                    oldTable.Name, newTable.Name);
            }
            catch (Exception ex)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx,
                        "MoveTable: Error occurred during rollback for table {OldTableId} to {NewTableId}",
                        oldTableId, request.NewTableId);
                }

                _logger.LogError(ex,
                    "MoveTable: Error occurred while moving from table {OldTableId} to {NewTableId}",
                    oldTableId, request.NewTableId);
                throw;
            }

            // ===== RETURN RESPONSE (Outside transaction scope) =====
            // PushTableAsync is called outside transaction to avoid "transaction has completed" error
            // The DbContext needs to be in a clean state after transaction disposal
            await _moderatorDashboardRefresher.PushTableAsync(newTable.Id);

            // ===== STEP 7: Send SignalR notification to customers =====
            // Notify customers on the old table that they have been moved to a new table
            try
            {
                var tableMovedNotification = new Application.DTO.Response.Notification.TableMovedNotification
                {
                    OldTableId = oldTableId,
                    OldTableName = oldTable.Name,
                    NewTableId = request.NewTableId,
                    NewTableName = newTable.Name,
                    Reason = request.Reason,
                    MovedBy = "Moderator",
                    MovedAt = now,
                    Message = $"Bàn của bạn đã được chuyển từ {oldTable.Name} sang {newTable.Name}"
                };

                await _notificationService.SendTableMovedNotificationAsync(tableMovedNotification);

                _logger.LogInformation(
                    "MoveTable: Sent table moved notification to customers on table {OldTableId}",
                    oldTableId);
            }
            catch (Exception ex)
            {
                // Don't fail the operation if notification fails
                _logger.LogError(ex,
                    "MoveTable: Failed to send table moved notification, but operation succeeded");
            }

            var response = _mapper.Map<TableResponse>(newTable);
            return new BaseResponseModel<TableResponse>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                response,
                null,
                $"Đã chuyển bàn thành công từ {oldTable.Name} sang {newTable.Name}");
        }

        /// <summary>
        /// Check if a device token matches the table's current device
        /// </summary>
        /// <param name="tableId">The ID of the table to check</param>
        /// <param name="deviceId">The device ID to verify</param>
        /// <returns>Response indicating if the device matches and table information</returns>
        public async Task<BaseResponseModel<CheckDeviceTokenResponse>> CheckTableAndDeviceToken(Guid tableId, string deviceId)
        {
            _logger.LogInformation(
                "CheckTableAndDeviceToken: Checking table {TableId} for device {DeviceId}",
                tableId, deviceId);

            // Get table from database
            var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(tableId);

            // Case 1: Table not found - return isMatch = false (graceful degradation)
            if (table == null)
            {
                _logger.LogWarning(
                    "CheckTableAndDeviceToken: Table {TableId} not found",
                    tableId);

                return new BaseResponseModel<CheckDeviceTokenResponse>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    new CheckDeviceTokenResponse
                    {
                        IsMatch = false,
                        TableId = tableId,
                        TableName = "Unknown",
                        CurrentDeviceId = null,
                        Status = TableEnums.Available,
                        IsQrLocked = false,
                        LastAccessedAt = null
                    },
                    null,
                    "Bàn không tồn tại");
            }

            // Case 2: Check if deviceId matches
            bool isMatch = !string.IsNullOrEmpty(table.DeviceId) &&
                           !string.IsNullOrEmpty(deviceId) &&
                           table.DeviceId.Equals(deviceId, StringComparison.Ordinal);

            var response = new CheckDeviceTokenResponse
            {
                IsMatch = isMatch,
                TableId = table.Id,
                TableName = table.Name,
                CurrentDeviceId = table.DeviceId,
                Status = table.Status,
                IsQrLocked = table.IsQrLocked,
                LastAccessedAt = table.LastAccessedAt
            };

            string message = isMatch
                ? $"Device khớp với {table.Name}"
                : table.DeviceId == null
                    ? $"{table.Name} chưa có device nào"
                    : $"Device không khớp với {table.Name}";

            _logger.LogInformation(
                "CheckTableAndDeviceToken: Table {TableName} - IsMatch: {IsMatch}, CurrentDeviceId: {CurrentDeviceId}, RequestDeviceId: {RequestDeviceId}",
                table.Name, isMatch, table.DeviceId, deviceId);

            return new BaseResponseModel<CheckDeviceTokenResponse>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                response,
                null,
                message);
        }

        /// <summary>
        /// Random script to scan a table and create an order with random products (no toppings).
        /// </summary>
        /// <param name="tableId">Optional table ID. If not provided, a random available table will be selected.</param>
        /// <returns>Result containing scan result and order creation result</returns>
        public async Task<BaseResponseModel<RandomScanAndOrderResponse>> RandomScanAndOrderAsync(Guid? tableId = null)
        {
            try
            {
                var random = new Random();

                // Step 1: Generate random deviceId
                var randomDeviceId = Guid.NewGuid().ToString();

                // Step 2: Get table (random or specified)
                Guid selectedTableId;
                if (tableId.HasValue)
                {
                    selectedTableId = tableId.Value;
                    var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(selectedTableId);
                    if (table == null)
                    {
                        return new BaseResponseModel<RandomScanAndOrderResponse>(
                            StatusCodes.Status400BadRequest,
                            "TABLE_NOT_FOUND",
                            "Table not found");
                    }
                }
                else
                {
                    // Get a random available table
                    var availableTables = (await _unitOfWork.Repository<Table, Guid>()
                        .GetListAsync(t => t.Status == TableEnums.Available && !t.DeletedTime.HasValue)).ToList();

                    if (availableTables == null || !availableTables.Any())
                    {
                        return new BaseResponseModel<RandomScanAndOrderResponse>(
                            StatusCodes.Status400BadRequest,
                            "NO_AVAILABLE_TABLES",
                            "No available tables found");
                    }

                    selectedTableId = availableTables[random.Next(availableTables.Count)].Id;
                }

                // Step 3: Scan QR Code
                var scanResult = await ScanQrCode01(selectedTableId, randomDeviceId);
                if (scanResult.StatusCode != StatusCodes.Status200OK)
                {
                    return new BaseResponseModel<RandomScanAndOrderResponse>(
                        scanResult.StatusCode,
                        scanResult.ResponseCode ?? "SCAN_FAILED",
                        null,
                        null,
                        scanResult.Message ?? "Failed to scan table");
                }

                // Step 4: Get all products from database
                var allProducts = (await _unitOfWork.Repository<Product, Guid>()
                    .GetListAsync(p => !p.DeletedTime.HasValue)).ToList();

                if (allProducts == null || !allProducts.Any())
                {
                    return new BaseResponseModel<RandomScanAndOrderResponse>(
                        StatusCodes.Status400BadRequest,
                        "NO_PRODUCTS",
                        "No products found in database");
                }

                // Step 5: Randomly select 1-3 products
                var numberOfItems = random.Next(1, 4); // 1 to 3 items
                var selectedProducts = allProducts.OrderBy(x => random.Next()).Take(numberOfItems).ToList();

                // Step 6: For each product, get a random product size
                var orderItems = new List<CreateOrderItemRequest>();
                foreach (var product in selectedProducts)
                {
                    var productSizes = (await _unitOfWork.Repository<ProductSize, Guid>()
                        .GetListAsync(ps => ps.ProductId == product.Id && !ps.DeletedTime.HasValue)).ToList();

                    if (productSizes == null || !productSizes.Any())
                    {
                        continue; // Skip products without sizes
                    }

                    var randomSize = productSizes[random.Next(productSizes.Count)];

                    orderItems.Add(new CreateOrderItemRequest
                    {
                        ProductId = product.Id,
                        ProductSizeId = randomSize.Id,
                        ToppingIds = new List<Guid>(), // No toppings as requested
                        Note = null
                    });
                }

                if (!orderItems.Any())
                {
                    return new BaseResponseModel<RandomScanAndOrderResponse>(
                        StatusCodes.Status400BadRequest,
                        "NO_VALID_PRODUCTS",
                        "No products with valid sizes found");
                }

                // Step 7: Create order
                var createOrderRequest = new CreateOrderRequest
                {
                    TableId = selectedTableId,
                    deviceToken = randomDeviceId,
                    Items = orderItems
                };

                var orderResult = await _orderService.HandleOrderAsync(createOrderRequest);

                // Step 8: Set payment status to Paid for order and all order items
                if (orderResult.StatusCode == StatusCodes.Status200OK || orderResult.StatusCode == StatusCodes.Status201Created)
                {
                    if (orderResult.Data != null && orderResult.Data.Id != Guid.Empty)
                    {
                        var orderId = orderResult.Data.Id;

                        // Load order with order items
                        var order = await _unitOfWork.Repository<Order, Guid>()
                            .GetByIdWithIncludeAsync(o => o.Id == orderId, true, o => o.OrderItems);

                        if (order != null)
                        {
                            // Set payment status to Paid for order
                            order.PaymentStatus = PaymentStatusEnums.Paid;
                            order.LastUpdatedTime = DateTime.UtcNow;

                            // Set payment status to Paid for all order items
                            foreach (var orderItem in order.OrderItems)
                            {
                                orderItem.PaymentStatus = PaymentStatusEnums.Paid;
                                orderItem.LastUpdatedTime = DateTime.UtcNow;
                                _unitOfWork.Repository<OrderItem, Guid>().Update(orderItem);
                            }

                            // Update order
                            _unitOfWork.Repository<Order, Guid>().Update(order);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                // Return combined result
                var response = new RandomScanAndOrderResponse
                {
                    DeviceId = randomDeviceId,
                    TableId = selectedTableId,
                    ScanResult = scanResult,
                    OrderResult = orderResult
                };

                return new BaseResponseModel<RandomScanAndOrderResponse>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    response,
                    null,
                    "Random scan and order created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RandomScanAndOrderAsync: Error occurred while executing random scan and order");
                return new BaseResponseModel<RandomScanAndOrderResponse>(
                    StatusCodes.Status500InternalServerError,
                    "INTERNAL_ERROR",
                    null,
                    null,
                    ex.Message);
            }
        }

    }
}