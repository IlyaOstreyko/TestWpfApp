using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestWpfApp.Data.DataModels;

namespace TestWpfApp.Data.Context
{
    public class ApplicationContext : DbContext
    {
        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Speciality> Specialities { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<SpecialityThemeSetting> SpecialityThemeSettings { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
    : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Question -> Theme
            modelBuilder.Entity<TestQuestion>()
                .HasOne(q => q.Theme)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);

            // SpecialityThemeSetting -> Speciality
            modelBuilder.Entity<SpecialityThemeSetting>()
                .HasOne(s => s.Speciality)
                .WithMany(s => s.ThemeSettings)
                .HasForeignKey(s => s.SpecialityId);

            // Many-to-Many: Speciality <-> Theme
            modelBuilder.Entity<Speciality>()
                .HasMany(s => s.Themes)
                .WithMany(t => t.Specialities);

            // Many-to-Many: Group <-> Speciality
            modelBuilder.Entity<Group>()
                .HasMany(g => g.Specialities)
                .WithMany(s => s.Groups);
        }
    }
}

//public ApplicationContext(DbContextOptions<ApplicationContext> options)
//    : base(options)
//{
//    Database.EnsureCreated();
//}
//public ApplicationContext()
//{
//    Database.EnsureCreated();
//}

//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//{
//    optionsBuilder.UseSqlite("Data Source=TestApp.db");
//    //optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=NewTestQuestion1;Username=postgres;Password=7906");
//}