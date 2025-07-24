using AutoMapper;
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Application.Mapping;
using SEP490_Robot_FoodOrdering.Application.Service.Implementation;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;


    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseResponseModel<InforBill>> CreateBill(Guid idOrder)
    {
        try
        {
            var temp = await _unitOfWork.Repository<Order, Guid>().GetByIdAsync(idOrder);

            decimal sum = 0;
            foreach (var tempOrderItem in temp.OrderItems)
            {
                sum += tempOrderItem.ProductSize.Price;
                sum += tempOrderItem.OrderItemTopping.Sum(topping => topping.Price);
            }

            var res = MapOrder.MapBill(temp, sum);

            updateStatus(temp, PaymentStatusEnums.Pending);

            return new BaseResponseModel<InforBill>(StatusCodes.Status200OK, ResponseCodeConstants.SUCCESS, res);
        }
        catch (Exception ex)
        {
            throw new ErrorException(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    protected async void updateStatus(Order order, PaymentStatusEnums status)
    {
        order.Payment.PaymentStatus = status;
        _unitOfWork.Repository<Order, Guid>().Update(order);
        await _unitOfWork.SaveChangesAsync();
    }
}