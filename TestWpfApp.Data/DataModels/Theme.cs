using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class Theme
    {
        [Key]
        public int ThemeId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public ICollection<TestQuestion> Questions { get; set; } =
            new List<TestQuestion>();
        public ICollection<Speciality> Specialities { get; set; } =
            new List<Speciality>();
        public ICollection<Group> Groups { get; set; } =
            new List<Group>();
    }
}
