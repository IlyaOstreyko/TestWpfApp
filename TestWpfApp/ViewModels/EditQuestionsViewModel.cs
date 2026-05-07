using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Data.Repositories;
using TestWpfApp.Interfaces;
using TestWpfApp.Models;
using TestWpfApp.Views;
using Xceed.Wpf.AvalonDock.Themes;

namespace TestWpfApp.ViewModels
{
    public class EditQuestionsViewModel : INotifyPropertyChanged
    {
        public System.Collections.ObjectModel.ObservableCollection<TestQuestion> testQuestions;
        public System.Collections.ObjectModel.ObservableCollection<TestQuestion> TestQuestions
        {
            get { return testQuestions; }
            set
            {
                testQuestions = value;
                // add appropriate event raising pattern
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("TestQuestions"));
            }
        }
        public List<string> Themes { get; set; }
        public string SelectionTheme { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public int DeleteQuestionId { get; set; }
        //public int HightRow { get; set; } = 100;
        public bool VisibilityManager { get; set; }
        public ICommand ShowQuestionsCommand { get; }
        public ICommand ShowCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        private readonly IDialogService _dialogService;
        private readonly IWindowService _windowService;
        private readonly IQuestionRepository _db; // Используем интерфейс
        private readonly IMapper _mapper;
        public EditQuestionsViewModel(string title, bool editTrue, IQuestionRepository db, IDialogService dialogService,IWindowService windowService, IMapper mapper)
        {
            Title = title;
            VisibilityManager = editTrue;
            ShowQuestionsCommand = new RelayCommand(ShowQuestions);
            ShowCommand = new RelayCommand(Show);
            EditCommand = new RelayCommand(Edit);
            DeleteCommand = new RelayCommand(Delete);
            _db = db;
            _dialogService = dialogService;
            _windowService = windowService;
            _mapper = mapper;
            Themes = db.GetThemes().ToList();
        }

        public void ShowQuestions(object obj)
        {
            //TestQuestions = db.GetQuestionsInTheme(SelectionTheme);
            //var TestQuestionsList = db.GetQuestionsInTheme(SelectionTheme);
            //HightRow = SelectionTheme.Count(x => x == '\n') * 50 + 100;
            var dataQuestions = _db.GetQuestionsInTheme(SelectionTheme);
            var viewQuestions = _mapper.Map<IEnumerable<TestQuestion>>(dataQuestions);
            //TestQuestions = new ObservableCollection<TestQuestion>();
            //foreach (var item in TestQuestionsList)
            //TestQuestions.Add(item);
            TestQuestions = new ObservableCollection<TestQuestion>(viewQuestions);
            RaisePropertyChanged("TestQuestions");
        }

        public void Show(object obj)
        {
            string theme;
            //TestQuestions = db.GetQuestionsInTheme(SelectionTheme);
            if (obj == null) { theme = ""; }
            else { theme = obj.ToString(); }
            var TestQuestionsList = _db.GetQuestionsInTheme(theme);
            var viewQuestions = _mapper.Map<IEnumerable<TestQuestion>>(TestQuestionsList);
            //HightRow = SelectionTheme.Count(x => x == '\n') * 50 + 100;
            TestQuestions = new ObservableCollection<TestQuestion>();
            string currentDir = Directory.GetCurrentDirectory();
            foreach (var item in viewQuestions)
            {
                TestQuestions.Add(item);
            }                
            RaisePropertyChanged("TestQuestions");
        }

        public void Edit(object obj)
        {
            if (obj is TestQuestion question)
            {
                AddQuestion firstQuestion = new AddQuestion(question);
                firstQuestion.ShowDialog();
                //TestQuestions = db.GetQuestionsInTheme(SelectionTheme);
                //var TestQuestionsList = db.GetQuestionsInTheme(SelectionTheme);
                //TestQuestions = new System.Collections.ObjectModel.ObservableCollection<TestQuestion>();
                //foreach (var item in TestQuestionsList)
                //estQuestions.Add(item);
                var TestQuestionsList = _db.GetQuestionsInTheme(question.NameTheme);
                var viewQuestions = _mapper.Map<IEnumerable<TestQuestion>>(TestQuestionsList);
                TestQuestions = new ObservableCollection<TestQuestion>();
                foreach (var item in viewQuestions)
                    TestQuestions.Add(item);
                RaisePropertyChanged("FullImageQuestion");
                RaisePropertyChanged("TestQuestions");
            }
        }

        public void Delete(object obj)
        {
            if (obj is TestQuestion question)
            {
                var Result = MessageBox.Show("Удалить вопрос?" + "\r\n" + "\r\n" + question.NameQuestion, "Удаление вопроса", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (Result == MessageBoxResult.Yes)
                {
                    _db.Delete((int)question.QuestionId);
                    //TestQuestions = db.GetQuestionsInTheme(SelectionTheme);
                    //var TestQuestionsList = db.GetQuestionsInTheme(SelectionTheme);
                    //TestQuestions = new System.Collections.ObjectModel.ObservableCollection<TestQuestion>();
                    //foreach (var item in TestQuestionsList)
                    //{
                    //    TestQuestions.Add(item);
                    //}
                    
                    //TestQuestions.Remove(TestQuestions.FirstOrDefault(u => u.QuestionId == (int)question.QuestionId));
                    TestQuestions.Remove(question);

                    if (TestQuestions.Count == 0)
                    {
                        MessageBox.Show("В данной теме больше нет вопросов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        //TestQuestions = null;
                        TestQuestions.Clear();
                        SelectionTheme = null;
                        Themes = _db.GetThemes().ToList();
                        RaisePropertyChanged("Themes");
                        RaisePropertyChanged("SelectionTheme");
                        return;
                    }
                    //RaisePropertyChanged("TestQuestions");
                    MessageBox.Show("Вы удалили вопрос:" + "\r\n" + question.NameQuestion, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (Result == MessageBoxResult.No)
                {
                    return;
                }

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
