using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.TableSession
{
    public class TableSessionResponse
    {
        public Guid Id { get; set; }
        public string TableName { get; set; }
        public string Status { get; set; }   
        public string SessionToken { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? CustomerName { get; set; }   
        public string? PhoneNumber { get; set; }    


    }
}
