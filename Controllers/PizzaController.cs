using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaStoreApi.Database;
using PizzaStoreApi.Entities;
using PizzaStoreApi.Requests.Pizza;

namespace PizzaStoreApi.Controllers;

[ApiController]
[Route("api/pizzas")]
public class PizzaController : ControllerBase
{
    private readonly PizzaStoreDbContext _context;

    public PizzaController(PizzaStoreDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPizzas()
    {
        var pizzas = await _context.Pizzas
            .Include(pizza => pizza.Toppings)
            .OrderBy(pizza => pizza.Id)
            .ToListAsync();

        return Ok(pizzas.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPizza(int id)
    {
        var pizza = await _context.Pizzas
            .Include(pizza => pizza.Toppings)
            .FirstOrDefaultAsync(pizza => pizza.Id == id);

        if (pizza == null)
        {
            return NotFound(new { message = "Pizza not found." });
        }

        return Ok(ToResponse(pizza));
    }

    [HttpPost]
    public async Task<IActionResult> CreatePizza(CreatePizza request)
    {
        if (request.ToppingIds == null)
        {
            return BadRequest(new { message = "Topping IDs are required." });
        }

        var name = request.Name.Trim();
        var normalizedName = name.ToUpperInvariant();

        var duplicateExists = await _context.Pizzas
            .AnyAsync(pizza => pizza.Name.ToUpperInvariant() == normalizedName);

        if (duplicateExists)
        {
            return Conflict(new { message = "A pizza with this name already exists." });
        }

        var toppingIds = request.ToppingIds.Distinct().ToList();
        var toppings = await _context.Toppings
            .Where(topping => toppingIds.Contains(topping.Id))
            .ToListAsync();

        var missingIds = toppingIds.Except(toppings.Select(topping => topping.Id)).ToList();
        if (missingIds.Count > 0)
        {
            return BadRequest(new { message = "Some topping IDs do not exist.", toppingIds = missingIds });
        }

        var pizza = new Pizza { Name = name, Toppings = toppings };
        _context.Pizzas.Add(pizza);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPizza), new { id = pizza.Id }, ToResponse(pizza));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePizza(int id, UpdatePizza request)
    {
        var pizza = await _context.Pizzas
            .Include(pizza => pizza.Toppings)
            .FirstOrDefaultAsync(pizza => pizza.Id == id);

        if (pizza == null)
        {
            return NotFound(new { message = "Pizza not found." });
        }

        var name = request.Name.Trim();
        var normalizedName = name.ToUpperInvariant();

        var duplicateExists = await _context.Pizzas
            .AnyAsync(other => other.Id != id && other.Name.ToUpperInvariant() == normalizedName);

        if (duplicateExists)
        {
            return Conflict(new { message = "A pizza with this name already exists." });
        }

        pizza.Name = name;
        await _context.SaveChangesAsync();

        return Ok(ToResponse(pizza));
    }

    [HttpPut("{id:int}/toppings")]
    public async Task<IActionResult> UpdatePizzaToppings(int id, UpdatePizzaToppings request)
    {
        if (request.ToppingIds == null)
        {
            return BadRequest(new { message = "Topping IDs are required." });
        }

        var pizza = await _context.Pizzas
            .Include(pizza => pizza.Toppings)
            .FirstOrDefaultAsync(pizza => pizza.Id == id);

        if (pizza == null)
        {
            return NotFound(new { message = "Pizza not found." });
        }

        var toppingIds = request.ToppingIds.Distinct().ToList();
        var toppings = await _context.Toppings
            .Where(topping => toppingIds.Contains(topping.Id))
            .ToListAsync();

        var missingIds = toppingIds.Except(toppings.Select(topping => topping.Id)).ToList();
        if (missingIds.Count > 0)
        {
            return BadRequest(new { message = "Some topping IDs do not exist.", toppingIds = missingIds });
        }

        // Replace the current selection; an empty list removes all toppings.
        pizza.Toppings.Clear();
        pizza.Toppings.AddRange(toppings);
        await _context.SaveChangesAsync();

        return Ok(ToResponse(pizza));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePizza(int id)
    {
        var pizza = await _context.Pizzas
            .Include(pizza => pizza.Toppings)
            .FirstOrDefaultAsync(pizza => pizza.Id == id);

        if (pizza == null)
        {
            return NotFound(new { message = "Pizza not found." });
        }

        // Remove the links while keeping the toppings available to other pizzas.
        pizza.Toppings.Clear();
        _context.Pizzas.Remove(pizza);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static object ToResponse(Pizza pizza)
    {
        return new
        {
            pizza.Id,
            pizza.Name,
            Toppings = pizza.Toppings.OrderBy(topping => topping.Id)
                .Select(topping => new { topping.Id, topping.Name }).ToList()
        };
    }
}
