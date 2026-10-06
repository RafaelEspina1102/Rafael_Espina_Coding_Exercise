# API integration tests

These xUnit tests send HTTP requests through ASP.NET Core's in-memory test server.
You do not need to start the API separately. Each test uses a fresh in-memory database.

Run from the repository root:

```bash
dotnet test PizzaStoreTest/PizzaStoreTest.csproj
```

The first run downloads the test packages from NuGet.

The tests cover topping creation, duplicate rejection, updates, deletion and missing
records, plus pizza creation with toppings, duplicate rejection, invalid topping
IDs, replacing or clearing toppings, and deleting pizzas without deleting shared
toppings. Responses are checked alongside data retrieved in subsequent requests.
