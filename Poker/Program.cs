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

// Configure CORS so that explicit origins are set on responses instead of a
// wildcard.  Some corporate SSL-inspection proxies (e.g. Zscaler) will pass
// an explicit Access-Control-Allow-Origin header through unchanged, whereas
// they replace a missing/wildcard ACAO with their own "*".  Setting an
// explicit origin + AllowCredentials() satisfies the browser's requirement
// that ACAO must not be "*" when credentials are included.
//
// Populate "AllowedOrigins" in appsettings.json (or via the ALLOWEDORIGINS__0
// environment variable on Render) with your production URL, e.g.:
//   "AllowedOrigins": [ "https://planning-poker-rqig.onrender.com" ]
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Fallback: allow any origin without credentials (safe default).
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
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
