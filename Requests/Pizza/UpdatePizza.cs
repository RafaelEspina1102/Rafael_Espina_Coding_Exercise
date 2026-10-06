using System.ComponentModel.DataAnnotations;

namespace PizzaStoreApi.Requests.Pizza;

public class UpdatePizza
{
    [Required(ErrorMessage = "Pizza name is required.")]
    public string Name { get; set; } = string.Empty;
}
