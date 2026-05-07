using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TestWpfApp.MigrationOldQuestions
{
    public class LegacyContext : DbContext
    {
        public DbSet<OldQuestion> TestQuestionDataModels { get; set; }

        public LegacyContext(DbContextOptions<LegacyContext> options) : base(options) { }
    }
}
