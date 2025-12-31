using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Table
{
    public class TableRedirectResponse : TableResponse
    {
        public Guid RedirectTableId { get; set; }
        public string RedirectUrl { get; set; }
    }
}
