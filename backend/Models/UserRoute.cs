namespace backend.Models;

public class UserRoute
{
    public int Id { get; set; }
    public virtual List<RouteCoordinate> RouteCoordinates { get; set; } = new();
}

public class UserRouteModel
{
    public UserRouteModel(UserRoute routeRaw)
    {
        foreach (var coordinateRaw in routeRaw.RouteCoordinates)
            Coordinates.Add(new[] { coordinateRaw!.Longitude, coordinateRaw.Latitude });
    }

    public List<double[]> Coordinates { get; set; } = new();
}