using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWpfApp.Data.DataModels;

namespace TestWpfApp.Data.Interfaces
{
    public interface ISpecialityRepository
    {
        IEnumerable<Speciality> GetAll();
        string GetName(int idSpeciality);
        Speciality? GetTracked(int id);
        Task<List<Speciality>> GetAllForAdminAsync();
        void Add(Speciality speciality);
        void Remove(Speciality speciality);
    }
}
