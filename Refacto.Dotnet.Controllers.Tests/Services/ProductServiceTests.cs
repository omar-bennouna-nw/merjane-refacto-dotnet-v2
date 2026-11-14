using Moq;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Interfaces;
using Refacto.DotNet.Controllers.Services;

namespace Refacto.DotNet.Controllers.Tests.Services
{
    public class ProductServiceTests
    {
        private const string ProductName = "Test product";

        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockDbContext = new Mock<AppDbContext>();
            _productService = new ProductService(_mockNotificationService.Object, _mockDbContext.Object);
        }

        [Fact]
        public void NotifyDelay_ShouldPersistLeadTimeAndDelegateNotification()
        {
            Product product = CreateProduct(available: 0);

            _productService.NotifyDelay(15, product);

            Assert.Equal(15, product.LeadTime);
            _mockDbContext.Verify(context => context.SaveChanges(), Times.Once());
            _mockNotificationService.Verify(
                service => service.SendDelayNotification(15, ProductName),
                Times.Once());
            _mockNotificationService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(20, -5, 10, false, true)]
        [InlineData(5, 10, 30, false, false)]
        [InlineData(5, -10, 30, true, false)]
        public void HandleSeasonalProduct_ShouldApplyStockAndNotificationRules(
            int leadTime,
            int seasonStartOffsetDays,
            int seasonEndOffsetDays,
            bool expectsDelayNotification,
            bool expectsZeroStock)
        {
            Product product = CreateProduct(available: 4, leadTime: leadTime);
            product.SeasonStartDate = DateTime.Now.AddDays(seasonStartOffsetDays);
            product.SeasonEndDate = DateTime.Now.AddDays(seasonEndOffsetDays);

            _productService.HandleSeasonalProduct(product);

            Assert.Equal(expectsZeroStock ? 0 : 4, product.Available);
            _mockDbContext.Verify(context => context.SaveChanges(), Times.Once());
            if (expectsDelayNotification)
            {
                _mockNotificationService.Verify(
                    service => service.SendDelayNotification(leadTime, ProductName),
                    Times.Once());
                _mockNotificationService.Verify(
                    service => service.SendOutOfStockNotification(It.IsAny<string>()),
                    Times.Never());
            }
            else
            {
                _mockNotificationService.Verify(
                    service => service.SendOutOfStockNotification(ProductName),
                    Times.Once());
                _mockNotificationService.Verify(
                    service => service.SendDelayNotification(It.IsAny<int>(), It.IsAny<string>()),
                    Times.Never());
            }
        }

        [Theory]
        [InlineData(5, 20, 4, false)]
        [InlineData(0, 20, 0, true)]
        [InlineData(5, -2, 0, true)]
        public void HandleExpiredProduct_ShouldApplyStockAndNotificationRules(
            int available,
            int expiryOffsetDays,
            int expectedAvailable,
            bool expectsExpirationNotification)
        {
            Product product = CreateProduct(available: available);
            product.ExpiryDate = DateTime.Now.AddDays(expiryOffsetDays);

            _productService.HandleExpiredProduct(product);

            Assert.Equal(expectedAvailable, product.Available);
            _mockDbContext.Verify(context => context.SaveChanges(), Times.Once());
            if (expectsExpirationNotification)
            {
                _mockNotificationService.Verify(
                    service => service.SendExpirationNotification(
                        ProductName,
                        product.ExpiryDate.Value),
                    Times.Once());
            }
            else
            {
                _mockNotificationService.Verify(
                    service => service.SendExpirationNotification(
                        It.IsAny<string>(),
                        It.IsAny<DateTime>()),
                    Times.Never());
            }
        }

        private static Product CreateProduct(int available, int leadTime = 0)
        {
            return new Product
            {
                Name = ProductName,
                Type = "NORMAL",
                Available = available,
                LeadTime = leadTime
            };
        }
    }
}
