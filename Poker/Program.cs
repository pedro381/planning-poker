using Microsoft.AspNetCore.HttpOverrides;
using Poker.Components;
using Poker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<PlanningPokerService>();

// Trust the X-Forwarded-* headers from the reverse proxy (e.g. Render).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clear known networks/proxies so any upstream proxy is trusted.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure CORS to allow credentials from any origin by reflecting the
// request origin (required by browser CORS rules when credentials are included).
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Must be first so subsequent middleware sees the correct scheme/host.
app.UseForwardedHeaders();

var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    if (!pathBase.StartsWith('/'))
    {
        pathBase = $"/{pathBase}";
    }

    app.UsePathBase(pathBase);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Do not redirect to HTTPS here; TLS is terminated by the reverse proxy.

app.UseStaticFiles();
app.UseCors();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
