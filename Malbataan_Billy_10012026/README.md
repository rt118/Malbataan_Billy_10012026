# Malbataan_Billy_10012026

API for uploading and processing employee data in JSON or CSV formats. Accepts multipart file uploads, delegates processing to pluggable services, and records processing metrics in-memory.

- Language: C# 14
- Target framework: .NET 10
- Project folder: `Malbataan_Billy_10012026`

Quick reference
- Upload endpoint: POST `/api/UploadFile/upload`
- Report endpoint: GET `/api/Report`
- Swagger (if enabled): `/swagger`

Overview
This Web API supports two flows:
- JSON: upload a JSON array of objects; supply filter rules to select matching records.
- CSV: upload a CSV with a header row; supply an aggregate operation to compute metrics on numeric columns.

Architecture & key components
- Controllers
  - `UploadFileController` — accepts multipart uploads, validates files, dispatches to services, records processing records.
  - `ReportController` — returns in-memory processing counters and recent logs.
- Services / Interfaces
  - `ICSVProcessorService` — CSV parsing and aggregation.
  - `IJSONProcessorService` — JSON filtering/selection logic.
  - `IUploadTrackingService` — records processing metadata (default in-memory).
- Filters / Validators
  - `Filters/FileValidatorAttribute.cs` — validates uploads (size/type/etc).
  - `Validators/ApiKeyValidator.cs` — optional API key validation.
- Configuration / DI
  - Service registrations live in `Extensions/ServiceCollectionsExtension.cs`.
  - OpenAPI config in `Extensions/OpenApiConfigurationExtension.cs`.

## Run in Visual Studio 2026
1. Open the solution or folder in Visual Studio.
2. To enable XML documentation for Swagger/IntelliSense:
   - Open __Project Properties__ → __Build__ and enable __XML documentation file__.
   - Or edit the `.csproj` and add:
     ```xml
     <PropertyGroup>
       <GenerateDocumentationFile>true</GenerateDocumentationFile>
     </PropertyGroup>
     ```
3. Start debugging:
   - Use __Debug > Start Debugging__ (F5) or __Debug > Start Without Debugging__.
4. Use the __Output__ / __Debug__ windows to view logs and binding URL.

## Docker: build and run (single container)
The repository contains a `Dockerfile` that builds and runs the published app. The image exposes ports `8080` and `8081`.

Build the image:
- From repository root:
  - `docker build -t malbataan-upload-processor -f Malbataan_Billy_10012026/Dockerfile .`

Run the container:
- Example mapping container port 8080 to host port 8080 and set Kestrel URL:
  - `docker run --rm -e "ASPNETCORE_URLS=http://+:8080" -p 8080:8080 --name malbataan malbataan-upload-processor`
- If you want both exposed ports mapped:
  - `docker run --rm -e "ASPNETCORE_URLS=http://+:8080;http://+:8081" -p 8080:8080 -p 8081:8081 --name malbataan malbataan-upload-processor`

Notes:
- The Dockerfile exposes `8080` and `8081`. Ensure ASPNETCORE_URLS environment variable includes the port(s) you map.
- Use `docker logs -f malbataan` to stream container logs.

# Upload endpoints 

This section documents the upload endpoints in detail, including exact form fields, sample payloads for JSON and CSV, and example values for the `Filters` and `Aggregate` form fields.

## Endpoint summary

- POST `/api/UploadFile/upload` — Accepts a multipart/form-data upload. Handles `.json` files (filtering) and `.csv` files (aggregation).
- Validation: `Filters/FileValidatorAttribute` runs before controller action and may reject files by extension, content type or size.
- Tracking: every upload produces a `FileProcessingRecord` persisted by `IUploadTrackingService`.

## Multipart form fields

- `file` (required)
  - Type: file part (IFormFile)
  - Accepted extensions: `.json`, `.csv` (see `FileValidatorAttribute` for exact rules)
- `Filters` (optional, JSON uploads only)
  - Type: string form field whose contents are a JSON object or JSON array of rule objects.
  - Processed only when the uploaded file is `.json`.
- `Aggregate` (optional, CSV uploads only)
  - Type: string form field. Accepted values: `avg`, `sum`, `min`, `max`, `count`.
  - Defaults to `avg` when omitted.
  - Processed only when the uploaded file is `.csv`.

## Filter rule format (`Filters`)

`Filters` must be a JSON object (single rule) or an array of rule objects. Each rule follows this shape:

- `field` (string) — property name or dotted path (dot notation supported, e.g. `address.city`).
- `op` (string) — operator (see supported values below).
- `value` (any JSON value) — literal to compare to the field.

Common operator values (implementation may support more — check `IJSONProcessorService`):
- `eq` — equals
- `neq` — not equals
- `gt` — greater than
- `gte` — greater than or equal
- `lt` — less than
- `lte` — less than or equal
- `contains` — substring or collection contains (strings/arrays)
- `in` — value is in provided array
 
Example single rule:
{ "field": "Department", "op": "eq", "value": "Sales" }

Example multiple rules (Sales department and Salary > 60000):
[ { "field": "Department", "op": "eq", "value": "Sales" }, { "field": "Salary", "op": "gt", "value": 60000 } ]

## JSON upload — sample file and curl

Sample file `employees.json` (array of employee objects):
[ { "Id": 1, "Name": "Alice Johnson", "Department": "Sales", "Email": "alice.johnson@example.com", "Salary": 72000, "HiredDate": "2021-03-15" }, { "Id": 2, "Name": "Bob Smith", "Department": "Engineering", "Email": "bob.smith@example.com", "Salary": 95000, "HiredDate": "2019-07-01" }, { "Id": 3, "Name": "Carla Gomez", "Department": "Sales", "Email": "carla.gomez@example.com", "Salary": 58000, "HiredDate": "2022-11-02" } ]

## Expected response shapes (typical)

JSON response (filtering):
{ "Filename": "employees.json", "TotalRecords": 3, "MatchedRecords": 1, "FiltersApplied": [ /* the rules / ], "Data": [ / matched objects */ ] }

Notes:
- The `Filters` form value must be valid JSON (either an object or array) encoded as a string in the multipart request.
- Processor returns a JSON response (e.g., `JSONResponse`) containing matched data, counts and applied filters.

---
## CSV upload — sample file and curl

Sample file `employees.csv`:
Id,Name,Department,Email,Salary,HiredDate 1,Alice Johnson,Sales,alice.johnson@example.com,72000,2021-03-15 2,Bob Smith,Engineering,bob.smith@example.com,95000,2019-07-01 3,Carla Gomez,Sales,carla.gomez@example.com,58000,2022-11-02 4,David Lee,Engineering,david.lee@example.com,88000,2020-08-20

CSV response (aggregation):
{ "Filename": "employees.csv", "TotalRecords": 4, "Column": "Salary", "Aggregate": "sum", "Value": 313000 }

Aggregate values — semantics and examples
- The `Aggregate` form field controls which aggregation the CSV processor computes. Accepted values:
  - `sum` — Sum of the chosen numeric column.
    - Example response:
      ```json
      { "Filename":"employees.csv","TotalRecords":4,"Column":"Salary","Aggregate":"sum","Value":313000 }
      ```
  - `avg` — Arithmetic mean of the chosen numeric column (default).
    - Example response:
      ```json
      { "Filename":"employees.csv","TotalRecords":4,"Column":"Salary","Aggregate":"avg","Value":78250.0 }
      ```
  - `min` — Smallest numeric value in the chosen column.
    - Example response:
      ```json
      { "Filename":"employees.csv","TotalRecords":4,"Column":"Salary","Aggregate":"min","Value":58000 }
      ```
  - `max` — Largest numeric value in the chosen column.
    - Example response:
      ```json
      { "Filename":"employees.csv","TotalRecords":4,"Column":"Salary","Aggregate":"max","Value":95000 }
      ```
  - `count` — Number of rows (or non-empty values) considered for the chosen column.
    - Example response:
      ```json
      { "Filename":"employees.csv","TotalRecords":4,"Column":"Salary","Aggregate":"count","Value":4 }
      ```
Notes on aggregation behavior
- Column selection:
  - The CSV processor implementation typically chooses the numeric column to aggregate based on headers (commonly `Salary`). If the project exposes a configuration or parameter to pick a specific column, consult `ICSVProcessorService` implementation. Otherwise ensure the CSV header includes an obvious numeric column name.
- Missing / non-numeric values:
  - Most implementations ignore empty or non-numeric cells for numeric aggregations (sum, avg, min, max). `count` counts rows with a value depending on implementation.
- Default:
  - If `Aggregate` is omitted the controller passes the default value `avg` from `UploadRequest`.



### GET `/api/Report`

Purpose  
Returns processing counters and the most recent file processing records (newest first). Useful for quick health checks, diagnostics and verifying uploads were processed.

Query parameters
- `recent` (optional, integer) — number of recent entries to include. Default: `20`. Acceptable range: `1`–`100` (the controller clamps/validates in the tracking service; if your implementation differs adjust accordingly).

Behavior
- Data is returned from the configured `IUploadTrackingService`. The default implementation is in-memory and will reset when the application restarts.
- The endpoint returns aggregated counters (total files, successes, failures, counts by file type) and an ordered list of recent `FileProcessingRecord` objects (newest first).
- The endpoint is safe to call frequently; it only reads in-memory state.

Sample request (curl)
curl "http://localhost:8080/api/Report?recent=10"
Sample response (HTTP 200)
{ "totalFiles": 42, "succeeded": 38, "failed": 4, "filesByType": { "json": 30, "csv": 12 }, "averageProcessingTimeMs": 123.45, "recentFiles": [ { "filename": "employees.csv", "fileType": "csv", "sizeBytes": 2048, "succeeded": true, "error": null, "processingTimeMs": 98.12, "processedAtUtc": "2026-10-02T14:12:34Z" }, { "filename": "employees.json", "fileType": "json", "sizeBytes": 4096, "succeeded": false, "error": "Invalid filter rule: unknown operator", "processingTimeMs": 205.67, "processedAtUtc": "2026-10-02T13:59:02Z" } ] }

Field descriptions
- `totalFiles` — total uploads recorded since the tracker started.
- `succeeded` / `failed` — counters of outcomes.
- `filesByType` — counts grouped by file extension/type (e.g., `json`, `csv`).
- `averageProcessingTimeMs` — mean processing time across recorded uploads.
- `recentFiles` — array of `FileProcessingRecord` objects (most recent first):
  - `filename` — original uploaded filename.
  - `fileType` — extension without leading dot.
  - `sizeBytes` — uploaded file size.
  - `succeeded` — boolean outcome.
  - `error` — optional error message when `succeeded` is `false`.
  - `processingTimeMs` — measured processing duration.
  - `processedAtUtc` — UTC timestamp when processing completed.


## API Key authentication (X-API-KEY)

The project includes a simple API key middleware implemented in `Validators/ApiKeyValidator.cs`. The middleware expects a single API key configured under `Authentication:ApiKey` and enforces it for requests whose path starts with `/api`.

Behavior summary
- Header: `X-API-KEY`
- Configuration key: `Authentication:ApiKey`
- If the configured key is missing the middleware constructor will throw; the middleware returns HTTP 401 when the header is missing or invalid.
- Error payload:
- { "Error": "Unauthorized Access", "Message": "The provided API Key is invalid or missing from the headers." }
 
Add the key to configuration
- appsettings.json (example)
    { "Authentication": { "ApiKey": "your-strong-api-key-here" } }

## Using the endpoints from Swagger UI
1. Open `/swagger` in your browser.
2. If an API key security scheme is present use **Authorize**, or add header `X-API-KEY` when executing.
3. For `POST /api/UploadFile/upload`:
   - Click **Try it out**.
   - Choose the `file` on disk.
   - Paste `Filters` JSON object or array for JSON uploads.
   - Enter `Aggregate` for CSV uploads.
   - Click **Execute** and inspect the response.
   -