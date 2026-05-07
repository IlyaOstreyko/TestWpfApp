using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using Window = System.Windows.Window;
using Application = System.Windows.Application;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using Microsoft.Win32;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Binding = System.Windows.Data.Binding;
using System.Windows.Threading;
using TestWpfApp.Models;
using TestWpfApp.Views;

namespace TestWpfApp.ViewModels
{
    public class ShowQuestionViewModel : INotifyPropertyChanged
    {
        public SaveFileDialog saveFileDialog;
        //public ICommand 
        public ICommand NextQuestionCommand { get; }
        public ICommand SkipQuestionCommand { get; }
        
        public ICommand CloseWindowsCommand { get; }
        public ImageSource Image { get; private set; }

        private TestQuestion testQuestion;
        private UserInfo UserInfo;
        private Answers answers;
        public bool VisibilityNameSpeciality { get; private set; } = false;
        public bool VisibilityTest { get; private set; }
        public bool VisibilityTime { get; private set; } = true;
        private bool answer1;
        private bool answer2;
        private bool answer3;
        private bool answer4;
        private int numberQuestion;
        private int mark;
        //private int numberAnswer;
        private int quantityQuestion;
        private string nameButton;
        private bool isRunning;
        private DispatcherTimer timer;
        private TimeSpan timeLeft;
        public string FormattedTimeLeft => TimeLeft.ToString(@"mm\:ss");
        public bool TimeOver = true;
        public TimeSpan TimeLeft
        {
            get => timeLeft;
            set
            {
                timeLeft = value;

                RaisePropertyChanged(nameof(TimeLeft));
                RaisePropertyChanged(nameof(FormattedTimeLeft));
            }
        }
        private List<Result> Results;
        public List<TestQuestion> TestQuestions { get; set; }

        public TestQuestion TestQuestion
        {
            get => testQuestion;
            set
            {
                testQuestion = value;

                RaisePropertyChanged(nameof(TestQuestion));
            }
        }

        public int QuantityQuestion
        {
            get => quantityQuestion;
            set
            {
                quantityQuestion = value;
                RaisePropertyChanged(nameof(QuantityQuestion));
            }
        }

        public int NumberQuestion
        {
            get => numberQuestion;
            set
            {
                numberQuestion = value;
                RaisePropertyChanged(nameof(NumberQuestion));
            }
        }

        public string NameButton
        {
            get => nameButton;
            set
            {
                nameButton = value;
                RaisePropertyChanged(nameof(NameButton));
            }
        }

        //public int NumberAnswer
        //{
        //    get => numberAnswer;
        //    set
        //    {
        //        numberAnswer = value;
        //        RaisePropertyChanged(nameof(NumberAnswer));
        //    }
        //}

        public bool Answer1
        {
            get => answer1;
            set
            {
                answer1 = value;
                RaisePropertyChanged(nameof(Answer1));
            }
        }


        public bool Answer2
        {
            get => answer2;
            set
            {
                answer2 = value;
                RaisePropertyChanged(nameof(Answer2));
            }
        }

        public bool Answer3
        {
            get => answer3;
            set
            {
                answer3 = value;
                RaisePropertyChanged(nameof(Answer3));
            }
        }
        public bool Answer4
        {
            get => answer4;
            set
            {
                answer4 = value;
                RaisePropertyChanged(nameof(Answer4));
            }
        }
        public Answers Answers
        {
            get => answers;
            set
            {
                answers = value;

                RaisePropertyChanged(nameof(Answers));
            }
        }

        public ShowQuestionViewModel(TestQuestion question)
        {
            CloseWindowsCommand = new RelayCommand(CloseWindows);
            NextQuestionCommand = new RelayCommand(NextQuestion);
            //SaveQuestionCommand = new RelayCommand(SaveQuestion);
            //AddImageCommand = new RelayCommand(AddImage);
            //Image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            question.ImageQuestion = "icon_2.jpg";
            //if (question.ImageQuestion == null)
            //{
            //    question.ImageQuestion = "icon_2.jpg";
            //}
            Image = new BitmapImage(new Uri("pack://application:,,,/" + question.ImageQuestion));
            RaisePropertyChanged("Image");
            TestQuestion = question;
        }

        public ShowQuestionViewModel(List<TestQuestion> questions, UserInfo userInfo, List<Result> results, bool isTest)
        {
            
            isRunning = true;
            VisibilityNameSpeciality = false;
            VisibilityTest = isTest;
            Results = results;
            CloseWindowsCommand = new RelayCommand(CloseWindows);
            QuantityQuestion = questions.Count();
            NumberQuestion = 1;
            mark = 0;
            NameButton = "Следующий вопрос";
            NextQuestionCommand = new RelayCommand(NextQuestion);
            SkipQuestionCommand = new RelayCommand(SkipQuestion);
            TestQuestions = questions;
            for (int i = 0; i < questions.Count; i++) 
            {
                TestQuestions[i].NumberInList = i + 1;
                TestQuestions[i].Status = false;
            }
            UserInfo = userInfo;
            TimeLeft = TimeSpan.FromSeconds(UserInfo.Time * 60); // Начальное время
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += TimerTick;
            timer.Start();

            TestQuestion = TestQuestions[NumberQuestion - 1];

            List<string> RndAnswers = new List<string>() { TestQuestion.NameAnswerCorrect1, TestQuestion.NameAnswerIncorrect1, TestQuestion.NameAnswerIncorrect2, TestQuestion.NameAnswerIncorrect3 };
            Shuffle(RndAnswers);

            Answers = new Answers()
            {
                NameAnswer1 = RndAnswers[0],
                NameAnswer2 = RndAnswers[1],
                NameAnswer3 = RndAnswers[2],
                NameAnswer4 = RndAnswers[3]
            };
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string currentDirectory = Directory.GetCurrentDirectory();

            string linkImage;
            if (TestQuestion.ImageQuestion == null)
            {
                linkImage = currentDirectory + @"\images\icon_2.jpg";
                //TestQuestion.ImageQuestion = currentDirectory + @"\images\icon_2.jpg";
            }
            else
            {
                linkImage = TestQuestion.FullImageQuestion;
                //TestQuestion.ImageQuestion = currentDirectory + @"\" + TestQuestion.ImageQuestion;
            }
            //Uri link = new Uri(TestQuestion.ImageQuestion);
            Uri link = new Uri(linkImage);
            Image = new BitmapImage(link);
            if (TestQuestion.NameSpeciality != null)
            {
                VisibilityNameSpeciality = true;
            }
            RaisePropertyChanged("VisibilityNameSpeciality");
            RaisePropertyChanged("Image");
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (VisibilityTest)
            {
                VisibilityTime = false;
                RaisePropertyChanged("VisibilityTime");
            }
            else
            {
                if (TimeLeft.TotalSeconds > 0)
                {
                    TimeLeft = TimeLeft.Subtract(TimeSpan.FromSeconds(1));
                }
                else
                {
                    timer.Stop();
                    isRunning = false;
                    TimeOver = false;
                    System.Windows.MessageBox.Show(
                        "Установленное время теста истекло.",
                        "Время вышло!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }

        }

        private void SkipQuestion(object obj)
        {
            LoadNextUnansweredQuestion(TestQuestion.NumberInList);
        }
        private void NextQuestion(object obj)
        {
            if (!(Answer1 || Answer2 || Answer3 || Answer4))
            {
                return;
            }
            var answers = new[]
            {
                new { Selected = Answer1, Name = Answers.NameAnswer1 },
                new { Selected = Answer2, Name = Answers.NameAnswer2 },
                new { Selected = Answer3, Name = Answers.NameAnswer3 },
                new { Selected = Answer4, Name = Answers.NameAnswer4 }
            };

            var selectedAnswer = answers.FirstOrDefault(a => a.Selected);

            if (selectedAnswer.Name == TestQuestion.NameAnswerCorrect1)
            {
                mark++;
                Results.First(n => n.Theme == TestQuestion.NameTheme).NumberMistake--;
            }
            else
            {
                TestQuestion.NameAnswer = selectedAnswer.Name;
            }

            TestQuestion.Status = true;

            if (TestQuestions.Count(q => q.Status) == TestQuestions.Count - 1)
            {
                NameButton = "Показать результат";
                RaisePropertyChanged(nameof(NameButton));
            }

            if (!LoadNextUnansweredQuestion(TestQuestion.NumberInList))
            {
                System.Windows.MessageBox.Show($"Вы ответили правильно на {mark} вопросов.", "Результат теста",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                Resaults resultsWindow = new Resaults(TestQuestions, UserInfo, Results, VisibilityTest, TimeOver);
                resultsWindow.ShowDialog();
            }
        }

        private bool LoadNextUnansweredQuestion(int startIndex)
        {
            if (!TestQuestions.Any(q => !q.Status))
                return false; // Все отвечены

            int nextIndex = startIndex;
            if (nextIndex >= TestQuestions.Count)
                nextIndex = 0;

            // Поиск следующего неотвеченного вопроса
            while (TestQuestions[nextIndex].Status)
            {
                nextIndex++;
                if (nextIndex >= TestQuestions.Count)
                    nextIndex = 0; // зацикливаем
            }

            // Устанавливаем текущий вопрос
            TestQuestion = TestQuestions[nextIndex];

            // Перемешиваем варианты
            List<string> rndAnswers = new List<string>()
            {
                TestQuestion.NameAnswerCorrect1,
                TestQuestion.NameAnswerIncorrect1,
                TestQuestion.NameAnswerIncorrect2,
                TestQuestion.NameAnswerIncorrect3
            };

            Shuffle(rndAnswers);

            Answers = new Answers()
            {
                NameAnswer1 = rndAnswers[0],
                NameAnswer2 = rndAnswers[1],
                NameAnswer3 = rndAnswers[2],
                NameAnswer4 = rndAnswers[3]
            };

            // Загружаем картинку
            LoadImageForQuestion(TestQuestion);

            // Сбрасываем ответы
            ResetAnswers();

            // Обновляем привязки
            RaisePropertyChanged(nameof(TestQuestion));
            RaisePropertyChanged(nameof(Image));
            RaisePropertyChanged(nameof(Answers));

            return true;
        }

        private void LoadImageForQuestion(TestQuestion question)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string currentDirectory = Directory.GetCurrentDirectory();
            string imagePath = string.IsNullOrEmpty(question.ImageQuestion)
                ? Path.Combine(currentDirectory, "images", "icon_2.jpg")
                : question.FullImageQuestion;

            try
            {
                Image = new BitmapImage(new Uri(imagePath));
            }
            catch
            {
                System.Windows.MessageBox.Show(
                    "Путь картинки вопроса некорректен",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ResetAnswers()
        {
            Answer1 = Answer2 = Answer3 = Answer4 = false;
            RaisePropertyChanged(nameof(Answer1));
            RaisePropertyChanged(nameof(Answer2));
            RaisePropertyChanged(nameof(Answer3));
            RaisePropertyChanged(nameof(Answer4));
        }

        public class RadioButtonCheckedConverter : IValueConverter

        {

            public object Convert(object value, Type targetType, object parameter,

                System.Globalization.CultureInfo culture)

            {

                return value.Equals(parameter);

            }



            public object ConvertBack(object value, Type targetType, object parameter,

                System.Globalization.CultureInfo culture)

            {

                return value.Equals(true) ? parameter : Binding.DoNothing;

            }

        }

        private Random rng = new Random();

        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        

        private void CloseWindows(object obj)
        {
            foreach (Window window in Application.Current.Windows)
            {
                window.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
