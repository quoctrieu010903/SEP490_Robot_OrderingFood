using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;

public record ComplainPeedingInfo(string TableName ,TableEnums tableStatus , int Counter  , int DeliveredCount, int ServeredCount,  
    int PaidCount,
    int TotalItems);