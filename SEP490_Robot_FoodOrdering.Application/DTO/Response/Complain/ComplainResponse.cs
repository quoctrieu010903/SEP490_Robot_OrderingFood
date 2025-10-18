﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain
{
    public class ComplainResponse
    {
       public Guid ComplainId { get; set; }
        public Guid IdTable { get; set; }
        public string FeedBack { get; set; }
        public bool IsPending { get; set; }
        public DateTime CreateData { get; set; }
        public List<OrderItemDTO> Dtos { get; set; }
    }
}
