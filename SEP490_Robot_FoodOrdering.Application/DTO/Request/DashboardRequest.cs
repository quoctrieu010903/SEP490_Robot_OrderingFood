namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    /// <summary>
    /// Request model for dashboard statistics filtering
    /// </summary>
    public class DashboardRequest
    {
        /// <summary>
        /// Year for filtering (default: current year)
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Month for filtering (1-12, default: current month)
        /// </summary>
        public int? Month { get; set; }

        /// <summary>
        /// Day for filtering (1-31, optional)
        /// </summary>
        public int? Day { get; set; }
    }
}
