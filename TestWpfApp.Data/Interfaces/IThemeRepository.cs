using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWpfApp.Data.DataModels;

namespace TestWpfApp.Data.Interfaces
{
    public interface IThemeRepository
    {
        // Базовые операции
        Theme? Get(int id);
        Theme? GetByName(string nameTheme);
        Theme? GetThemeWithSpecialities(int id);
        List<Theme> GetAll();
        void Create(Theme item);
        void Update(Theme item);
        void Delete(int id);

        // Специфические методы для работы со специальностями

        /// <summary>
        /// Получает все темы, привязанные к конкретной специальности
        /// </summary>
        List<Theme> GetThemesBySpeciality(int specialityId);

        /// <summary>
        /// Получает все существующие темы, включая информацию о вопросах (для подсчета количества)
        /// </summary>
        List<Theme> GetAllWithQuestions();

        /// <summary>
        /// Проверяет существование темы по названию
        /// </summary>
        bool Exists(string nameTheme);
    }
}
