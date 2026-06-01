using Aufgabe3_RestfulApi.Model;
using Microsoft.EntityFrameworkCore;


namespace Aufgabe3_RestfulApi.Infrastructure {
    public class BlogsContext : DbContext
    {

        public BlogsContext(DbContextOptions<BlogsContext> options) : base(options)
        {
        }

        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<FeaturedPost> FeaturedPosts => Set<FeaturedPost>();
        public DbSet<Author> Authors => Set<Author>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Alternative solution: [Owned]
            modelBuilder.Entity<Author>().OwnsOne(x => x.Contact);
        }

       
    }
}
