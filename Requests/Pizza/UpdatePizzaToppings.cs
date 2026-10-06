using System.ComponentModel.DataAnnotations;

namespace PizzaStoreApi.Requests.Pizza;

public class UpdatePizzaToppings
{
    [Required(ErrorMessage = "Topping IDs are required. Use an empty list for no toppings.")]
    public List<int>? ToppingIds { get; set; }
}
