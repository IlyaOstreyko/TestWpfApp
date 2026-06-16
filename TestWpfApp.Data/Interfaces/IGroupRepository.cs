using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWpfApp.Data.DataModels;

namespace TestWpfApp.Data.Interfaces
{
    public interface IGroupRepository
    {
        IEnumerable<Group> GetAll();

        Group? GetById(int id);

        Group? GetByName(string name);

        void Add(Group group);

        void Update(Group group);

        void Delete(int id);

        bool Exists(int id);
    }
}
