

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SEP490_Robot_FoodOrdering.Application.Abstractions.ServerEndPoint;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Options;

namespace SEP490_Robot_FoodOrdering.Infrastructure.ServerEndpoints
{
    public class ServerEndpointService : IServerEndpointService
    {
        private readonly ServerEndpointOptions _serverEndpointOption;
        public ServerEndpointService( IOptions<ServerEndpointOptions> serverEndpointOption)
        {
            
            _serverEndpointOption = serverEndpointOption.Value;
        }

        public string GetBackendUrl()
        {
            return _serverEndpointOption.BackendBaseUrl;
        }

        public string GetFrontendUrl()
        {
            return _serverEndpointOption.FrontendBaseUrl;
        }
    }
}
