using Microsoft.AspNetCore.Mvc;
using Refacto.DotNet.Controllers.Dtos.Product;
using Refacto.DotNet.Controllers.Services;

namespace Refacto.DotNet.Controllers.Controllers
{
    /// <summary>Exposes HTTP endpoints for order operations.</summary>
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        /// <summary>Creates an order controller.</summary>
        /// <param name="orderService">Service that processes orders.</param>
        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>Processes the products in an order.</summary>
        /// <param name="orderId">Identifier of the order to process.</param>
        /// <returns>The identifier of the processed order.</returns>
        [HttpPost("{orderId}/processOrder")]
        [ProducesResponseType(typeof(ProcessOrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public ActionResult<ProcessOrderResponse> ProcessOrder(long orderId)
        {
            var order = _orderService.ProcessOrder(orderId);
            return new ProcessOrderResponse(order.Id);
        }
    }
}
