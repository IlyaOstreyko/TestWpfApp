using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestWpfApp.Data.Context;
using TestWpfApp.Data.DataModels;
using TestWpfApp.Data.Interfaces;

namespace TestWpfApp.Data.Repositories
{
    public class ThemeRepository : IThemeRepository
    {
        private readonly ApplicationContext _context;

        public ThemeRepository(ApplicationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Theme? Get(int id)
        {
            // Сначала смотрим в кэше контекста (те, что уже отслеживаются)
            var local = _context.Themes.Local.FirstOrDefault(t => t.ThemeId == id);
            if (local != null) return local;

            // Если в памяти нет, идем в базу
            return _context.Themes.Find(id);
        }
        public Theme? GetThemeWithSpecialities(int id)
        {
            return _context.Themes
                .Include(x => x.Specialities)
                .FirstOrDefault(x => x.ThemeId == id);
        }
        public Theme? GetByName(string nameTheme)
        {
            // Сначала смотрим в кэше контекста (те, что уже отслеживаются)
            var local = _context.Themes.Local.FirstOrDefault(t => t.Name == nameTheme);
            if (local != null) return local;

            // Если в памяти нет, идем в базу
            return _context.Themes.FirstOrDefault(t => t.Name == nameTheme);
        }

        public List<Theme> GetAll()
        {
            return _context.Themes.Include(x => x.Specialities)
                .AsNoTracking()
                .ToList();
        }

        public void Create(Theme item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _context.Themes.Add(item);
            _context.SaveChanges();
        }

        public void Update(Theme item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _context.Themes.Update(item);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var item = _context.Themes.Find(id);
            if (item != null)
            {
                _context.Themes.Remove(item);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Получает темы конкретной специальности через таблицу связей
        /// </summary>
        public List<Theme> GetThemesBySpeciality(int specialityId)
        {
            try
            {
                // Проверка входных данных
                if (specialityId <= 0)
                {
                    throw new ArgumentException(
                        "Некорректный идентификатор специальности.",
                        nameof(specialityId));
                }

                // Проверка существования специальности
                bool specialityExists = _context.Specialities
                    .AsNoTracking()
                    .Any(s => s.SpecialityId == specialityId);

                if (!specialityExists)
                {
                    return new List<Theme>();
                }

                // Получаем темы выбранной специальности
                // + подгружаем вопросы этих тем
                var themes = _context.Themes
                    .AsNoTracking()
                    .Where(t => t.Specialities.Any(s => s.SpecialityId == specialityId))
                    .Include(t => t.Questions)
                    .OrderBy(t => t.Name)
                    .ToList();

                return themes;
            }
            catch (Exception ex)
            {
                // Здесь обычно используют ILogger
                Console.WriteLine(
                    $"Ошибка получения тем специальности {specialityId}: {ex}");

                return new List<Theme>();
            }
        }

        /// <summary>
        /// Загружает все темы и сразу подтягивает связанные вопросы
        /// </summary>
        public List<Theme> GetAllWithQuestions()
        {
            return _context.Themes
                .AsNoTracking()
                .Include(t => t.Questions)
                .ToList();
        }

        public bool Exists(string nameTheme)
        {
            if (string.IsNullOrWhiteSpace(nameTheme)) return false;
            return _context.Themes
                .Any(t => t.Name == nameTheme);
        }
    }
}
