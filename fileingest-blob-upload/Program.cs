using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

string accountUrl = builder.Configuration["BLOB_ACCOUNT_URL"];
BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(accountUrl!), new DefaultAzureCredential());

BlobContainerClient container = blobServiceClient.GetBlobContainerClient("uploads");

await container.CreateIfNotExistsAsync();

builder.Services.AddSingleton(container);

builder.Build().Run();