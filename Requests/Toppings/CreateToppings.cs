using System.ComponentModel.DataAnnotations;

namespace PizzaStoreApi.Requests.Toppings;

public class CreateToppings
{
    [Required(ErrorMessage = "Topping name is required.")]
    public string Name { get; set; } = string.Empty;
}
