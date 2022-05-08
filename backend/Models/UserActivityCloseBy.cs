namespace backend.Models;

public class UserActivityCloseBy
{
    public UserActivityCloseBy()
    {
    }

    public UserActivityCloseBy(long id)
    {
        ActivityId = id;
    }

    public long Id { get; set; }
    public long ActivityId { get; set; }
    public virtual UserActivitiesCloseBy UserActivitiesCloseBy { get; set; } = null!;
}