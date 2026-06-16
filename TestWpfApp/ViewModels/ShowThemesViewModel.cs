using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TestWpfApp.Data.DataModels;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Data.Repositories;
using TestWpfApp.Interfaces;
using TestWpfApp.Models;
using TestWpfApp.Views;
using Xceed.Wpf.AvalonDock.Themes;
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
        public List<TestQuestionVM> TestQuestions { get; set; }
        public List<TableTheme> TableThemes { get; set; }
        private List<Data.DataModels.Theme> _themesDataModel;
        private string nameSpec;
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
        public List<Data.DataModels.Theme> ThemesDataModel
        {
            get => _themesDataModel;
            set
            {
                _themesDataModel = value;
                OnPropertyChanged();
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
        }
        public void Initialize(List<Data.DataModels.Theme> themes, List<SpecialityThemeSetting> settings, string speciality)
        {
            ThemesDataModel = themes ?? new List<Data.DataModels.Theme>();
            TableThemes = new List<TableTheme>(ThemesDataModel.Count());
            for (int i = 0; i < ThemesDataModel.Count(); i++)
            {
                var setting = settings.FirstOrDefault(x =>
            x.ThemeId == ThemesDataModel[i].ThemeId);
                int num = 0;
                if (setting != null) 
                {
                    num = setting.QuestionsCount;
                }
                var Theme = new TableTheme
                {
                    Theme = ThemesDataModel[i].Name,
                    Tag = false,
                    Number = num,
                    AllNumber = ThemesDataModel[i].Questions.Count(),
                };
                AllNumberQuestions += Theme.AllNumber;
                TableThemes.Add(Theme);
            }
            foreach (var item in TableThemes)
            {
                item.PropertyChanged += TableTheme_PropertyChanged;
            }
            nameSpec = speciality;
        }
        /// <summary>
        /// начать экзамен
        /// </summary>
        private void ShowQuestions(object obj)
        {
            var (questions, results) = PrepareTestData();
            if (questions == null) return;

            User enterUser = new User(questions, results, nameSpec);
            enterUser.Owner = System.Windows.Application.Current.MainWindow;
            enterUser.ShowDialog();
        }
        /// <summary>
        /// начать тестовую попытку
        /// </summary>
        private void StartTest(object obj)
        {
            var (questions, results) = PrepareTestData();
            if (questions == null) return;

            bool isTest = true;
            ShowQuestion showQuestions = new ShowQuestion(questions, isTest, results);
            showQuestions.Owner = System.Windows.Application.Current.MainWindow;
            showQuestions.ShowDialog();
        }
        private (List<TestQuestionVM> questions, List<Result> results) PrepareTestData()
        {
            var selectedThemes = TableThemes.Where(n => n.Number > 0).ToList();

            if (!selectedThemes.Any())
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Выберите количество вопросов.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null);
            }

            var allQuestions = new List<TestQuestionVM>();
            var allResults = new List<Result>();

            foreach (var tableTheme in selectedThemes)
            {
                // 1. Создаем объект результата для статистики
                allResults.Add(new Result
                {
                    Theme = tableTheme.Theme,
                    AllNumberQustions = tableTheme.AllNumber,
                    NumberQustions = tableTheme.Number,
                    NumberMistake = tableTheme.Number
                });
                var dataQuestions = GetRandomQuestionsByTheme(tableTheme.Theme, tableTheme.Number).ToList();

                // 3. Маппим во ViewModel-версию вопроса
                var mappedQuestions = _mapper.Map<IEnumerable<TestQuestionVM>>(dataQuestions);
                allQuestions.AddRange(mappedQuestions);
            }

            return (allQuestions, allResults);
        }
        public List<TestQuestion> GetRandomQuestionsByTheme(string themeName, int count)
        {
            foreach (var theme in ThemesDataModel)
            {
                if(theme.Name == themeName)
                {
                    var que = Shuffle<TestQuestion>(theme.Questions.ToList(), count);
                    return que;
                }
            }

            return null;
        }
        private Random rng = new Random();
        public List<T> Shuffle<T>(IList<T> list, int num)
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
            return list.Take(num).ToList();
        }

        private void CloseWindows(object obj)
        {
            foreach (Window window in Application.Current.Windows)
            {
                window.Close();
            }
        }
        private void UpdateTotalSum()
        {
            AllNumberSelectedQuestions = TableThemes.Sum(item => item.Number);
        }
        private void TableTheme_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TableTheme.Number))
            {
                UpdateTotalSum();
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
