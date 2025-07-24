using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Application.Mapping;

public static class MapOrder
{
    public static InforBill MapBill(Order order, decimal totalPrice)
    {
        var inforProductions = order.OrderItems?
            .Select(InforProduction)
            .ToList();

        return new InforBill(
            idOrder: order.Id,
            totalPrice: totalPrice,
            InforProdcutions: inforProductions
        );
    }

    private static InforProdcution InforProduction(OrderItem item)
    {
        var nameToppings = item.OrderItemTopping
            .Select(t => t.Topping.Name)
            .ToList();

        var totalPrice = item.ProductSize.Price +
                         item.OrderItemTopping.Sum(t => t.Price);

        return new InforProdcution(
            idProduction: item.ProductId,
            toppings: nameToppings,
            price: totalPrice
        );
    }
}