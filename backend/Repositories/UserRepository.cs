using System.Diagnostics;
using System.Net.Http.Headers;
using backend.Classes;
using backend.Common;
using backend.Models;
using Backend.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace backend.Repositories;

public class UserRepository : BaseRepository
{
    public static IResult DeleteAccount(AppDbContext dbContext, HttpClient httpClient,
        HttpContext context,
        IConfiguration config, IBackgroundJobClient backgroundJobs, IServiceProvider serviceProvider)
    {
        var user = GetLoggedInUser(dbContext, context);
        if (user is null)
            return Results.NotFound();
        backgroundJobs.Enqueue(() =>
            new ProcessActivityClass(serviceProvider, config).RunDeleteUser(user.Id));
        return Results.Ok();
    }

    public static async Task<IResult> ResetAccount(AppDbContext dbContext, HttpClient httpClient,
        HttpContext context,
        IConfiguration config, IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
    {
        var user = await AuthRepository.GetUserRefreshTokens(dbContext, httpClient, context, config);
        if (user is null)
            return Results.NotFound();

        await LoadActivities(dbContext, httpClient, context, config, backgroundJobs, serviceProvider,recurringJobManager);
        user.LastUpdated = DateTime.Now;
        return Results.Ok();
    }

    public static IResult IsLoaded(AppDbContext dbContext,HttpContext context, IRecurringJobManager recurringJobManager)
    {
        var user = GetLoggedInUser(dbContext, context);
        if (user is null)
            return Results.NotFound();
        if (user.Activities.Count(x => x.ActivityStatus == ActivityStatus.Finished) == (user.Activities.Count -
                user.Activities.Count(x => x.ActivityStatus == ActivityStatus.ToBeDeleted)))
        {
            recurringJobManager.RemoveIfExists("averageall");
            return Results.Ok(true);
        }
        return Results.Ok(false);
    }
    public static async Task<IResult> LoadNewActivities(AppDbContext dbContext, HttpClient httpClient,
        HttpContext context,
        IConfiguration config, IBackgroundJobClient backgroundJobs, IServiceProvider serviceProvider, IRecurringJobManager recurringJobManager)
    {
        var user = await AuthRepository.GetUserRefreshTokens(dbContext, httpClient, context, config);
        if (user is null)
            return Results.NotFound();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", user.AccessToken);
        var t = user.LastUpdated - new DateTime(1970, 1, 1);
        string uri;
        if (t.TotalSeconds == 0)
            uri = "https://www.strava.com/api/v3/athlete/activities";
        else
        {
            var secondsSinceEpoch = (int)t.TotalSeconds - 43200;
            uri = $"https://www.strava.com/api/v3/athlete/activities?after={secondsSinceEpoch}";
        }

        var athleteResponse = await httpClient.GetStringAsync(uri);
        if (athleteResponse == "[]")
        {
            user.LastUpdated = DateTime.Now;
            return Results.Ok(0);
        }

        dynamic activities = JsonConvert.DeserializeObject(athleteResponse)!;
        var newActs = 0;
        foreach (var activityRaw in activities)
        {
            var activity = activityRaw as JObject;
            if (user.Activities.Exists(x => x.ActivityStatus != ActivityStatus.ToBeDeleted && x.StravaId == activity!["id"]!.ToObject<long>()))
                continue;
            newActs++;
            var userActivity = new UserActivity(activity!["id"]!.ToObject<long>(), activity["name"]!.ToString(),
                activity["distance"]!.ToObject<float>(),
                activity["total_elevation_gain"]!.ToObject<float>(),
                activity["elapsed_time"]!.ToObject<float>(),
                activity["max_speed"]!.ToObject<float>(), activity["average_speed"]!.ToObject<float>(),
                activity["start_date_local"]!.ToObject<DateTime>());
            userActivity = await LoadGpsData(httpClient, user, userActivity);

            user.Activities.Add(userActivity);
            await dbContext.SaveChangesAsync();
            if(userActivity.ActivityStatus == ActivityStatus.Fresh)
                backgroundJobs.Enqueue(() => 
                    new ProcessActivityClass(serviceProvider, config).RunProcessing(userActivity.Id, user.Id, httpClient));
        }
        recurringJobManager.AddOrUpdate("averageall",() => new ProcessActivityClass(serviceProvider,config).RunAveragingAll(user.Id),"0/30 * * * * ?");
        user.LastUpdated = DateTime.Now;
        await dbContext.SaveChangesAsync();
        return Results.Ok(newActs);
    }

    public static async Task<IResult> LoadActivities(AppDbContext dbContext, HttpClient httpClient, HttpContext context,
        IConfiguration config, IBackgroundJobClient backgroundJobs, IServiceProvider serviceProvider, IRecurringJobManager recurringJobManager)
    {
        var user = await AuthRepository.GetUserRefreshTokens(dbContext, httpClient, context, config);
        if (user is null)
            return Results.NotFound();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", user.AccessToken);
        var athleteResponse =
            await httpClient.GetStringAsync(new Uri("https://www.strava.com/api/v3/athlete/activities"));
        dynamic activities = JsonConvert.DeserializeObject(athleteResponse) ?? throw new InvalidOperationException();
        foreach (var activityRaw in activities)
        {
            var activity = activityRaw as JObject;
            if (user.Activities.Any(x => x.ActivityStatus != ActivityStatus.ToBeDeleted && x.StravaId == activity!["id"]!.ToObject<long>()))
                continue;

            var userActivity = new UserActivity(activity!["id"]!.ToObject<long>(), activity["name"]!.ToString(),
                activity["distance"]!.ToObject<float>(),
                activity["total_elevation_gain"]!.ToObject<float>(),
                activity["elapsed_time"]!.ToObject<float>(),
                activity["max_speed"]!.ToObject<float>(), activity["average_speed"]!.ToObject<float>(),
                activity["start_date_local"]!.ToObject<DateTime>());
            userActivity = await LoadGpsData(httpClient, user, userActivity); 
            user.Activities.Add(userActivity);
            await dbContext.SaveChangesAsync();
            
            if(userActivity.ActivityStatus == ActivityStatus.Fresh)
                backgroundJobs.Enqueue(() =>
                    new ProcessActivityClass(serviceProvider, config).RunProcessing(userActivity.Id, user.Id, httpClient));
        }
        user.LastUpdated = DateTime.Now;
        recurringJobManager.AddOrUpdate("averageall",() => new ProcessActivityClass(serviceProvider,config).RunAveragingAll(user.Id),"0/30 * * * * ?");


        await dbContext.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<UserActivity> LoadGpsData(HttpClient httpClient, User user,
        UserActivity activity)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", user.AccessToken);
        var activityResponse = await httpClient.GetStringAsync(
            new Uri($"https://www.strava.com/api/v3/activities/{activity.StravaId}/streams?keys=latlng&key_by_type="));

        dynamic streams = JsonConvert.DeserializeObject(activityResponse) ?? throw new InvalidOperationException();
        foreach (var streamRaw in streams)
        {
            var stream = streamRaw as JObject;

            if (stream!["type"]!.ToString() != "latlng")
                continue;
            var data = stream["data"]?.ToObject<JArray>();
            var route = new UserRoute();
            foreach (var item in data!)
                route.RouteCoordinates.Add(new RouteCoordinate(item[0]!.ToObject<double>(),
                    item[1]!.ToObject<double>(), false));
            activity.RawRoute = route;
        }

        return activity;
    }

    public static async Task<IResult> UserBasicInfo(AppDbContext dbContext, HttpClient httpClient, HttpContext context,
        IConfiguration config)
    {
        var user = await AuthRepository.GetUserRefreshTokens(dbContext, httpClient, context, config);
        if (user is null)
            return Results.NotFound();
        return Results.Ok(new UserBasicInfo
        {
            Name = user.Name
        });
    }

    public static IResult GetActivities(AppDbContext dbContext, HttpContext context, [FromQuery] int perPage)
    {
        var user = GetLoggedInUser(dbContext, context);
        if (user is null)
            return Results.NotFound();
        
        var model = new List<List<UserActivityBasic>>();
        if (user.Activities.Count(x=>x.ActivityStatus != ActivityStatus.ToBeDeleted) == 0)
            return Results.Ok(model);
        var pages = (int)Math.Ceiling((double)user.Activities.Count / perPage);
        for (var index = 0; index < user.Activities.Count; index += perPage)
        {
            if (index + perPage > user.Activities.Count)
                perPage = user.Activities.Count - index;
            var helpVar = user.Activities.GetRange(index, perPage);
            var helpList = new List<UserActivityBasic>();
            foreach (var activity in helpVar)
            {
                if (activity.ActivityStatus == ActivityStatus.ToBeDeleted)
                    continue;
                helpList.Add(
                    new UserActivityBasic(activity.Name, activity.Id, activity.ElapsedTime, activity.StartDate));
            }

            model.Add(helpList);
        }

        return Results.Ok(new UserActivitiesBasic(perPage, pages, model));
    }

    public static IResult GetActivityById(AppDbContext dbContext, HttpContext context, long id)
    {
        var user = GetLoggedInUser(dbContext, context);
        if (user is null)
            return Results.NotFound();
        var activity = user.Activities.FirstOrDefault(x => x.Id == id);
        if (activity is null)
            return Results.NotFound();
        var activityModel = new UserActivityDetailed(activity.Name, activity.Distance, activity.ElevationGain,
            activity.ElapsedTime, activity.MaxSpeed, activity.AverageSpeed, activity.StartDate,
            new UserRouteModel(activity.RawRoute!));
        return Results.Ok(activityModel);
    }

    public static IResult DeleteActivityById(AppDbContext dbContext, HttpContext context,
        IBackgroundJobClient backgroundJobs, IServiceProvider serviceProvider, IConfiguration config, long id)
    {
        var user = GetLoggedInUser(dbContext, context);
        if (user is null)
            return Results.NotFound();
        var activity = dbContext.UserActivities.FirstOrDefault(x => x.Id == id);
        if (activity is null)
            return Results.NotFound();;
        activity.ActivityStatus = ActivityStatus.ToBeDeleted;
        dbContext.SaveChanges();
        backgroundJobs.Enqueue(() =>
            new ProcessActivityClass(serviceProvider, config).RunDeleteActivity(id, false));

        return Results.Ok();
    }


    public static IResult GetAveragedActivities(AppDbContext dbContext, HttpContext context, IRecurringJobManager recurringJobManager)
    {
        var user = GetLoggedInUser(dbContext, context);
        if (user is null)
            return Results.NotFound();
        var activityModel = new List<UserActivityProcessed>();
        if (user.Activities.Count(x=>x.ActivityStatus != ActivityStatus.ToBeDeleted) == 0)
            return Results.Ok(activityModel);
        var averagedCount = user.Activities.Count(x => x.ActivityStatus is ActivityStatus.FullyAveraged or ActivityStatus.Finished);
        var normalAveraged = user.Activities.Count(x => x.ActivityStatus == ActivityStatus.Averaged);
        var centered = user.Activities.Count(x => x.ActivityStatus == ActivityStatus.Centered)*2;
        
        var toBeDeleted = user.Activities.Count(x => x.ActivityStatus == ActivityStatus.ToBeDeleted);
        var count = averagedCount*3+centered*2+normalAveraged-toBeDeleted*3;
        var completePercent = Convert.ToInt32(Math.Round((double)(count) / (user.Activities.Count+count), 2)*100);
        if (averagedCount != user.Activities.Count-toBeDeleted) return Results.Accepted(null, $"{completePercent}");
        foreach (var activity in user.Activities!)
        {
            if (activity.ActivityStatus == ActivityStatus.ToBeDeleted)
                continue;
            activityModel.Add(new UserActivityProcessed(activity.Id, activity.Name, activity.StartDate,
                new UserRouteModel(activity.AveragedRoute!)));
            
        }
        return Results.Ok(activityModel);
    }

    public static IResult GetCenteredActivities(AppDbContext dbContext, HttpContext context,IRecurringJobManager recurringJobManager, [FromQuery] bool centered)
    {
        var user = GetLoggedInUser(dbContext, context);
        if (user is null)
            return Results.NotFound();
        var activityModel = new List<UserActivityProcessed>();
        if (user.Activities.Count(x=>x.ActivityStatus != ActivityStatus.ToBeDeleted) == 0)
            return Results.Ok(activityModel);
        var centeredCount = user.Activities.Count(x => x.ActivityStatus >= ActivityStatus.Centered);
        var justAveraged = user.Activities.Count(x => x.ActivityStatus >= ActivityStatus.Averaged); 
        var toBeDeleted = user.Activities.Count(x => x.ActivityStatus == ActivityStatus.ToBeDeleted);
        var completePercent = Convert.ToInt32(Math.Round((double)(centeredCount+justAveraged-toBeDeleted) / (user.Activities.Count+justAveraged-toBeDeleted), 2) * 100);
       
        if (centeredCount != user.Activities.Count-toBeDeleted) return Results.Accepted(null, $"{completePercent}");
        var result = new List<List<List<double[]>>>();
        var regionIds = new List<long>();
        foreach (var activity in user.Activities!)
        {
            if (activity.ActivityStatus == ActivityStatus.ToBeDeleted)
                continue;
            activityModel.Add(new UserActivityProcessed(activity.Id, activity.Name, activity.StartDate,
                new UserRouteModel(centered? activity.CenteredRoute! :activity.RawRoute!)));

            foreach (var fullRegion in activity.Regions)
                if (fullRegion != null && !regionIds.Contains(fullRegion.Id))
                {
                    var region = fullRegion.Region.Replace("'", "").Split("],[").ToList();
                    var coordinateList = new List<double[]>();
                    var helpResult = new List<List<double[]>>();
                    region[0] = region[0].Replace("[", "");
                    region[^1] = region[^1].Replace("]\r", "");
                    foreach (var coordinate in region)
                    {
                        var split = coordinate.Split(",");
                        var latitude = Convert.ToDouble(split[0]);
                        var longtitude = Convert.ToDouble(split[1]);
                        coordinateList.Add(new[] { latitude, longtitude });
                    }

                    helpResult.Add(coordinateList);
                    result.Add(helpResult);
                    regionIds.Add(fullRegion.Id);
                }
        }

        return Results.Ok(new UserActivityWithRegion(activityModel, result));
    }
}