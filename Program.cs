using InMemoryCaching.Data;
using InMemoryCaching.Models;
using InMemoryCaching.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//register memory cache
builder.Services.AddMemoryCache();

//register Product services
builder.Services.AddScoped<IProductService, ProductService>();

// registiration for ApplicationDbContext as Scopped 
builder.Services.AddDbContext<ApplicationDbContext>(cfg => cfg.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/products", async (IProductService service) =>
{
    var products = await service.GetAll();
    return Results.Ok(products);
});

app.MapGet("/products/{id:guid}", async (Guid id, IProductService service) =>
{
    var product = await service.GetById(id);
    return Results.Ok(product);
});



app.MapPost("/products", async (ProductDTO request, IProductService service) =>
{
    await service.Add(request);
	return Results.Ok();
});

app.Run();
