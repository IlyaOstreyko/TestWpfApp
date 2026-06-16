using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class Speciality
    {
        [Key]
        public int SpecialityId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        public ICollection<Theme> Themes { get; set; } = new List<Theme>();
        public ICollection<SpecialityThemeSetting> ThemeSettings { get; set; } =
            new List<SpecialityThemeSetting>();
    }
}
