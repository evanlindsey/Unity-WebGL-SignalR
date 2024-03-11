using Microsoft.AspNetCore.StaticFiles;
using SignalRServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();

var app = builder.Build();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings.Add(".unityweb", "application/octet-stream");
app.UseFileServer(new FileServerOptions
{
    StaticFileOptions = { ContentTypeProvider = provider },
    EnableDirectoryBrowsing = true,
});

app.MapHub<MainHub>("/mainhub");

app.Run();
