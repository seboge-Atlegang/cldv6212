using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetailsFunctions.Models
{
    

    public sealed record CustomerDto(string Id, string Name, string Surname, string Username, string Email, string ShippingAddress);
    public sealed record ProductDto(string Id, string ProductName, string Description, double Price, int StockAvailable, string ImageUrl);
    public sealed record OrderDto(string Id, string CustomerId, string ProductId, string ProductName, int Quantity, double UnitPrice, DateTime OrderDateUtc, string Status);

    public sealed record CreateCustomerRequest(string Name, string Surname, string Username, string Email, string ShippingAddress);
    public sealed record CreateProductRequest(string ProductName, string Description, double Price, int StockAvailable, string? ImageUrl = null);
    public sealed record CreateOrderRequest(string CustomerId, string ProductId, int Quantity);
    public sealed record UpdateStatusRequest(string Status);

    public sealed class OrderMessage
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; } = default!;
        public string Username { get; set; } = string.Empty;
        public string ProductId { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public DateTimeOffset OrderDateUtc { get; set; } = DateTimeOffset.UtcNow;
        public string Status { get; set; } = "Submitted";
    }
}
