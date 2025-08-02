

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response
{
    public class OrderStaticsResponse
    {
        public int DeliveredCount { get; set; }
        public int TotalOrderItems { get; set; }
        public int PaidCount { get; set; }

        // Optional: constructor có tham số
        public OrderStaticsResponse() { }

        public OrderStaticsResponse(int delivered, int total, int paid)
        {
            DeliveredCount = delivered;
            TotalOrderItems = total;
            PaidCount = paid;
        }
    }
}
