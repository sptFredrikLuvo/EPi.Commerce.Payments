using System.ComponentModel.DataAnnotations;
using Geta.Netaxept.Checkout.Models;

namespace Geta.Netaxept.Checkout.Extensions
{
    internal static class PaymentRequestExtensions
    {
        /// <summary>
        /// Validates the specified object, throws on first invalid property.
        /// </summary>
        /// <param name="objectToValidate">The object to validate.</param>
        /// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">
        /// <paramref name="objectToValidate" /> is not valid.</exception>
        public static void Validate(this PaymentRequest objectToValidate)
        {
            var context = new ValidationContext(objectToValidate, null, null);
            Validator.ValidateObject(objectToValidate, context, true);
        }
    }
}
