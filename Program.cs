using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebApplication8.Data;
using WebApplication8.Models;
using WebApplication8.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();



builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UpdateUserRole", policy =>
        policy.RequireClaim("UpdateUserRole", "true"));

    options.AddPolicy("CreateAdmin", policy =>
        policy.RequireClaim("CreateAdmin", "true"));

    options.AddPolicy("DeleteAdmin", policy =>
        policy.RequireClaim("DeleteAdmin", "true"));

    options.AddPolicy("HandleEmployeeRequests", policy =>
        policy.RequireClaim("HandleEmployeeRequests", "true"));

    options.AddPolicy("CanViewAnalytics", policy =>
        policy.RequireClaim("ViewAnalytics", "true"));

    options.AddPolicy("EditMainPage", policy =>
        policy.RequireClaim("EditMainPage", "true"));

    options.AddPolicy("ManageEmployee", policy =>
        policy.RequireClaim("ManageEmployee", "true"));
});
builder.Services.AddScoped<ITokenService, TokenService>();


// Read Redis connection string from configuration
//var redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;

// Register Redis connection multiplexer as a singleton
//builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
//builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddRazorPages();

// Add rate limiting services
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();



app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Manage", StringComparison.OrdinalIgnoreCase) ||
        context.Request.Path.StartsWithSegments("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase) ||
         context.Request.Path.StartsWithSegments("/Identity/Account/ResendEmailConfirmation", StringComparison.OrdinalIgnoreCase)

        )
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("404 Not Found");
        return;
    }

    await next();
});
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.MapRazorPages();

app.Run();
