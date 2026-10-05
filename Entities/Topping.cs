namespace PizzaStoreApi.Entities;

public class Topping
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Pizza> Pizzas { get; set; } = new List<Pizza>();
}
