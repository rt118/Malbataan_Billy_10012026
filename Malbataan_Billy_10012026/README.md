# Malbataan_Billy_10012026

API for uploading and processing employee data in JSON or CSV formats. Accepts multipart file uploads, delegates processing to pluggable services, and records processing metrics in-memory.

- Language: C# 14
- Target framework: .NET 10
- Project folder: `Malbataan_Billy_10012026`

## Overview

This Web API supports two primary flows:

- Upload a JSON file (array of objects). Provide `Filters` (JSON string/array) to select matching records.
- Upload a CSV file (header row required). Provide `Aggregate` to compute `avg|sum|min|max|count` on a numeric column.

Processing is performed by implementations of:
- `ICSVProcessorService`
- `IJSONProcessorService`

Processing metadata is tracked by:
- `IUploadTrackingService` (in-memory by default)

Controllers:
- `UploadFileController` — handles file uploads and delegates processing.
- `ReportController` — returns processing metrics and recent log entries.

## Quick start

Prerequisites:
- .NET 10 SDK
- Visual Studio 2026 or the `dotnet` CLI
- Optional: Docker

Run locally with dotnet CLI:
1. `dotnet build`
2. `dotnet run --project Malbataan_Billy_10012026`

Run from Visual Studio: open the solution and press F5 (or use __Debug > Start Debugging__).

If Swagger/OpenAPI is configured, browse to `/swagger` after the app starts.

## Endpoints

### POST `/api/UploadFile/upload`
- Content type: `multipart/form-data`
- Form fields:
  - `file` (required): uploaded file (`.json` or `.csv`)
  - `Filters` (optional, JSON uploads): JSON array or object describing filter rules
  - `Aggregate` (optional, CSV uploads): `avg`, `sum`, `min`, `max`, `count` (default: `avg`)

Behavior:
- `.json` files: `Filters` is deserialized to `FilterRule` and processed by `IJSONProcessorService`.
- `.csv` files: `Aggregate` is passed to `ICSVProcessorService`.
- Requests are validated by `Filters/FileValidatorAttribute.cs` before processing.
- Processing results and metrics are recorded by `IUploadTrackingService`.

Successful responses return processor-specific JSON (CSVResponse or JSONResponse). On processing failures the API returns HTTP 500 with a payload like: