using Microsoft.EntityFrameworkCore;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Exceptions;

namespace Refacto.DotNet.Controllers.Services
{
    /// <summary>Orchestrates order processing and persists its product changes.</summary>
    public class OrderService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ProductService _productService;

        /// <summary>Creates an order service with its persistence and product-processing dependencies.</summary>
        /// <param name="appDbContext">Database context used to load and save orders.</param>
        /// <param name="productService">Service applying product business rules.</param>
        public OrderService(AppDbContext appDbContext, ProductService productService)
        {
            _appDbContext = appDbContext;
            _productService = productService;
        }

        /// <summary>Processes every product in an order and saves the resulting changes.</summary>
        /// <param name="orderId">Identifier of the order to process.</param>
        /// <returns>The processed order.</returns>
        /// <exception cref="OrderNotFoundException">The order does not exist.</exception>
        /// <exception cref="UnknownProductTypeException">An order contains an unrecognized product type.</exception>
        public Order ProcessOrder(long orderId)
        {
            Order? order = _appDbContext.Orders
                .Include(existingOrder => existingOrder.Items)
                .SingleOrDefault(existingOrder => existingOrder.Id == orderId);

            if (order is null)
            {
                throw new OrderNotFoundException(orderId);
            }

            foreach (Product product in order.Items ?? Array.Empty<Product>())
            {
                _productService.ProcessProduct(product);
            }

            _ = _appDbContext.SaveChanges();
            return order;
        }
    }
}
