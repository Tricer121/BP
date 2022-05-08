using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models;

namespace backend.Models;

public class User
{
    public User(long id, string name, string username, string? accessToken, string? refreshToken,
        DateTime? accessExpiresDate)
    {
        Id = id;
        Name = name;
        Username = username;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessExpiresDate = accessExpiresDate;
        LastUpdated = new DateTime(1970, 1, 1);
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Required]
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    [StringLength(50)] public string Username { get; set; } = null!;

    [StringLength(300)] public string? AccessToken { get; set; }

    [StringLength(300)] public string? RefreshToken { get; set; }

    public DateTime? AccessExpiresDate { get; set; }

    public DateTime LastUpdated { get; set; }


    public virtual List<UserActivity> Activities { get; set; } = new();

    public virtual List<UserActivitiesCloseBy> ActivitiesCloseBy { get; set; } = new();
}

public class UserBasicInfo
{
    public string Name { get; set; } = null!;
}