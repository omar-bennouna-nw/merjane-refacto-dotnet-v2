using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services.Impl
{
    public class ProductService
    {
        private readonly INotificationService _notificationService;
        private readonly AppDbContext _appDbContext;

        public ProductService(INotificationService notificationService, AppDbContext appDbContext)
        {
            _notificationService = notificationService;
            _appDbContext = appDbContext;
        }

        public void NotifyDelay(int leadTime, Product product)
        {
            product.LeadTime = leadTime;
            _ = _appDbContext.SaveChanges();
            _notificationService.SendDelayNotification(leadTime, product.Name);
        }

        public void HandleSeasonalProduct(Product product)
        {
            if (DateTime.Now.AddDays(product.LeadTime) > product.SeasonEndDate)
            {
                _notificationService.SendOutOfStockNotification(product.Name);
                product.Available = 0;
                _ = _appDbContext.SaveChanges();
            }
            else if (product.SeasonStartDate > DateTime.Now)
            {
                _notificationService.SendOutOfStockNotification(product.Name);
                _ = _appDbContext.SaveChanges();
            }
            else
            {
                NotifyDelay(product.LeadTime, product);
            }
        }

        public void HandleExpiredProduct(Product product)
        {
            if (product.Available > 0 && product.ExpiryDate > DateTime.Now)
            {
                product.Available -= 1;
                _ = _appDbContext.SaveChanges();
            }
            else
            {
                _notificationService.SendExpirationNotification(product.Name, (DateTime)product.ExpiryDate);
                product.Available = 0;
                _ = _appDbContext.SaveChanges();
            }
        }
    }
}
