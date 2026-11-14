using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Exceptions;
using Refacto.DotNet.Controllers.Interfaces;

namespace Refacto.DotNet.Controllers.Services
{
    /// <summary>Applies product-specific stock and availability rules.</summary>
    public class ProductService
    {
        private readonly INotificationService _notificationService;

        /// <summary>Creates a product service with its notification-delivery dependency.</summary>
        /// <param name="notificationService">Service used to deliver product notifications.</param>
        public ProductService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>Applies the business rule associated with a product's type.</summary>
        /// <param name="product">Product to process.</param>
        /// <exception cref="UnknownProductTypeException">The product type is not recognized.</exception>
        /// <exception cref="InvalidProductDataException">Required data for the product type is missing.</exception>
        public void ProcessProduct(Product product)
        {
            switch (product.Type)
            {
                case "NORMAL":
                    ProcessNormalProduct(product);
                    break;
                case "SEASONAL":
                    ProcessSeasonalProduct(product);
                    break;
                case "EXPIRABLE":
                    ProcessExpirableProduct(product);
                    break;
                default:
                    throw new UnknownProductTypeException(product.Type);
            }
        }

        private void ProcessNormalProduct(Product product)
        {
            if (product.Available > 0)
            {
                product.Available--;
            }
            else if (product.LeadTime > 0)
            {
                _notificationService.SendDelayNotification(product.LeadTime, GetProductName(product));
            }
        }

        private void ProcessSeasonalProduct(Product product)
        {
            DateTime seasonStartDate = product.SeasonStartDate
                ?? throw new InvalidProductDataException("A seasonal product must have a season start date.");
            DateTime seasonEndDate = product.SeasonEndDate
                ?? throw new InvalidProductDataException("A seasonal product must have a season end date.");
            DateTime today = DateTime.Now.Date;
            DateTime seasonStart = seasonStartDate.Date;
            DateTime seasonEnd = seasonEndDate.Date;

            if (today < seasonStart || today > seasonEnd)
            {
                _notificationService.SendOutOfStockNotification(GetProductName(product));
                return;
            }

            if (product.Available > 0)
            {
                product.Available--;
                return;
            }

            DateTime expectedAvailabilityDate = today.AddDays(product.LeadTime);
            if (expectedAvailabilityDate > seasonEnd)
            {
                product.Available = 0;
                _notificationService.SendOutOfStockNotification(GetProductName(product));
                return;
            }

            _notificationService.SendDelayNotification(product.LeadTime, GetProductName(product));
        }

        private void ProcessExpirableProduct(Product product)
        {
            DateTime expiryDate = product.ExpiryDate
                ?? throw new InvalidProductDataException("An expirable product must have an expiry date.");

            if (expiryDate.Date < DateTime.Now.Date)
            {
                product.Available = 0;
                return;
            }

            if (product.Available > 0)
            {
                product.Available--;
            }
        }

        private static string GetProductName(Product product)
        {
            return product.Name
                ?? throw new InvalidProductDataException("A product must have a name to send a notification.");
        }
    }
}
