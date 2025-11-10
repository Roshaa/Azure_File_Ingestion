# Azure File Ingestion (APIM + Functions + Blob + Queue + Cosmos)

Production-style file ingestion flow using Azure services and CI/CD.

- API behind APIM
- HTTP-triggered Function for upload
- Blob Storage for raw files
- Queue-based processing
- Cosmos DB for metadata
- GitHub Actions for deployment
- Infra created manually with least privilege

All code is in this repo. Infra proof is in screenshots and policy files.

---

## What this does

End-to-end flow:

1. Client calls `POST /File/upload` on the Web API (behind APIM).
2. API forwards the file to `BlobUpload` Azure Function.
3. `BlobUpload`:
   - Validates headers
   - Uploads PDF into `uploads` container
   - Sets metadata:
     - `originalFileName`
     - `contact`
4. Blob events are sent into `blob-events` queue.
5. `CosmosPersist` Function (queue trigger):
   - Reads queue message
   - Resolves blob and metadata
   - Writes an `IngestedFile` document into Cosmos DB
     - Partition key: `/contact`

Goal: model a realistic Azure ingestion pipeline, not a demo controller.

---

## Architecture

**Components**

- API:
  - ASP.NET Web API (`FileController`)
  - Exposed via APIM
  - Forwards uploads to Function with function key

- Azure Functions (isolated worker):
  - `BlobUpload`
    - HTTP trigger
    - Uses `BlobContainerClient`
    - Uploads with metadata
  - `CosmosPersist`
    - Queue trigger on `blob-events`
    - Uses `BlobServiceClient` + `CosmosClient`
    - `UpsertItemAsync` into Cosmos

- Storage:
  - Blob Storage
    - Container: `uploads`
  - Queue Storage / Event Grid
    - Queue: `blob-events` for processing trigger

- Database:
  - Cosmos DB
    - Database: `fileingest` (default)
    - Container: `uploads`
    - Partition key: `/contact`
    - Model: `IngestedFile`

- Front door:
  - Azure API Management in front of the Web API
  - Product + subscription key
  - Policies from `APIMpolicies.txt`

- Hosting:
  - App Service for Web API
  - Azure Functions (flex/consumption) for functions

**Text diagram**

Client  
→ APIM (policies, subscription, validation)  
→ Web API (App Service)  
→ BlobUpload Function (HTTP)  
→ Blob `uploads/` (metadata: file + contact)  
→ Queue `blob-events`  
→ CosmosPersist Function (queue trigger)  
→ Cosmos DB `fileingest/uploads`

---

## APIM configuration

Policies stored as code in `APIMpolicies.txt`.

Key behaviors:

- Rate limit per subscription/IP:
  - `5` calls per `60` seconds
- Concurrency limit:
  - Max `5` in-flight per key
- CORS:
  - Restricted allowed origins
- Request validation:
  - Only `multipart/form-data`
  - Block wrong `Content-Type` with `415`
  - Enforce `Content-Length` ≤ 5 MB
  - Return `413` and `Retry-After` for oversized uploads
- Security hardening:
  - Removes `Server` and `X-Powered-By` headers
- APIM sits in front of the App Service
- App Service inbound locked so only APIM can call it

APIM is updated via CI/CD using the OpenAPI definition from the API. Each deploy creates a new API revision.

---

## Implementation details

### API (`Azure_File_Ingestion`)

- `FileController`
  - `POST /File/upload`
  - Validates:
    - Required file
    - Max 5 MB
    - Only PDF
    - Valid `Name`
    - Valid `Contact` (phone format)
  - Uses `IHttpClientFactory` to call `BlobUpload` Function
  - Sends:
    - Body as stream
    - `x-file-name`
    - `x-contact`
    - Function key via query (`BLOBUPLOAD_FUNCTION_CODE`)

### Functions (`fileingest-blob-upload`)

- `Program.cs`
  - Registers:
    - `BlobServiceClient` using `DefaultAzureCredential` and `BLOB_ACCOUNT_URL`
    - `BlobContainerClient` for `uploads` and creates if not exists
    - `CosmosClient`
    - Cosmos database and container if not exists

- `BlobUpload`
  - HTTP trigger
  - Reads headers
  - Writes blob with metadata
  - Returns:
    - `blobName`
    - `originalFileName`

- `CosmosPersist`
  - Queue trigger: `blob-events`
  - Parses event payload
  - Reads blob properties
  - Builds `IngestedFile`
  - Upserts into Cosmos with `PartitionKey(contact)`
  - Logs saved item

- `IngestedFile`
  - `id`
  - `originalFileName`
  - `blobName`
  - `created`
  - `size`
  - `type`
  - `contact`

---

## Configuration

All secrets and connection details via environment variables / App Settings.

**API**

- `FUNC_BASE_URL`
- `BLOBUPLOAD_FUNCTION_CODE`

**Functions**

- `BLOB_ACCOUNT_URL`
- `BLOB_CONNECTION_STRING` (for queue trigger binding)
- `COSMOS_CONNECTION_STRING`
- `COSMOS_DB_NAME` (optional)
- `COSMOS_CONTAINER_NAME` (optional)

Example local settings and appsettings are included in code comments/docs, not with real secrets.

---

## CI/CD

Workflows live under `.github/workflows`.

Behavior:

- On push to main:
  - Restore
  - Build
  - (Hook for tests)
  - Deploy Web API to App Service
  - Deploy Azure Functions to Function App
- Uses:
  - `azure/login` with OIDC
  - Azure deploy actions
- APIM is updated using the API OpenAPI spec so revisions stay in sync.

---

## Observability

- Application Insights configured for Functions.
- Logs include:
  - Incoming upload requests
  - Blob upload events
  - Cosmos writes
- Used during development to:
  - Debug trigger behavior
  - Validate end-to-end pipeline

---

## Infra notes

- All Azure resources were created manually:
  - Cheapest tiers available to keep costs down
  - Roles assigned with least privilege in mind
- App Service inbound locked:
  - Only APIM allowed
- APIM:
  - Product and subscription configured
  - Tested using Postman
- Storage:
  - Blob container for uploads
  - Queue/Event Grid wiring for `blob-events`
- Cosmos:
  - Account + DB + container created and wired

Infra has been deleted to avoid costs.
Evidence and configuration snapshots are stored under `azure project prints` and referenced by `prints.md`.

---

## Why this project exists

- Practice real-world Azure patterns on a small, focused scenario.
- Demonstrate:
  - APIM as a gateway
  - Functions as compute boundary
  - Blob + Queue + Cosmos integration
  - CI/CD to App Service and Functions
  - Policies-as-code and documented infra

This is a backend-focused, cloud-native ingestion flow suitable as portfolio proof.
