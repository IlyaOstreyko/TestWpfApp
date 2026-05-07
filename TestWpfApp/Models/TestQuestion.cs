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
    public class TestQuestion : INotifyPropertyChanged
    {
        [Required, Key]
        public int? QuestionId { get; set; }
        private string nameQuestion { get; set; }
        private string imageQuestion { get; set; }
        private string fullImageQuestion { get; set; }
        public int? SpecialityId { get; set; }
        private string nameSpeciality { get; set; }

        public int? ThemeId { get; set; }
        private string nameTheme { get; set; }
        private string nameArticle { get; set; }

        public int? AnswersId { get; set; }
        private string nameAnswerCorrect1 { get; set; }
        private string nameAnswerIncorrect1 { get; set; }
        private string nameAnswerIncorrect2 { get; set; }
        private string nameAnswerIncorrect3 { get; set; }
        private string nameAnswer { get; set; }
        private bool status { get; set; }
        private int numberInList { get; set; }


        public string NameQuestion
        {
            get { return nameQuestion; }
            set
            {
                nameQuestion = value;
                OnPropertyChanged("NameQuestion");
            }
        }

        public string NameArticle
        {
            get { return nameArticle; }
            set
            {
                nameArticle = value;
                OnPropertyChanged("NameArticle");
            }
        }
        public string ImageQuestion
        {
            get { return imageQuestion; }
            set
            {
                imageQuestion = value;
                OnPropertyChanged("ImageQuestion");
            }
        }

        public string FullImageQuestion
        {
            get { return fullImageQuestion; }
            set
            {
                fullImageQuestion = value;
                OnPropertyChanged("FullImageQuestion");
            }
        }

        public string NameSpeciality
        {
            get { return nameSpeciality; }
            set
            {
                nameSpeciality = value;
                OnPropertyChanged("NameSpeciality");
            }
        }
        public string NameTheme
        {
            get { return nameTheme; }
            set
            {
                nameTheme = value;
                OnPropertyChanged("NameTheme");
            }
        }

        public string NameAnswerIncorrect1
        {
            get { return nameAnswerIncorrect1; }
            set
            {
                nameAnswerIncorrect1 = value;
                OnPropertyChanged("NameAnswerIncorrect1");
            }
        }

        public string NameAnswerIncorrect2
        {
            get { return nameAnswerIncorrect2; }
            set
            {
                nameAnswerIncorrect2 = value;
                OnPropertyChanged("NameAnswerIncorrect2");
            }
        }

        public string NameAnswerIncorrect3
        {
            get { return nameAnswerIncorrect3; }
            set
            {
                nameAnswerIncorrect3 = value;
                OnPropertyChanged("NameAnswerIncorrect3");
            }
        }

        public string NameAnswerCorrect1
        {
            get { return nameAnswerCorrect1; }
            set
            {
                nameAnswerCorrect1 = value;
                OnPropertyChanged("NameAnswerCorrect1");
            }
        }

        public string NameAnswer
        {
            get { return nameAnswer; }
            set
            {
                nameAnswer = value;
                OnPropertyChanged("NameAnswer");
            }
        }
        public bool Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }
        public int NumberInList
        {
            get { return numberInList; }
            set
            {
                numberInList = value;
                OnPropertyChanged("NumberInList");
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
