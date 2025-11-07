using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.SystemSettings
{
    public class SystemSettingResponse
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
       
    }
}
