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
        Task<int> SaveAsync();
    }
}
