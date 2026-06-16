using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWpfApp.Data.DataModels;

namespace TestWpfApp.Data.Interfaces
{
    public interface ISpecialityThemeSettingRepository
    {
        List<SpecialityThemeSetting> GetBySpecialityId(int specialityId);
    }
}
