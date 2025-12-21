namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Dashboard
{
    /// <summary>
    /// Response model for dashboard statistics
    /// </summary>
        public class DashboardResponse
        {
            /// <summary>
            /// Total number of user accounts
            /// </summary>
            public int TotalUsers { get; set; }

            /// <summary>
            /// Total number of products in the system
            /// </summary>
            public int TotalProducts { get; set; }

            /// <summary>
            /// Most ordered product with its count
            /// </summary>
            public ProductOrderStat? MostOrderedProduct { get; set; }

            /// <summary>
            /// Least ordered product with its count
            /// </summary>
            public ProductOrderStat? LeastOrderedProduct { get; set; }

            /// <summary>
            /// Total number of cancelled order items
            /// </summary>
            public int TotalCancelledItems { get; set; }

            /// <summary>
            /// Total number of complaints in the selected period
            /// </summary>
            public int TotalComplains { get; set; }

            /// <summary>
            /// Total number of pending complaints in the selected period
            /// </summary>
            public int TotalComplainsPending { get; set; }

            /// <summary>
            /// Total number of resolved/handled complaints in the selected period
            /// </summary>
            public int TotalComplainsHandled { get; set; }

            /// <summary>
            /// Total number of remade order items in the selected period
            /// </summary>
            public int TotalRemakeItems { get; set; }

            /// <summary>
            /// Total number of order items in the selected period
            /// </summary>
            public int TotalOrderItems { get; set; }

            /// <summary>
            /// Top 5 most ordered products with their counts
            /// </summary>
            public List<ProductOrderStat> Top5MostOrderedProducts { get; set; } = new List<ProductOrderStat>();
        }

        /// <summary>
        /// Product order statistics
        /// </summary>
        public class ProductOrderStat
        {
            /// <summary>
            /// Product ID
            /// </summary>
            public Guid ProductId { get; set; }

            /// <summary>
            /// Product name
            /// </summary>
            public string ProductName { get; set; } = string.Empty;

            /// <summary>
            /// Number of times this product was ordered (OrderItem count)
            /// </summary>
            public int OrderCount { get; set; }
        }
}
