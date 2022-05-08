using System.Net.Http.Headers;
using System.Security.Claims;
using backend.Models;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace backend.Repositories;

public class AuthRepository : BaseRepository
{
    public static IResult Authorize(AppDbContext dbContext, IConfiguration config)
    {
        var sUrl = config["Strava:sURL"] + config["Tokens:audience"] + "/auth" + config["Strava:register"];
        return Results.Ok(sUrl);
    }

    private static async Task<User> BuildAccessTokenRequest(HttpClient httpClient, IConfiguration config, User user)
    {
        var param = new Dictionary<string, string>
        {
            { "client_id", config["Strava:clientid"]! },
            { "client_secret", config["Strava:clientsecret"]! },
            { "grant_type", "refresh_token" },
            { "refresh_token", user.RefreshToken! }
        };
        var content = new FormUrlEncodedContent(param);
        var response2 = await httpClient.PostAsync(config["Strava:authPost"], content);
        var responsestring = await response2.Content.ReadAsStringAsync();
        var joResponse = JObject.Parse(responsestring);
        user.AccessToken = joResponse["access_token"]?.ToString();
        user.RefreshToken = joResponse["refresh_token"]?.ToString();
        var expires = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(joResponse["expires_at"])).DateTime;
        user.AccessExpiresDate = expires;
        return user;
    }

    private static async Task<JObject> BuildAccessTokenRequest(HttpClient httpClient, IConfiguration config,
        string code)
    {
        var param = new Dictionary<string, string>
        {
            { "client_id", config["Strava:clientid"]! },
            { "client_secret", config["Strava:clientsecret"]! },
            { "code", code },
            { "grant_type", "authorization_code" }
        };
        var content = new FormUrlEncodedContent(param);
        var response2 = await httpClient.PostAsync(config["Strava:authPost"], content);
        var responsestring = await response2.Content.ReadAsStringAsync();
        var joResponse = JObject.Parse(responsestring);
        return await Task.FromResult(joResponse);
    }

    public static async Task<IResult> Register(AppDbContext dbContext, HttpClient httpClient, HttpContext context,
        IConfiguration config, IBackgroundJobClient backgroundJobs,
        [FromQuery] string state, [FromQuery] string code, [FromQuery] string scope)
    {
        var joResponse = BuildAccessTokenRequest(httpClient, config, code).Result;
        var access = joResponse["access_token"]?.ToString();
        var refresh = joResponse["refresh_token"]?.ToString();
        var expires = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(joResponse["expires_at"])).DateTime;
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", access);
        var athleteResponse = await httpClient.GetStringAsync(new Uri("https://www.strava.com/api/v3/athlete"));
        joResponse = JObject.Parse(athleteResponse);
        var userInDb = dbContext.Users.FirstOrDefault(x => x.Id == joResponse["id"]!.ToObject<long>());
        if (userInDb != null)
        {
            userInDb.AccessToken = access;
            userInDb.RefreshToken = refresh;
            userInDb.AccessExpiresDate = expires;
            await dbContext.SaveChangesAsync();
            await SetCookie(userInDb, context);
            return Results.Ok();
        }

        //else
        var user = new User(Convert.ToInt64(joResponse["id"]), joResponse["firstname"] + " " + joResponse["lastname"],
            joResponse["username"]!.ToString(), access, refresh, expires);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        await SetCookie(user, context);
        return Results.Ok();
    }

    public static async Task<User?> GetUserRefreshTokens(AppDbContext dbContext, HttpClient httpClient,
        HttpContext context, IConfiguration config)
    {
        var user = GetLoggedInUser(dbContext, context);
        if (user is null)
            return user;
        if (user.AccessExpiresDate < DateTime.Now)
        {
            var user2 = BuildAccessTokenRequest(httpClient, config, user).Result;
            user.AccessToken = user2.AccessToken;
            user.RefreshToken = user2.RefreshToken;
            user.AccessExpiresDate = user2.AccessExpiresDate;
        }

        await SetCookie(user, context);
        await dbContext.SaveChangesAsync();
        return user;
    }

    [Authorize]
    public static IResult GetAll(AppDbContext dbContext)
    {
        return Results.Ok(dbContext.Users.ToList());
    }

    public static IResult Logout(AppDbContext dbContext, HttpContext context)
    {
        return Results.Ok(context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme));
    }

    private static async Task<IResult> SetCookie(User user, HttpContext context)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.Id.ToString()),
            new("access", user.AccessToken!)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            IsPersistent = true
        };

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
        return Results.Ok();
    }
}