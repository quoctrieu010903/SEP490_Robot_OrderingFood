using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Core.Enums; // Added this line to include the correct namespace for RoleNameEnums

namespace SEP490_Robot_FoodOrdering.Core.Ultils
{
    public class GenerateEmploymentCodeFromGuid
    {
        /// <summary>
        /// T?o EmploymentCode d?a trên RoleEnum
        /// </summary>
        /// <param name="role">RoleEnum</param>
        /// <param name="length">S? ký t? random t? GUID, m?c ??nh 4</param>
        /// <returns>Ví d?: AD-1A3F</returns>
        public static string GenerateEmploymentCode(RoleNameEnums role, int length = 4)
        {
            // Gi?i h?n length t? 3-5 ký t?
            if (length < 3) length = 3;
            if (length > 5) length = 5;

            // L?y prefix theo role enum
            string prefix = role switch
            {
                // Add cases for RoleNameEnums here
            };

            // T?o ph?n random t? GUID
            string guidPart = Guid.NewGuid().ToString("N").ToUpper().Substring(0, length);

            // Ghép prefix + ph?n GUID
            return $"{prefix}-{guidPart}";
        }
    }
}
