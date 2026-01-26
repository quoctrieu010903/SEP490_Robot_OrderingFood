namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class AttachDeviceRequest
    {
        public string NewDeviceId { get; set; } = null!;
        public string? Reason { get; set; }
    }
}
