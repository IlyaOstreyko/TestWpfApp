using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class SettingsSpecialityDataModel
    {
        [Required, Key]
        public int? SettingsSpecialityId { get; set; }
        public ThemeDataModel? ThemeDataModel { get; set; }
        public int? NumberQuestionsInTheme { get; set; }
    }
}
