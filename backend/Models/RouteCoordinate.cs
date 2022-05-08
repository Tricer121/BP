namespace backend.Models;

public class RouteCoordinate
{
    public RouteCoordinate(double latitude, double longitude, bool centered)
    {
        Latitude = latitude;
        Longitude = longitude;
        Centered = centered;
    }

    public long Id { get; set; }

    public virtual UserRoute UserRoute { get; set; } = null!;

    public virtual List<CoordinateIndex> CoordinateIndexes { get; set; } = new();

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool Centered { get; set; }

    //https://www.movable-type.co.uk/scripts/latlong.html
    public double GetDistanceTo(RouteCoordinate other)
    {
        var R = 6371e3;
        var num1 = Latitude * (Math.PI / 180.0);
        var num2 = other.Latitude * (Math.PI / 180.0);
        var d1 = (Latitude-other.Latitude) * (Math.PI / 180.0);
        var d2 = (Longitude-other.Longitude) * (Math.PI / 180.0);
        var a = Math.Sin(d1 / 2) * Math.Sin(d1 / 2) +
                Math.Cos(num1) * Math.Cos(num2) *
                Math.Sin(d2 / 2) * Math.Sin(d2 / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}