namespace Geta.Klarna.Checkout.Models
{
    public abstract class Address
    {
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string StreetAddress { get; set; }
        public string StreetName { get; set; }
        public string StreetNumber { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Title { get; set; }
    }

    public class BillingAddress : Address
    {
    }

    public class ShippingAddress : Address
    {
    }
}