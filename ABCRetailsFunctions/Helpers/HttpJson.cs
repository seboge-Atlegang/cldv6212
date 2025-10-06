using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetailsFunctions.Helpers;

public static class HttpJson
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<T?> ReadAsync<T>(HttpRequestData req)
    {
        using var s = req.Body;
        return await JsonSerializer.DeserializeAsync<T>(s, Json);
    }

    public static HttpResponseData Ok(HttpRequestData req, object body) => Write(req, HttpStatusCode.OK, body);
    public static HttpResponseData Created(HttpRequestData req, object body) => Write(req, HttpStatusCode.Created, body);
    public static HttpResponseData Bad(HttpRequestData req, string msg) => Write(req, HttpStatusCode.BadRequest, new { error = msg });
    public static HttpResponseData NotFound(HttpRequestData req) => Write(req, HttpStatusCode.NotFound, new { error = "Not found" });
    public static HttpResponseData NoContent(HttpRequestData req) => req.CreateResponse(HttpStatusCode.NoContent);
    public static HttpResponseData Accepted(HttpRequestData req) => req.CreateResponse(HttpStatusCode.Accepted);

    private static HttpResponseData Write(HttpRequestData req, HttpStatusCode code, object body)
    {
        var resp = req.CreateResponse(code);
        resp.Headers.Add("Content-Type", "application/json; charset=utf-8");
        resp.WriteString(JsonSerializer.Serialize(body, Json));
        return resp;
    }
}