using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using ABCRetailsFunctions.Helpers;
using ABCRetailsFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetailsFunctions.Functions;

public class OrdersFunctions
{
    private readonly TableServiceClient _tables;
    private readonly QueueServiceClient _queues;
    private readonly string _ordersTable;
    private readonly string _productsTable;
    private readonly string _queue;

    public OrdersFunctions(TableServiceClient tables, QueueServiceClient queues)
    {
        _tables = tables; _queues = queues;
        _ordersTable = Environment.GetEnvironmentVariable("TABLE_ORDERS")!;
        _productsTable = Environment.GetEnvironmentVariable("TABLE_PRODUCTS")!;
        _queue = Environment.GetEnvironmentVariable("QUEUE_ORDERS")!;
    }

    [Function("orders-list")]
    public async Task<HttpResponseData> List([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")] HttpRequestData req)
    {
        var t = _tables.GetTableClient(_ordersTable);
        await t.CreateIfNotExistsAsync();
        var items = new List<OrderDto>();
        await foreach (var e in t.QueryAsync<TableEntity>(x => x.PartitionKey == "Order")) items.Add(e.ToDtoOrder());
        return HttpJson.Ok(req, items);
    }

    [Function("orders-get")]
    public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{id}")] HttpRequestData req, string id)
    {
        var t = _tables.GetTableClient(_ordersTable);
        try { var e = await t.GetEntityAsync<TableEntity>("Order", id); return HttpJson.Ok(req, e.Value.ToDtoOrder()); }
        catch { return HttpJson.NotFound(req); }
    }

    [Function("orders-create")]
    public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequestData req)
    {
        var body = await HttpJson.ReadAsync<CreateOrderRequest>(req);
        if (body is null) return HttpJson.Bad(req, "Invalid payload");

        var pt = _tables.GetTableClient(_productsTable);
        var product = await pt.GetEntityAsync<TableEntity>("Product", body.ProductId);

        var msg = new OrderMessage
        {
            CustomerId = body.CustomerId,
            ProductId = body.ProductId,
            ProductName = product.Value.GetString("ProductName") ?? "",
            Quantity = body.Quantity,
            UnitPrice = Convert.ToDouble(product.Value.GetDouble("Price") ?? 0),
            TotalPrice = Convert.ToDouble(product.Value.GetDouble("Price") ?? 0) * body.Quantity,
            Status = "Submitted"
        };

        var qc = _queues.GetQueueClient(_queue);
        await qc.CreateIfNotExistsAsync();
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
        await qc.SendMessageAsync(base64);

        return HttpJson.Accepted(req);
    }

    [Function("orders-update-status")]
    public async Task<HttpResponseData> UpdateStatus([HttpTrigger(AuthorizationLevel.Function, "patch", Route = "orders/{id}/status")] HttpRequestData req, string id)
    {
        var body = await HttpJson.ReadAsync<UpdateStatusRequest>(req);
        if (body is null || string.IsNullOrWhiteSpace(body.Status)) return HttpJson.Bad(req, "Status required");

        var qc = _queues.GetQueueClient(_queue); await qc.CreateIfNotExistsAsync();
        var msg = new OrderMessage { OrderId = id, Status = body.Status };
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg)));
        await qc.SendMessageAsync(base64);
        return HttpJson.Accepted(req);
    }

    [Function("orders-delete")]
    public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{id}")] HttpRequestData req, string id)
    {
        var t = _tables.GetTableClient(_ordersTable);
        await t.DeleteEntityAsync("Order", id);
        return HttpJson.NoContent(req);
    }
}