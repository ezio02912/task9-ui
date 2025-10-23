using BootstrapBlazor.Server.Components;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Services.CPD;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddHttpClient();
RequestClient.Initialize(builder.Configuration);

// Add CPD Services - Configure with API base URL
builder.Services.AddCpdServices(builder.Configuration["RemoteServices:BaseUrl"]);

builder.Services.AddBootstrapBlazorServerService();

var app = builder.Build();

var option = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (option != null)
{
    app.UseRequestLocalization(option.Value);
}

app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseUploaderStaticFiles();

app.UseAntiforgery();
app.UseBootstrapBlazor();

app.MapStaticAssets();

var cors = app.Configuration["AllowOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);
if (cors?.Length > 0)
{
    app.UseCors(options => options.WithOrigins()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
}

app.MapDefaultControllerRoute();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
