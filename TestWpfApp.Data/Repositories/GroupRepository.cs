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
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationContext _context;

        public GroupRepository(ApplicationContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Group> GetAll()
        {
            return _context.Groups
                .AsNoTracking()
                .Include(g => g.Specialities)
                .OrderBy(g => g.Name)
                .ToList();
        }

        public Group? GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return _context.Groups
                .Include(g => g.Specialities)
                .FirstOrDefault(g => g.GroupId == id);
        }

        public Group? GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            string normalizedName = name.Trim().ToLower();

            return _context.Groups
                .Include(g => g.Specialities)
                .FirstOrDefault(g =>
                    g.Name != null &&
                    g.Name.ToLower() == normalizedName);
        }

        public void Add(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (string.IsNullOrWhiteSpace(group.Name))
            {
                throw new ArgumentException(
                    "Название группы не может быть пустым.");
            }

            string normalizedName = group.Name.Trim().ToLower();

            bool exists = _context.Groups
                .Any(g =>
                    g.Name != null &&
                    g.Name.ToLower() == normalizedName);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Группа с таким названием уже существует.");
            }

            group.Name = group.Name.Trim();

            _context.Groups.Add(group);
        }

        public void Update(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (group.GroupId <= 0)
            {
                throw new ArgumentException(
                    "Некорректный идентификатор группы.");
            }

            if (string.IsNullOrWhiteSpace(group.Name))
            {
                throw new ArgumentException(
                    "Название группы не может быть пустым.");
            }

            var existingGroup = _context.Groups
                .FirstOrDefault(g =>
                    g.GroupId == group.GroupId);

            if (existingGroup == null)
            {
                throw new InvalidOperationException(
                    "Группа не найдена.");
            }

            string normalizedName = group.Name.Trim().ToLower();

            bool duplicateExists = _context.Groups
                .Any(g =>
                    g.GroupId != group.GroupId &&
                    g.Name != null &&
                    g.Name.ToLower() == normalizedName);

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    "Группа с таким названием уже существует.");
            }

            existingGroup.Name = group.Name.Trim();
        }

        public void Delete(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Некорректный идентификатор группы.");
            }

            var group = _context.Groups
                .Include(g => g.Specialities)
                .FirstOrDefault(g => g.GroupId == id);

            if (group == null)
            {
                throw new InvalidOperationException(
                    "Группа не найдена.");
            }

            // Очищаем many-to-many связи
            if (group.Specialities != null)
            {
                group.Specialities.Clear();
            }

            _context.Groups.Remove(group);
        }

        public bool Exists(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            return _context.Groups
                .AsNoTracking()
                .Any(g => g.GroupId == id);
        }
    }
}
