using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();


///////////////////////
/// Blob setup
///////////////////////

string accountUrl = builder.Configuration["BLOB_ACCOUNT_URL"]
    ?? throw new InvalidOperationException("BLOB_ACCOUNT_URL not configured.");

builder.Services.AddSingleton(new BlobServiceClient(new Uri(accountUrl), new DefaultAzureCredential()));

BlobContainerClient containerClient = new BlobContainerClient(
    new Uri($"{accountUrl}/uploads"),
    new DefaultAzureCredential());

await containerClient.CreateIfNotExistsAsync();
builder.Services.AddSingleton(containerClient);

///////////////////////



///////////////////////
/// Cosmos DB setup
///////////////////////


string cosmosConn = builder.Configuration["COSMOS_CONNECTION_STRING"]!;
string dbName = builder.Configuration["COSMOS_DB_NAME"] ?? "fileingest";
string container = builder.Configuration["COSMOS_CONTAINER_NAME"] ?? "uploads";

var client = new CosmosClient(cosmosConn);
var db = await client.CreateDatabaseIfNotExistsAsync(dbName);
await db.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(container, "/contact"));
builder.Services.AddSingleton(client);
builder.Services.AddSingleton(sp => client.GetContainer(dbName, container));

///////////////////////

builder.Build().Run();