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
    public class SpecialityRepository : ISpecialityRepository
    {
        private readonly ApplicationContext _context;

        public SpecialityRepository(ApplicationContext context)
        {
            _context = context;
        }

        public IEnumerable<Speciality> GetAll()
        {
            return _context.Specialities
                .AsNoTracking()
                .Include(s => s.Groups)
                .OrderBy(s => s.Name)
                .ToList();
        }
        public Speciality? GetTracked(int id)
        {
            return _context.Specialities
                .FirstOrDefault(x => x.SpecialityId == id);
        }
        public string GetName(int idSpeciality)
        {
            var speciality = _context.Specialities
                .AsNoTracking()
                .FirstOrDefault(s => s.SpecialityId == idSpeciality);
            return speciality != null ? speciality.Name : "Специальность не найдена";
        }
        public async Task<List<Speciality>> GetAllForAdminAsync()
        {
            return await _context.Specialities
                .Include(s => s.Themes)
                .Include(s => s.ThemeSettings) // Подгружаем настройки кол-ва вопросов
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public void Add(Speciality speciality) => _context.Specialities.Add(speciality);
        public void Remove(Speciality speciality) => _context.Specialities.Remove(speciality);
    }
}
