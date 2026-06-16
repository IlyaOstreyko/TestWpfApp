using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using TestWpfApp.Data.Context;
using TestWpfApp.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace TestWpfApp.ViewModels
{
    public class AdminGroupsDataViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationContext _db;

        public ObservableCollection<Group> Groups { get; set; }
        public ObservableCollection<SpecSelectionWrapper> AvailableSpec { get; set; } = new();

        private Group? _selectedGroup;
        public Group? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                OnPropertyChanged();
                RefreshSpecLinks();
            }
        }

        public ICommand AddGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand ToggleSpecLinkCommand { get; }

        public AdminGroupsDataViewModel(ApplicationContext db)
        {
            _db = db;

            // Загружаем данные с учетом связей
            _db.Groups.Include(s => s.Specialities).Load();
            _db.Specialities.Load();

            Groups = _db.Groups.Local.ToObservableCollection();

            AddGroupCommand = new RelayCommand(_ => AddGroup());
            DeleteGroupCommand = new RelayCommand(_ => DeleteGroup(), _ => SelectedGroup != null);
            ToggleSpecLinkCommand = new RelayCommand(obj => ToggleSpecLink(obj as SpecSelectionWrapper));

            LoadAllSpec();
        }

        private void LoadAllSpec()
        {
            AvailableSpec.Clear();
            foreach (var spec in _db.Specialities.ToList())
            {
                AvailableSpec.Add(new SpecSelectionWrapper { Speciality = spec });
            }
        }

        private void RefreshSpecLinks()
        {
            if (SelectedGroup == null)
            {
                foreach (var t in AvailableSpec) t.IsLinked = false;
                return;
            }

            // Отмечаем те темы, которые уже есть в списке выбранной специальности
            foreach (var themeWrapper in AvailableSpec)
            {
                themeWrapper.IsLinked = SelectedGroup.Specialities!
                    .Any(t => t.SpecialityId == themeWrapper.Speciality.SpecialityId);
            }
        }

        private void AddGroup()
        {
            // Здесь можно вызвать InputDialog или просто создать заглушку
            string name = Microsoft.VisualBasic.Interaction.InputBox("Введите название группы:", "Новая группа");
            if (!string.IsNullOrWhiteSpace(name))
            {
                var newGroup = new Group { Name = name };
                _db.Groups.Add(newGroup);
                _db.SaveChanges();
            }
        }

        private void DeleteGroup()
        {
            if (SelectedGroup != null)
            {
                var result = MessageBox.Show($"Удалить группу {SelectedGroup.Name}?", "Предупреждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _db.Groups.Remove(SelectedGroup);
                    _db.SaveChanges();
                }
            }
        }
        private void ToggleSpecLink(SpecSelectionWrapper? wrapper)
        {
            if (wrapper == null || SelectedGroup == null) return;

            // Находим реальную тему в контексте БД
            var spec = _db.Specialities.Find(wrapper.Speciality.SpecialityId);
            if (spec == null) return;

            if (wrapper.IsLinked)
            {
                if (!SelectedGroup.Specialities!.Any(t => t.SpecialityId == spec.SpecialityId))
                    SelectedGroup.Specialities!.Add(spec);
            }
            else
            {
                var linkedSpec = SelectedGroup.Specialities!.FirstOrDefault(t => t.SpecialityId == spec.SpecialityId);
                if (linkedSpec != null)
                    SelectedGroup.Specialities!.Remove(linkedSpec);
            }

            _db.SaveChanges();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class SpecSelectionWrapper : INotifyPropertyChanged
    {
        public Speciality Speciality { get; set; } = null!;
        public string NameSpeciality => Speciality.Name ?? "Без названия";

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
