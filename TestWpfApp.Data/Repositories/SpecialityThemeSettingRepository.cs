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
    public class SpecialityThemeSettingRepository : ISpecialityThemeSettingRepository
    {
        private readonly ApplicationContext _context;

        public SpecialityThemeSettingRepository(ApplicationContext context)
        {
            _context = context;
        }

        public List<SpecialityThemeSetting> GetBySpecialityId(int specialityId)
        {
            if (specialityId <= 0)
                return new List<SpecialityThemeSetting>();

            return _context.SpecialityThemeSettings
                .AsNoTracking()
                .Include(x => x.Theme)
                .Where(x => x.SpecialityId == specialityId)
                .ToList();
        }
    }
}
