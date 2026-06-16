using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using TestWpfApp.Data.Context;
using TestWpfApp.Data.DataModels;
using GalaSoft.MvvmLight;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Interfaces;

namespace TestWpfApp.ViewModels
{
    public class AdminDataViewModel : INotifyPropertyChanged
    {
        private readonly IUnitOfWork _uow;
        private readonly IDialogService _dialogService;
        public ObservableCollection<Speciality> Specialities { get; set; } = new();
        public ObservableCollection<ThemeSelectionWrapper> AvailableThemes { get; set; } = new();

        private Speciality? _selectedSpeciality;
        public Speciality? SelectedSpeciality
        {
            get => _selectedSpeciality;
            set
            {
                _selectedSpeciality = value;
                OnPropertyChanged();
                RefreshThemeLinks();
            }
        }

        public ICommand AddSpecCommand { get; }
        public ICommand DeleteSpecCommand { get; }
        public ICommand ToggleThemeLinkCommand { get; }
        public ICommand SaveCountCommand { get; }
        public AdminDataViewModel(IDialogService dialogService, IUnitOfWork uow)
        {
            _uow = uow;
            _dialogService = dialogService;
            AddSpecCommand = new RelayCommand(async _ => await AddSpecialityAsync());
            DeleteSpecCommand = new RelayCommand(async _ => await DeleteSpecialityAsync(), _ => SelectedSpeciality != null);
            ToggleThemeLinkCommand = new RelayCommand(async obj => await ToggleThemeLinkAsync(obj as ThemeSelectionWrapper));
            SaveCountCommand = new RelayCommand(async obj => await SaveQuestionCountAsync(obj as ThemeSelectionWrapper));

            // Запускаем первичную загрузку
            _ = InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            // 1. Загружаем темы с вопросами
            var themes = _uow.Themes.GetAllWithQuestions();
            AvailableThemes.Clear();
            foreach (var t in themes)
                AvailableThemes.Add(new ThemeSelectionWrapper { Theme = t });

            // 2. Загружаем специальности со всеми связями
            var specs = await _uow.Specialities.GetAllForAdminAsync();
            Specialities.Clear();
            foreach (var s in specs)
                Specialities.Add(s);
        }

        private void RefreshThemeLinks()
        {
            if (SelectedSpeciality == null) return;

            foreach (var wrapper in AvailableThemes)
            {
                // 1. Проверяем, привязана ли тема к специальности (Many-to-Many)
                bool isThemeInSpeciality = SelectedSpeciality.Themes
                    .Any(t => t.ThemeId == wrapper.Theme.ThemeId);

                // 2. Ищем настройки для этой темы
                var setting = SelectedSpeciality.ThemeSettings
                    .FirstOrDefault(s => s.ThemeId == wrapper.Theme.ThemeId);

                // Галочка стоит, если тема есть в списке Themes специальности
                wrapper.IsLinked = isThemeInSpeciality;

                // Количество вопросов берем из настроек, если их нет — 0
                wrapper.QuestionCount = setting?.QuestionsCount ?? 0;
            }
        }

        private async Task AddSpecialityAsync()
        {
            string? specialityName = _dialogService.ShowInputDialog(
    "Введите название новой специальности."
    , "Добавление специальности");
            if (string.IsNullOrWhiteSpace(specialityName)) return;

            var newSpec = new Speciality { Name = specialityName };
            _uow.Specialities.Add(newSpec); // Только помечаем на добавление

            await _uow.SaveAsync(); // Сохраняем через UoW
            Specialities.Add(newSpec); // Обновляем UI
        }

        private async Task DeleteSpecialityAsync()
        {
            if (SelectedSpeciality == null) return;

            var result = MessageBox.Show($"Удалить {SelectedSpeciality.Name}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            _uow.Specialities.Remove(SelectedSpeciality);
            await _uow.SaveAsync();

            Specialities.Remove(SelectedSpeciality);
            SelectedSpeciality = null;
        }

        private async Task ToggleThemeLinkAsync(ThemeSelectionWrapper? wrapper)
        {
            try
            {
                if (wrapper == null || SelectedSpeciality == null) return;

                // Используем ID для поиска, чтобы не сравнивать ссылки на объекты
                var themeId = wrapper.Theme.ThemeId;

                var themeInSpec = SelectedSpeciality.Themes
                    .FirstOrDefault(t => t.ThemeId == themeId);

                var setting = SelectedSpeciality.ThemeSettings
                    .FirstOrDefault(s => s.ThemeId == themeId);

                if (wrapper.IsLinked)
                {
                    if (themeInSpec == null)
                    {
                        // Получаем тему, которая гарантированно принадлежит текущему контексту
                        var trackedTheme = _uow.Themes.Get(themeId);
                        if (trackedTheme != null)
                            SelectedSpeciality.Themes.Add(trackedTheme);
                    }

                    if (setting == null)
                    {
                        SelectedSpeciality.ThemeSettings.Add(new SpecialityThemeSetting
                        {
                            SpecialityId = SelectedSpeciality.SpecialityId,
                            ThemeId = themeId,
                            QuestionsCount = 0
                        });
                    }
                }
                else
                {
                    if (themeInSpec != null) SelectedSpeciality.Themes.Remove(themeInSpec);
                    if (setting != null) SelectedSpeciality.ThemeSettings.Remove(setting);
                }

                await _uow.SaveAsync();
            }
            catch (Exception ex)
            {
                // Если ошибка все равно лезет, попробуйте временно отключить Refresh, 
                // чтобы понять, не в нем ли дело
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }

            RefreshThemeLinks();
        }
        private async Task SaveQuestionCountAsync(ThemeSelectionWrapper? wrapper)
        {
            if (wrapper == null || SelectedSpeciality == null) return;

            // Ищем настройку. Если её нет (хотя галочка стоит), создаем её.
            var setting = SelectedSpeciality.ThemeSettings
                .FirstOrDefault(s => s.ThemeId == wrapper.Theme.ThemeId);

            if (setting != null)
            {
                setting.QuestionsCount = wrapper.QuestionCount;
            }
            else if (wrapper.IsLinked)
            {
                // Если вдруг настройки не было, но тема привязана — создаем
                SelectedSpeciality.ThemeSettings.Add(new SpecialityThemeSetting
                {
                    SpecialityId = SelectedSpeciality.SpecialityId,
                    ThemeId = wrapper.Theme.ThemeId,
                    QuestionsCount = wrapper.QuestionCount
                });
            }

            await _uow.SaveAsync();
            MessageBox.Show("Количество вопросов сохранено", "Инфо");
        }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
    public class ThemeSelectionWrapper : INotifyPropertyChanged
    {
        public Theme Theme { get; set; } = null!;
        public string NameTheme => Theme.Name ?? "Без названия";
        public int TotalQuestionsCount => Theme.Questions?.Count ?? 0;

        private bool _isLinked;
        public bool IsLinked
        {
            get => _isLinked;
            set
            {
                _isLinked = value;
                OnPropertyChanged();
                if (!_isLinked) QuestionCount = 0; // Сбрасываем при снятии галочки
            }
        }
        private int _questionCount;

        public int QuestionCount
        {
            get => _questionCount;
            set
            {
                var normalized = value;

                if (normalized < 0)
                    normalized = 0;

                if (normalized > TotalQuestionsCount)
                    normalized = TotalQuestionsCount;

                _questionCount = normalized;

                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
