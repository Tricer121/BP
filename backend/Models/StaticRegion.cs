using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

[Index(nameof(Index1), nameof(Index2))]
public class StaticRegion
{
    public int Id { get; set; }
    public double Index1 { get; set; }
    public double Index2 { get; set; }

    public string Region { get; set; } = null!;
    public virtual List<UserActivity> UserActivities { get; set; } = new();
}