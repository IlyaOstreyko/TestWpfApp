using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Microsoft.VisualBasic;
using TestWpfApp.Models;
using TestWpfApp.Data.Repositories;
using TestWpfApp.Views;
using TestWpfApp.Data.Interfaces;
using AutoMapper;
using TestWpfApp.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace TestWpfApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // Наша картинка
        public ImageSource Image { get; private set; }
        public bool Visibility { get; private set; } = false;
        public bool VisibilityPass { get; private set; } = false;
        public string Pass { get; set; }

        public TestQuestion FirstQuestion;
        public string ThemeQuestion { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string _filePath { get; private set; }

        public List<TestQuestion> TestQuestions { get; set; }      
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private readonly IWindowService _windowService;
        private readonly IQuestionRepository _db; // Используем интерфейс
        private readonly IMapper _mapper;
        //ApplicationContext db = new ApplicationContext();
        RelayCommand addCommand;
        RelayCommand editCommand;
        RelayCommand showAllQuestionsCommand;
        RelayCommand showFirstQuestion;
        RelayCommand start;

        public ICommand AdminCommand { get; }
        public ICommand CloseWindowsCommand { get; }
        public ICommand AdminPassCommand { get; }
        public ICommand EnterUserCommand { get; }
        public ICommand SpecialitiesCommand { get; }

        public MainWindowViewModel(IDialogService dialogService, IWindowService windowService, IQuestionRepository db, IMapper mapper, IServiceProvider serviceProvider)
        {
            _dialogService = dialogService;
            _windowService = windowService;
            _db = db;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            //if (ThemeQuestion == null)
            //{
            //    ThemeQuestion = "Выберите тему экзамена";
            //}
            //else
            //{
            //    ThemeQuestion = "2222";
            //}
            CloseWindowsCommand = new RelayCommand(CloseWindows);
            AdminCommand = new RelayCommand(Admin);
            AdminPassCommand = new RelayCommand(AdminPass);
            SpecialitiesCommand = new RelayCommand(Specialities);
            //var dataQuestions = _db.GetAll();
            //TestQuestions = _mapper.Map<IEnumerable<TestQuestion>>(dataQuestions).ToList();

        }
        private void Specialities(object obj)
        {
            var window = _serviceProvider.GetRequiredService<AdminDataWindow>();
            window.Owner = Application.Current.MainWindow; // Хороший тон
            window.ShowDialog();
        }
        private void Admin(object obj)
        {
            if (VisibilityPass == true)
            {
                VisibilityPass = false;
                RaisePropertyChanged("VisibilityPass");
            }
            else
            {
                if (Visibility == false)
                {
                    VisibilityPass = true;
                    RaisePropertyChanged("VisibilityPass");
                }
                else
                {
                    Visibility = false;
                    Pass = null;
                    RaisePropertyChanged("Visibility");
                    RaisePropertyChanged("Pass");
                }
            }


        }
        private void AdminPass(object obj)
        {
            if (Pass == "1236547")
            {
                Visibility = true;
                RaisePropertyChanged("Visibility");
                VisibilityPass = false;
                RaisePropertyChanged("VisibilityPass");
            }
            else
            {
                MessageBox.Show("Неправильный пароль.", "Ошибка заполнения формы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand((o) =>
                  {
                      AddQuestion addQuestion = new AddQuestion();
                      addQuestion.ShowDialog();
                  }));
            }
        }

        public RelayCommand ShowAllQuestionsCommand
        {
            get
            {
                return showAllQuestionsCommand ??
                  (showAllQuestionsCommand = new RelayCommand((o) =>
                  {
                      //db = new QuestionRepository();
                      var Themes = _db.GetThemes();
                      if (Themes.Count == 0)
                      {
                          MessageBox.Show("Вопросов не добавленно.", "Ошибка заполнения формы", MessageBoxButton.OK, MessageBoxImage.Information);
                      }
                      else
                      {
                          EditQuestions editQuestions = new EditQuestions(" Просмотр вопросов ", false);
                          editQuestions.ShowDialog();
                      }

                  }));
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return editCommand ??
                  (editCommand = new RelayCommand((o) =>
                  {
                      //db = new QuestionRepository();
                      var Themes = _db.GetThemes();
                      if (Themes.Count == 0)
                      {
                          MessageBox.Show("Вопросов не добавленно.", "Ошибка заполнения формы", MessageBoxButton.OK, MessageBoxImage.Information);
                      }
                      else
                      {
                          EditQuestions editQuestions = new EditQuestions(" Редактирование вопросов ", true);
                          editQuestions.ShowDialog();
                      }

                  }));
            }
        }

        public RelayCommand ShowFirstQuestion
        {
            get
            {
                return showFirstQuestion ??
                    (showFirstQuestion = new RelayCommand((o) =>
                    {
                        FirstQuestion = TestQuestions.Last();
                        ShowQuestion firstQuestion = new ShowQuestion(FirstQuestion);
                        firstQuestion.ShowDialog();
                    }));
            }
        }

        public RelayCommand Start
        {

            get
            {
                return start ??
                    (start = new RelayCommand((o) =>
                    {
                        //ShowThemes showThemesWindow = new ShowThemes();
                        var showThemesWindow = new ShowThemes
                        {
                            Owner = Application.Current.MainWindow,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        showThemesWindow.ShowDialog();
                    }));
            }
        }

        private void CloseWindows(object obj)
        {            
            foreach (Window window in Application.Current.Windows)
            {
                window.Close();
            }
        }

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
