using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using Microsoft.VisualBasic;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Application = System.Windows.Application;
using TestWpfApp.Models;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Data.Repositories;
using TestWpfApp.Service;
using TestWpfApp.Views;
using AutoMapper;
using TestWpfApp.Interfaces;
using TestWpfApp.Data.DataModels;
using System.Collections.ObjectModel;
using TestWpfApp.Data.UnitOfWork;

namespace TestWpfApp.ViewModels
{
    public class AddQuestionViewModel : INotifyPropertyChanged
    {
        private readonly IDialogService _dialogService;
        private readonly IWindowService _windowService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly MigrationOldTableService _migrationService;
        private readonly QuestionFileImporter _questionFileImporter;

        public ICommand SaveQuestionCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand DeleteImageCommand { get; }
        public ICommand AddQuestionFromFileCommand { get; }
        public ICommand FileCommand { get; }
        public ICommand AddNewThemeCommand { get; }
        public ICommand AddNewSpecialityCommand { get; }
        public ICommand MigrateOldDatabaseCommand { get; }
        public ICommand ClearSpecialityCommand { get; }

        private FileInfo? _fileInfo;

        public string Title { get; set; } = string.Empty;

        public bool VisibilityEdit { get; set; }

        public bool VisibilityAdd { get; set; }

        public bool VisibilityDelete { get; set; }

        public bool VisibilityImportButtons { get; set; }

        public bool ImageEdit { get; set; }

        public string TitleImageButton { get; set; }
            = " Добавить картинку ";

        public string? ImagePath { get; set; }

        private ImageSource? _imageSource;

        public ImageSource? ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                RaisePropertyChanged(nameof(ImageSource));
            }
        }

        private ObservableCollection<string> _themes = new();

        public ObservableCollection<string> Themes
        {
            get => _themes;
            set
            {
                _themes = value;
                RaisePropertyChanged(nameof(Themes));
            }
        }

        private string _selectionTheme = string.Empty;

        public string SelectionTheme
        {
            get => _selectionTheme;
            set
            {
                _selectionTheme = value;
                RaisePropertyChanged(nameof(SelectionTheme));

            }
        }
        private ObservableCollection<string> _specialities = new();

        public ObservableCollection<string> Specialities
        {
            get => _specialities;
            set
            {
                _specialities = value;
                RaisePropertyChanged(nameof(Specialities));
                
            }
        }

        private string _selectionSpeciality = string.Empty;

        public string SelectionSpeciality
        {
            get => _selectionSpeciality;
            set
            {
                _selectionSpeciality = value;
                RaisePropertyChanged(nameof(SelectionSpeciality));
                FilterThemes();
            }
        }
        private TestQuestionVM _testQuestion = new();

        public TestQuestionVM TestQuestion
        {
            get => _testQuestion;
            set
            {
                _testQuestion = value;
                RaisePropertyChanged(nameof(TestQuestion));
            }
        }

        private int _progressValue;

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                RaisePropertyChanged(nameof(ProgressValue));
            }
        }

        public AddQuestionViewModel(
            IDialogService dialogService,
            IWindowService windowService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            MigrationOldTableService migrationService,
            QuestionFileImporter questionFileImporter,
            AddQuestionParameters? parameters = null)
        {
            _dialogService = dialogService;
            _windowService = windowService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _migrationService = migrationService;
            _questionFileImporter = questionFileImporter;

            SaveQuestionCommand = new RelayCommand(SaveQuestion);
            AddImageCommand = new RelayCommand(AddImage);
            DeleteImageCommand = new RelayCommand(DeleteImage);
            AddQuestionFromFileCommand = new RelayCommand(AddQuestionFromFile);
            FileCommand = new RelayCommand(SampleFile);
            AddNewThemeCommand = new RelayCommand(AddNewTheme);
            ClearSpecialityCommand = new RelayCommand(_ => ClearSpeciality());
            //AddNewSpecialityCommand = new RelayCommand(AddNewSpeciality);

            MigrateOldDatabaseCommand =
                new RelayCommand(async _ => await MigrateOldDatabase());
            LoadSpecialities();
            LoadThemes();

            InitializeMode(parameters);
        }

        private void InitializeMode(
            AddQuestionParameters? parameters)
        {
            bool isEditMode =
                parameters?.Question != null &&
                !string.IsNullOrWhiteSpace(
                    parameters.Question.NameQuestion);

            if (!isEditMode)
            {
                InitializeCreateMode();
                return;
            }

            InitializeEditMode(parameters!);
        }

        private void InitializeCreateMode()
        {
            Title = " Добавление вопроса ";

            VisibilityAdd = true;

            VisibilityEdit = false;

            VisibilityDelete = false;

            VisibilityImportButtons = true;

            TestQuestion = new TestQuestionVM();

            RaiseAllProperties();
        }

        private void InitializeEditMode(
            AddQuestionParameters parameters)
        {
            Title = " Редактирование вопроса ";

            VisibilityAdd = false;

            VisibilityEdit = true;

            VisibilityImportButtons = false;

            TestQuestion = parameters.Question!;

            SelectionSpeciality =
                parameters.SelectedSpeciality;

            FilterThemes();

            SelectionTheme =
                parameters.SelectedTheme;

            VisibilityDelete =
                TestQuestion.ImageQuestionBytes?.Length > 0;

            LoadImageFromBytes();

            RaiseAllProperties();
        }
        private void ClearSpeciality()
        {
            SelectionSpeciality = null;
        }
        private void FilterThemes()
        {
            try
            {
                // специальность НЕ выбрана
                if (string.IsNullOrWhiteSpace(SelectionSpeciality))
                {
                    LoadThemes();
                }
                // специальность выбрана
                else
                {
                    List<string> themes;
                    themes = _unitOfWork.Themes
                        .GetAll()
                        .Where(x =>
                            x.Specialities.Any(s =>
                                s.Name == SelectionSpeciality))
                        .Select(x => x.Name)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
                    Themes = new ObservableCollection<string>(themes);
                }                
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Ошибка фильтрации тем:\n{ex.Message}",
                    "Ошибка");
            }
        }
        private void LoadSpecialities()
        {
            try 
            {
                List<Speciality> specialities = _unitOfWork.Specialities.GetAll().ToList();
                if (specialities.Count > 0)
                {
                    Specialities = new ObservableCollection<string>(
                        specialities.Select(s => s.Name));
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Ошибка загрузки специальностей: {ex.Message}",
                    "Ошибка");
                return;
            }
        }
        private void LoadThemes()
        {
            List<string> themes;
            try
            {                
                themes = _unitOfWork.Themes
    .GetAll()
    .Where(x => !x.Specialities.Any())
    .Select(x => x.Name)
    .Distinct()
    .OrderBy(x => x)
    .ToList();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Ошибка загрузки тем:\n{ex.Message}",
                    "Ошибка");
                return;
            }
            Themes = new ObservableCollection<string>(themes);
        }

        private void LoadImageFromBytes()
        {
            if (TestQuestion.ImageQuestionBytes == null ||
                TestQuestion.ImageQuestionBytes.Length == 0)
            {
                ImageSource = null;
                return;
            }

            using var ms =
                new MemoryStream(TestQuestion.ImageQuestionBytes);

            var bitmap = new BitmapImage();

            bitmap.BeginInit();

            bitmap.CacheOption = BitmapCacheOption.OnLoad;

            bitmap.StreamSource = ms;

            bitmap.EndInit();

            bitmap.Freeze();

            ImageSource = bitmap;

            TitleImageButton = " Изменить картинку ";
        }

        private async void AddQuestionFromFile(object obj)
        {
            try
            {
                _questionFileImporter.ProgressChanged += OnProgressChanged;

                _questionFileImporter.AddQuestionsFromFile();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    ex.Message,
                    "Ошибка импорта");
            }
            finally
            {
                _questionFileImporter.ProgressChanged -=
                    OnProgressChanged;
            }
        }

        private void OnProgressChanged(int progress)
        {
            ProgressValue = progress;
        }

        private void SaveQuestion(object obj)
        {
            if (!ValidateQuestion())
                return;

            try
            {
                var theme = _unitOfWork.Themes
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.Name == SelectionTheme);

                if (theme == null)
                {
                    _dialogService.ShowError(
                        "Тема не найдена.",
                        "Ошибка");

                    return;
                }

                // =====================================================
                // СОЗДАНИЕ
                // =====================================================

                if (VisibilityAdd)
                {
                    var newEntity =
                        _mapper.Map<Data.DataModels.TestQuestion>(
                            TestQuestion);

                    newEntity.ThemeId = theme.ThemeId;
                    newEntity.NameTheme = theme.Name;

                    if (_fileInfo != null)
                    {
                        newEntity.ImageQuestionBytes =
                            File.ReadAllBytes(_fileInfo.FullName);
                    }

                    bool exists =
                        _unitOfWork.Questions.CheckQuestions(newEntity);

                    if (exists)
                    {
                        _dialogService.ShowError(
                            "Такой вопрос уже существует.",
                            "Ошибка");

                        return;
                    }

                    _unitOfWork.Questions.Create(newEntity);

                    _unitOfWork.SaveAsync();

                    TestQuestion.QuestionId =
                        newEntity.QuestionId;
                }

                // =====================================================
                // РЕДАКТИРОВАНИЕ
                // =====================================================

                else
                {
                    // Получаем TRACKED entity
                    var entity =
                        _unitOfWork.Questions
                            .GetTracked((int)TestQuestion.QuestionId);

                    if (entity == null)
                    {
                        _dialogService.ShowError(
                            "Вопрос не найден.",
                            "Ошибка");

                        return;
                    }

                    // Обновляем поля вручную
                    entity.NameQuestion =
                        TestQuestion.NameQuestion;

                    entity.NameAnswerCorrect1 =
                        TestQuestion.NameAnswerCorrect1;

                    entity.NameAnswerIncorrect1 =
                        TestQuestion.NameAnswerIncorrect1;

                    entity.NameAnswerIncorrect2 =
                        TestQuestion.NameAnswerIncorrect2;

                    entity.NameAnswerIncorrect3 =
                        TestQuestion.NameAnswerIncorrect3;

                    entity.NameArticle =
                        TestQuestion.NameArticle;

                    entity.ThemeId =
                        theme.ThemeId;
                    entity.NameTheme = theme.Name;


                    if (ImageEdit)
                    {
                        if (_fileInfo != null)
                        {
                            entity.ImageQuestionBytes =
                                File.ReadAllBytes(_fileInfo.FullName);
                        }
                        else
                        {
                            entity.ImageQuestionBytes = null;
                        }
                    }

                    // Update НЕ НУЖЕН
                    // entity уже tracked

                    _unitOfWork.SaveAsync();
                }

                _dialogService.ShowMessage(
                    VisibilityAdd
                        ? "Вопрос успешно добавлен."
                        : "Вопрос успешно изменён.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Ошибка сохранения:\n{ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка");
            }
        }

        private bool ValidateQuestion()
        {
            if (string.IsNullOrWhiteSpace(TestQuestion.NameQuestion) ||
                string.IsNullOrWhiteSpace(SelectionTheme) ||
                string.IsNullOrWhiteSpace(TestQuestion.NameAnswerCorrect1) ||
                string.IsNullOrWhiteSpace(TestQuestion.NameAnswerIncorrect1) ||
                string.IsNullOrWhiteSpace(TestQuestion.NameAnswerIncorrect2) ||
                string.IsNullOrWhiteSpace(TestQuestion.NameAnswerIncorrect3))
            {
                _dialogService.ShowError(
                    "Заполните обязательные поля.",
                    "Ошибка");

                return false;
            }

            return true;
        }

        private void AddImage(object obj)
        {
            if (!_dialogService.OpenFileDialog("Выберите картинку", "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"))
                return;

            ImagePath = _dialogService.FilePath;

            _fileInfo = new FileInfo(ImagePath);

            TestQuestion.ImageQuestionBytes =
                File.ReadAllBytes(ImagePath);

            LoadImageFromBytes();

            VisibilityDelete = true;

            ImageEdit = true;

            RaisePropertyChanged(nameof(VisibilityDelete));
        }

        private void DeleteImage(object obj)
        {
            TestQuestion.ImageQuestionBytes = null;

            ImageSource = null;

            _fileInfo = null;

            VisibilityDelete = false;

            TitleImageButton = " Добавить картинку ";

            ImageEdit = true;

            RaisePropertyChanged(nameof(VisibilityDelete));

            RaisePropertyChanged(nameof(TitleImageButton));
        }

        private async Task MigrateOldDatabase()
        {
            if (!_dialogService.OpenFileDialog("Выберите базу данных", "Data Base File (*.db)|*.db"))
                return;
            string oldDbPath = _dialogService.FilePath;
            try
            {
                // Диалог ввода специальности
                string? specialityName = _dialogService.ShowInputDialog(
                    "Введите название новой специальности.\n" +
                    "Если оставить пустым — будут импортированы только темы.", "Добавление специальности");
                if (specialityName == null)
                {
                    return;
                }
                Mouse.OverrideCursor = Cursors.Wait;
                specialityName = specialityName?.Trim();
                if (string.IsNullOrWhiteSpace(specialityName))
                {
                    specialityName = null;
                }
                await _migrationService.MigrateDataAsync(
                    oldDbPath,
                    specialityName);
                // Обновление UI
                var updatedThemes = _unitOfWork.Questions.GetThemes();
                Themes.Clear();
                foreach (var theme in updatedThemes)
                {
                    Themes.Add(theme);
                }
                if (specialityName is null)
                {
                    _dialogService.ShowMessage(
                        "Темы и вопросы успешно импортированы.");
                }
                else
                {
                    _dialogService.ShowMessage(
                        $"Создана специальность \"{specialityName}\" и импортированы темы.");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Ошибка миграции: {ex.Message}",
                    "Ошибка");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void SampleFile(object obj)
        {
            _windowService.ShowSample();
        }

        private void AddNewTheme(object obj)
        {
            string? themeName =
    _dialogService.ShowInputDialog(
        "Введите название новой темы.",
        "Добавление темы");

            if (string.IsNullOrWhiteSpace(themeName))
                return;

            themeName = themeName.Trim();

            try
            {
                // =====================================================
                // ИЩЕМ ТЕМУ
                // =====================================================

                var existingTheme = _unitOfWork.Themes
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.Name.ToLower() == themeName.ToLower());

                // =====================================================
                // СПЕЦИАЛЬНОСТЬ НЕ ВЫБРАНА
                // =====================================================

                if (string.IsNullOrWhiteSpace(SelectionSpeciality))
                {
                    // Тема уже существует
                    if (existingTheme != null)
                    {
                        _dialogService.ShowMessage(
                            "Такая тема уже существует.");

                        return;
                    }

                    // Создаём тему без специальности
                    var newTheme = new Theme
                    {
                        Name = themeName
                    };

                    _unitOfWork.Themes.Create(newTheme);

                    _unitOfWork.SaveAsync();

                    Themes.Add(newTheme.Name);

                    _dialogService.ShowMessage(
                        "Тема успешно добавлена.");

                    return;
                }

                // =====================================================
                // ИЩЕМ СПЕЦИАЛЬНОСТЬ (TRACKED)
                // =====================================================

                var speciality = _unitOfWork.Specialities
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.Name == SelectionSpeciality);

                if (speciality == null)
                {
                    _dialogService.ShowError(
                        "Специальность не найдена.",
                        "Ошибка");

                    return;
                }

                // Получаем TRACKED сущность
                speciality = _unitOfWork.Specialities
                    .GetTracked(speciality.SpecialityId);

                if (speciality == null)
                {
                    _dialogService.ShowError(
                        "Не удалось получить специальность.",
                        "Ошибка");

                    return;
                }

                // =====================================================
                // ТЕМЫ НЕТ -> СОЗДАЁМ НОВУЮ
                // =====================================================

                if (existingTheme == null)
                {
                    var newTheme = new Theme
                    {
                        Name = themeName
                    };

                    newTheme.Specialities.Add(speciality);

                    _unitOfWork.Themes.Create(newTheme);

                    _unitOfWork.SaveAsync();

                    Themes.Add(newTheme.Name);

                    _dialogService.ShowMessage(
                        "Тема успешно добавлена.");

                    return;
                }

                // =====================================================
                // ПОЛУЧАЕМ TRACKED THEME С SPECIALITIES
                // =====================================================

                existingTheme = _unitOfWork.Themes
                    .GetThemeWithSpecialities(existingTheme.ThemeId);

                if (existingTheme == null)
                {
                    _dialogService.ShowError(
                        "Тема не найдена.",
                        "Ошибка");

                    return;
                }

                // =====================================================
                // ПРОВЕРЯЕМ ПРИВЯЗКУ
                // =====================================================

                bool alreadyLinked =
                    existingTheme.Specialities
                        .Any(x =>
                            x.SpecialityId ==
                            speciality.SpecialityId);

                if (alreadyLinked)
                {
                    _dialogService.ShowMessage(
                        "Тема уже привязана к данной специальности.");

                    return;
                }

                // =====================================================
                // СПРАШИВАЕМ ПРИВЯЗКУ
                // =====================================================

                bool attachTheme =
                    MessageBox.Show(
                        $"Тема \"{existingTheme.Name}\" уже существует.\n\n" +
                        $"Привязать её к специальности \"{speciality.Name}\"?",
                        "Привязка темы",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question)
                    == MessageBoxResult.Yes;

                if (!attachTheme)
                    return;

                // =====================================================
                // ДОБАВЛЯЕМ СВЯЗЬ
                // =====================================================

                existingTheme.Specialities.Add(speciality);

                // Update НЕ НУЖЕН
                // EF уже отслеживает existingTheme

                 _unitOfWork.SaveAsync();

                if (!Themes.Contains(existingTheme.Name))
                {
                    Themes.Add(existingTheme.Name);
                }

                _dialogService.ShowMessage(
                    "Тема успешно привязана к специальности.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Ошибка добавления темы:\n{ex.Message}",
                    "Ошибка");
            }
        }

        private void RaiseAllProperties()
        {
            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(VisibilityAdd));
            RaisePropertyChanged(nameof(VisibilityEdit));
            RaisePropertyChanged(nameof(VisibilityDelete));
            RaisePropertyChanged(nameof(VisibilityImportButtons));
        }

        public event PropertyChangedEventHandler?
            PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
