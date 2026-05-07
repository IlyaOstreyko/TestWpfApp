using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using TestWpfApp.Data.Context;
using TestWpfApp.Data.DataModels;
using TestWpfApp.MigrationOldQuestions;

namespace TestWpfApp.Service
{
    public class MigrationOldTableService
    {
        private readonly ApplicationContext _newDb;

        // Внедряем основной контекст через конструктор
        public MigrationOldTableService(ApplicationContext newDb)
        {
            _newDb = newDb;
        }

        public async Task MigrateDataAsync(string oldDbPath)
        {
            // 1. Настраиваем подключение к старой базе
            var legacyOptions = new DbContextOptionsBuilder<LegacyContext>()
                .UseSqlite($"Data Source={oldDbPath}")
                .Options;

            using var oldDb = new LegacyContext(legacyOptions);

            // 2. Получаем данные из старой базы
            try
            {
                var oldQuestions = await oldDb.TestQuestionDataModels
                    .AsNoTracking()
                    .ToListAsync();
                MigrateDataAsync(oldQuestions, _newDb);
                // Дальнейшая логика обработки данных
            }
            catch (OperationCanceledException ex)
            {
                // Обработка случая, если операция была отменена (например, через CancellationToken)
                Console.WriteLine($"Запрос был отменен: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Обработка всех остальных ошибок (БД недоступна, ошибка маппинга и т.д.)
                // В реальном проекте здесь должен быть логгер, например: _logger.LogError(ex, "Ошибка при получении данных");
                Console.WriteLine($"Произошла ошибка при чтении из старой БД: {ex.Message}");

                // Важно: если этот метод должен прервать выполнение, используйте 'throw;'
                throw;
            }



        }

        private byte[]? ConvertPathToBytes(string? path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            try
            {
                return File.ReadAllBytes(path);
            }
            catch
            {
                return null; // Или логируем ошибку доступа к файлу
            }
        }
        public async Task MigrateDataAsync(List<OldQuestion> oldQuestions, ApplicationContext _newDb)
        {
            // --- ЭТАП 1: ОБРАБОТКА ТЕМ ---

            // Собираем все уникальные названия тем из входящего списка (исключая null и пустые)
            var incomingThemeNames = oldQuestions
                .Select(q => q.NameTheme?.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            // Загружаем из базы уже существующие темы с такими именами
            var existingThemes = await _newDb.ThemeDataModels
                .Where(t => incomingThemeNames.Contains(t.NameTheme))
                .ToListAsync();

            var existingThemeNamesMap = existingThemes.ToDictionary(t => t.NameTheme, t => t);

            // Определяем, каких тем еще нет в базе
            var themesToInsert = incomingThemeNames
                .Where(name => !existingThemeNamesMap.ContainsKey(name))
                .Select(name => new ThemeDataModel { NameTheme = name })
                .ToList();

            if (themesToInsert.Any())
            {
                _newDb.ThemeDataModels.AddRange(themesToInsert);
                await _newDb.SaveChangesAsync(); // Сохраняем, чтобы получить ID для новых тем

                // Добавляем новые созданные темы в общий словарь маппинга
                foreach (var theme in themesToInsert)
                {
                    existingThemeNamesMap.Add(theme.NameTheme, theme);
                }
            }

            // --- ЭТАП 2: ОБРАБОТКА ВОПРОСОВ ---

            // Для проверки уникальности вопроса создаем вспомогательный метод генерации ключа
            // Ключ: Текст вопроса + ID темы + Правильный ответ + 3 неправильных
            string GenerateQuestionKey(string? name, int? themeId, string? c1, string? i1, string? i2, string? i3)
            {
                return $"{name?.Trim()}|{themeId}|{c1?.Trim()}|{i1?.Trim()}|{i2?.Trim()}|{i3?.Trim()}".ToLower();
            }

            // Получаем ID всех задействованных тем
            var themeIds = existingThemeNamesMap.Values.Select(t => t.ThemeId).ToList();

            // Загружаем существующие вопросы для этих тем, чтобы избежать дублей с БД
            var existingQuestionsKeys = await _newDb.TestQuestionDataModels
                .Where(q => themeIds.Contains(q.ThemeId))
                .Select(q => new { q.NameQuestion, q.ThemeId, q.NameAnswerCorrect1, q.NameAnswerIncorrect1, q.NameAnswerIncorrect2, q.NameAnswerIncorrect3 })
                .AsNoTracking()
                .ToListAsync();

            var duplicateCheckSet = existingQuestionsKeys
                .Select(q => GenerateQuestionKey(q.NameQuestion, q.ThemeId, q.NameAnswerCorrect1, q.NameAnswerIncorrect1, q.NameAnswerIncorrect2, q.NameAnswerIncorrect3))
                .ToHashSet();

            var questionsToInsert = new List<TestQuestionDataModel>();

            foreach (var old in oldQuestions)
            {
                var themeName = old.NameTheme?.Trim();
                if (string.IsNullOrEmpty(themeName) || !existingThemeNamesMap.TryGetValue(themeName, out var theme))
                    continue;

                // Генерируем ключ для текущего вопроса из списка
                var currentKey = GenerateQuestionKey(
                    old.NameQuestion,
                    theme.ThemeId,
                    old.NameAnswerCorrect1,
                    old.NameAnswerIncorrect1,
                    old.NameAnswerIncorrect2,
                    old.NameAnswerIncorrect3);

                // Проверяем: нет ли его в базе И не добавили ли мы его уже в этом цикле (дубли в самом List<OldQuestion>)
                if (!duplicateCheckSet.Contains(currentKey))
                {
                    var newQuestion = new TestQuestionDataModel
                    {
                        ThemeId = theme.ThemeId,
                        NameQuestion = old.NameQuestion,
                        NameArticle = old.NameArticle,
                        ImageQuestion = old.ImageQuestion,
                        NameAnswerCorrect1 = old.NameAnswerCorrect1,
                        NameAnswerIncorrect1 = old.NameAnswerIncorrect1,
                        NameAnswerIncorrect2 = old.NameAnswerIncorrect2,
                        NameAnswerIncorrect3 = old.NameAnswerIncorrect3
                        // ImageQuestionBytes можно заполнить здесь, если есть логика конвертации
                    };

                    questionsToInsert.Add(newQuestion);
                    duplicateCheckSet.Add(currentKey); // Помечаем как добавленный
                }
            }

            // Финальное сохранение вопросов
            if (questionsToInsert.Any())
            {
                try
                {
                    Console.WriteLine($"Попытка сохранить {questionsToInsert.Count} вопросов...");
                    _newDb.TestQuestionDataModels.AddRange(questionsToInsert);
                    int result = await _newDb.SaveChangesAsync();
                    Console.WriteLine($"Успешно сохранено: {result} строк.");
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                {
                    // Ошибка базы данных (нарушение ключей, типов данных и т.д.)
                    Console.WriteLine($"Ошибка БД: {dbEx.Message}");
                    Console.WriteLine($"Детали: {dbEx.InnerException?.Message}");
                }
                catch (Exception ex)
                {
                    // Любая другая ошибка
                    Console.WriteLine($"Общая ошибка при сохранении: {ex.Message}");
                }
            }
        }
    }
}
