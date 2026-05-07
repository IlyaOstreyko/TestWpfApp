using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class GroupDataModel
    {
        [Required, Key]
        public int? GroupId { get; set; }
        public string? NameGroup { get; set; }
        public List<SpecialityDataModel>? SpecialitysDataModel { get; set; } = new List<SpecialityDataModel>();
    }
}
