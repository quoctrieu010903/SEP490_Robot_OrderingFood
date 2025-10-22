using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.ServerEndPoint
{
    public interface IServerEndpointService
    {
        String GetBackendUrl();
        String GetFrontendUrl();
    }
}
