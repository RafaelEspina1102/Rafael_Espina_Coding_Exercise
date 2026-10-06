using System.ComponentModel.DataAnnotations;

namespace PizzaStoreApi.Requests.Pizza;

public class CreatePizza
{
    [Required(ErrorMessage = "Pizza name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Topping IDs are required. Use an empty list for no toppings.")]
    public List<int>? ToppingIds { get; set; }
}
