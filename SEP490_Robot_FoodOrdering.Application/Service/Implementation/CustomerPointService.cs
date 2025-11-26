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
            var now = DateTime.UtcNow;

            var invoiceRepo = _unitOfWork.Repository<Invoice, Guid>();
            var orderRepo = _unitOfWork.Repository<Order, Guid>();
            var customerRepo = _unitOfWork.Repository<Customer, Guid>();

            var invoice = await invoiceRepo.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Invoice không tồn tại");

           

          
            Guid? customerId = invoice.CustomerId;

            if (!customerId.HasValue)
            {
                // Invoice tạo từ 1 order -> lấy order để lấy CustomerId
                var order = await orderRepo.GetByIdAsync(invoice.OrderId);
                if (order == null)
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Order của invoice không tồn tại");

                customerId = order.CustomerId;
            }

            if (!customerId.HasValue || customerId.Value == Guid.Empty)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.VALIDATION_ERROR,
                    "Không có thông tin khách để tích điểm (Invoice/Order chưa có CustomerId)");

            var customer = await customerRepo.GetByIdAsync(customerId.Value);
            if (customer == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Customer không tồn tại");

            // Tính điểm
            // Tổng tiền tùy model bạn: invoice.TotalMoney / invoice.TotalAmount ...
            var total = invoice.TotalMoney;
            var points = (int)Math.Floor((total * 0.01m) / 1000m);

            if (points < 0) points = 0;

            // Update customer
            customer.TotalPoints += points;
            customer.LifetimePoints += points;
            customer.LastUpdatedTime = now;
            customerRepo.Update(customer);

           
            invoice.LastUpdatedTime = now;
            //invoiceRepo.Update(invoice);

            //await _unitOfWork.SaveChangesAsync();
        }
    }
}
