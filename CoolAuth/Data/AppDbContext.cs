using CoolAuth.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoolAuth.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
    : DbContext(options)
{
    //public DbSet<User> Users { get; set; } = null!;
    /*public DbSet<RefreshSession> Sessions { get; set; } = null!;
    public DbSet<Friendship> Friendships { get; set; } = null!;
    public DbSet<UserSimplifiedDTO> UserSimplified { get; set; }= null!;
    public DbSet<Video> Videos { get; set; } = null!;
    public DbSet<Playlist> Playlists { get; set; } = null!;
    public DbSet<PlaylistVideo> PlaylistVideos { get; set; } = null!;*/
    
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("en_US.UTF-8");

        
        modelBuilder.Entity<Session>()
            .HasOne<User>()
            .WithMany()     
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
   
        
        
        base.OnModelCreating(modelBuilder);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));
    }
}