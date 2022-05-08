using System.Security.Claims;
using backend.Models;
using Microsoft.AspNetCore.Http;

namespace backend.Repositories;

public class BaseRepository
{
    protected static User? GetLoggedInUser(AppDbContext dbContext, HttpContext context)
    {
        var userSid = Convert.ToInt32(context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value);
        var userToken = context.User.Claims.FirstOrDefault(x => x.Type == "access")?.Value;
        var user = dbContext.Users.FirstOrDefault(x => x.Id == userSid && x.AccessToken == userToken);
        return user;
    }
}