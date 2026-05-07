using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class SpecialityDataModel
    {
        [Required, Key]
        public int? SpecialityId { get; set; }
        public string? NameSpeciality { get; set; }
        public List<GroupDataModel>? GroupsDataModel { get; set; } = new List<GroupDataModel>();
        public List<ThemeDataModel>? ThemesDataModel { get; set; } = new List<ThemeDataModel>();
        public List<SettingsSpecialityDataModel>? SettingsSpecialitysDataModel { get; set; } = new List<SettingsSpecialityDataModel>();
        

    }
}
