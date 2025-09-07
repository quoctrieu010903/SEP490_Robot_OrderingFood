namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Options
{
    public class VNPayOptions
    {
        public string PayCommand { get; set; } = "pay";
        public string BookingPackageType { get; set; } = "billpayment";
        public string Url { get; set; } = string.Empty;
        public string TmnCode { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string Locale { get; set; } = "vn";
        // Frontend page to redirect user after VNPay returns (e.g., waiter page)
        public string FrontendReturnUrl { get; set; } = string.Empty;
    }
}
