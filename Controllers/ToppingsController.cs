using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaStoreApi.Database;
using PizzaStoreApi.Entities;
using PizzaStoreApi.Requests.Toppings;

namespace PizzaStoreApi.Controllers;

[ApiController]
[Route("api/toppings")]
public class ToppingsController : ControllerBase
{
    private readonly PizzaStoreDbContext _context;

    public ToppingsController(PizzaStoreDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetToppings()
    {
        var toppings = await _context.Toppings
            .OrderBy(topping => topping.Id)
            .Select(topping => new { topping.Id, topping.Name })
            .ToListAsync();

        return Ok(toppings);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTopping(int id)
    {
        var topping = await _context.Toppings.FindAsync(id);

        if (topping == null)
        {
            return NotFound(new { message = "Topping not found." });
        }

        return Ok(new { topping.Id, topping.Name });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTopping(CreateToppings request)
    {
        var name = request.Name.Trim();
        var normalizedName = name.ToUpperInvariant();

        var duplicateExists = await _context.Toppings
            .AnyAsync(topping => topping.Name.ToUpperInvariant() == normalizedName);

        if (duplicateExists)
        {
            return Conflict(new { message = "A topping with this name already exists." });
        }

        var topping = new Topping { Name = name };

        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTopping), new { id = topping.Id },
            new { topping.Id, topping.Name });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTopping(int id, UpdateToppings request)
    {
        var topping = await _context.Toppings.FindAsync(id);

        if (topping == null)
        {
            return NotFound(new { message = "Topping not found." });
        }

        var name = request.Name.Trim();
        var normalizedName = name.ToUpperInvariant();

        var duplicateExists = await _context.Toppings
            .AnyAsync(other => other.Id != id && other.Name.ToUpperInvariant() == normalizedName);

        if (duplicateExists)
        {
            return Conflict(new { message = "A topping with this name already exists." });
        }

        topping.Name = name;
        await _context.SaveChangesAsync();

        return Ok(new { topping.Id, topping.Name });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTopping(int id)
    {
        var topping = await _context.Toppings
            .Include(topping => topping.Pizzas)
            .FirstOrDefaultAsync(topping => topping.Id == id);

        if (topping == null)
        {
            return NotFound(new { message = "Topping not found." });
        }

        // Remove the links to pizzas while keeping the pizzas themselves.
        topping.Pizzas.Clear();
        _context.Toppings.Remove(topping);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
