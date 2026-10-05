namespace PizzaStoreApi.Entities;

public class Pizza
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Topping> Toppings { get; set; } = new List<Topping>();
}
