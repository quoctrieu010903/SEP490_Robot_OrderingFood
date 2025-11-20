using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;

public record ComplainPeedingInfo(
    Guid Id,
    string? SessionId,
    string TableName,
    TableEnums tableStatus,
    PaymentStatusEnums paymentStatus,
    int Counter,
    int DeliveredCount,
    int ServeredCount,
    int PaidCount,
    int TotalItems,
    DateTime? LastOrderUpdatedTime,
    int PendingItems,
    bool IsWaitingDish,
    int? WaitingDurationInMinutes

    );