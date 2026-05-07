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
        public DbSet<TestQuestionDataModel> TestQuestionDataModels { get; set; }
        public DbSet<GroupDataModel> GroupDataModels { get; set; }
        public DbSet<SpecialityDataModel> SpecialityDataModels { get; set; }
        public DbSet<ThemeDataModel> ThemeDataModels { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
    : base(options)
        {
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