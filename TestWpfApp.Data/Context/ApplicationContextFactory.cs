using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace TestWpfApp.Data.Context
{
    public class ApplicationContextFactory
        : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseSqlite("Data Source=TestApp.db")
                .Options;

            return new ApplicationContext(options);
        }
    }
}
