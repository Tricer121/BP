using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Common;
using backend.Models;

namespace Backend.Models;

public class UserActivity
{
    public UserActivity(long stravaId, string name, float distance, float elevationGain, float elapsedTime, float maxSpeed,
        float averageSpeed, DateTime startDate)
    {
        StravaId = stravaId;
        Name = name;
        Distance = distance;
        ElevationGain = elevationGain;
        ElapsedTime = elapsedTime;
        MaxSpeed = maxSpeed;
        AverageSpeed = averageSpeed;
        StartDate = startDate;
        ActivityStatus = ActivityStatus.Fresh;
    }


    public virtual UserRoute? CenteredRoute { get; set; }
    public int? CenteredRouteId { get; set; }

    public virtual UserRoute? AveragedRoute { get; set; }
    public int? AveragedRouteId { get; set; }

    public virtual UserRoute? RawRoute { get; set; }
    public int? RawRouteId { get; set; }

    [Key]
    [Required]
    public long Id { get; set; }
    public long StravaId { get; set; }

    [Required] [StringLength(50)] public string Name { get; init; }

    [Required] public float Distance { get; init; }

    [Required] public float ElevationGain { get; init; }

    [Required] public float ElapsedTime { get; init; }

    [Required] public float MaxSpeed { get; init; }

    [Required] public float AverageSpeed { get; init; }

    [Required] public DateTime StartDate { get; init; }

    [Required] public ActivityStatus ActivityStatus { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual List<StaticRegion?> Regions { get; set; } = new();
}

public record UserActivityBasic(string Name, long Id, float ElapsedTime, DateTime StartDate);

public record UserActivitiesBasic(int PerPage, int Pages, List<List<UserActivityBasic>> Activities);

public record UserActivityDetailed(string Name, float Distance, float ElevationGain, float ElapsedTime, float MaxSpeed,
    float AverageSpeed, DateTime StartDate, UserRouteModel RawRoute);

public record UserActivityProcessed(long Id, string Name, DateTime StartDate, UserRouteModel RawRoute);

public record UserActivityWithRegion(List<UserActivityProcessed> Routes, List<List<List<double[]>>> Regions);