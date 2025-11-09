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


string accountUrl = builder.Configuration["BLOB_ACCOUNT_URL"]
    ?? throw new InvalidOperationException("BLOB_ACCOUNT_URL not configured.");

BlobContainerClient containerClient = new BlobContainerClient(
    new Uri($"{accountUrl}/uploads"),
    new DefaultAzureCredential());

await containerClient.CreateIfNotExistsAsync();

builder.Services.AddSingleton(containerClient);


//string cosmosConn = builder.Configuration["COSMOS_CONNECTION_STRING"];
//string cosmosDb = builder.Configuration["COSMOS_DB_NAME"];
//string cosmosContainerName = builder.Configuration["COSMOS_CONTAINER_NAME"];

//CosmosClient cosmosClient = new CosmosClient(cosmosConn);

//builder.Services.AddSingleton(cosmosClient.GetContainer(cosmosDb!, cosmosContainerName!));


builder.Build().Run();