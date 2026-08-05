# Blazor Pivot Table - Firebase Realtime Database Connection using ASP.NET Core

## Project Overview

This repository demonstrates how to bind Firebase Realtime Database data to the Syncfusion Blazor Pivot Table through an ASP.NET Core API and the URL Adaptor. The sample calls the Realtime Database REST API with `HttpClient` to load and persist documents under an `Orders` node, while the Pivot Table consumes the API through `SfDataManager`.

The server handles Realtime Database access, JSON serialization, and CRUD operations, keeping the Pivot Table focused on aggregation and editing.

### Key Features

- Firebase Realtime Database-backed data source for a lightweight JSON store sample
- Syncfusion Blazor Pivot Table with URL Adaptor remote binding
- CRUD operations exposed through API endpoints
- Same-origin relative URLs for read and write requests
- Primary key configuration for drill-through editing
- REST API access through `HttpClient` with `System.Text.Json` serialization

## Repository

Sample location: https://github.com/SyncfusionExamples/syncfusion-blazor-pivot-table-firebase-realtime

## Prerequisites

| Component | Version | Purpose |
|-----------|---------|---------|
| .NET SDK | 10.0 | Build and run the sample targeting net10.0 |
| Visual Studio 2026 / VS Code | 18.0+ / latest | Development environment with ASP.NET and web workloads |
| Firebase Project | Active project | Realtime Database host with a service account |
| Syncfusion.Blazor.PivotTable | 34.1.33 | Pivot Table UI component |
| Syncfusion.Blazor.Themes | 34.1.33 | Pivot Table styling |
| Newtonsoft.Json | 13.0.4 | JSON serialization for CRUD payloads |

## Firebase Realtime Database Setup

### 1. Create a Firebase Project

Create a new project in the [Firebase Console](https://console.firebase.google.com/) (for example, `pivottablerealtimedatabase`).

### 2. Create a Realtime Database

Navigate to **Build → Realtime Database** and click **Create Database**. Choose **Start in test mode** and select a region close to your users (for example, `asia-southeast1`).

### 3. Create the Orders node

Create a root node named:

```text
Orders
```

The Realtime Database stores data as a hierarchical JSON tree, so each order is stored as a child keyed by its `orderId`.

### 4. Import sample order data

Add the following sample data to the `Orders` node through the Firebase console **Import JSON** panel:

```json
{
  "Orders": {
    "1": {
      "orderId": 1,
      "customerName": "John Smith",
      "employeeId": 101,
      "freight": 32.5,
      "shipCity": "New York"
    },
    "2": {
      "orderId": 2,
      "customerName": "Toms",
      "employeeId": 102,
      "freight": 80.2,
      "shipCity": "London"
    },
    "3": {
      "orderId": 3,
      "customerName": "Sven",
      "employeeId": 101,
      "freight": 52.1,
      "shipCity": "Berlin"
    }
  }
}
```

The Realtime Database returns numeric keys as a JSON array, so the controller deserializes the response into a `List<Order?>` and filters null entries before returning the result to the Pivot Table.

### 5. Generate a Firebase Service Account key

Open **Project settings → Service accounts** and click **Generate new private key** to download a JSON credentials file.

### 6. Place the key file in the project

Rename the downloaded file to `serviceAccountKey.json` and place it at:

```text
Firebase/serviceAccountKey.json
```

## Service Account Key

> **Important:** Do not commit the Firebase service account key to source control. Generate your own key from the Firebase console and store it securely.

A dummy example is shown below. Replace the placeholder values with the details from your own Firebase project:

```json
{
  "type": "service_account",
  "project_id": "<YOUR_FIREBASE_PROJECT_ID>",
  "private_key_id": "<PRIVATE_KEY_ID>",
  "private_key": "-----BEGIN PRIVATE KEY-----\n<PRIVATE_KEY>\n-----END PRIVATE KEY-----\n",
  "client_email": "<SERVICE_ACCOUNT_EMAIL>",
  "client_id": "<CLIENT_ID>",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "<CLIENT_X509_CERT_URL>",
  "universe_domain": "googleapis.com"
}
```

> The `serviceAccountKey.json` file shown here is a placeholder for demonstration only. Before running the sample, generate your own key and replace the placeholder file.

## Project Structure

| File/Folder | Purpose |
|-------------|---------|
| /Controllers/OrderController.cs | API controller exposing read and CRUD endpoints using the Realtime Database REST API |
| /Components/Pages/Home.razor | Pivot Table page with URL Adaptor configuration, editing, and drill-through setup |
| /Program.cs | ASP.NET Core and Syncfusion service registration, Firebase credential setup, and middleware |
| /appsettings.json | Realtime Database URL configuration |
| /Firebase/serviceAccountKey.json | Firebase service account key used for authentication |
| /PivotTableRealtimeDatabase.csproj | Project file with Syncfusion packages and Newtonsoft.Json NuGet references |

## Application Flow

```text
Blazor Pivot Table
        ↓
   SfDataManager
        ↓
    UrlAdaptor
        ↓
  OrderController
        ↓
    HttpClient
        ↓
Firebase Realtime Database
        ↓
      Orders
```

## NuGet Packages

The Realtime Database sample uses the REST API directly through `HttpClient`, so no Firebase client SDK is required:

```xml
<PackageReference Include="Syncfusion.Blazor.PivotTable" Version="34.1.33" />
<PackageReference Include="Syncfusion.Blazor.Themes" Version="34.1.33" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
```

## Configuration

### appsettings.json

```json
{
  "FirebaseSettings": {
    "DatabaseUrl": "<YOUR_FIREBASE_REALTIME_DATABASE_URL>"
  }
}

```

### Program.cs

The service account key path is set as the `GOOGLE_APPLICATION_CREDENTIALS` environment variable so the Realtime Database REST API can authenticate the application:

```csharp
Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    Path.Combine(
        builder.Environment.ContentRootPath,
        "Firebase",
        "serviceAccountKey.json"));
```

## Order Model

The `Order` model maps .NET properties to Realtime Database JSON fields through `[JsonPropertyName]` attributes. The `OrderID` property carries `[Key]` so the Pivot Table can identify the primary key for update and delete requests:

```csharp
public class Order
{
    [Key]
    [JsonPropertyName("orderId")]
    public int? OrderID { get; set; }

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("employeeId")]
    public int? EmployeeID { get; set; }

    [JsonPropertyName("freight")]
    public double? Freight { get; set; }

    [JsonPropertyName("shipCity")]
    public string? ShipCity { get; set; }
}
```

## Realtime Database Data Structure

Each child of the `Orders` node follows the structure below. The numeric key is used by the controller to address orders for update and delete operations:

```json
{
  "Orders": {
    "1": {
      "orderId": 1,
      "customerName": "John Smith",
      "employeeId": 101,
      "freight": 32.5,
      "shipCity": "New York"
    }
  }
}
```

## CRUD Operations

The controller uses `HttpClient` to call the Realtime Database REST API and `System.Text.Json` to serialize and deserialize JSON payloads.

### Read

The `POST /api/Order` endpoint requests the `Orders` node through `HttpClient.GetStringAsync`, deserializes the response into a `List<Order?>`, filters null entries, orders by `OrderID`, and returns the `{ result, count }` format expected by the Pivot Table:

```csharp
string url = $"{databaseUrl}/Orders.json";
string json = await httpClient.GetStringAsync(url);

List<Order?>? firebaseData =
    JsonSerializer.Deserialize<List<Order?>>(json);

List<Order> orders =
    firebaseData?.Where(x => x != null)
        .Cast<Order>()
        .OrderBy(x => x.OrderID)
        .ToList() ?? new();
```

### Insert

The `POST /api/Order/Insert` endpoint calculates the next `orderId` from the maximum existing value and writes the order through `HttpClient.PutAsync` using the computed key:

```csharp
int nextOrderId =
    firebaseData?.Where(x => x != null)
        .Max(x => x!.OrderID ?? 0) + 1 ?? 1;

order.OrderID = nextOrderId;

await httpClient.PutAsync(
    $"{databaseUrl}/Orders/{nextOrderId}.json",
    new StringContent(
        JsonSerializer.Serialize(order),
        System.Text.Encoding.UTF8,
        "application/json"));
```

### Update

The `POST /api/Order/Update` endpoint serializes the order and overwrites the node at `Orders/{OrderID}.json` through `HttpClient.PutAsync`.

### Delete

The `POST /api/Order/Delete` endpoint removes the node at `Orders/{orderId}.json` through `HttpClient.DeleteAsync`:

```csharp
HttpResponseMessage response =
    await httpClient.DeleteAsync(
        $"{databaseUrl}/Orders/{orderId}.json");
```

## API Endpoints

| Endpoint | Method | Payload | Description |
|----------|--------|---------|-------------|
| /api/Order | POST | DataManagerRequest | Returns `{ result, count }` for Pivot Table binding |
| /api/Order/Insert | POST | CRUDModel<Order> | Inserts a new order and returns the created document |
| /api/Order/Update | POST | CRUDModel<Order> | Updates an existing order document |
| /api/Order/Delete | POST | CRUDModel<Order> | Deletes an order document using the key value |

```text
POST /api/Order
POST /api/Order/Insert
POST /api/Order/Update
POST /api/Order/Delete
```

## Home.razor

The Pivot Table uses same-origin relative URLs and marks `OrderID` as the primary key for drill-through editing:

```razor
<SfDataManager Url="/api/Order"
               InsertUrl="/api/Order/Insert"
               UpdateUrl="/api/Order/Update"
               RemoveUrl="/api/Order/Delete"
               Adaptor="Adaptors.UrlAdaptor">
</SfDataManager>
```

```csharp
private void BeginDrillThrough(BeginDrillThroughEventArgs args)
{
    for (int i = 0; i < args.GridObj.Columns.Count; i++)
    {
        if (args.GridObj.Columns[i].Field == "OrderID")
        {
            args.GridObj.Columns[i].IsPrimaryKey = true;
        }
        else
        {
            args.GridObj.Columns[i].Visible = true;
        }
    }
}
```

## Important Notes

- The service account JSON file is required for authentication with the Realtime Database.
- Do not commit the key file to source control. Add it to `.gitignore` and provide it locally.
- For development the database can run in **test mode**; secure the rules before deploying to production.

## Getting Started

1. Clone or open the repository in Visual Studio or VS Code.
2. Restore packages and build:

   ```bash
   dotnet restore
   dotnet build
   ```

3. Create a Firebase project, provision a Realtime Database, and add the `Orders` node as described above.
4. Generate a service account key and place it at `Firebase/serviceAccountKey.json`.
5. Update `FirebaseSettings:DatabaseUrl` in `appsettings.json` with your Realtime Database URL.
6. Run the application:

   ```bash
   dotnet run --project PivotTableRealtimeDatabase
   ```

7. Open the local URL shown in the terminal and verify the Pivot Table loads and CRUD endpoints respond.

## Troubleshooting

| Issue | Possible Cause | Resolution |
|-------|----------------|------------|
| Missing service account key | `serviceAccountKey.json` not placed in the project | Generate a new key from the Firebase console and copy it to `Firebase/serviceAccountKey.json` |
| Invalid `DatabaseUrl` | The URL in `appsettings.json` does not match the database | Copy the URL from **Build → Realtime Database** in the Firebase console |
| Connection failures | Network restrictions or invalid database URL | Verify the `DatabaseUrl` is reachable and outbound HTTPS is allowed |
| Permission issues | Security rules block read or write access | Update the database rules or test in development with **test mode** |
| `Orders` node not found | The node was not created | Create the `Orders` node and import the sample JSON before running |
| CRUD failures | The order does not exist or `orderId` could not be matched | Ensure orders are inserted with valid `orderId` values matching the numeric key |
| UrlAdaptor request failures | The Pivot Table could not reach the API | Confirm controller routes are mapped and the relative URLs in `Home.razor` match the actions |
| JSON deserialization issues | The `Orders` node uses an unexpected shape | Verify the node uses numeric keys and each child contains the required fields |

## Full Documentation

For the complete step-by-step guide, including URL Adaptor configuration, CRUD mappings, and Firebase Realtime Database connection setup, see the official Syncfusion Blazor Pivot Table documentation.

## Summary

This sample demonstrates how to bind Firebase Realtime Database data to the Syncfusion Blazor Pivot Table through an ASP.NET Core API and the URL Adaptor. The controller uses `HttpClient` to read, insert, update, and delete orders under the `Orders` node through the Realtime Database REST API, while the Pivot Table performs aggregation and editing on the client. The same-origin relative URLs and primary key configuration keep the remote binding simple and secure, and the `[JsonPropertyName]` attributes map the camel-cased Realtime Database fields to .NET properties for clean CRUD handling.
