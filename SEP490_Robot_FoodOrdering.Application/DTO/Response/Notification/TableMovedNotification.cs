namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Notification
{
    /// <summary>
    /// DTO for table moved notification when moderator moves a table
    /// </summary>
    public class TableMovedNotification
    {
        public Guid OldTableId { get; set; }
        public string OldTableName { get; set; } = string.Empty;
        public Guid NewTableId { get; set; }
        public string NewTableName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string MovedBy { get; set; } = string.Empty;
        public DateTime MovedAt { get; set; }
        public string NotificationType { get; set; } = "TableMoved";
        public string Message { get; set; } = string.Empty;
    }
}

