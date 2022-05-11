using System.Net;
using System.Web.Http;
using backend.Common;
using backend.Models;
using Backend.Models;
using Castle.Core.Internal;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace backend.Classes;

public class ProcessActivityClass
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public ProcessActivityClass(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }
    
    [AutomaticRetry(Attempts = 0)]
    public async Task RunDeleteActivity(long id, bool fullDelete)
    {
        using (var scope = _serviceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>())
        {
            var activity = dbContext.UserActivities.FirstOrDefault(x => x.Id == id);
            if (activity is null)
                return;
            activity.ActivityStatus = ActivityStatus.ToBeDeleted;
            await dbContext.SaveChangesAsync();
           
            if (!fullDelete)
            {
                var user = dbContext.Users.FirstOrDefault(x => x.Id == activity.User.Id);
                if (user != null)
                {
                    var activityGroup = user.ActivitiesCloseBy.FirstOrDefault(x => x.ActivityIds
                        .Exists(y => y.ActivityId == activity.Id));
                    if (activityGroup is null)
                        return;
                    activityGroup.ActivityIds.RemoveAll(x => x.ActivityId == activity.Id);
                    foreach (var activityHelpId in activityGroup.ActivityIds)
                    {
                        var activityHelp = dbContext.UserActivities.FirstOrDefault(x => x.Id == activityHelpId.ActivityId);
                        if (activityHelp is null)
                            continue;
                        activityHelp.ActivityStatus = ActivityStatus.Recalculate;
                        
                    }
                    await dbContext.SaveChangesAsync();
                    var routes = new List<UserRoute> { activity.RawRoute!, activity.CenteredRoute!, activity.AveragedRoute! };
                    var routeCordsToRemove = new List<RouteCoordinate>();
                    routeCordsToRemove.AddRange(activity.RawRoute!.RouteCoordinates);
                    routeCordsToRemove.AddRange(activity.AveragedRoute!.RouteCoordinates);
                    routeCordsToRemove.AddRange(activity.CenteredRoute!.RouteCoordinates);

                    dbContext.UserActivities.Remove(activity);
                    dbContext.UserRoutes.RemoveRange(routes);
                    dbContext.RouteCoordinates.RemoveRange(routeCordsToRemove);
            
                    await dbContext.SaveChangesAsync();

                    foreach (var activityHelpId in activityGroup.ActivityIds)
                    {
                        var activityHelp =
                            dbContext.UserActivities.FirstOrDefault(x => x.Id == activityHelpId.ActivityId);
                        if (activityHelp is null)
                            continue;
                        var helpRoutes = new List<UserRoute> { activityHelp.AveragedRoute! };
                        var helpRouteCordsToRemove = new List<RouteCoordinate>();
                        helpRouteCordsToRemove.AddRange(activityHelp.AveragedRoute!.RouteCoordinates);
                        dbContext.UserRoutes.RemoveRange(helpRoutes);
                        dbContext.RouteCoordinates.RemoveRange(helpRouteCordsToRemove);
                        await dbContext.SaveChangesAsync();
                        if (activityHelp.ActivityStatus == ActivityStatus.ToBeDeleted)
                            continue;
                        activityHelp.AveragedRoute = AverageRoute(activityHelp.RawRoute!.RouteCoordinates);
                        activityHelp.ActivityStatus = ActivityStatus.Averaged;
                        await dbContext.SaveChangesAsync();
                        AverageAllRoutes(user, activityHelp);
                        if (activityHelp.ActivityStatus == ActivityStatus.ToBeDeleted)
                            continue;
                        activityHelp.ActivityStatus = ActivityStatus.Finished;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task RunDeleteUser(long id)
    {
        using (var scope = _serviceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>())
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Id == id);
            if (user is null)
                return;
            dbContext.Users.Remove(user);
            var activities = user.Activities.ToList();
            user.ActivitiesCloseBy.Clear();
            await dbContext.SaveChangesAsync();
            foreach (var activity in activities) await RunDeleteActivity(activity.Id, true);
           
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task RunAveragingAll(long userId)
    {
        using (var scope = _serviceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>())
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user is null)
                return;
            var onlyAveraged = user.Activities.Count(x => x.ActivityStatus is ActivityStatus.Centered or ActivityStatus.Finished);
            var toBeDeleted = user.Activities.Count(x => x.ActivityStatus == ActivityStatus.ToBeDeleted);
            if (onlyAveraged + toBeDeleted == user.Activities.Count)
            {
                foreach (var activity in user.Activities)
                {
                    if (activity.ActivityStatus is ActivityStatus.Finished or ActivityStatus.FullyAveraged or ActivityStatus.ToBeDeleted)
                        continue;
                    AverageAllRoutes(user, activity);
                    if (activity.ActivityStatus == ActivityStatus.ToBeDeleted)
                        return;
                    if ( activity.ActivityStatus == ActivityStatus.Centered)
                        activity.ActivityStatus = ActivityStatus.Finished;
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }

    public async Task RunProcessing(long activityId, long userId, HttpClient httpClient)
    {
        var url = _configuration["Overpass:interpreter"];
        using (var scope = _serviceProvider.CreateScope())
        using (var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>())
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user is null)
                return;
            var activity = user.Activities.FirstOrDefault(x => x.Id == activityId);
            if (activity is null)
                return;
            if (activity.ActivityStatus == ActivityStatus.ToBeDeleted)
                return;
            activity.AveragedRoute = AverageRoute(activity.RawRoute!.RouteCoordinates);
            activity.ActivityStatus = ActivityStatus.Averaged;
            await dbContext.SaveChangesAsync();

            activity.CenteredRoute = await CenterRoute(activity.RawRoute!.RouteCoordinates.ToList()!, url!, httpClient);
            AssignRegions(dbContext, user, activity);

            if (activity.ActivityStatus == ActivityStatus.ToBeDeleted)
                return;
            if (activity.ActivityStatus == ActivityStatus.FullyAveraged)
                activity.ActivityStatus = ActivityStatus.Finished;
            else activity.ActivityStatus = ActivityStatus.Centered;
            await dbContext.SaveChangesAsync();
            
            
            
        }
    }

    private async Task<JObject> centerRequest(string url, HttpClient httpClient)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, url) { Version = new Version(2, 0) };
        var result = await httpClient.SendAsync(message);
        if (result.StatusCode != HttpStatusCode.OK)
            throw new HttpResponseException(result.StatusCode);
        var coordinatesRaw = await result.Content.ReadAsStringAsync();
        return JObject.Parse(coordinatesRaw);
    }

    private async Task<RouteCoordinate> GetCenteredCoordinate(double lat, double lon, string url, HttpClient httpClient)
    {
        var initCord = new RouteCoordinate(lat, lon, false);

        var lookDistance = 10;
        var finalurl = url + $"?data=[out:json];node(around:{lookDistance},{lat},{lon});out geom;";
        var coordRaw = await centerRequest(finalurl, httpClient);
        while (coordRaw["elements"].IsNullOrEmpty())
        {
            lookDistance += 50;
            finalurl = url + $"?data=[out:json];node(around:{lookDistance},{lat},{lon});out geom;";
            coordRaw = await centerRequest(finalurl, httpClient);
            if (lookDistance >= 200)
            {
                return initCord;
            }
        }
        double shortestDistance = lookDistance;
        var element0 = coordRaw["elements"]![0];
        var closestCoord = new RouteCoordinate(Convert.ToDouble(element0!["lat"]), Convert.ToDouble(element0["lon"]), true);
        foreach (var element in ((JArray)coordRaw["elements"]!)!)
        {
            var centeredCoord = new RouteCoordinate(Convert.ToDouble(element["lat"]),
                Convert.ToDouble(element["lon"]), true);
            var distance = centeredCoord.GetDistanceTo(initCord);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestCoord.Latitude = centeredCoord.Latitude;
                closestCoord.Longitude = centeredCoord.Longitude;
                closestCoord.Centered = true;
            }
        }
        return closestCoord;
    }

    private async Task<UserRoute> CenterRoute(List<RouteCoordinate?> route, string url, HttpClient httpClient)
    {
        var centeredRoute = new List<RouteCoordinate>();
        var batchSize = 20;
        var numberOfBatches = (int)Math.Ceiling((double)route.Count / batchSize);
        
        for (var i = 0; i < numberOfBatches; i++)
        {
            var originalCoords = route.Skip(i * batchSize).Take(batchSize);
            var tasks = originalCoords.Select(x => GetCenteredCoordinate(x!.Latitude, x!.Longitude, url, httpClient));
            centeredRoute.AddRange(await Task.WhenAll(tasks));
        }
        for (var i = 1; i < centeredRoute.Count;)
            if (Math.Abs(centeredRoute[i - 1].Latitude - centeredRoute[i].Latitude) < 0.0000000001 &&
                Math.Abs(centeredRoute[i - 1].Longitude - centeredRoute[i].Longitude) < 0.0000000001)
                centeredRoute.RemoveAt(i);
            else
                i++;
        return new UserRoute
        {
            RouteCoordinates = centeredRoute
        };
    }

    private void AssignRegions(AppDbContext dbContext, User user, UserActivity activity)
    {
        foreach (var coordinate in activity.CenteredRoute!.RouteCoordinates)
        {
            if (!coordinate.Centered)
                continue;
            var region = dbContext.StaticRegions.FirstOrDefault(x =>
                x.Index2 - 0.0000000001 <= coordinate.Latitude &&
                x.Index2 + 0.0000000001 >= coordinate.Latitude &&
                x.Index1 - 0.0000000001 <= coordinate.Longitude &&
                x.Index1 + 0.0000000001 >= coordinate.Longitude);

            if (region is not null)
                if (!activity.Regions.Contains(region))
                    activity.Regions.Add(region);
        }
    }

    private void AverageAllRoutes(User user, UserActivity activityToAverage)
    {
        foreach (var activity in user.Activities)
        {
            if (activity.ActivityStatus == ActivityStatus.ToBeDeleted || activity.Id == activityToAverage.Id)
                continue;
            var closeBy1 =
                user.ActivitiesCloseBy.FirstOrDefault(x => x.ActivityIds.Exists(x => x.ActivityId == activity.Id));
            var closeBy2 =
                user.ActivitiesCloseBy.FirstOrDefault(x =>
                    x.ActivityIds.Exists(x => x.ActivityId == activityToAverage.Id));
            bool influenced = false;
            if (activity.ActivityStatus >= ActivityStatus.Averaged)
                influenced = AverageTwoRoutes(activity.AveragedRoute!, activityToAverage.AveragedRoute!);
            if (influenced)
            {
                if (closeBy1 is null)
                {
                    if(closeBy2 is null)
                        user.ActivitiesCloseBy.Add(new UserActivitiesCloseBy(new List<long>
                                        { activity.Id, activityToAverage.Id }));
                    else
                    {
                        if(closeBy2.ActivityIds.All(x => x.ActivityId != activity.Id))
                            closeBy2.ActivityIds.Add(new UserActivityCloseBy(activity.Id));
                    }
                }
                else
                {
                    if (closeBy2 is null)
                    {
                        if (closeBy1.ActivityIds.All(x => x.ActivityId != activityToAverage.Id))
                            closeBy1.ActivityIds.Add(new UserActivityCloseBy(activityToAverage.Id));
                    }
                    else
                    {
                        if (closeBy1.Id != closeBy2.Id)
                        {
                            foreach (var id in closeBy2.ActivityIds)
                            {
                                if(closeBy1.ActivityIds.All(x => x.ActivityId != id.ActivityId))
                                    closeBy1.ActivityIds.Add(new UserActivityCloseBy(id.ActivityId));
                            }
                            user.ActivitiesCloseBy.Remove(closeBy2);
                        }
                    }
                        
                }
            }
            else
            {
                if (closeBy1 is null)
                    user.ActivitiesCloseBy.Add(new UserActivitiesCloseBy(new List<long> { activity.Id }));
                if (closeBy2 is null)
                    user.ActivitiesCloseBy.Add(new UserActivitiesCloseBy(new List<long> { activityToAverage.Id }));
            }
        }
    }

    private bool AverageTwoRoutes(UserRoute routeA, UserRoute routeB)
    {
        var indexFirstRoute = 0;
        var accuracy = 15;
        var usedIndexesInA = new List<int>();
        var usedIndexesInB = new List<int>();
        var influenced = false;
        while (indexFirstRoute < routeA.RouteCoordinates.Count)
        {
            var usedIndexesInNodeInB = new List<int>();
            var listSameLat = new List<double>();
            var listSameLon = new List<double>();
            var indexSecondRoute = 0;
            while (indexSecondRoute < routeB.RouteCoordinates.Count)
            {
                if (usedIndexesInA.Contains(indexFirstRoute))
                    break;
                if (!usedIndexesInB.Contains(indexSecondRoute))
                {
                    var distance =
                        routeA.RouteCoordinates[indexFirstRoute]!.GetDistanceTo(
                            routeB.RouteCoordinates[indexSecondRoute]);
                    if (distance <= accuracy)
                    {
                        influenced = true;
                        listSameLat.Add(routeB.RouteCoordinates[indexSecondRoute]!.Latitude);
                        listSameLon.Add(routeB.RouteCoordinates[indexSecondRoute]!.Longitude);
                        usedIndexesInB.Add(indexSecondRoute);
                        usedIndexesInB.AddRange(routeB.RouteCoordinates[indexSecondRoute]
                            .CoordinateIndexes.Select(x => x.Index));
                        usedIndexesInNodeInB.Add(indexSecondRoute);
                    }
                }

                if (indexSecondRoute == routeB.RouteCoordinates.Count - 1)
                {
                    listSameLat.Add(routeA.RouteCoordinates[indexFirstRoute]!.Latitude);
                    listSameLon.Add(routeA.RouteCoordinates[indexFirstRoute]!.Longitude);
                    var coord = new RouteCoordinate(listSameLat.Average(), listSameLon.Average(), false);
                    var coords = routeA.RouteCoordinates[indexFirstRoute];
                    if (coords is null)
                        continue;
                    routeA.RouteCoordinates[indexFirstRoute]!.Latitude = coord.Latitude;
                    routeA.RouteCoordinates[indexFirstRoute]!.Longitude = coord.Longitude;
                    foreach (var coordinate in coords.CoordinateIndexes)
                    {
                        routeA.RouteCoordinates[coordinate.Index]!.Latitude = coord.Latitude;
                        routeA.RouteCoordinates[coordinate.Index]!.Longitude = coord.Longitude;
                    }

                    foreach (var node in usedIndexesInNodeInB)
                    {
                        routeB.RouteCoordinates[node]!.Latitude = coord.Latitude;
                        routeB.RouteCoordinates[node]!.Longitude = coord.Longitude;
                        foreach (var coordinate in routeB.RouteCoordinates[node].CoordinateIndexes)
                        {
                            routeB.RouteCoordinates[coordinate.Index]!.Latitude = coord.Latitude;
                            routeB.RouteCoordinates[coordinate.Index]!.Longitude = coord.Longitude;
                        }
                    }

                    usedIndexesInA.Add(indexFirstRoute);
                    usedIndexesInA.AddRange(
                        routeA.RouteCoordinates[indexFirstRoute]!.CoordinateIndexes
                            .Select(x => x.Index));
                }

                indexSecondRoute++;
            }
            indexFirstRoute++;
        }
        return influenced;
    }

    private UserRoute AverageRoute(List<RouteCoordinate> route)
    {
        var indexFirstNode = 0;
        var accuracy = 15;
        var usedIndexes = new List<int>();
        var routeCopy = route.Select(x => new RouteCoordinate(x.Latitude, x.Longitude, false)).ToList();
        while (indexFirstNode < routeCopy.Count)
        {
            var usedIndexesInNode = new List<int>();
            var listSameLat = new List<double>();
            var listSameLon = new List<double>();
            var indexSecondNode = indexFirstNode + 1;
            while (indexSecondNode < routeCopy.Count)
            {
                if (usedIndexes.Contains(indexFirstNode))
                    break;
                if (!usedIndexes.Contains(indexSecondNode))
                {
                    var distance = routeCopy[indexFirstNode].GetDistanceTo(routeCopy[indexSecondNode]);
                    if (distance <= accuracy)
                    {
                        listSameLat.Add(routeCopy[indexSecondNode].Latitude);
                        listSameLon.Add(routeCopy[indexSecondNode].Longitude);
                        usedIndexes.Add(indexSecondNode);
                        usedIndexesInNode.Add(indexSecondNode);
                    }
                }

                if (indexSecondNode == routeCopy.Count - 1)
                {
                    listSameLat.Add(routeCopy[indexFirstNode].Latitude);
                    listSameLon.Add(routeCopy[indexFirstNode].Longitude);
                    var coord = new RouteCoordinate(listSameLat.Average(), listSameLon.Average(), false);

                    var indexes = new List<CoordinateIndex>();
                    foreach (var node in usedIndexesInNode)
                        indexes.Add(new CoordinateIndex
                        {
                            Index = node
                        });

                    coord.CoordinateIndexes.AddRange(indexes);
                    routeCopy[indexFirstNode] = coord;
                    foreach (var node in coord.CoordinateIndexes)
                    {
                        routeCopy[node.Index].Latitude = coord.Latitude;
                        routeCopy[node.Index].Longitude = coord.Longitude;
                    }

                    usedIndexes.Add(indexFirstNode);
                }

                indexSecondNode++;
            }

            indexFirstNode++;
        }

        return new UserRoute
        {
            RouteCoordinates = routeCopy
        };
    }
}