using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Table
{
    public class QrShareResponse

    {
        public string QrCodeBase64 { get; set; }   // Base64 QR to show on screen
        public string ShareToken { get; set; }     // Random secure token
        public string ShareUrl { get; set; }       // Full link: server/table/{id}?token=...
        public DateTime ExpireAt { get; set; }
    }

}
