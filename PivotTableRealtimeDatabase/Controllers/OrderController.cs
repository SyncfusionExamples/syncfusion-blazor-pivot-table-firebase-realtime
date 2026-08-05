using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace PivotTableRealtimeDatabase.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly HttpClient httpClient;
        private readonly string databaseUrl;

        public OrderController(IConfiguration configuration)
        {
            databaseUrl =
                configuration["FirebaseSettings:DatabaseUrl"]
                ?? throw new InvalidOperationException(
                    "Firebase Realtime Database URL is not configured.");

            httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<object> Post(
            [FromBody] DataManagerRequest request)
        {
            _ = request;

            string url = $"{databaseUrl}/Orders.json";

            string json =
                await httpClient.GetStringAsync(url);

            List<Order?>? firebaseData =
                JsonSerializer.Deserialize<List<Order?>>(json);

            List<Order> orders =
                firebaseData?
                    .Where(x => x != null)
                    .Cast<Order>()
                    .OrderBy(x => x.OrderID)
                    .ToList()
                ?? new();

            return new
            {
                result = orders,
                count = orders.Count
            };
        }

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(
            [FromBody] CRUDModel<Order> value)
        {
            if (value.Value is not Order order)
            {
                return BadRequest();
            }

            string url = $"{databaseUrl}/Orders.json";

            string json =
                await httpClient.GetStringAsync(url);

            List<Order?>? firebaseData =
                JsonSerializer.Deserialize<List<Order?>>(json);

            int nextOrderId =
                firebaseData?
                    .Where(x => x != null)
                    .Max(x => x!.OrderID ?? 0) + 1
                ?? 1;

            order.OrderID = nextOrderId;

            string orderJson =
                JsonSerializer.Serialize(order);

            await httpClient.PutAsync(
                $"{databaseUrl}/Orders/{nextOrderId}.json",
                new StringContent(
                    orderJson,
                    System.Text.Encoding.UTF8,
                    "application/json"));

            return Ok(order);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update(
            [FromBody] CRUDModel<Order> value)
        {
            if (value.Value is not Order order
                || !order.OrderID.HasValue)
            {
                return BadRequest();
            }

            string orderJson =
                JsonSerializer.Serialize(order);

            HttpResponseMessage response =
                await httpClient.PutAsync(
                    $"{databaseUrl}/Orders/{order.OrderID}.json",
                    new StringContent(
                        orderJson,
                        System.Text.Encoding.UTF8,
                        "application/json"));

            return response.IsSuccessStatusCode
                ? Ok(order)
                : BadRequest();
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(
            [FromBody] CRUDModel<Order> value)
        {
            if (!int.TryParse(
                value.Key?.ToString(),
                out int orderId))
            {
                return BadRequest();
            }

            HttpResponseMessage response =
                await httpClient.DeleteAsync(
                    $"{databaseUrl}/Orders/{orderId}.json");

            return response.IsSuccessStatusCode
                ? NoContent()
                : NotFound();
        }

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

        public class CRUDModel<T> where T : class
        {
            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("keyColumn")]
            public string? KeyColumn { get; set; }

            [JsonPropertyName("key")]
            public object? Key { get; set; }

            [JsonPropertyName("value")]
            public T? Value { get; set; }

            [JsonPropertyName("added")]
            public List<T>? Added { get; set; }

            [JsonPropertyName("changed")]
            public List<T>? Changed { get; set; }

            [JsonPropertyName("deleted")]
            public List<T>? Deleted { get; set; }

            [JsonPropertyName("params")]
            public IDictionary<string, object>? Params { get; set; }
        }
    }
}