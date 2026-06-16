using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using TestWpfApp.Models;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Interfaces;
using TestWpfApp.Views;
using TestWpfApp.Data.DataModels;
using TestWpfApp.Data.UnitOfWork;
using System.Text.RegularExpressions;

namespace TestWpfApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private readonly IWindowService _windowService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private bool _visibility = false;
        private bool _visibilityPass = false;
        private string _pass;
        private string _themeQuestion;
        private List<TestQuestionVM> _testQuestions;
        private List<Speciality> _specialities;
        private List<Data.DataModels.Group> _groups;
        private List<Speciality> _filteredSpecialities;
        private Data.DataModels.Group? _selectedGroup;
        private Speciality _selectedSpeciality;


        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        public ImageSource Image { get; private set; }
        public List<Speciality> Specialities
        {
            get => _specialities;
            set { _specialities = value; OnPropertyChanged(); }
        }

        public Speciality SelectedSpeciality
        {
            get => _selectedSpeciality;
            set { _selectedSpeciality = value; OnPropertyChanged(); }
        }
        public List<Data.DataModels.Group> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                OnPropertyChanged();
            }
        }

        public Data.DataModels.Group? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;

                OnPropertyChanged();

                FilterSpecialities();
            }
        }
        public List<Speciality> FilteredSpecialities
        {
            get => _filteredSpecialities;
            set
            {
                _filteredSpecialities = value;
                OnPropertyChanged();
            }
        }
        public bool Visibility
        {
            get => _visibility;
            set { _visibility = value; OnPropertyChanged(); }
        }

        public bool VisibilityPass
        {
            get => _visibilityPass;
            set { _visibilityPass = value; OnPropertyChanged(); }
        }

        public string Pass
        {
            get => _pass;
            set { _pass = value; OnPropertyChanged(); }
        }

        public string ThemeQuestion
        {
            get => _themeQuestion;
            set { _themeQuestion = value; OnPropertyChanged(); }
        }

        public List<TestQuestionVM> TestQuestions
        {
            get => _testQuestions;
            set { _testQuestions = value; OnPropertyChanged(); }
        }

        public TestQuestionVM FirstQuestion { get; set; }
        #endregion

        #region Commands
        public ICommand AdminCommand { get; }
        public ICommand CloseWindowsCommand { get; }
        public ICommand AdminPassCommand { get; }
        public ICommand SpecialitiesCommand { get; }
        public ICommand GroupsCommand { get; }        
        public ICommand AddCommand { get; }
        public ICommand ShowAllQuestionsCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand StartCommand { get; }
        #endregion

        public MainWindowViewModel(
            IDialogService dialogService,
            IWindowService windowService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IServiceProvider serviceProvider)
        {
            _dialogService = dialogService;
            _windowService = windowService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceProvider = serviceProvider;

            // Инициализация команд
            CloseWindowsCommand = new RelayCommand(CloseWindows);
            AdminCommand = new RelayCommand(Admin);
            AdminPassCommand = new RelayCommand(AdminPass);
            SpecialitiesCommand = new RelayCommand(OpenSpecialities);
            GroupsCommand = new RelayCommand(OpenGroups);            
            AddCommand = new RelayCommand(OpenAddQuestion);
            ShowAllQuestionsCommand = new RelayCommand(_ => OpenEditQuestions(" Просмотр вопросов ", false));
            EditCommand = new RelayCommand(_ => OpenEditQuestions(" Редактирование вопросов ", true));
            StartCommand = new RelayCommand(OpenThemes);
            LoadGroups();
            LoadSpecialities();
            FilterSpecialities();
        }
        private void LoadSpecialities()
        {
            try
            {
                Specialities = _unitOfWork.Specialities
                    .GetAll()
                    .OrderBy(s => s.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка загрузки специальностей:\n{ex.Message}", "Ошибка");
                Specialities = new List<Speciality>();
            }
        }
        private void LoadGroups()
        {
            try
            {
                Groups = _unitOfWork.Groups
                    .GetAll()
                    .OrderBy(g => g.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка загрузки групп:\n{ex.Message}", "Ошибка");
                Groups = new List<Data.DataModels.Group>();
            }
        }
        private void FilterSpecialities()
        {
            try
            {
                // Если группа НЕ выбрана
                if (SelectedGroup == null)
                {
                    FilteredSpecialities = Specialities
                        .Where(s =>
                            s.Groups == null ||
                            !s.Groups.Any())
                        .OrderBy(s => s.Name)
                        .ToList();

                    return;
                }

                // Если группа выбрана
                FilteredSpecialities = Specialities
                    .Where(s =>
                        s.Groups != null &&
                        s.Groups.Any(g =>
                            g.GroupId == SelectedGroup.GroupId))
                    .OrderBy(s => s.Name)
                    .ToList();

                // Если выбранная специальность
                // больше не входит в фильтр — сбрасываем
                if (SelectedSpeciality != null &&
                    !FilteredSpecialities.Any(s =>
                        s.SpecialityId == SelectedSpeciality.SpecialityId))
                {
                    SelectedSpeciality = null;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка фильтрации специальностей:\n{ex.Message}", "Ошибка");
            }
        }
        #region Command Methods

        private void OpenSpecialities(object obj)
        {
            var window = _serviceProvider.GetRequiredService<AdminDataWindow>();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            LoadGroups();
            LoadSpecialities();
            FilterSpecialities();
        }
        private void OpenGroups(object obj)
        {
            var window = _serviceProvider.GetRequiredService<AdminGroupsDataWindow>();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            LoadGroups();
            LoadSpecialities();
            FilterSpecialities();
        }

        private void OpenAddQuestion(object obj)
        {
            var window = _serviceProvider.GetRequiredService<AddQuestion>();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            LoadGroups();
            LoadSpecialities();
            FilterSpecialities();
        }

        private void OpenEditQuestions(string title, bool isEditMode)
        {
            if (SelectedSpeciality == null)
            {
                _dialogService.ShowError($"Сначала выберите специальность из списка!", "Внимание");
                return;
            }

            if (SelectedSpeciality?.SpecialityId == null) 
            {
                _dialogService.ShowError($"Не найден id специальности:\n{SelectedSpeciality.Name}", "Внимание");
                return;
            }            
            var id = SelectedSpeciality.SpecialityId;
            try
            { 
                var filteredThemes = _unitOfWork.Themes.GetThemesBySpeciality(id);
                if (filteredThemes.Count == 0)
                {
                    _dialogService.ShowError($"Вопросов не добавлено.", "Инфо");
                    return;
                }
                var window = _serviceProvider.GetRequiredService<EditQuestions>();
                window.Initialize(title, isEditMode, id);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка тем выбранной специальности:\n{ex.Message}", "Ошибка");
            }
        }

        private void OpenThemes(object obj)
        {
            // ПРОВЕРКА: выбрана ли специальность
            if (SelectedSpeciality == null)
            {
                _dialogService.ShowError($"Сначала выберите специальность из списка!", "Внимание");
                return;
            }
            // Передаем только те темы, которые относятся к SelectedSpeciality.Id
            // Предполагается, что в репозитории вопросов есть метод GetThemesBySpeciality
            if (SelectedSpeciality?.SpecialityId == null) return;
            var id = SelectedSpeciality.SpecialityId;
            var filteredThemes = _unitOfWork.Themes.GetThemesBySpeciality(id);
            var settings = _unitOfWork.SpecialityThemeSettings.GetBySpecialityId(id);
            // Логика перехода к началу теста
            var window = _serviceProvider.GetRequiredService<ShowThemes>();
            // Настраиваем контекст окна выбора тем
            if (window.DataContext is ShowThemesViewModel themesVm)
            {
                themesVm.Initialize(filteredThemes, settings, SelectedSpeciality.Name);
            }

            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
        private void Admin(object obj)
        {
            if (VisibilityPass)
            {
                VisibilityPass = false;
            }
            else
            {
                if (!Visibility)
                {
                    VisibilityPass = true;
                }
                else
                {
                    Visibility = false;
                    Pass = null;
                }
            }
        }

        private void AdminPass(object obj)
        {
            if (Pass == "1236547")
            {
                Visibility = true;
                VisibilityPass = false;
            }
            else
            {
                _dialogService.ShowError($"Неправильный пароль", "Ошибка");
            }
        }

        private void CloseWindows(object obj)
        {
            Application.Current.Shutdown();
        }

        #endregion

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}