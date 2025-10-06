using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using ABCRetailsFunctions.Entities;
using ABCRetailsFunctions.Helpers;
using ABCRetailsFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetailsFunctions.Functions;

public class ProductsFunctions
{
    private readonly TableServiceClient _tables;
    private readonly BlobServiceClient _blobs;
    private readonly string _table;
    private readonly string _container;

    public ProductsFunctions(TableServiceClient tables, BlobServiceClient blobs)
    {
        _tables = tables;
        _blobs = blobs;
        _table = Environment.GetEnvironmentVariable("TABLE_PRODUCTS")!;
        _container = Environment.GetEnvironmentVariable("BLOB_CONTAINER")!;
    }

    [Function("products-list")]
    public async Task<HttpResponseData> List([HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequestData req)
    {
        var t = _tables.GetTableClient(_table);
        await t.CreateIfNotExistsAsync();
        var items = new List<ProductDto>();
        await foreach (var e in t.QueryAsync<TableEntity>(x => x.PartitionKey == "Product")) items.Add(e.ToDtoProduct());
        return HttpJson.Ok(req, items);
    }

    [Function("products-get")]
    public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{id}")] HttpRequestData req, string id)
    {
        var t = _tables.GetTableClient(_table);
        try { var e = await t.GetEntityAsync<TableEntity>("Product", id); return HttpJson.Ok(req, e.Value.ToDtoProduct()); }
        catch { return HttpJson.NotFound(req); }
    }

    [Function("products-create")]
    public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequestData req)
    {
        var body = await HttpJson.ReadAsync<CreateProductRequest>(req);
        if (body is null) return HttpJson.Bad(req, "Invalid payload");
        var ent = new ProductEntity { ProductName = body.ProductName, Description = body.Description, Price = body.Price, StockAvailable = body.StockAvailable, ImageUrl = body.ImageUrl ?? "" };
        var t = _tables.GetTableClient(_table); await t.CreateIfNotExistsAsync(); await t.AddEntityAsync(ent);
        return HttpJson.Created(req, new ProductDto(ent.RowKey, ent.ProductName, ent.Description, ent.Price, ent.StockAvailable, ent.ImageUrl));
    }

    [Function("products-update")]
    public async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, "put", Route = "products/{id}")] HttpRequestData req, string id)
    {
        var body = await HttpJson.ReadAsync<CreateProductRequest>(req);
        if (body is null) return HttpJson.Bad(req, "Invalid payload");
        var t = _tables.GetTableClient(_table);
        try
        {
            var e = await t.GetEntityAsync<ProductEntity>("Product", id); var ent = e.Value;
            ent.ProductName = body.ProductName; ent.Description = body.Description; ent.Price = body.Price; ent.StockAvailable = body.StockAvailable;
            if (!string.IsNullOrWhiteSpace(body.ImageUrl)) ent.ImageUrl = body.ImageUrl;
            await t.UpdateEntityAsync(ent, ent.ETag, TableUpdateMode.Replace);
            return HttpJson.Ok(req, new ProductDto(ent.RowKey, ent.ProductName, ent.Description, ent.Price, ent.StockAvailable, ent.ImageUrl));
        }
        catch { return HttpJson.NotFound(req); }
    }

    [Function("products-delete")]
    public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "products/{id}")] HttpRequestData req, string id)
    {
        var t = _tables.GetTableClient(_table);
        await t.DeleteEntityAsync("Product", id);
        return HttpJson.NoContent(req);
    }
}
