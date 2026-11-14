using Moq;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Exceptions;
using Refacto.DotNet.Controllers.Interfaces;
using Refacto.DotNet.Controllers.Services;

namespace Refacto.DotNet.Controllers.Tests.Services
{
    public class ProductServiceTests
    {
        private const string ProductName = "Test product";

        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _productService = new ProductService(_mockNotificationService.Object);
        }

        [Theory]
        [InlineData(2, 15, 1, false)]
        [InlineData(0, 15, 0, true)]
        [InlineData(0, 0, 0, false)]
        public void ProcessProduct_NormalProduct_ShouldApplyStockAndDelayRules(
            int available,
            int leadTime,
            int expectedAvailable,
            bool expectsDelayNotification)
        {
            Product product = CreateProduct("NORMAL", available, leadTime);

            _productService.ProcessProduct(product);

            Assert.Equal(expectedAvailable, product.Available);
            _mockNotificationService.Verify(
                service => service.SendDelayNotification(leadTime, ProductName),
                expectsDelayNotification ? Times.Once() : Times.Never());
            _mockNotificationService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(4, 5, -5, 10, 3, "none")]
        [InlineData(0, 5, -5, 3, 0, "out-of-stock")]
        [InlineData(4, 5, 10, 30, 4, "out-of-stock")]
        [InlineData(0, 5, -10, 30, 0, "delay")]
        [InlineData(4, 5, -10, -1, 4, "out-of-stock")]
        public void ProcessProduct_SeasonalProduct_ShouldApplySeasonRules(
            int available,
            int leadTime,
            int seasonStartOffsetDays,
            int seasonEndOffsetDays,
            int expectedAvailable,
            string expectedNotification)
        {
            Product product = CreateProduct("SEASONAL", available, leadTime);
            product.SeasonStartDate = DateTime.Now.AddDays(seasonStartOffsetDays);
            product.SeasonEndDate = DateTime.Now.AddDays(seasonEndOffsetDays);

            _productService.ProcessProduct(product);

            Assert.Equal(expectedAvailable, product.Available);
            VerifyNotification(expectedNotification, leadTime);
        }

        [Theory]
        [InlineData(5, 20, 4)]
        [InlineData(0, 20, 0)]
        [InlineData(5, -1, 0)]
        [InlineData(5, 0, 4)]
        public void ProcessProduct_ExpirableProduct_ShouldApplyExpiryRules(
            int available,
            int expiryOffsetDays,
            int expectedAvailable)
        {
            Product product = CreateProduct("EXPIRABLE", available);
            product.ExpiryDate = DateTime.Now.AddDays(expiryOffsetDays);

            _productService.ProcessProduct(product);

            Assert.Equal(expectedAvailable, product.Available);
            _mockNotificationService.Verify(
                service => service.SendExpirationNotification(It.IsAny<string>(), It.IsAny<DateTime>()),
                Times.Never());
            _mockNotificationService.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("UNKNOWN")]
        [InlineData(null)]
        public void ProcessProduct_UnknownType_ShouldThrowDomainException(string? productType)
        {
            Product product = CreateProduct(productType, available: 1);

            Assert.Throws<UnknownProductTypeException>(() => _productService.ProcessProduct(product));
            _mockNotificationService.VerifyNoOtherCalls();
        }

        [Fact]
        public void ProcessProduct_ExpirableProductWithoutExpiryDate_ShouldThrowDomainException()
        {
            Product product = CreateProduct("EXPIRABLE", available: 1);

            Assert.Throws<InvalidProductDataException>(() => _productService.ProcessProduct(product));
        }

        private void VerifyNotification(string expectedNotification, int leadTime)
        {
            switch (expectedNotification)
            {
                case "delay":
                    _mockNotificationService.Verify(
                        service => service.SendDelayNotification(leadTime, ProductName),
                        Times.Once());
                    _mockNotificationService.Verify(
                        service => service.SendOutOfStockNotification(It.IsAny<string>()),
                        Times.Never());
                    break;
                case "out-of-stock":
                    _mockNotificationService.Verify(
                        service => service.SendOutOfStockNotification(ProductName),
                        Times.Once());
                    _mockNotificationService.Verify(
                        service => service.SendDelayNotification(It.IsAny<int>(), It.IsAny<string>()),
                        Times.Never());
                    break;
                default:
                    _mockNotificationService.VerifyNoOtherCalls();
                    return;
            }

            _mockNotificationService.VerifyNoOtherCalls();
        }

        private static Product CreateProduct(string? type, int available, int leadTime = 0)
        {
            return new Product
            {
                Name = ProductName,
                Type = type,
                Available = available,
                LeadTime = leadTime
            };
        }
    }
}
