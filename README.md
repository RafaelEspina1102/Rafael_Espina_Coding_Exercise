# Pizza Store API

A backend coding exercise using ASP.NET Core 8 and Entity Framework Core.
The API lets you add, list, update, and delete pizzas and toppings.

## Requirements

- .NET 8 SDK
- Postman for manual testing

The project uses an in-memory database, so there is no database to install.
Data is cleared when you stop the app. The first build needs internet access
for the NuGet packages.

## Build and run

Run these commands from the project folder:

```bash
dotnet restore PizzaStore.sln
dotnet build PizzaStore.sln --no-restore
dotnet run --project PizzaStoreApi.csproj --urls http://localhost:5000
```

The API runs at `http://localhost:5000`. Keep the terminal open while testing.
Press Ctrl+C to stop it.

## Endpoints

| Method | URL | What it does |
| --- | --- | --- |
| GET | `/api/toppings` | List toppings |
| GET | `/api/toppings/{id}` | Get a topping |
| POST | `/api/toppings` | Add a topping |
| PUT | `/api/toppings/{id}` | Update a topping name |
| DELETE | `/api/toppings/{id}` | Delete a topping |
| GET | `/api/pizzas` | List pizzas with toppings |
| GET | `/api/pizzas/{id}` | Get a pizza with toppings |
| POST | `/api/pizzas` | Add a pizza |
| PUT | `/api/pizzas/{id}` | Update a pizza name |
| PUT | `/api/pizzas/{id}/toppings` | Replace pizza toppings |
| DELETE | `/api/pizzas/{id}` | Delete a pizza |

## Testing with Postman

Start the API, then open Postman and create an HTTP request.
Choose the method, enter the URL, and click **Send**.
For POST and PUT, select **Body → raw → JSON**. Use the bodies below.
GET and DELETE do not need a body.

### 1. Add a topping

Send `POST http://localhost:5000/api/toppings`:

```json
{
  "name": "Cheese"
}
```

Expect `201 Created`. Save the topping ID from the response.
You can add another topping the same way.

Send `GET http://localhost:5000/api/toppings` to see the list.
Try adding ` cheese ` again. It should return `409 Conflict`.

### 2. Update a topping

Send `PUT http://localhost:5000/api/toppings/1`:

```json
{
  "name": "Mozzarella"
}
```

Replace `1` with your topping ID. Expect `200 OK`.

### 3. Add a pizza

Send `POST http://localhost:5000/api/pizzas`:

```json
{
  "name": "Cheese Pizza",
  "toppingIds": [1]
}
```

Use your actual topping ID. Expect `201 Created` and save the pizza ID.
Send `GET http://localhost:5000/api/pizzas` to see pizzas with their toppings.
Adding the same pizza name again should return `409 Conflict`.

To create a pizza with more than one topping, put the IDs in the same list:

```json
{
  "name": "Meat Lovers",
  "toppingIds": [1, 2, 3]
}
```

Send this to the same POST URL. Create the toppings first and use their actual IDs.

### 4. Update a pizza

Send `PUT http://localhost:5000/api/pizzas/1`:

```json
{
  "name": "Special Cheese Pizza"
}
```

Use your pizza ID. Expect `200 OK`. This changes only the name.

To change toppings, send `PUT http://localhost:5000/api/pizzas/1/toppings`:

```json
{
  "toppingIds": []
}
```

Expect `200 OK`. An empty list removes all toppings.
To add them back, send a list of existing topping IDs, such as `[1]`.
An ID that does not exist should return `400 Bad Request`.

To update multiple toppings, send this to the same toppings PUT URL:

```json
{
  "toppingIds": [1, 2, 3]
}
```

This replaces the whole selection. Include every topping you want the pizza to
have, including any you want to keep. For example, if it already has topping `1`
and you want to add `2` and `3`, send `[1, 2, 3]`. Use your actual topping IDs
and replace the pizza ID in the URL.

### 5. Delete records

Send `DELETE http://localhost:5000/api/pizzas/1` using your pizza ID.
Expect `204 No Content`. Getting that pizza again should return `404 Not Found`.
Its topping records should still exist.

Send `DELETE http://localhost:5000/api/toppings/1` using your topping ID.
Expect `204 No Content`. If other pizzas use that topping, they stay in the
database but lose that topping.

After updates and deletions, send a GET request to check the changes.

## Automated tests

Run this from the project folder:

```bash
dotnet test PizzaStoreTest/PizzaStoreTest.csproj
```

The tests use xUnit and start the API automatically with a separate database
for each test. You do not need to run the app first.

They cover creating, updating, and deleting toppings, missing toppings,
duplicate names, creating pizzas with toppings, invalid topping IDs,
changing pizza toppings, and deleting pizzas. There are 14 test cases.

## Notes

- Names cannot be empty or only spaces.
- Duplicate names are checked without capitalization or surrounding spaces.
- Create toppings before using their IDs in a pizza.
- Updating toppings replaces the whole selection.
- The database is empty again after restarting the app.
