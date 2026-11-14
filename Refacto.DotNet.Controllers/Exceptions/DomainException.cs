namespace Refacto.DotNet.Controllers.Exceptions
{
    /// <summary>Represents a business rule violation that can be returned to an API client.</summary>
    public abstract class DomainException : Exception
    {
        /// <summary>Creates a domain exception with a descriptive message.</summary>
        /// <param name="message">Description of the business rule violation.</param>
        protected DomainException(string message) : base(message)
        {
        }
    }
}
