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

namespace TestWpfApp.ViewModels
{
    public class AddQuestionViewModel : INotifyPropertyChanged
    {
        private readonly IDialogService _dialogService; 
        private readonly IWindowService _windowService;
        private readonly IQuestionRepository _db; // Используем интерфейс
        private readonly IMapper _mapper;
        private readonly MigrationOldTableService _migrationService;
        public OpenFileDialog openFileDialog;
        public ICommand SaveQuestionCommand { get; }
        public ICommand SaveEditQuestionCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand DeleteImageCommand { get; }
        public ICommand AddQuestionFromFileCommand { get; }
        public ICommand FileCommand { get; }
        public ICommand CloseWindowsCommand { get; }
        public ICommand AddNewThemeCommand { get; }
        public ICommand MigrateOldDatabaseCommand { get; }
        public ImageSource Image { get; private set; }
        public FileInfo? FileInf;
        public string Title { get; set; }
        public string TitleImageButton { get; set; } = " Добавить картинку ";
        public bool VisibilityEdit { get; set; }
        public bool VisibilityDelete { get; set; }
        public bool ImageEdit { get; set; } = false;
        
        public bool VisibilityAdd { get; set; }
        public string? ImagePath { get; set; }
        private ImageSource imageSource;
        public ImageSource? ImageSource
        {
            get => imageSource;
            set
            {
                imageSource = value!;

                RaisePropertyChanged(nameof(ImageSource));
            }
        }
        private System.Collections.ObjectModel.ObservableCollection<string> themes;
        public System.Collections.ObjectModel.ObservableCollection<string> Themes
        {
            get => themes;
            set
            {
                themes = value;

                RaisePropertyChanged(nameof(Themes));
            }
        }
        public string SelectionTheme { get; set; }

        private TestQuestion testQuestion;

        public TestQuestion TestQuestion
        {
            get => testQuestion;
            set
            {
                testQuestion = value;

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
                RaisePropertyChanged(nameof(ProgressValue)); // уведомляем UI
            }
        }
        IQuestionRepository db;

        public AddQuestionViewModel(IDialogService dialogService, IWindowService windowService, IQuestionRepository db, IMapper mapper, MigrationOldTableService migrationService)
        {
            _dialogService = dialogService; 
            _windowService = windowService;
            _db = db;
            _mapper = mapper;
            _migrationService = migrationService;
            Title = " Добавление вопроса ";
            VisibilityEdit = false;
            VisibilityAdd = true;
            VisibilityDelete = false;
            //CloseWindowsCommand = new RelayCommand(CloseWindows);
            AddNewThemeCommand = new RelayCommand(AddNewTheme);
            SaveQuestionCommand = new RelayCommand(SaveQuestion);
            AddImageCommand = new RelayCommand(AddImage);
            DeleteImageCommand = new RelayCommand(DeleteImage);
            AddQuestionFromFileCommand = new RelayCommand(AddQuestionFromFile);
            FileCommand = new RelayCommand(SampleFile);
            MigrateOldDatabaseCommand = new RelayCommand(async (obj) => await MigrateOldDatabase());
            TestQuestion = new TestQuestion();
            //db = new QuestionRepository();
            var ThemesList = _db.GetThemes();
            Themes = new System.Collections.ObjectModel.ObservableCollection<string>();
            foreach (var item in ThemesList)
                Themes.Add(item);
        }

        public AddQuestionViewModel(TestQuestion questionEdit, IDialogService dialogService, IWindowService windowService, IQuestionRepository db, IMapper mapper)
        {
            _dialogService = dialogService;
            _windowService = windowService;
            _db = db;
            _mapper = mapper;
            Title = " Редактирование вопроса ";
            VisibilityEdit = true;
            VisibilityAdd = false;
            //CloseWindowsCommand = new RelayCommand(CloseWindows);
            SaveQuestionCommand = new RelayCommand(SaveQuestion);
            AddImageCommand = new RelayCommand(AddImage);
            DeleteImageCommand = new RelayCommand(DeleteImage);
            AddNewThemeCommand = new RelayCommand(AddNewTheme);
            AddQuestionFromFileCommand = new RelayCommand(AddQuestionFromFile);
            TestQuestion = questionEdit;
            //db = new QuestionRepository();
            var ThemesList = _db.GetThemes();
            Themes = new System.Collections.ObjectModel.ObservableCollection<string>();
            foreach (var item in ThemesList)
                Themes.Add(item);
            SelectionTheme = TestQuestion.NameTheme;
            if (TestQuestion.ImageQuestion  != null)
            {
                ImagePath = TestQuestion.FullImageQuestion;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(ImagePath);
                bmp.CacheOption = BitmapCacheOption.OnLoad; // ← ключ
                bmp.EndInit();
                bmp.Freeze(); // ← освобождает файл
                ImageSource = bmp;

                TitleImageButton = " Изменить картинку ";
                VisibilityDelete = true;
            }
        }
        private async Task MigrateOldDatabase()
        {
            // 1. Просим пользователя выбрать файл старой базы данных
            if (_dialogService.OpenFileDialog())
            {
                string oldDbPath = _dialogService.FilePath;

                try
                {
                    // Можно добавить индикацию начала процесса (например, курсор ожидания)
                    Mouse.OverrideCursor = Cursors.Wait;

                    await _migrationService.MigrateDataAsync(oldDbPath);

                    // Обновляем список тем в UI после миграции, если появились новые
                    var updatedThemes = _db.GetThemes();
                    Themes.Clear();
                    foreach (var theme in updatedThemes)
                    {
                        Themes.Add(theme);
                    }

                    _dialogService.ShowMessage("Данные успешно перенесены из старой базы.");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Ошибка при миграции: {ex.Message}", "Ошибка миграции");
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
        private void AddQuestionFromFile(object obj)
        {
            var importer = new QuestionFileImporter(db);
            importer.AddQuestionsFromFile(_mapper);
            //importer.ProgressChanged += percent =>
            //{
            //    ProgressValue = percent;
            //};          
        }

        private void SampleFile(object obj)
        {
            _windowService.ShowSample();
        }


        private void AddNewTheme(object obj)
        {
            string? newTheme = _windowService.ShowNewTheme(); 
            if (!string.IsNullOrWhiteSpace(newTheme)) 
            { 
                Themes.Add(newTheme); 
                RaisePropertyChanged("Themes"); 
            }
        }


        private void AddImage(object obj)
        {
            //openFileDialog = new OpenFileDialog()
            //{
            //    Multiselect = true,
            //    Filter = "Image files (*.BMP, *.JPG, *.GIF, *.TIF, *.PNG, *.ICO, *.EMF, *.WMF)|*.bmp;*.jpg;*.gif; *.tif; *.png; *.ico; *.emf; *.wmf"
            //};
            if (_dialogService.OpenFileDialog()) 
            { 
                ImagePath = _dialogService.FilePath; 
                var bmp = new BitmapImage(); 
                bmp.BeginInit(); 
                bmp.UriSource = new Uri(ImagePath); 
                bmp.CacheOption = BitmapCacheOption.OnLoad; 
                bmp.EndInit(); bmp.Freeze(); 
                ImageSource = bmp; 

                RaisePropertyChanged("ImageSource"); 
                FileInf = new FileInfo(ImagePath); 
                ImageEdit = true; 
                TitleImageButton = " Изменить картинку "; 
                RaisePropertyChanged("TitleImageButton"); 
                VisibilityDelete = true; 
                RaisePropertyChanged("VisibilityDelete"); 
            }
        }

        private void DeleteImage(object obj)
        {
            ImagePath = null;
            FileInf = null;
            VisibilityDelete = false;            
            TitleImageButton = " Добавить картинку ";
            if (TestQuestion.ImageQuestion != null)
            {
                ImageEdit = true;
            }

            ImageSource = null;

            RaisePropertyChanged("ImageSource");
            RaisePropertyChanged("VisibilityDelete");
            RaisePropertyChanged("TitleImageButton");

        }

        private void SaveQuestion(object obj)
        {
            if (string.IsNullOrWhiteSpace(TestQuestion.NameQuestion) ||
                string.IsNullOrWhiteSpace(SelectionTheme) ||
                string.IsNullOrWhiteSpace(TestQuestion.NameAnswerCorrect1) ||
                string.IsNullOrWhiteSpace(TestQuestion.NameAnswerIncorrect1) ||
                string.IsNullOrWhiteSpace(TestQuestion.NameAnswerIncorrect2) ||
                string.IsNullOrWhiteSpace(TestQuestion.NameAnswerIncorrect3))
            {
                _dialogService.ShowError("Заполните все поля(поля Сециальность и пояснение необязательные)).", "Ошибка заполнения формы"); 
                return;
            }

            //db = new QuestionRepository();
            TestQuestion.NameTheme = SelectionTheme;
            var dataModel = _mapper.Map<TestQuestionDataModel>(TestQuestion);
            if (!db.CheckQuestions(dataModel) || ImageEdit)
            {
                // создание нового вопроса
                if (VisibilityAdd)
                {
                    db.Create(dataModel);
                    TestQuestion.QuestionId = dataModel.QuestionId;
                    // добавляется ли картинка
                    if (ImageEdit && FileInf != null)
                    {
                        ImageMethod();
                    }
                    _dialogService.ShowMessage("Вопрос успешно добавлен.");
                }
                // редактирование
                if (VisibilityEdit)
                {
                    if (ImageEdit)
                    {
                        ImageMethod();
                    }
                    else { db.Update(dataModel); }
                    _dialogService.ShowMessage("Вопрос успешно изменен.");
                }
            }
            else
            {
                _dialogService.ShowError("Такой вопрос уже существует.", "Ошибка добавления вопроса");
            }
        }
        private void ImageMethod()
        {
            var dataModel = new TestQuestionDataModel();
            // добавление или редактирование картинки
            if (FileInf != null)
            {
                string currentDir = Directory.GetCurrentDirectory();
                dataModel = _mapper.Map<TestQuestionDataModel>(TestQuestion);
                int idQuestion = (int)db.GetIdQuestions(dataModel);
                string dirName = @"Images\";
                // если папка не существует
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                string fileType = FileInf.Extension;
                string newPath = dirName + idQuestion + fileType;
                string fullNewPath = currentDir + "\\" + newPath;
                if (File.Exists(fullNewPath))
                {
                    try { File.Delete(fullNewPath); }
                    catch
                    {
                        _dialogService.ShowError("Старая картинка не удалена с диска.", "Ошибка удаления картинки");
                    }
                }

                if (FileInf.Exists)
                {                    
                    FileInf.CopyTo(newPath, true);
                    TestQuestion.ImageQuestion = newPath;
                    TestQuestion.FullImageQuestion = fullNewPath;
                    dataModel = _mapper.Map<TestQuestionDataModel>(TestQuestion);
                    db.Update(dataModel);
                }
                else
                {
                    _dialogService.ShowError("Картинка не найдена в источнике.", "Ошибка сохранения картинки");
                }

            }
            // картинка удалена
            if (FileInf == null)
            {
                try
                {
                    File.Delete(TestQuestion.FullImageQuestion);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Ошибка удаления файла с диска: {ex.Message}", "Ошибка");
                }
                TestQuestion.ImageQuestion = null;
                TestQuestion.FullImageQuestion = null;
                dataModel = _mapper.Map<TestQuestionDataModel>(TestQuestion);
                db.Update(dataModel);
                _dialogService.ShowMessage("Вопрос успешно изменен, картинка с вопроса удалена.");
            }
        }

        private void DeleteImageForQuestion(TestQuestion question)
        {
            try
            {
                File.Delete(question.FullImageQuestion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления файла: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Обнуляем ссылки в вопросе
            question.ImageQuestion = null;
            question.FullImageQuestion = null;
            var dataModel = _mapper.Map<TestQuestionDataModel>(TestQuestion);
            db.Update(dataModel);
        }

        /// <summary>
        /// Сохраняет картинку для вопроса.
        /// </summary>
        private void SaveImageForQuestion(TestQuestion question, FileInfo file)
        {
            if (file == null || !file.Exists)
                return;

            string currentDir = Directory.GetCurrentDirectory();
            string dirName = @"Images\";

            // Если раньше картинка лежала в спецпапке
            if (!string.IsNullOrEmpty(question.ImageQuestion) &&
                question.ImageQuestion.StartsWith("I"))
            {
                dirName = @"1Images\";
            }

            // Создаём папку, если её нет
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            // Формируем пути
            string fileType = file.Extension;
            string newRelativePath = dirName + question.QuestionId + fileType;
            string fullNewPath = currentDir + "\\" + newRelativePath;

            // Удаляем старый файл, если существует
            if (File.Exists(fullNewPath))
            {
                try { File.Delete(fullNewPath); }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления старого файла: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Копируем новый файл
            try
            {
                file.CopyTo(fullNewPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения картинки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновляем ссылки в вопросе
            question.ImageQuestion = newRelativePath;
            question.FullImageQuestion = fullNewPath;
            var dataModel = _mapper.Map<TestQuestionDataModel>(TestQuestion);
            db.Update(dataModel);
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
