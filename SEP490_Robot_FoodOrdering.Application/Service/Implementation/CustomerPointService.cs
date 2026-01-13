using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public sealed class CustomerPointService : ICustomerPointService
    {
        private readonly IUnitOfWork _unitOfWork;

    

        public CustomerPointService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AwardPointsForInvoiceAsync(Guid invoiceId)
        {
            const decimal VndPerPoint = 1000m; // 1 điểm / 1.000đ
            var now = DateTime.UtcNow;

            var invoiceRepo = _unitOfWork.Repository<Invoice, Guid>();
            var orderRepo = _unitOfWork.Repository<Order, Guid>();
            var customerRepo = _unitOfWork.Repository<Customer, Guid>();

            var invoice = await invoiceRepo.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Invoice không tồn tại");

            // ✅ (Khuyến nghị) chống cộng điểm trùng nếu API bị gọi lại
            // Nếu model chưa có field này thì bạn nên thêm: invoice.AwardedPoints / invoice.PointAwardedAt
            // if (invoice.PointAwardedAt.HasValue) return;

            Guid? customerId = invoice.CustomerId;

            if (!customerId.HasValue)
            {
                var order = await orderRepo.GetByIdAsync(invoice.OrderId);
                if (order == null)
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Order của invoice không tồn tại");

                customerId = order.CustomerId;
            }

            if (!customerId.HasValue || customerId.Value == Guid.Empty)
                return; // cho phép checkout dù không có khách

            var customer = await customerRepo.GetByIdAsync(customerId.Value);
            if (customer == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Customer không tồn tại");

          
            decimal totalVnd = invoice.TotalMoney;

            

            
            var points = (int)Math.Floor(totalVnd / VndPerPoint);
            if (points < 0) points = 0;

            customer.TotalPoints += points;
            customer.LifetimePoints += points;
            customer.LastUpdatedTime = now;
            customerRepo.Update(customer);


            invoice.LastUpdatedTime = now;
            invoiceRepo.Update(invoice);

            // KHÔNG SaveChanges ở đây (đúng theo flow của bạn)
        }


    }
}
