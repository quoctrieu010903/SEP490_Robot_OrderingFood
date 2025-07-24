namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    public class CreateProductCategoryRequest
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
    }
}