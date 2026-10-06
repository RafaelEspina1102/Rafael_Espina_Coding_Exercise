using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PizzaStoreTest;

public class PizzaControllerTests : ApiTest
{
    [Fact]
    public async Task CreatePizza_ReturnsCreatedWithExistingToppings()
    {
        var cheese = await CreateTopping("Cheese");
        var ham = await CreateTopping("Ham");

        using var response = await Client.PostAsJsonAsync("/api/pizzas",
            new { name = " Hawaiian ", toppingIds = new[] { cheese.Id, ham.Id } });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var pizza = Assert.IsType<PizzaResponse>(await response.Content.ReadFromJsonAsync<PizzaResponse>());
        Assert.True(pizza.Id > 0);
        Assert.Equal("Hawaiian", pizza.Name);
        Assert.Equal(new[] { cheese, ham }, pizza.Toppings);
        Assert.EndsWith($"/api/pizzas/{pizza.Id}", response.Headers.Location?.ToString());

        var stored = Assert.IsType<PizzaResponse>(
            await Client.GetFromJsonAsync<PizzaResponse>($"/api/pizzas/{pizza.Id}"));
        Assert.Equal(pizza.Name, stored.Name);
        Assert.Equal(pizza.Toppings, stored.Toppings);
        var pizzas = Assert.IsType<List<PizzaResponse>>(
            await Client.GetFromJsonAsync<List<PizzaResponse>>("/api/pizzas"));
        Assert.Equal(pizza.Toppings, Assert.Single(pizzas).Toppings);
    }

    [Fact]
    public async Task CreatePizza_RejectsDuplicateIgnoringCaseAndSpaces()
    {
        var original = await CreatePizza("Hawaiian");

        using var response = await Client.PostAsJsonAsync("/api/pizzas",
            new { name = " hAWAIIAN ", toppingIds = Array.Empty<int>() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = Assert.IsType<ErrorResponse>(await response.Content.ReadFromJsonAsync<ErrorResponse>());
        Assert.Equal("A pizza with this name already exists.", error.Message);
        var pizzas = Assert.IsType<List<PizzaResponse>>(
            await Client.GetFromJsonAsync<List<PizzaResponse>>("/api/pizzas"));
        Assert.Equal(original.Id, Assert.Single(pizzas).Id);
    }

    [Theory]
    [InlineData(999999)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task InvalidToppingId_RejectsCreationAndUpdateWithoutChangingData(int invalidId)
    {
        var cheese = await CreateTopping("Cheese");
        var pizza = await CreatePizza("Hawaiian", cheese.Id);
        var payload = new { name = "Invalid Pizza", toppingIds = new[] { cheese.Id, invalidId } };

        using var createResponse = await Client.PostAsJsonAsync("/api/pizzas", payload);
        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
        var error = Assert.IsType<InvalidToppingsResponse>(
            await createResponse.Content.ReadFromJsonAsync<InvalidToppingsResponse>());
        Assert.Equal("Some topping IDs do not exist.", error.Message);
        Assert.Equal(new[] { invalidId }, error.ToppingIds);

        using var updateResponse = await Client.PutAsJsonAsync($"/api/pizzas/{pizza.Id}/toppings",
            new { toppingIds = new[] { invalidId } });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var stored = Assert.IsType<PizzaResponse>(
            await Client.GetFromJsonAsync<PizzaResponse>($"/api/pizzas/{pizza.Id}"));
        Assert.Equal(new[] { cheese }, stored.Toppings);
        var pizzas = Assert.IsType<List<PizzaResponse>>(
            await Client.GetFromJsonAsync<List<PizzaResponse>>("/api/pizzas"));
        Assert.Equal(pizza.Id, Assert.Single(pizzas).Id);
    }

    [Fact]
    public async Task UpdatePizzaToppings_ReplacesSelectionAndAllowsClearing()
    {
        var cheese = await CreateTopping("Cheese");
        var ham = await CreateTopping("Ham");
        var pizza = await CreatePizza("Hawaiian", cheese.Id);

        using var response = await Client.PutAsJsonAsync($"/api/pizzas/{pizza.Id}/toppings",
            new { toppingIds = new[] { ham.Id } });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = Assert.IsType<PizzaResponse>(await response.Content.ReadFromJsonAsync<PizzaResponse>());
        Assert.Equal(pizza.Name, updated.Name);
        Assert.Equal(new[] { ham }, updated.Toppings);
        var stored = Assert.IsType<PizzaResponse>(
            await Client.GetFromJsonAsync<PizzaResponse>($"/api/pizzas/{pizza.Id}"));
        Assert.Equal(new[] { ham }, stored.Toppings);

        using var clearResponse = await Client.PutAsJsonAsync($"/api/pizzas/{pizza.Id}/toppings",
            new { toppingIds = Array.Empty<int>() });
        Assert.Equal(HttpStatusCode.OK, clearResponse.StatusCode);
        var cleared = Assert.IsType<PizzaResponse>(
            await Client.GetFromJsonAsync<PizzaResponse>($"/api/pizzas/{pizza.Id}"));
        Assert.Empty(cleared.Toppings);
        Assert.Equal(pizza.Name, cleared.Name);
    }

    [Fact]
    public async Task DeletePizza_RemovesPizzaAndPreservesSharedToppings()
    {
        var cheese = await CreateTopping("Cheese");
        var pizza = await CreatePizza("Hawaiian", cheese.Id);
        var otherPizza = await CreatePizza("Cheese Pizza", cheese.Id);

        using var response = await Client.DeleteAsync($"/api/pizzas/{pizza.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        using var getResponse = await Client.GetAsync($"/api/pizzas/{pizza.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(cheese, await Client.GetFromJsonAsync<ToppingResponse>($"/api/toppings/{cheese.Id}"));
        var remaining = Assert.IsType<PizzaResponse>(
            await Client.GetFromJsonAsync<PizzaResponse>($"/api/pizzas/{otherPizza.Id}"));
        Assert.Equal(new[] { cheese }, remaining.Toppings);
        var pizzas = Assert.IsType<List<PizzaResponse>>(
            await Client.GetFromJsonAsync<List<PizzaResponse>>("/api/pizzas"));
        Assert.Equal(otherPizza.Id, Assert.Single(pizzas).Id);
    }
}
