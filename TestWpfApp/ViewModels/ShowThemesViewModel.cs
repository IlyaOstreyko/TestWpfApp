using AutoMapper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Data.Repositories;
using TestWpfApp.Interfaces;
using TestWpfApp.Models;
using TestWpfApp.Views;
using Xceed.Wpf.Toolkit;

namespace TestWpfApp.ViewModels
{
    public class ShowThemesViewModel : INotifyPropertyChanged
    {
        private readonly IDialogService _dialogService;
        private readonly IWindowService _windowService;
        private readonly IQuestionRepository _db; // Используем интерфейс
        private readonly IMapper _mapper;
        public OpenFileDialog openFileDialog;
        public List<TestQuestion> TestQuestions { get; set; }
        public List<TableTheme> TableThemes { get; set; }
        public int AllNumberQuestions { get; set; }
        public IEnumerable<string> IsChecked { get; set; }
        
        public ICommand ShowQuestionsCommand { get; }
        public ICommand StartTestCommand { get; }        
        public ICommand AddImageCommand { get; }
        public ICommand CloseWindowsCommand { get; }
        public int allNumberSelectedQuestions { get; set; }
        public int AllNumberSelectedQuestions
        {
            get { return allNumberSelectedQuestions; }
            set
            {
                allNumberSelectedQuestions = value;
                RaisePropertyChanged("AllNumberSelectedQuestions");
            }
        }        

        public ShowThemesViewModel(IQuestionRepository db, IDialogService dialogService, IWindowService windowService, IMapper mapper)
        {
            _db = db;
            _dialogService = dialogService;
            _windowService = windowService;
            _mapper = mapper;
            CloseWindowsCommand = new RelayCommand(CloseWindows);
            ShowQuestionsCommand = new RelayCommand(ShowQuestions);
            StartTestCommand = new RelayCommand(StartTest);
            //db = new QuestionRepository();
            
            var Themes = _db.GetThemes().ToList();
            //AllNumberQuestions = Themes;
            TableThemes = new List<TableTheme>(Themes.Count());
            for (int i = 0; i < Themes.Count(); i++)
            {
                var Theme = new TableTheme
                {
                    Theme = Themes[i],
                    Tag = false,
                    Number = 0,
                    AllNumber = _db.GetNumberQuestionInTheme(Themes[i]),
                    
                };
                AllNumberQuestions += Theme.AllNumber;
                //AllNumberSelectedQuestions += Theme.Number;
                TableThemes.Add(Theme);
            }
            foreach (var item in TableThemes)
            {
                item.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(TableTheme.Number))
                    {
                        UpdateTotalSum();
                    }
                };
            }
        }

        private void ShowQuestions(object obj)
        {
            //var selectedTheme = TableThemes.Where(n => n.Tag == true).ToList();
            var selectedTheme = TableThemes.Where(n => n.Number > 0).ToList();
            if (selectedTheme.Count == 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Выберите колличество вопросов.", "Ошибка заполнения формы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                TestQuestions = new List<TestQuestion>();
                List <Result> Results = new List<Result>();
                foreach (TableTheme a in selectedTheme)
                {
                    Result result = new Result();
                    result.Theme = a.Theme;
                    result.AllNumberQustions = a.AllNumber;
                    result.NumberQustions = a.Number;
                    result.NumberMistake = a.Number;
                    Results.Add(result);
                    var dataQuestions = _db.GetRndQuestionsInTheme(a.Theme, a.Number).ToList();
                    var questionsInTheme = _mapper.Map<IEnumerable<TestQuestion>>(dataQuestions);

                    //if (TestQuestions == null)
                    //{
                    //    TestQuestions = new List<TestQuestion>();
                    //}

                    TestQuestions.AddRange(questionsInTheme);

                }

                //ShowQuestion firstQuestion = new ShowQuestion(TestQuestions);
                User enterUser = new User(TestQuestions, Results);
                enterUser.ShowDialog();
            }
        }

        private void StartTest(object obj)
        {
            //var selectedTheme = TableThemes.Where(n => n.Tag == true).ToList();
            var selectedTheme = TableThemes.Where(n => n.Number > 0).ToList();
            if (selectedTheme.Count == 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Выберите колличество вопросов.", "Ошибка заполнения формы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                TestQuestions = new List<TestQuestion>();
                List<Result> Results = new List<Result>();
                foreach (TableTheme a in selectedTheme)
                {
                    Result result = new Result();
                    result.Theme = a.Theme;
                    result.AllNumberQustions = a.AllNumber;
                    result.NumberQustions = a.Number;
                    result.NumberMistake = a.Number;
                    Results.Add(result);
                    var dataQuestions = _db.GetRndQuestionsInTheme(a.Theme, a.Number).ToList();
                    //if (TestQuestions == null)
                    //{
                    //    TestQuestions = new List<TestQuestion>();
                    //}
                    var questionsInTheme = _mapper.Map<IEnumerable<TestQuestion>>(dataQuestions);
                    TestQuestions.AddRange(questionsInTheme);

                }

                //ShowQuestion firstQuestion = new ShowQuestion(TestQuestions);
                bool isTest = true;
                ShowQuestion showQuestions = new ShowQuestion(TestQuestions, isTest, Results);
                showQuestions.ShowDialog();
            }

        }
        private void UpdateTotalSum()
        {
            AllNumberSelectedQuestions = TableThemes.Sum(item => item.Number);
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
        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
