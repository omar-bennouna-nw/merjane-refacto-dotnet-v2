namespace Refacto.DotNet.Controllers.Exceptions
{
    /// <summary>Indicates that a product has an unsupported type.</summary>
    public sealed class UnknownProductTypeException : DomainException
    {
        /// <summary>Creates an exception for the specified product type.</summary>
        /// <param name="productType">The unsupported product type.</param>
        public UnknownProductTypeException(string? productType)
            : base($"The product type '{productType ?? "<null>"}' is not supported.")
        {
        }
    }
}
