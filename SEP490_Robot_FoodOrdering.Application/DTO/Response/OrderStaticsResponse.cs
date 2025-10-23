

using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response
{
    public class OrderStaticsResponse
    {
        public int DeliveredCount { get; set; }
        public int TotalOrderItems { get; set; }
        public int PaidCount { get; set; }
        public int ServedCount { get; set; }
        public PaymentStatusEnums PaymentStatus { get; set; }

        // Optional: constructor có tham số
        public OrderStaticsResponse() { }

        public OrderStaticsResponse(int delivered, int total, int paid , int servedCount)
        {
            DeliveredCount = delivered;
            TotalOrderItems = total;
            PaidCount = paid;
            ServedCount = servedCount;
        }
    }
}
