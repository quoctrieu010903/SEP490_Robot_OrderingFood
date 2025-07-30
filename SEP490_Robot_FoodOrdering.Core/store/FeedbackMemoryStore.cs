namespace SEP490_Robot_FoodOrdering.Infrastructure.Repository;

public class FeedbackMemoryStore
{
    public Dictionary<string, List<object>> Store { get; set; } = new();
}