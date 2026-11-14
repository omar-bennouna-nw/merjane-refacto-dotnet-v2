namespace Refacto.DotNet.Controllers.Exceptions
{
    /// <summary>Indicates that an order could not be found.</summary>
    public sealed class OrderNotFoundException : Exception
    {
        /// <summary>Creates an exception for the specified order identifier.</summary>
        /// <param name="orderId">Identifier of the missing order.</param>
        public OrderNotFoundException(long orderId)
            : base($"Order '{orderId}' was not found.")
        {
        }
    }
}
