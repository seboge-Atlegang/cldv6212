using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.Tables;
using ABCRetailsFunctions.Models;

namespace ABCRetailsFunctions.Helpers;

public static class Map
{
    public static CustomerDto ToDto(this TableEntity e) => new(
        e.RowKey, Get(e, "Name"), Get(e, "Surname"), Get(e, "Username"), Get(e, "Email"), Get(e, "ShippingAddress"));

    public static ProductDto ToDtoProduct(this TableEntity e) => new(
        e.RowKey, Get(e, "ProductName"), Get(e, "Description"), GetDouble(e, "Price") ?? 0,
        GetInt(e, "StockAvailable") ?? 0, Get(e, "ImageUrl"));

    public static OrderDto ToDtoOrder(this TableEntity e) => new(
        e.RowKey, Get(e, "CustomerId"), Get(e, "ProductId"), Get(e, "ProductName"),
        GetInt(e, "Quantity") ?? 0, GetDouble(e, "UnitPrice") ?? 0,
        GetDate(e, "OrderDate")?.ToUniversalTime() ?? DateTime.UtcNow, Get(e, "Status"));

    private static string Get(TableEntity e, string k) => e.TryGetValue(k, out var v) && v is not null ? v.ToString()! : "";
    private static int? GetInt(TableEntity e, string k) => e.TryGetValue(k, out var v) && v is not null ? Convert.ToInt32(v) : (int?)null;
    private static double? GetDouble(TableEntity e, string k) => e.TryGetValue(k, out var v) && v is not null ? Convert.ToDouble(v) : (double?)null;
    private static DateTime? GetDate(TableEntity e, string k) => e.TryGetValue(k, out var v) && v is DateTime dt ? dt : (DateTime?)null;
}