namespace backend.Models;

public class UserActivitiesCloseBy
{
    public UserActivitiesCloseBy()
    {
    }

    public UserActivitiesCloseBy(List<long> ids)
    {
        foreach (var id in ids)
            ActivityIds.Add(new UserActivityCloseBy(id));
    }

    public virtual long Id { get; set; }
    public virtual List<UserActivityCloseBy> ActivityIds { get; set; } = new();
}