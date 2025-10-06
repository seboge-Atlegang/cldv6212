using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var cfg = context.Configuration;
        var cs = cfg["Values:STORAGE_CONNECTION"] ?? cfg["STORAGE_CONNECTION"] ??
                 cfg["Values:AzureWebJobsStorage"] ?? cfg["AzureWebJobsStorage"];
        if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("STORAGE_CONNECTION not set");

        services.AddSingleton(new TableServiceClient(cs));
        services.AddSingleton(new BlobServiceClient(cs));
        services.AddSingleton(new QueueServiceClient(cs));
        services.AddSingleton(new ShareServiceClient(cs));
    })
    .Build();

host.Run();
