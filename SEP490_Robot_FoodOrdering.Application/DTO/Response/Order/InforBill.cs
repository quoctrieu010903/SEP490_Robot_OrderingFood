namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;

public record InforBill(Guid idOrder, decimal totalPrice, List<InforProdcution> InforProdcutions);

public record InforProdcution(Guid idProduction, List<string> toppings, decimal price);