using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWpfApp.Data.Context;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Data.Repositories;

namespace TestWpfApp.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        // Ленивая инициализация репозитория
        private IQuestionRepository? _questions;
        public IQuestionRepository Questions =>
            _questions ??= new QuestionRepository(_context);

        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
        }

        // Правильная async реализация
        public async Task<int> SaveAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при сохранении изменений: {ex.Message}");
                throw; // пробрасываем дальше, чтобы не скрывать ошибку
            }
        }

        // Правильный Dispose-паттерн
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
