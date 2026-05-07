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

namespace TestWpfApp.ViewModels
{
    public class AdminDataViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationContext _db;

        public ObservableCollection<SpecialityDataModel> Specialities { get; set; }
        public ObservableCollection<ThemeSelectionWrapper> AvailableThemes { get; set; } = new();

        private SpecialityDataModel? _selectedSpeciality;
        public SpecialityDataModel? SelectedSpeciality
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

        public AdminDataViewModel(ApplicationContext db)
        {
            _db = db;

            // Загружаем данные с учетом связей
            _db.SpecialityDataModels.Include(s => s.ThemesDataModel).Load();
            _db.ThemeDataModels.Load();

            Specialities = _db.SpecialityDataModels.Local.ToObservableCollection();

            AddSpecCommand = new RelayCommand(_ => AddSpeciality());
            DeleteSpecCommand = new RelayCommand(_ => DeleteSpeciality(), _ => SelectedSpeciality != null);
            ToggleThemeLinkCommand = new RelayCommand(obj => ToggleThemeLink(obj as ThemeSelectionWrapper));

            LoadAllThemes();
        }

        private void LoadAllThemes()
        {
            AvailableThemes.Clear();
            foreach (var theme in _db.ThemeDataModels.ToList())
            {
                AvailableThemes.Add(new ThemeSelectionWrapper { Theme = theme });
            }
        }

        private void RefreshThemeLinks()
        {
            if (SelectedSpeciality == null)
            {
                foreach (var t in AvailableThemes) t.IsLinked = false;
                return;
            }

            // Отмечаем те темы, которые уже есть в списке выбранной специальности
            foreach (var themeWrapper in AvailableThemes)
            {
                themeWrapper.IsLinked = SelectedSpeciality.ThemesDataModel!
                    .Any(t => t.ThemeId == themeWrapper.Theme.ThemeId);
            }
        }

        private void AddSpeciality()
        {
            // Здесь можно вызвать InputDialog или просто создать заглушку
            string name = Microsoft.VisualBasic.Interaction.InputBox("Введите название специальности:", "Новая специальность");
            if (!string.IsNullOrWhiteSpace(name))
            {
                var newSpec = new SpecialityDataModel { NameSpeciality = name };
                _db.SpecialityDataModels.Add(newSpec);
                _db.SaveChanges();
            }
        }

        private void DeleteSpeciality()
        {
            if (SelectedSpeciality != null)
            {
                var result = MessageBox.Show($"Удалить специальность {SelectedSpeciality.NameSpeciality}?", "Предупреждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _db.SpecialityDataModels.Remove(SelectedSpeciality);
                    _db.SaveChanges();
                }
            }
        }

        private void ToggleThemeLink(ThemeSelectionWrapper? wrapper)
        {
            if (wrapper == null || SelectedSpeciality == null) return;

            // Находим реальную тему в контексте БД
            var theme = _db.ThemeDataModels.Find(wrapper.Theme.ThemeId);
            if (theme == null) return;

            if (wrapper.IsLinked)
            {
                if (!SelectedSpeciality.ThemesDataModel!.Any(t => t.ThemeId == theme.ThemeId))
                    SelectedSpeciality.ThemesDataModel!.Add(theme);
            }
            else
            {
                var linkedTheme = SelectedSpeciality.ThemesDataModel!.FirstOrDefault(t => t.ThemeId == theme.ThemeId);
                if (linkedTheme != null)
                    SelectedSpeciality.ThemesDataModel!.Remove(linkedTheme);
            }

            _db.SaveChanges();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Вспомогательный класс для UI
    public class ThemeSelectionWrapper : INotifyPropertyChanged
    {
        public ThemeDataModel Theme { get; set; } = null!;
        public string NameTheme => Theme.NameTheme ?? "Без названия";

        private bool _isLinked;
        public bool IsLinked
        {
            get => _isLinked;
            set { _isLinked = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
