using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Dtos.Product;
using Refacto.DotNet.Controllers.Services;

namespace Refacto.DotNet.Controllers.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly AppDbContext _appDbContext;

        public OrderController(ProductService productService, AppDbContext appDbContext)
        {
            _productService = productService;
            _appDbContext = appDbContext;
        }

        [HttpPost("{orderId}/processOrder")]
        [ProducesResponseType(200)]
        public ActionResult<ProcessOrderResponse> ProcessOrder(long orderId)
        {
            Entities.Order? order = _appDbContext.Orders
                .Include(o => o.Items)
                .SingleOrDefault(o => o.Id == orderId);

            ICollection<Entities.Product>? products = order.Items;

            foreach (Entities.Product item in products)
            {
                if (item.Type == "NORMAL")
                {
                    if (item.Available > 0)
                    {
                        item.Available -= 1;
                        _appDbContext.Entry(item).State = EntityState.Modified;
                        _ = _appDbContext.SaveChanges();

                    }
                    else
                    {
                        int leadTime = item.LeadTime;
                        if (leadTime > 0)
                        {
                            _productService.NotifyDelay(leadTime, item);
                        }
                    }
                }
                else if (item.Type == "SEASONAL")
                {
                    if (DateTime.Now.Date > item.SeasonStartDate && DateTime.Now.Date < item.SeasonEndDate && item.Available > 0)
                    {
                        item.Available -= 1;
                        _ = _appDbContext.SaveChanges();
                    }
                    else
                    {
                        _productService.HandleSeasonalProduct(item);
                    }
                }
                else if (item.Type == "EXPIRABLE")
                {
                    if (item.Available > 0 && item.ExpiryDate > DateTime.Now.Date)
                    {
                        item.Available -= 1;
                        _ = _appDbContext.SaveChanges();
                    }
                    else
                    {
                        _productService.HandleExpiredProduct(item);
                    }
                }
            }

            return new ProcessOrderResponse(order.Id);
        }
    }
}
