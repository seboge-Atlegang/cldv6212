using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Azure.Data.Tables;
using ABCRetailsFunctions.Entities;
using ABCRetailsFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Storage;



namespace ABCRetailsFunctions.Functions;

public class QueueProcessorFunctions
{
    private readonly TableServiceClient _tables;
    private readonly string _orders;

    public QueueProcessorFunctions(TableServiceClient tables)
    {
        _tables = tables;
        _orders = Environment.GetEnvironmentVariable("TABLE_ORDERS")!;
    }

    [Function("orders-queue-processor")]
    public async Task Run([Microsoft.Azure.Functions.Worker.QueueTrigger("%QUEUE_ORDERS%", Connection = "STORAGE_CONNECTION")] string base64, FunctionContext ctx)
    {
        var log = ctx.GetLogger("orders-queue-processor");
        var msg = JsonSerializer.Deserialize<OrderMessage>(base64, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (msg is null) { log.LogWarning("Invalid message"); return; }

        var t = _tables.GetTableClient(_orders);
        await t.CreateIfNotExistsAsync();

        if (!string.IsNullOrWhiteSpace(msg.CustomerId) && !string.IsNullOrWhiteSpace(msg.ProductId))
        {
            var ent = new OrderEntity
            {
                RowKey = msg.OrderId,
                CustomerId = msg.CustomerId,
                Username = msg.Username ?? "",
                ProductId = msg.ProductId,
                ProductName = msg.ProductName,
                Quantity = msg.Quantity,
                UnitPrice = msg.UnitPrice,
                TotalPrice = msg.TotalPrice,
                OrderDate = msg.OrderDateUtc.UtcDateTime,
                Status = "Processing"
            };
            await t.UpsertEntityAsync(ent, TableUpdateMode.Replace);
            log.LogInformation("Order created {OrderId}", ent.RowKey);
        }
        else if (!string.IsNullOrWhiteSpace(msg.OrderId) && !string.IsNullOrWhiteSpace(msg.Status))
        {
            try
            {
                var e = await t.GetEntityAsync<OrderEntity>("Order", msg.OrderId); var ent = e.Value;
                ent.Status = msg.Status; await t.UpdateEntityAsync(ent, ent.ETag, TableUpdateMode.Replace);
                log.LogInformation("Order {OrderId} -> {Status}", ent.RowKey, ent.Status);
            }
            catch (RequestFailedException ex) when (ex.Status == 404) { log.LogWarning("Order {OrderId} not found", msg.OrderId); }
        }
    }
}