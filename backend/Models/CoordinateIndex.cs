namespace backend.Models;

public class CoordinateIndex
{
    public virtual int Id { get; set; }
    public virtual int Index { get; set; }

    public virtual List<RouteCoordinate> RouteCoordinate { get; set; } = new();
}