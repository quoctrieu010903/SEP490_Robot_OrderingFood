using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SEP490_Robot_FoodOrdering.API.Converters;

namespace SEP490_Robot_FoodOrdering.API.Extentions
{

    public class JsonOptionsConfigurator : IConfigureOptions<JsonOptions>
    {
        public void Configure(JsonOptions options)
        {
            options.JsonSerializerOptions.Converters.Add(new VietnamDateTimeConverter());
        }
    }

}
