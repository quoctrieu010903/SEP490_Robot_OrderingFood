

namespace SEP490_Robot_FoodOrdering.Domain.Specifications.Params
{
    public class CancelledItemFilterRequestParam
    {
            public Guid? OrderItemId { get; set; }

            public Guid? CancelledByUserId { get; set; }

            // Filter by date range
            public DateTime? From { get; set; }

            public DateTime? To { get; set; }

            // Optional search keyword (e.g., reason, note)
            public string? Search { get; set; }

            // Optional sorting: e.g. "CreatedAt" or "-CreatedAt" for descending
            public string? Sort { get; set; }

            // Optional paging, to be used in specification
            public int PageIndex { get; set; } = 1;

            public int PageSize { get; set; } = 10;
        }
    }

