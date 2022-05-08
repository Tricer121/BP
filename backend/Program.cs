using backend;
using backend.Repositories;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(
        options => { options.DefaultScheme = "Cookies"; })
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "auth_cookie";
        options.Cookie.SameSite = SameSiteMode.None;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = redirectContext =>
            {
                redirectContext.HttpContext.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddHangfire(configuration =>
{
    configuration.UseSqlServerStorage(builder.Configuration["ConnectionStrings:AppDb"], new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true
        }).UseRecommendedSerializerSettings()
        .UseSimpleAssemblyNameTypeSerializer()
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
});
builder.Services.AddCors(policyBuilder =>
    policyBuilder.AddPolicy("AllowLocalhost", 
            x=> x.AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowCredentials()));
builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseSqlServer(builder.Configuration["ConnectionStrings:AppDb"]);
    x.UseLazyLoadingProxies();
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton(new HttpClient());
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = "server1";
    options.WorkerCount = 10;
});
builder.Services.AddSingleton(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict
});

app.UseRouting();
app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();
app.MapPost("/api/register", AuthRepository.Register)
    .AllowAnonymous();

app.MapPost("/api/logout", AuthRepository.Logout)
    .RequireAuthorization();

app.MapPost("/api/authorize", AuthRepository.Authorize)
    .AllowAnonymous();

app.MapGet("/user", UserRepository.UserBasicInfo)
    .RequireAuthorization();

app.MapGet("/user/activities", UserRepository.GetActivities)
    .RequireAuthorization();

app.MapGet("/user/activity/{id}", UserRepository.GetActivityById)
    .RequireAuthorization();

app.MapDelete("/user/activity/{id}", UserRepository.DeleteActivityById)
    .RequireAuthorization();

app.MapGet("/user/averagedactivities", UserRepository.GetAveragedActivities)
    .RequireAuthorization();

app.MapGet("/user/centeredactivities", UserRepository.GetCenteredActivities)
    .RequireAuthorization();

app.MapPost("/user/loadallactivities", UserRepository.LoadActivities)
    .RequireAuthorization();

app.MapDelete("/user/account", UserRepository.DeleteAccount)
    .RequireAuthorization();

app.MapPost("/user/loadnewactivities", UserRepository.LoadNewActivities)
    .RequireAuthorization();

app.MapPost("/user/reset", UserRepository.ResetAccount)
    .RequireAuthorization();
app.Run();