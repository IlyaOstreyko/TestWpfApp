using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Models
{
    public class TableTheme : INotifyPropertyChanged
    {
        [Required, Key]
        private string theme { get; set; }
        private bool tag { get; set; }
        private int number { get; set; }
        private int allNumber { get; set; }

        public string Theme
        {
            get { return theme; }
            set
            {
                theme = value;
                OnPropertyChanged("Theme");
            }
        }

        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                OnPropertyChanged("Number");
            }
        }

        public int AllNumber
        {
            get { return allNumber; }
            set
            {
                allNumber = value;
                OnPropertyChanged("AllNumber");
            }
        }
        public bool Tag
        {
            get { return tag; }
            set
            {
                tag = value;
                OnPropertyChanged("Tag");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
