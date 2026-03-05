using Microsoft.AspNetCore.StaticFiles;
using SignalRServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

// Allow cross-origin requests from Unity Editor during development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials());
    });
}

var app = builder.Build();

// Enforce HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors();
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
