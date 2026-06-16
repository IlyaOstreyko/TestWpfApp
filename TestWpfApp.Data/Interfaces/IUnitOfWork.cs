using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IQuestionRepository Questions { get; }
        ISpecialityRepository Specialities { get; }
        IThemeRepository Themes { get; }
        IGroupRepository Groups { get; }
        ISpecialityThemeSettingRepository SpecialityThemeSettings { get; }
        Task<int> SaveAsync();
    }
}
