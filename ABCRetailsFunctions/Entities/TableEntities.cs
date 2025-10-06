using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace ABCRetailsFunctions.Entities;

public sealed class CustomerEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Customer";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
}

public sealed class ProductEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Product";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string ProductName { get; set; } = "";
    public string Description { get; set; } = "";
    public double Price { get; set; }
    public int StockAvailable { get; set; }
    public string ImageUrl { get; set; } = "";
}

public sealed class OrderEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Order";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string CustomerId { get; set; } = "";
    public string Username { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double TotalPrice { get; set; }
    public string Status { get; set; } = "Submitted";
}