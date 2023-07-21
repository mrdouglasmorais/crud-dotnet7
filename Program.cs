using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>();
var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

if (app.Environment.IsStaging())
  app.MapGet("/", () => "Staging enviroment!");

if (app.Environment.IsDevelopment())
  app.MapGet("/", () => "Development enviroment!");

if (app.Environment.IsProduction())
  app.MapGet("/", () => "Production enviroment!");

app.MapPost("/user", () => new { name = "Douglas Morais", age = 35});
app.MapGet("/header", (HttpResponse response) => {
  response.Headers.Add("test", "Douglas Morais");
  return new {
    name = "Douglas Morais",
    age = 35,
  };
});

// usando post e retornando o dados
app.MapPost("/product", (Product product) => {
  ProductRepository.Add(product);
  var resultData = Results.Created($"/product/{product.Code}", product.Code);
  return resultData;
});

// usaando query params metodo get
app.MapGet("/products", ([FromQuery]string dateStart, [FromQuery]string dateEnd) => {
  return dateStart + " - " + dateEnd; 
});

// usando o parametro da rota
app.MapGet("/product/{code}", ([FromRoute]string code) => {
  var product = ProductRepository.GetBy(code);
  if (product != null)
    return Results.Ok(product);
  return Results.NotFound();
});

// usando o parametro pelo header
app.MapGet("/product-code", (HttpRequest request) => {
  return request.Headers["product-code"].ToString();
});

app.MapPut("/product", (Product product) => {
  var productSaved = ProductRepository.GetBy(product.Code);
  productSaved.Name = product.Name;
  return Results.Ok();
  
});

// rota para deletar o produto
app.MapDelete("/product/{code}", ([FromRoute] string code) => {
  var productSaved = ProductRepository.GetBy(code);
  ProductRepository.Remove(productSaved);
  return Results.Ok();
});

app.MapGet("/configuration/database", (IConfiguration configuration) => {
  return Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}");
});

app.Run();

// Repositório de produtos armazenamento em memória local
public static class ProductRepository {
  public static List<Product> Products {get; set;} = Products = new List<Product>();

  public static void Init(IConfiguration configuration) {
    var products = configuration.GetSection("Products").Get<List<Product>>();
    Products = products;
  }
  public static void Add(Product product) {
    Products.Add(product);
  }

  public static Product GetBy(string code) {
    return Products.FirstOrDefault( p => p.Code == code);
  }

  public static void Remove(Product product) {
    Products.Remove(product);
  }
};

public class Product {
  public int Id { get; set; }
  public string Code { get; set; }
  public string Name { get; set; }
};

public class ApplicationDbContext : DbContext {
  
  public required DbSet<Product> Products { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder options)
   => options.UseSqlServer("Server=localhost;Database=Products;User Id=sa;Password=@SQL2023douglas;MultipleActiveResultSets=true;Encrypt=YES;TrustServerCertificate=YES");
}