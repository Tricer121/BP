using backend.Models;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public virtual DbSet<StaticRegion> StaticRegions => Set<StaticRegion>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRoute> UserRoutes => Set<UserRoute>();
    public virtual DbSet<UserActivity> UserActivities => Set<UserActivity>();
    public virtual DbSet<RouteCoordinate> RouteCoordinates => Set<RouteCoordinate>();
    public virtual DbSet<CoordinateIndex> CoordinateIndexes => Set<CoordinateIndex>();
    public virtual DbSet<UserActivitiesCloseBy> ActivitiesCloseBy => Set<UserActivitiesCloseBy>();
    public virtual DbSet<UserActivityCloseBy> ActivityCloseBy => Set<UserActivityCloseBy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<UserActivity>()
            .HasOne(e => e.CenteredRoute)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserActivity>()
            .HasOne(e => e.RawRoute)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserActivity>()
            .HasOne(e => e.AveragedRoute)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}