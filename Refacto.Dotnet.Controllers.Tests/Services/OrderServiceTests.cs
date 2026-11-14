using Microsoft.EntityFrameworkCore;
using Moq;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Exceptions;
using Refacto.DotNet.Controllers.Interfaces;
using Refacto.DotNet.Controllers.Services;

namespace Refacto.DotNet.Controllers.Tests.Services
{
    public class OrderServiceTests
    {
        [Fact]
        public void ProcessOrder_ShouldPersistProductRuleChanges()
        {
            using AppDbContext appDbContext = CreateDbContext();
            Product normalProduct = new()
            {
                Name = "Normal product",
                Type = "NORMAL",
                Available = 2
            };
            Product seasonalProduct = new()
            {
                Name = "Seasonal product",
                Type = "SEASONAL",
                Available = 3,
                SeasonStartDate = DateTime.Now.AddDays(-5),
                SeasonEndDate = DateTime.Now.AddDays(10)
            };
            Order order = new()
            {
                Items = new HashSet<Product> { normalProduct, seasonalProduct }
            };
            _ = appDbContext.Orders.Add(order);
            _ = appDbContext.SaveChanges();
            long orderId = order.Id;

            OrderService orderService = new(
                appDbContext,
                new ProductService(Mock.Of<INotificationService>()));

            Order processedOrder = orderService.ProcessOrder(orderId);

            Assert.Equal(orderId, processedOrder.Id);
            Assert.Equal(1, normalProduct.Available);
            Assert.Equal(2, seasonalProduct.Available);
            appDbContext.ChangeTracker.Clear();
            Assert.Equal(1, appDbContext.Products.Single(product => product.Name == "Normal product").Available);
            Assert.Equal(2, appDbContext.Products.Single(product => product.Name == "Seasonal product").Available);
        }

        [Fact]
        public void ProcessOrder_WhenOrderDoesNotExist_ShouldThrowOrderNotFoundException()
        {
            using AppDbContext appDbContext = CreateDbContext();
            OrderService orderService = new(
                appDbContext,
                new ProductService(Mock.Of<INotificationService>()));

            Assert.Throws<OrderNotFoundException>(() => orderService.ProcessOrder(1234));
        }

        private static AppDbContext CreateDbContext()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
    }
}
