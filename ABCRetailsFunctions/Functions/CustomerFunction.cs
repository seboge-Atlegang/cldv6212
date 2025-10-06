using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using ABCRetailsFunctions.Helpers;
using ABCRetailsFunctions.Entities;
using ABCRetailsFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetailsFunctions.Functions;

public class CustomersFunctions
{
    private readonly TableServiceClient _tables;
    private readonly string _table;

    public CustomersFunctions(TableServiceClient tables)
    {
        _tables = tables;
        _table = Environment.GetEnvironmentVariable("TABLE_CUSTOMERS")!;
    }

    [Function("customers-list")]
    public async Task<HttpResponseData> List([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequestData req)
    {
        var t = _tables.GetTableClient(_table);
        await t.CreateIfNotExistsAsync();
        var items = new List<CustomerDto>();
        await foreach (var e in t.QueryAsync<TableEntity>(x => x.PartitionKey == "Customer")) items.Add(e.ToDto());
        return HttpJson.Ok(req, items);
    }

    [Function("customers-get")]
    public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{id}")] HttpRequestData req, string id)
    {
        var t = _tables.GetTableClient(_table);
        try { var e = await t.GetEntityAsync<TableEntity>("Customer", id); return HttpJson.Ok(req, e.Value.ToDto()); }
        catch { return HttpJson.NotFound(req); }
    }

    [Function("customers-create")]
    public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
    {
        var body = await HttpJson.ReadAsync<CreateCustomerRequest>(req);
        if (body is null) return HttpJson.Bad(req, "Invalid payload");
        var ent = new CustomerEntity { Name = body.Name, Surname = body.Surname, Username = body.Username, Email = body.Email, ShippingAddress = body.ShippingAddress };
        var t = _tables.GetTableClient(_table); await t.CreateIfNotExistsAsync(); await t.AddEntityAsync(ent);
        return HttpJson.Created(req, new CustomerDto(ent.RowKey, ent.Name, ent.Surname, ent.Username, ent.Email, ent.ShippingAddress));
    }

    [Function("customers-update")]
    public async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, "put", Route = "customers/{id}")] HttpRequestData req, string id)
    {
        var body = await HttpJson.ReadAsync<CreateCustomerRequest>(req);
        if (body is null) return HttpJson.Bad(req, "Invalid payload");
        var t = _tables.GetTableClient(_table);
        try
        {
            var e = await t.GetEntityAsync<CustomerEntity>("Customer", id); var ent = e.Value;
            ent.Name = body.Name; ent.Surname = body.Surname; ent.Username = body.Username; ent.Email = body.Email; ent.ShippingAddress = body.ShippingAddress;
            await t.UpdateEntityAsync(ent, ent.ETag, TableUpdateMode.Replace);
            return HttpJson.Ok(req, new CustomerDto(ent.RowKey, ent.Name, ent.Surname, ent.Username, ent.Email, ent.ShippingAddress));
        }
        catch { return HttpJson.NotFound(req); }
    }

    [Function("customers-delete")]
    public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customers/{id}")] HttpRequestData req, string id)
    {
        var t = _tables.GetTableClient(_table);
        await t.DeleteEntityAsync("Customer", id);
        return HttpJson.NoContent(req);
    }
}