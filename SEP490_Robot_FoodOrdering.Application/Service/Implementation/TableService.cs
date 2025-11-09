
using AutoMapper;
using Microsoft.AspNetCore.Http;
using QRCoder;
using System.Drawing; 
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
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

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class TableService : ITableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IUtilsService _utill;
        private readonly IServerEndpointService _enpointService;
        private readonly ILogger<TableService> _logger;

        public TableService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService , IUtilsService utils , IServerEndpointService endpointService, ILogger<TableService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _logger = logger;
            
            _utill = utils;
            _enpointService = endpointService;
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
            var list = await _unitOfWork.Repository<Table, Table>().GetAllWithSpecAsync(new TableSpecification(paging.PageNumber, paging.PageSize, status, tableName));
            var mapped = _mapper.Map<List<TableResponse>>(list);
            mapped = mapped
                        .OrderBy(t =>
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(t.Name, @"\d+");
                            return match.Success ? Convert.ToInt32(match.Value) : int.MaxValue;
                        })
                        .ToList();
            foreach (var table in mapped)
            {
                // Tạo URL chứa id của bàn
                //string url = $"{ServerEndpoint.}/{table.Id}";

                //// Sinh QR code dạng Base64
                //table.QRCode = "data:image/png;base64," + GenerateQrCodeBase64_NoDrawing(url);

            }


            return PaginatedList<TableResponse>.Create(mapped, paging.PageNumber, paging.PageSize);
        }
        public async Task<TableResponse> GetById(Guid id)
        {
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

             string url = _enpointService.GetFrontendUrl() + $"/{existed.Id}";

                // Sinh QR code dạng Base64
              
            var response = _mapper.Map<TableResponse>(existed);
            response.QRCode = "data:image/png;base64," +_utill.GenerateQrCodeBase64_NoDrawing(url);
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
     

        public async Task<TableResponse> ChangeTableStatus(Guid tableId, TableEnums newStatus, string? reason = null, string updatedBy = "System")
        {
            var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(tableId);
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

            // Nếu trạng thái giống nhau thì không cần thay đổi
            if (table.Status == newStatus)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST,
                    $"Bàn đã ở trạng thái {newStatus}");

            // Load orders + orderItems của bàn
            var orders = await _unitOfWork.Repository<Order, Order>().GetAllWithSpecAsync(
                new OrdersByTableIdsSpecification(tableId)
            );
            var allItems = orders.SelectMany(o => o.OrderItems).ToList();

            // Lưu trạng thái cũ để log
            var oldStatus = table.Status;

            switch (table.Status, newStatus)
            {
                // 1️⃣ Occupied → Available
                case (TableEnums.Occupied, TableEnums.Available):
                    await HandleOccupiedToAvailable(table, allItems, orders.ToList(), updatedBy);
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
                            i.Order.OrderItems.Any(x=>x.PaymentStatus != PaymentStatusEnums.Paid ) &&
                             i.PaymentStatus == PaymentStatusEnums.Pending));

                if (unpaidInvoices != null)
                {
                    _logger.LogWarning("ScanQrCode: device {DeviceId} còn hóa đơn pending ở bàn {TableName}",
                        deviceId, currentTable.Name);

                    throw new ErrorException(StatusCodes.Status403Forbidden,
                        ResponseCodeConstants.FORBIDDEN,
                        $"Bạn đang có hóa đơn chưa thanh toán ở bàn {currentTable.Name}, vui lòng thanh toán trước khi đổi bàn.");
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
        private async Task HandleOccupiedToAvailable(Table table, List<OrderItem> allItems, List<Order> orders, string updatedBy)
        {
            // 🧩 1️⃣ Xử lý từng OrderItem
            foreach (var item in allItems)
            {
                var oldStatus = item.Status;

                switch (item.Status)
                {
                    case OrderItemStatus.Pending:
                    case OrderItemStatus.Preparing:
                    case OrderItemStatus.Ready:
                        item.Status = OrderItemStatus.Cancelled;
                        break;
                    // Các trạng thái đã hoàn thành thì giữ nguyên
                    case OrderItemStatus.Served:
                    case OrderItemStatus.Completed:
                    case OrderItemStatus.Cancelled:
                    case OrderItemStatus.RequestCancel:
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

            // 🧩 2️⃣ Xử lý từng Order
            foreach (var order in orders)
            {
                var relatedItems = allItems.Where(i => i.OrderId == order.Id).ToList();
                if (!relatedItems.Any()) continue;

                // Tính lại trạng thái order và payment
                var (newOrderStatus, newPaymentStatus) = CalculateOrderAndPaymentStatus(relatedItems, order);

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
                    _unitOfWork.Repository<Order, Order>().Update(order);
                }

                // Đánh dấu order đã đóng lại (vì bàn đã được giải phóng)
                order.LastUpdatedBy = "";
                order.LastUpdatedTime = DateTime.UtcNow;
                order.PaymentStatus = PaymentStatusEnums.None;
                _unitOfWork.Repository<Order, Order>().Update(order);
            }

            await _unitOfWork.SaveChangesAsync();

            // 🧩 3️⃣ Cập nhật lại thông tin bàn
            table.Status = TableEnums.Available;
           
            table.DeviceId = null;
            table.IsQrLocked = false;
            table.LockedAt = null;
            table.LastAccessedAt = null;
            table.LastUpdatedBy = updatedBy;
            table.LastUpdatedTime = DateTime.UtcNow;

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

        private (OrderStatus orderStatus, PaymentStatusEnums paymentStatus) CalculateOrderAndPaymentStatus(
        List<OrderItem> allItems, Order currentOrder)
        {
            var totalItems = allItems.Count;
            var servedItems = allItems.Count(x => x.Status == OrderItemStatus.Served ||
                                                 x.Status == OrderItemStatus.Completed);
            var cancelledItems = allItems.Count(x => x.Status == OrderItemStatus.Cancelled);
            var requestCancelItems = allItems.Count(x => x.Status == OrderItemStatus.RequestCancel);

            // Xác định Order Status
            OrderStatus newOrderStatus;
            if (requestCancelItems > 0)
            {
                // Có món đang chờ xác nhận hủy
                newOrderStatus = OrderStatus.Cancelled;
            }
            else if (servedItems == totalItems)
            {
                // Tất cả món đã được phục vụ
                newOrderStatus = OrderStatus.Completed;
            }
            else if (cancelledItems == totalItems)
            {
                // Tất cả món đều bị hủy
                newOrderStatus = OrderStatus.Cancelled;
            }
            else if (servedItems > 0 && cancelledItems > 0)
            {
                // Hỗn hợp: một phần đã phục vụ, một phần bị hủy
                newOrderStatus = OrderStatus.Completed;
            }
            else
            {
                // Trường hợp khác (fallback)
                newOrderStatus = OrderStatus.Cancelled;
            }

            // Xác định Payment Status
            PaymentStatusEnums newPaymentStatus;
            if (requestCancelItems > 0)
            {
                // Có món chờ xác nhận hủy → chờ xử lý
                newPaymentStatus = PaymentStatusEnums.Pending;
            }
            else if (cancelledItems == totalItems)
            {
                // Tất cả món bị hủy → hoàn tiền (nếu đã thanh toán)
                newPaymentStatus = currentOrder.PaymentStatus == PaymentStatusEnums.Paid
                    ? PaymentStatusEnums.Refunded
                    : PaymentStatusEnums.Pending;
            }
            else if (servedItems == totalItems)
            {
                // Tất cả món đã phục vụ → giữ nguyên hoặc chờ thanh toán
                newPaymentStatus = currentOrder.PaymentStatus;
            }
            else if (servedItems > 0 && cancelledItems > 0)
            {
                // Trường hợp hỗn hợp → cần xử lý hoàn tiền một phần
                // Tạm thời set Pending để xử lý manual
                newPaymentStatus = PaymentStatusEnums.Pending;
            }
            else
            {
                // Trường hợp khác
                newPaymentStatus = PaymentStatusEnums.Pending;
            }

            return (newOrderStatus, newPaymentStatus);
        }

        public async Task<BaseResponseModel<QrShareResponse>> ShareTableAsync(Guid tableId, string CurrentDevideId)
        {
            var table = await _unitOfWork.Repository<Table, Guid>().GetWithSpecAsync(new BaseSpecification<Table>(x => x.Id == tableId && x.DeviceId == CurrentDevideId));
            if (table == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Không tìm thấy người dùng hiện tại ở bàn {table.Name} ");
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
    }
}