using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using TestWpfApp.Data.DataModels;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Data.Repositories;
using TestWpfApp.Interfaces;
using TestWpfApp.Models;
using TestWpfApp.Service;
using TestWpfApp.Views;
using Xceed.Wpf.AvalonDock.Themes;

namespace TestWpfApp.ViewModels
{
    public class EditQuestionsViewModel : INotifyPropertyChanged
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDialogService _dialogService;
        private readonly IWindowService _windowService;
        private readonly IMapper _mapper;

        private ObservableCollection<TestQuestionVM> _testQuestions = new();
        private List<string> _themes = new();
        private string _title;
        private string nameSpec;
        private int SpecialityId;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        private bool _visibilityManager;
        public bool VisibilityManager
        {
            get => _visibilityManager;
            set
            {
                _visibilityManager = value;
                OnPropertyChanged();
            }
        }
        private string? _selectionTheme;
        public ObservableCollection<TestQuestionVM> TestQuestions
        {
            get => _testQuestions;
            set
            {
                _testQuestions = value;
                OnPropertyChanged();
            }
        }

        public List<string> Themes
        {
            get => _themes;
            set
            {
                _themes = value;
                OnPropertyChanged();
            }
        }

        public string? SelectionTheme
        {
            get => _selectionTheme;
            set
            {
                _selectionTheme = value;
                OnPropertyChanged();
            }
        }
        public ICommand ShowQuestionsCommand { get; }
        public ICommand ShowCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ChangeImageCommand { get; }
        public ICommand SaveWordCommand { get; }
        public EditQuestionsViewModel(IUnitOfWork unitOfWork, IDialogService dialogService,IWindowService windowService, IMapper mapper)
        {
            ShowQuestionsCommand = new RelayCommand(ShowQuestions);
            ShowCommand = new RelayCommand(Show);
            EditCommand = new RelayCommand(Edit);
            DeleteCommand = new RelayCommand(Delete);
            ChangeImageCommand = new RelayCommand(ChangeImage);
            SaveWordCommand = new RelayCommand(SaveWord);
            _unitOfWork = unitOfWork;
            _dialogService = dialogService;
            _windowService = windowService;
            _mapper = mapper;
        }
        public void Initialize(string title, bool editMode, int specialityId)
        {
            Title = title;
            VisibilityManager = editMode;
            SpecialityId = specialityId;
            nameSpec = _unitOfWork.Specialities.GetName(SpecialityId);
            LoadThemes();
        }
        private void LoadThemes()
        {
            try 
            {
                var filteredThemes = _unitOfWork.Themes.GetThemesBySpeciality(SpecialityId);
                if (filteredThemes.Count == 0)
                {
                    _dialogService.ShowError($"Тем в специальности:\n{SpecialityId}\nне найдено", "Ошибка");
                    return;
                }
                foreach (var theme in filteredThemes) 
                {
                    Themes.Add(theme.Name);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка загрузки тем:\n{ex.Message}", "Ошибка");
            }
        }
        private void LoadQuestionsByTheme(string theme)
        {
            var questions = _unitOfWork.Questions
                .GetQuestionsInTheme(theme);

            var mapped = _mapper.Map<IEnumerable<TestQuestionVM>>(questions);

            TestQuestions = new ObservableCollection<TestQuestionVM>(mapped);
        }
        private void ChangeImage(object obj)
        {
            if (obj is not TestQuestionVM questionVm)
                return;

            if (!_dialogService.OpenFileDialog("Выберите картинку", "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"))
                return;

            var imagePath = _dialogService.FilePath;

            try
            {
                byte[] imageBytes = File.ReadAllBytes(imagePath);

                var dbQuestion = _unitOfWork.Questions
                    .Get((int)questionVm.QuestionId);

                if (dbQuestion == null)
                {
                    _dialogService.ShowError("Вопрос не найден.", "Ошибка");
                    return;
                }

                dbQuestion.ImageQuestionBytes = imageBytes;

                //_unitOfWork.Questions.Update(dbQuestion);

                _unitOfWork.SaveAsync();

                questionVm.ImageQuestionBytes = imageBytes;

                OnPropertyChanged(nameof(TestQuestions));
                _dialogService.ShowMessage("Изображение успешно сохранено.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка загрузки изображения:\n{ex.Message}", "Ошибка");
            }
        }
        public void ShowQuestions(object obj)
        {
            if (string.IsNullOrWhiteSpace(SelectionTheme))
            {
                _dialogService.ShowError("Выберите тему для сохранения.", "Ошибка");
                return;
            }
            LoadQuestionsByTheme(SelectionTheme);
        }
        public void SaveWord(object obj)
        {
            if (string.IsNullOrWhiteSpace(SelectionTheme))
            {
                _dialogService.ShowError("Выберите тему для сохранения.", "Ошибка");
                return;
            }
            var mapped = _mapper.Map<IEnumerable<TestQuestion>>(TestQuestions);

            var testQuestionsDM = mapped.ToList();
            try 
            {
                if (!_dialogService.SaveFileDialog()) return;
                string outputDir = _dialogService.FilePath;
                string outputPath = outputDir + ".docx";
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Templates","TemplateQuestions.docx");
                WordGenerator.GenerateQuestionsDoc(
                    testQuestionsDM,
                    templatePath,
                    outputPath,
                    SelectionTheme,
                    nameSpec
                    );
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка сохранения:\n{ex.Message}", "Ошибка");
            }

        }

        public void Show(object obj)
        {
            if (obj is not string theme || string.IsNullOrWhiteSpace(theme))
                return;

            SelectionTheme = theme;

            LoadQuestionsByTheme(theme);
        }

        public void Edit(object obj)
        {
            if (obj is not TestQuestionVM question)
                return;

            var parameters = new AddQuestionParameters
            {
                Question = question,
                SelectedTheme = SelectionTheme,
                SelectedSpeciality = nameSpec
            };

            _windowService.ShowEditQuestion(parameters);

            if (!string.IsNullOrWhiteSpace(question.NameTheme))
            {
                LoadQuestionsByTheme(question.NameTheme);
            }
        }

        public void Delete(object obj)
        {
            if (obj is not TestQuestionVM question)
                return;

            var result = _dialogService.ShowConfirmation($"Удалить вопрос?\n\n{question.NameQuestion}", "Удаление вопроса");

            if (!result)
                return;
            try
            {
                _unitOfWork.Questions.Delete((int)question.QuestionId);

                _unitOfWork.SaveAsync();

                TestQuestions.Remove(question);

                if (TestQuestions.Count == 0)
                {
                    _dialogService.ShowMessage("В данной теме больше нет вопросов.");

                    SelectionTheme = null;

                    LoadThemes();
                }
                _dialogService.ShowMessage("Вопрос успешно удалён.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка удаления:\n{ex.Message}", "Ошибка");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
