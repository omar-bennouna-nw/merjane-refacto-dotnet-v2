using Microsoft.EntityFrameworkCore;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Filters;
using Refacto.DotNet.Controllers.Interfaces;
using Refacto.DotNet.Controllers.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<DomainExceptionFilter>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    _ = options.UseInMemoryDatabase($"InMemoryDb");
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }