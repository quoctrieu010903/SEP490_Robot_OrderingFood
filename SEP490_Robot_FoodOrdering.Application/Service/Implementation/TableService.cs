
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

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class TableService : ITableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IInvoiceService _invoiceService;
        public TableService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService , IInvoiceService invoiceService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _invoiceService = invoiceService;
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
        public async Task<PaginatedList<TableResponse>> GetAll(PagingRequestModel paging , TableEnums? status , string? tableName)
         {
            var list = await _unitOfWork.Repository<Table, Table>().GetAllWithSpecAsync( new TableSpecification(paging.PageNumber , paging.PageSize,status , tableName));
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
                string url = $"{ServerEndpoint.FrontendBase}/{table.Id}";

                // Sinh QR code dạng Base64
                table.QRCode = "data:image/png;base64," + GenerateQrCodeBase64_NoDrawing(url);

            }
            

            return PaginatedList<TableResponse>.Create(mapped, paging.PageNumber, paging.PageSize);
        }
        public async Task<TableResponse> GetById(Guid id)
        {
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

            await _unitOfWork.Repository<Table, Guid>().UpdateAsync(existed);
            await _unitOfWork.SaveChangesAsync();


            return _mapper.Map<TableResponse>(existed);
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
        private string GenerateQrCodeBase64_NoDrawing(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeImage);
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
            // Lấy thông tin bàn
            var existed = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(id);
            if (existed == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Table không tìm thấy");

            // Kiểm tra bàn có sẵn sàng để sử dụng không
            if (existed.Status == TableEnums.Reserved)
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN, "Bàn không khả dụng");
            var currentTable = (await _unitOfWork.Repository<Table, Guid>()
                .GetWithSpecAsync(new BaseSpecification<Table>(x => x.DeviceId == deviceId && x.Status == TableEnums.Occupied)));
            if (currentTable != null && currentTable.Id != id)
            {
                var unpaidInvoices = await _unitOfWork.Repository<Invoice, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<Invoice>(
                        i => i.TableId == currentTable.Id &&
                             (i.status == PaymentStatusEnums.Pending)
                    ));

                if (unpaidInvoices != null)
                {
                    throw new ErrorException(StatusCodes.Status403Forbidden,
                        ResponseCodeConstants.FORBIDDEN,
                        $"Bạn đang có hóa đơn chưa thanh toán ở bàn {currentTable.Name}, vui lòng thanh toán trước khi đổi bàn.");
                }
            }
            // Kiểm tra bàn đã bị sử dụng bởi thiết bị khác
            if (existed.Status == TableEnums.Occupied && existed.DeviceId != deviceId)
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN, "Bàn đã có người sử dụng, vui lòng chuyển sang bàn khác");

            // Nếu cùng thiết bị scan lại -> cho phép tiếp tục sử dụng (refresh session)
            if (existed.Status == TableEnums.Occupied && existed.DeviceId == deviceId)
            {
                // Chỉ refresh thông tin, không thay đổi trạng thái
                existed.LastAccessedAt = DateTime.UtcNow; // Cập nhật thời điểm truy cập cuối
                existed.LastUpdatedTime = DateTime.UtcNow;

                _unitOfWork.Repository<Table, Guid>().Update(existed);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponseModel<TableResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
                    _mapper.Map<TableResponse>(existed), null, "Tiếp tục sử dụng bàn");
            }

            // Bàn Available -> thiết bị mới checkin
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

                return new BaseResponseModel<TableResponse>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS,
                    _mapper.Map<TableResponse>(existed), null, "Đã checkin vào bàn thành công");
            }

            // Trường hợp khác (không nên xảy ra)
            throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Trạng thái bàn không hợp lệ");
        }





        // ===== HELPER METHODS =====
        private async Task HandleOccupiedToAvailable(Table table, List<OrderItem> allItems, List<Order> orders, string updatedBy)
        {
            foreach (var item in allItems)
            {
                var oldItemStatus = item.Status;

                switch (item.Status)
                {
                    case OrderItemStatus.Pending:
                        item.Status = OrderItemStatus.Cancelled;
                        break;
                    case OrderItemStatus.Preparing:
                        item.Status = OrderItemStatus.Cancelled;
                        break;
                    case OrderItemStatus.Ready:
                        item.Status = OrderItemStatus.Cancelled;
                        break;
                    case OrderItemStatus.Served:
                    case OrderItemStatus.Completed:
                    case OrderItemStatus.Cancelled:
                    case OrderItemStatus.RequestCancel:
                        // Giữ nguyên trạng thái
                        break;
                }

                if (item.Status != oldItemStatus)
                {
                    item.LastUpdatedTime = DateTime.UtcNow;
                    _unitOfWork.Repository<OrderItem, Guid>().Update(item);

                    // Send notification for each item status change

                }
            }

            foreach (var order in orders)
            {
                var orderItems = allItems.Where(item => item.OrderId == order.Id).ToList();
                if (!orderItems.Any()) continue;

                var (newOrderStatus, newPaymentStatus) = CalculateOrderAndPaymentStatus(orderItems, order);

                var orderChanged = false;
                if (order.Status != newOrderStatus)
                {
                    order.Status = newOrderStatus;
                    orderChanged = true;
                }

                if (order.PaymentStatus != newPaymentStatus)
                {
                    order.PaymentStatus = newPaymentStatus;
                    orderChanged = true;
                }

                if (orderChanged)
                {
                    order.LastUpdatedTime = DateTime.UtcNow;
                    order.LastUpdatedBy = updatedBy;
                    _unitOfWork.Repository<Order, Order>().Update(order);
                }
            }


            table.Status = TableEnums.Available;
            table.DeviceId = null;
            table.IsQrLocked = false;
            table.LockedAt = null;
            table.LastAccessedAt = null;
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

    }
} 