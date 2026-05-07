using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class TestQuestionDataModel
    {
        [Required, Key]
        public int? QuestionId { get; set; }
        public int? ThemeId { get; set; }
        public ThemeDataModel? ThemeDataModel { get; set; }
        public string? NameTheme { get; set; }
        public string? NameSpeciality { get; set; }
        
        public string? ImageQuestion { get; set; }
        public byte[]? ImageQuestionBytes { get; set; }
        public string? NameQuestion { get; set; }
        public string? NameArticle { get; set; }
        public string? NameAnswerCorrect1 { get; set; }
        public string? NameAnswerIncorrect1 { get; set; }
        public string? NameAnswerIncorrect2 { get; set; }
        public string? NameAnswerIncorrect3 { get; set; }

    }
}
