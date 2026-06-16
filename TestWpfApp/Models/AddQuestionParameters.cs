using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Models
{
    public class AddQuestionParameters
    {
        public TestQuestionVM? Question { get; set; }

        public string? SelectedTheme { get; set; }

        public string? SelectedSpeciality { get; set; }
    }
}
