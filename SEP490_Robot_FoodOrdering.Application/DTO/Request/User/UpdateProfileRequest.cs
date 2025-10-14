using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.User
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public IFormFile? Avatar { get; set; }

    }
}
