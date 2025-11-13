using System;
using System.ComponentModel.DataAnnotations;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    /// <summary>
    /// Request model for moving orders from one table to another
    /// </summary>
    public class MoveTableRequest
    {
        /// <summary>
        /// The ID of the new table to move orders to
        /// </summary>
        [Required(ErrorMessage = "NewTableId is required")]
        public Guid NewTableId { get; set; }

        /// <summary>
        /// Reason for moving the table (required for audit purposes)
        /// </summary>
        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Reason must be between 5 and 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}

