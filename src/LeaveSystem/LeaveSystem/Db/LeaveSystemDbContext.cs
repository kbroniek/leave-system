using LeaveSystem.Api.Domains;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Db;
public class LeaveSystemDbContext : DbContext
{
    public LeaveSystemDbContext(DbContextOptions<LeaveSystemDbContext> options) : base(options) {}
    public DbSet<LeaveType>? LeaveTypes { get; set; }
    //public DbSet<Blog>? Blogs { get; set; }
    //public DbSet<Post>? Posts { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseNpgsql(Configuration.GetConnectionString("WebApiDatabase"));
}

//public class Blog
//{
//    public int BlogId { get; set; }
//    public string? Url { get; set; }

//    public List<Post>? Posts { get; set; }
//}

//public class Post
//{
//    public int PostId { get; set; }
//    public string? Title { get; set; }
//    public string? Content { get; set; }

//    public int BlogId { get; set; }
//    public Blog? Blog { get; set; }
//}
