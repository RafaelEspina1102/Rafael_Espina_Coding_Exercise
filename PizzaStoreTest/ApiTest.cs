using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PizzaStoreTest;

public abstract class ApiTest : IDisposable
{
    private readonly PizzaStoreFactory _factory = new PizzaStoreFactory();
    protected readonly HttpClient Client;

    protected ApiTest()
    {
        // xUnit creates a new test class instance for each test, giving each a fresh database.
        Client = _factory.CreateClient();
    }

    protected async Task<ToppingResponse> CreateTopping(string name)
    {
        using var response = await Client.PostAsJsonAsync("/api/toppings", new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return Assert.IsType<ToppingResponse>(await response.Content.ReadFromJsonAsync<ToppingResponse>());
    }

    protected async Task<PizzaResponse> CreatePizza(string name, params int[] toppingIds)
    {
        using var response = await Client.PostAsJsonAsync("/api/pizzas", new { name, toppingIds });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return Assert.IsType<PizzaResponse>(await response.Content.ReadFromJsonAsync<PizzaResponse>());
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
    }
}

public record ToppingResponse(int Id, string Name);
public record PizzaResponse(int Id, string Name, List<ToppingResponse> Toppings);
public record ErrorResponse(string Message);
public record InvalidToppingsResponse(string Message, List<int> ToppingIds);
