using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetailsFunctions.Functions;

public class UploadsFunctions
{
    private readonly ShareServiceClient _shares;
    private readonly string _share;
    private readonly string _dir;

    public UploadsFunctions(ShareServiceClient shares)
    {
        _shares = shares;
        _share = Environment.GetEnvironmentVariable("FILE_SHARE")!;
        _dir = Environment.GetEnvironmentVariable("FILE_DIR_PAYMENTS")!;
    }

    [Function("uploads-proof-of-payment")]
    public async Task<HttpResponseData> Upload([HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploads/proof-of-payment")] HttpRequestData req)
    {
        var fileName = req.Query["fileName"];
        if (string.IsNullOrWhiteSpace(fileName)) fileName = $"upload_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bin";

        var share = _shares.GetShareClient(_share); await share.CreateIfNotExistsAsync();
        var dir = share.GetDirectoryClient(_dir); await dir.CreateIfNotExistsAsync();
        var file = dir.GetFileClient(fileName);

        using var ms = new MemoryStream();
        await req.Body.CopyToAsync(ms); ms.Position = 0;
        await file.CreateAsync(ms.Length);
        await file.UploadAsync(ms);

        var resp = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await resp.WriteAsJsonAsync(new { fileName });
        return resp;
    }
}