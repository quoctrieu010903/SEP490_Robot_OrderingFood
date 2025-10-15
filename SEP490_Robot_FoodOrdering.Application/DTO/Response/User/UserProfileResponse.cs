using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.User
{
    public class UserProfileResponse
    {
        public string EmploymentCode { get; set; }
        public string FullName { get; set; }    
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Avatar { get; set; }
        public string RoleName { get; set; }
    }
}
