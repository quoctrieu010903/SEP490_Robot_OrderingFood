using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Customer;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Customer;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class TableCustomerService : ITableCustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TableCustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BindCustomerToTableResult> BindCustomerToActiveSessionAsync(Guid tableId, string deviceId, BindCustomerToTableRequest req)
        {
            if(String.IsNullOrWhiteSpace(deviceId))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.VALIDATION_ERROR, "DeviceId là bắt buộc");
            }
            var name = (req.Name ?? "").Trim();
            var phone = (req.PhoneNumber ?? "").Trim();
            if ((string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone)))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.VALIDATION_ERROR, "Name và PhoneNumber là bắt buộc");
            }
            var session = await _unitOfWork.Repository<TableSession, Guid>()
            .GetWithSpecAsync(new BaseSpecification<TableSession>(s =>
                s.TableId == tableId && s.Status == TableSessionStatus.Active && s.DeviceId == deviceId));

            if (session == null)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_OPERATION,
                    "Không tìm thấy session active của bàn theo thiết bị hiện tại");
        
            var customerRepo = _unitOfWork.Repository<Customer, Guid>();
            var existedCustomer = await customerRepo.GetWithSpecAsync(new BaseSpecification<Customer>(c => c.PhoneNumber == phone));

            Customer customer;
            if (existedCustomer != null)
            {
                existedCustomer.Name = name;
                existedCustomer.LastUpdatedTime = DateTime.UtcNow;
                customerRepo.Update(existedCustomer);
                customer = existedCustomer;
            }
            else
            {
                customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = phone,
                    Name = name,
                    TotalPoints = 0,
                    LifetimePoints = 0,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow
                };
                await customerRepo.AddAsync(customer);
            }
            session.CustomerId = customer.Id;
            session.LastUpdatedTime = DateTime.UtcNow;
            _unitOfWork.Repository<TableSession, Guid>().Update(session);
            var order = await _unitOfWork.Repository<Order, Guid>()
            .GetWithSpecAsync(new BaseSpecification<Order>(o =>
                o.TableId == tableId
                && o.Status != OrderStatus.Completed
                && o.Status != OrderStatus.Cancelled));

            if (order != null && !order.CustomerId.HasValue)
            {
                order.CustomerId = customer.Id;
                order.LastUpdatedTime = DateTime.UtcNow;
                _unitOfWork.Repository<Order, Guid>().Update(order);
            }

            await _unitOfWork.SaveChangesAsync();

            return new BindCustomerToTableResult
            {
                CustomerId = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                TableId = tableId,
                SessionId = session.Id,
                OrderId = order?.Id,
                CreatAt = DateTime.UtcNow
            };
        }
        

       public async Task<CustomerResponse?> GetActiveCustomerByDeviceIdAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return null;

            // lấy session active mới nhất theo deviceId (để an toàn)
            var sessions = await _unitOfWork.Repository<TableSession, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<TableSession>(s =>
                    s.DeviceId == deviceId && s.Status == TableSessionStatus.Active));

            var session = sessions?.OrderByDescending(x => x.CreatedTime).FirstOrDefault();
            if (session == null || !session.CustomerId.HasValue) return null;

            var customer = await _unitOfWork.Repository<Customer, Guid>().GetByIdAsync(session.CustomerId.Value);
            if (customer == null) return null;

            return new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                TotalPoints = customer.TotalPoints,
                LifetimePoints = customer.LifetimePoints,
                CreatedTime = customer.CreatedTime,
                LastUpdatedTime = customer.LastUpdatedTime
            };
        }


        public async Task EnsureCustomerReadyForCheckoutAsync(Guid tableId)
        {
            var session = await _unitOfWork.Repository<TableSession, Guid>()
                .GetWithSpecAsync(new BaseSpecification<TableSession>(s =>
                    s.TableId == tableId && s.Status == TableSessionStatus.Active));

            if (session == null)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_OPERATION,
                    "Không tìm thấy session active của bàn");

            if (!session.CustomerId.HasValue || session.CustomerId.Value == Guid.Empty)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.VALIDATION_ERROR,
                    "Vui lòng nhập thông tin khách (tên + SĐT) trước khi xuất hóa đơn");

            var customer = await _unitOfWork.Repository<Customer, Guid>().GetByIdAsync(session.CustomerId.Value);

            if (customer == null || string.IsNullOrWhiteSpace(customer.Name) || string.IsNullOrWhiteSpace(customer.PhoneNumber))
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.VALIDATION_ERROR,
                    "Thông tin khách hàng chưa hợp lệ. Vui lòng cập nhật Tên và Số điện thoại trước khi xuất hóa đơn");
        }
    }
}

