using LandingApp.Data;
using LandingApp.Helpers;
using LandingApp.Interfaces;
using LandingApp.Logging;
using LandingApp.Mapping;
using LandingApp.Repository;
using LandingApp.Repository.IRepository;
using LandingApp.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Channels;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

// Канал для живой трансляции логов
var logChannel = Channel.CreateUnbounded<string>();

builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});


// Настройка Serilog с консолью, Seq и стримингом логов
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .WriteTo.Sink(new ChannelLogSink(logChannel))
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<ITariffService, TariffService>();
builder.Services.AddScoped<IGoogleSheetService, GoogleSheetService>();
builder.Services.AddScoped<ILeadExportService, LeadExportService>();
builder.Services.AddHostedService<LeadExportWorker>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<ITariffRepository, TariffRepository>();
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("ru"), new CultureInfo("kk") };
    options.DefaultRequestCulture = new RequestCulture("ru");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new[]
    {
        new RouteDataRequestCultureProvider { RouteDataStringKey = "culture", UIRouteDataStringKey = "culture" }
    };
});

builder.Services.AddRateLimiter(options =>
{
    // Для чата
    options.AddFixedWindowLimiter("ChatLimiter", o =>
    {
        o.Window = TimeSpan.FromSeconds(10);
        o.PermitLimit = 5;
    });

    // Для формы
    options.AddFixedWindowLimiter("FormLimiter", o =>
    {
        o.Window = TimeSpan.FromSeconds(10);
        o.PermitLimit = 10;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://kazakhtelcom.kz", "https://localhost:7130")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/kk/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var isDevelopment = context.Request.Host.Host.Contains("localhost");

    // CSP для продакшена
    var cspProduction =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://www.googletagmanager.com https://www.google-analytics.com https://mc.yandex.ru https://unpkg.com https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https://www.google-analytics.com https://mc.yandex.ru https://cdn.jsdelivr.net; " +
        "connect-src 'self' https://www.google-analytics.com https://www.googletagmanager.com https://mc.yandex.ru; " +
        "frame-ancestors 'none';";

    // CSP для локалки (разрешаем BrowserLink и WebSockets)
    var cspDevelopment =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://www.googletagmanager.com https://www.google-analytics.com https://mc.yandex.ru https://unpkg.com https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https://www.google-analytics.com https://mc.yandex.ru https://cdn.jsdelivr.net; " +
        "connect-src 'self' ws: wss: http://localhost:* https://localhost:* https://www.google-analytics.com https://www.googletagmanager.com https://mc.yandex.ru; " +
        "frame-ancestors 'none';";

    context.Response.Headers[HeaderNames.XFrameOptions] = "DENY";
    context.Response.Headers[HeaderNames.ContentSecurityPolicy] = isDevelopment ? cspDevelopment : cspProduction;
    context.Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
    context.Response.Headers[HeaderNames.XXSSProtection] = "1; mode=block";

    await next();
});



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRequestLocalization();

app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    if (method != HttpMethods.Get &&
        method != HttpMethods.Post &&
        method != HttpMethods.Head &&
        method != HttpMethods.Options)
    {
        context.Response.StatusCode = 405;
        await context.Response.WriteAsync("Method Not Allowed");
    }
    else
    {
        await next();
    }
});

app.UseRouting();
app.UseAuthorization();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
// SSE поток логов

var failedAttempts = new ConcurrentDictionary<string, (int Count, DateTime LastAttempt)>();

app.MapGet("/logs/stream", async context =>
{
    const int MaxAttempts = 5;
    const int LockoutMinutes = 1;

    var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var password = context.Request.Query["password"];
    var storedPassword = builder.Configuration["Admin:LogStreamPassword"];

    // Проверка на блокировку IP
    if (failedAttempts.TryGetValue(remoteIp, out var attempt) && attempt.Count >= MaxAttempts)
    {
        var timeSinceLastAttempt = DateTime.UtcNow - attempt.LastAttempt;
        if (timeSinceLastAttempt < TimeSpan.FromMinutes(LockoutMinutes))
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Too many failed attempts. Try again later.");
            return;
        }
        else
        {
            failedAttempts.TryRemove(remoteIp, out _);
        }
    }

    // Проверка пароля
    if (password != storedPassword)
    {
        failedAttempts.AddOrUpdate(remoteIp,
            (1, DateTime.UtcNow),
            (ip, old) => (old.Count + 1, DateTime.UtcNow));

        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    // Пароль верный - сбрасываем счётчик ошибок
    failedAttempts.TryRemove(remoteIp, out _);

    context.Response.Headers.Add("Cache-Control", "no-cache");
    context.Response.Headers.Add("Content-Type", "text/event-stream");

    var token = context.RequestAborted;
    var reader = logChannel.Reader;

    try
    {
        while (!token.IsCancellationRequested)
        {
            while (await reader.WaitToReadAsync(token))
            {
                while (reader.TryRead(out var log))
                {
                    var message = $"data: {log}\n\n";
                    await context.Response.WriteAsync(message);
                    await context.Response.Body.FlushAsync();
                }
            }
            await Task.Delay(200, token);
        }
    }
    catch (OperationCanceledException)
    {
        // Клиент отключился
    }
});



app.MapControllerRoute(
    name: "localizedDefault",
    pattern: "{culture=ru}",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Console.WriteLine("Unhandled exception: " + args.ExceptionObject);
};
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error");
app.Run();
