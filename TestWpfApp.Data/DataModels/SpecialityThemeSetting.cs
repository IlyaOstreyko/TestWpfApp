using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class SpecialityThemeSetting
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int SpecialityId { get; set; }
        public Speciality Speciality { get; set; } = null!;
        [Required]
        public int ThemeId { get; set; }
        public Theme Theme { get; set; } = null!;
        public int QuestionsCount { get; set; }
    }
}
