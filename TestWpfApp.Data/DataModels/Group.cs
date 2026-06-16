using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.DataModels
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public ICollection<Speciality> Specialities { get; set; } = new List<Speciality>();
    }
}
