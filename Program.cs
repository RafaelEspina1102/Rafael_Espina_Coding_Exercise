using Microsoft.EntityFrameworkCore;
using PizzaStoreApi.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<PizzaStoreDbContext>(options =>
    options.UseInMemoryDatabase("PizzaStore"));

var app = builder.Build();

app.MapControllers();

app.Run();
