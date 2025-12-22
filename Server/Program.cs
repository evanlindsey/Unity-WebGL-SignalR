using Microsoft.AspNetCore.StaticFiles;
using SignalRServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Enforce HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Configure static file serving for Unity WebGL builds
// Unity compresses .unityweb files with Brotli at build time
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".unityweb"] = "application/octet-stream";

app.UseDefaultFiles(); // Serves index.html for root requests
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx =>
    {
        // Tell browsers that .unityweb files are Brotli-compressed
        // This allows browsers to decompress them automatically
        if (ctx.File.Name.EndsWith(".unityweb"))
        {
            ctx.Context.Response.Headers.ContentEncoding = "br";
        }
    }
});

// Map endpoints
app.MapHealthChecks("/health");
app.MapHub<MainHub>("/mainhub");

app.Run();
