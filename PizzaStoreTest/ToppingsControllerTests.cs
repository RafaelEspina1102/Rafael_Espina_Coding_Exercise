using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PizzaStoreTest;

public class ToppingsControllerTests : ApiTest
{
    [Fact]
    public async Task CreateTopping_ReturnsCreatedAndStoresTrimmedName()
    {
        using var response = await Client.PostAsJsonAsync("/api/toppings", new { name = " Cheese " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var topping = Assert.IsType<ToppingResponse>(await response.Content.ReadFromJsonAsync<ToppingResponse>());
        Assert.True(topping.Id > 0);
        Assert.Equal("Cheese", topping.Name);
        Assert.EndsWith($"/api/toppings/{topping.Id}", response.Headers.Location?.ToString());

        using var getResponse = await Client.GetAsync($"/api/toppings/{topping.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(topping, await getResponse.Content.ReadFromJsonAsync<ToppingResponse>());
    }

    [Fact]
    public async Task CreateTopping_RejectsDuplicateIgnoringCaseAndSpaces()
    {
        var original = await CreateTopping("Cheese");

        using var response = await Client.PostAsJsonAsync("/api/toppings", new { name = " cHEESE " });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = Assert.IsType<ErrorResponse>(await response.Content.ReadFromJsonAsync<ErrorResponse>());
        Assert.Equal("A topping with this name already exists.", error.Message);

        var toppings = await Client.GetFromJsonAsync<List<ToppingResponse>>("/api/toppings");
        Assert.Equal(original, Assert.Single(Assert.IsType<List<ToppingResponse>>(toppings)));
    }

    [Fact]
    public async Task UpdateTopping_StoresNewName()
    {
        var topping = await CreateTopping("Cheese");

        using var response = await Client.PutAsJsonAsync($"/api/toppings/{topping.Id}",
            new { name = " Mozzarella " });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expected = new ToppingResponse(topping.Id, "Mozzarella");
        Assert.Equal(expected, await response.Content.ReadFromJsonAsync<ToppingResponse>());
        Assert.Equal(expected, await Client.GetFromJsonAsync<ToppingResponse>($"/api/toppings/{topping.Id}"));
    }

    [Fact]
    public async Task DeleteTopping_RemovesRecord()
    {
        var topping = await CreateTopping("Cheese");

        using var response = await Client.DeleteAsync($"/api/toppings/{topping.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        using var getResponse = await Client.GetAsync($"/api/toppings/{topping.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Empty(Assert.IsType<List<ToppingResponse>>(
            await Client.GetFromJsonAsync<List<ToppingResponse>>("/api/toppings")));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task MissingTopping_ReturnsNotFound(string method)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), "/api/toppings/999999");
        if (method == "PUT")
        {
            request.Content = JsonContent.Create(new { name = "Cheese" });
        }

        using var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = Assert.IsType<ErrorResponse>(await response.Content.ReadFromJsonAsync<ErrorResponse>());
        Assert.Equal("Topping not found.", error.Message);
    }
}
