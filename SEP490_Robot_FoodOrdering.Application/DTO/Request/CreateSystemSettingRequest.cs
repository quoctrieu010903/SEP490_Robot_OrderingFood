using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class CreateSystemSettingRequest
    {
       
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

    }
}
