using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Models
{
    public class Answers
    {

        //private string nameAnswerCorrect1 { get; set; }
        private string nameAnswer1 { get; set; }
        private string nameAnswer2 { get; set; }
        private string nameAnswer3 { get; set; }
        private string nameAnswer4 { get; set; }

        //public string NameAnswerCorrect1
        //{
        //    get { return nameAnswerCorrect1; }
        //    set
        //    {
        //        nameAnswerCorrect1 = value;
        //        OnPropertyChanged("NameAnswerCorrect1");
        //    }
        //}

        public string NameAnswer1
        {
            get { return nameAnswer1; }
            set
            {
                nameAnswer1 = value;
                OnPropertyChanged("NameAnswer1");
            }
        }

        public string NameAnswer2
        {
            get { return nameAnswer2; }
            set
            {
                nameAnswer2 = value;
                OnPropertyChanged("NameAnswer2");
            }
        }

        public string NameAnswer3
        {
            get { return nameAnswer3; }
            set
            {
                nameAnswer3 = value;
                OnPropertyChanged("NameAnswer3");
            }
        }

        public string NameAnswer4
        {
            get { return nameAnswer4; }
            set
            {
                nameAnswer4 = value;
                OnPropertyChanged("NameAnswer4");
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
