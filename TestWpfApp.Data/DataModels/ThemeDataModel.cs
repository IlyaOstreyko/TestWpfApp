using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class ThemeDataModel
    {
        [Required, Key]
        public int? ThemeId { get; set; }
        public string? NameTheme { get; set; }
        public List<TestQuestionDataModel>? QuestionsDataModel { get; set; } = new List<TestQuestionDataModel>();
        public List<SpecialityDataModel>? SpecialitysDataModel { get; set; } = new List<SpecialityDataModel>();
        public List<GroupDataModel>? GroupsDataModel { get; set; } = new List<GroupDataModel>();
    }
}
