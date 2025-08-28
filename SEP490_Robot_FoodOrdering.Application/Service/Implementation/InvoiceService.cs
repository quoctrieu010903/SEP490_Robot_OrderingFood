using SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation;

public class InvoiceService : Interface.IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<InvoiceResponse> createInvoice(InvoiceCreatRequest request)
    {
        Console.WriteLine($"Request received: request is null = {request == null}");
        if (request != null)
        {
            Console.WriteLine($"TableId: {request.tableId}, Status: {request.status}");
        }

        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null");

        if (request.tableId == Guid.Empty)
            throw new ArgumentException("Table ID cannot be empty", nameof(request.tableId));

        var table = await _unitOfWork.Repository<Table, Guid>().GetByIdAsync(request.tableId);

        if (table == null)
            throw new Exception("Table not found");

        Invoice invoice = new Invoice();
        Order order = null;

        var temp = await _unitOfWork.Repository<Order, Guid>()
            .GetAllWithSpecAsync(new OrdersByTableIdsSpecification(request.tableId));

        Console.WriteLine($"Found {temp?.Count() ?? 0} orders");

        if (temp == null || !temp.Any())
        {
            throw new Exception("No orders found for this table");
        }

        foreach (var tableOrder in temp)
        {
            if (tableOrder?.PaymentStatus == PaymentStatusEnums.Pending)
            {
                order = tableOrder;
                break;
            }
        }

        if (order == null)
        {
            throw new Exception("Order not found or no pending order available");
        }


        if (order.OrderItems == null || !order.OrderItems.Any())
            throw new Exception("Order has no items");

        List<InvoiceDetail> details = new List<InvoiceDetail>();

        if (order.Payment == null)
        {
            order.Payment = new Payment()
            {
                CreatedTime = DateTime.UtcNow,
                Order = order,
                PaymentStatus = PaymentStatusEnums.Pending,
                PaymentMethod = request.MethodEnums == null ? PaymentMethodEnums.COD : request.MethodEnums,
            };
        }

        if (request.status == StatusInvoice.Payment)
        {
            order.Payment.PaymentStatus = PaymentStatusEnums.Paid;

            foreach (var orderItem in order.OrderItems)
            {
                // Kiểm tra null cho từng orderItem
                if (orderItem == null)
                {
                    Console.WriteLine("Warning: Found null order item, skipping...");
                    continue;
                }

                if (orderItem.ProductSize == null)
                {
                    Console.WriteLine($"Warning: OrderItem {orderItem.Id} has null ProductSize, skipping...");
                    continue;
                }

                decimal toppingPrice = 0;
                try
                {
                    if (orderItem.OrderItemTopping != null && orderItem.OrderItemTopping.Any())
                    {
                        toppingPrice = orderItem.OrderItemTopping
                            .Where(t => t != null)
                            .Sum(topping => topping.Price);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating topping price for OrderItem {orderItem.Id}: {ex.Message}");
                    toppingPrice = 0;
                }

                details.Add(new InvoiceDetail()
                {
                    OrderItem = orderItem,
                    CreatedTime = DateTime.UtcNow,
                    totalMoney = orderItem.ProductSize.Price + toppingPrice
                });
            }

            invoice.CreatedTime = DateTime.UtcNow;
            _unitOfWork.Repository<Order, Guid>().Update(order);
        }
        else
        {
            order.Payment.PaymentStatus = PaymentStatusEnums.Failed;
            order.Status = OrderStatus.Cancelled;

            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem == null)
                {
                    Console.WriteLine("Warning: Found null order item in failed payment, skipping...");
                    continue;
                }

                details.Add(new InvoiceDetail()
                {
                    OrderItem = orderItem,
                    CreatedTime = DateTime.UtcNow,
                    totalMoney = 0
                });
            }
        }

        if (!details.Any())
        {
            throw new Exception("No valid order items found to create invoice");
        }

        invoice.Details = details;
        invoice.totalMoney = details.Sum(d => d.totalMoney);
        invoice.Table = table;

        _unitOfWork.Repository<Invoice, Guid>().Add(invoice);
        _unitOfWork.SaveChanges();

        return new InvoiceResponse()
        {
            Id = invoice.Id,
            TableName = table.Name ?? "Unknown Table",
            CreatedTime = invoice.CreatedTime,
            TotalMoney = invoice.totalMoney,
            PaymentStatus = order.Payment.PaymentStatus.ToString(),
            Details = details.Select(d => CreateInvoiceDetailResponse(d)).Where(d => d != null).ToList()
        };
    }

    private InvoiceDetailResponse CreateInvoiceDetailResponse(InvoiceDetail detail)
    {
        try
        {
            if (detail?.OrderItem == null)
                return null;

            var orderItem = detail.OrderItem;

            return new InvoiceDetailResponse()
            {
                OrderItemId = orderItem.Id,
                ProductName = orderItem.ProductSize?.Product?.Name ?? "Unknown Product",
                UnitPrice = orderItem.ProductSize?.Price ?? 0,
                Toppings = orderItem.OrderItemTopping?
                    .Where(t => t?.Topping != null)
                    .Select(t => t.Topping.Name ?? "Unknown Topping")
                    .ToList() ?? new List<string>(),
                TotalMoney = detail.totalMoney
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating invoice detail response: {ex.Message}");
            return new InvoiceDetailResponse()
            {
                OrderItemId = detail?.OrderItem?.Id ?? Guid.Empty,
                ProductName = "Error Loading Product",
                UnitPrice = 0,
                Toppings = new List<string>(),
                TotalMoney = detail?.totalMoney ?? 0
            };
        }
    }
}