namespace Refacto.DotNet.Controllers.Exceptions
{
    /// <summary>Indicates that required product data is missing or invalid.</summary>
    public sealed class InvalidProductDataException : DomainException
    {
        /// <summary>Creates an exception describing the invalid product data.</summary>
        /// <param name="message">Description of the invalid data.</param>
        public InvalidProductDataException(string message) : base(message)
        {
        }
    }
}
