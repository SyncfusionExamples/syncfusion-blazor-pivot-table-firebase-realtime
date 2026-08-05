using PivotTableRealtimeDatabase.Components;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Register the Syncfusion license before builder.Build().
// Syncfusion packages require a valid license or trial key.
// Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
//     "YOUR LICENSE KEY");

builder.Services.AddSyncfusionBlazor();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapControllers();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();