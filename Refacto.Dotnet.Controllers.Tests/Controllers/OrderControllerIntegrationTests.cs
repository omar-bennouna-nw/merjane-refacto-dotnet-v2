using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Dtos.Product;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Interfaces;

namespace Refacto.DotNet.Controllers.Tests.Controllers
{
    [Collection("Sequential")]
    public class OrderControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly AppDbContext _context;
        private readonly Mock<INotificationService> _mockNotificationService;

        public OrderControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _mockNotificationService = new Mock<INotificationService>();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                _ = builder.ConfigureServices(services =>
                {
                    _ = services.AddSingleton(_mockNotificationService.Object);

                    ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                    {
                        _ = services.Remove(descriptor);
                    }

                    // Add ApplicationDbContext using an in-memory database for testing
                    _ = services.AddDbContext<AppDbContext>(options =>
                    {
                        _ = options.UseInMemoryDatabase($"InMemoryDbForTesting-{GetType()}");
                    });
                });
            });

            IServiceScope scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task ProcessOrder_ShouldReturnOrderIdInResponseBody()
        {
            HttpClient client = _factory.CreateClient();
            List<Product> products = CreateProducts();
            Order order = CreateOrder(new HashSet<Product>(products));
            await _context.Products.AddRangeAsync(products);
            _ = await _context.Orders.AddAsync(order);
            _ = await _context.SaveChangesAsync();

            HttpResponseMessage response = await client.PostAsync($"/orders/{order.Id}/processOrder", null);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            ProcessOrderResponse? responseBody = await response.Content.ReadFromJsonAsync<ProcessOrderResponse>();
            Assert.NotNull(responseBody);
            Assert.Equal(order.Id, responseBody.Id);
        }

        [Fact]
        public async Task ProcessOrder_WhenOrderDoesNotExist_ShouldReturnNotFound()
        {
            HttpClient client = _factory.CreateClient();

            HttpResponseMessage response = await client.PostAsync("/orders/1234/processOrder", null);

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
            Assert.Equal((int)System.Net.HttpStatusCode.NotFound, problem.Status);
        }

        [Fact]
        public async Task ProcessOrder_WhenProductTypeIsUnknown_ShouldReturnUnprocessableEntity()
        {
            HttpClient client = _factory.CreateClient();
            Product product = new()
            {
                Name = "Unknown product",
                Type = "UNKNOWN",
                Available = 1
            };
            Order order = CreateOrder(new HashSet<Product> { product });
            _ = await _context.Products.AddAsync(product);
            _ = await _context.Orders.AddAsync(order);
            _ = await _context.SaveChangesAsync();

            HttpResponseMessage response = await client.PostAsync($"/orders/{order.Id}/processOrder", null);

            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, response.StatusCode);
            ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
            Assert.Equal((int)System.Net.HttpStatusCode.UnprocessableEntity, problem.Status);
            Assert.Contains("UNKNOWN", problem.Detail);
        }

        private static Order CreateOrder(HashSet<Product> products)
        {
            return new Order { Items = products };
        }

        private static List<Product> CreateProducts()
        {
            return new List<Product>
            {
                new Product { LeadTime = 15, Available = 30, Type = "NORMAL", Name = "USB Cable" },
                new Product { LeadTime = 10, Available = 0, Type = "NORMAL", Name = "USB Dongle" },
                new Product { LeadTime = 15, Available = 30, Type = "EXPIRABLE", Name = "Butter", ExpiryDate = DateTime.Now.AddDays(26) },
                new Product { LeadTime = 90, Available = 6, Type = "EXPIRABLE", Name = "Milk", ExpiryDate = DateTime.Now.AddDays(-2) },
                new Product { LeadTime = 15, Available = 30, Type = "SEASONAL", Name = "Watermelon", SeasonStartDate = DateTime.Now.AddDays(-2), SeasonEndDate = DateTime.Now.AddDays(58) },
                new Product { LeadTime = 15, Available = 30, Type = "SEASONAL", Name = "Grapes", SeasonStartDate = DateTime.Now.AddDays(180), SeasonEndDate = DateTime.Now.AddDays(240) }
            };
        }
    }
}
