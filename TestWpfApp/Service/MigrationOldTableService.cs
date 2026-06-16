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

        public async Task MigrateDataAsync(string oldDbPath, string? specialityName)
        {
            // 1. Настраиваем подключение к старой базе
            var legacyOptions = new DbContextOptionsBuilder<LegacyContext>()
                .UseSqlite($"Data Source={oldDbPath}")
                .Options;

            using var oldDb = new LegacyContext(legacyOptions);

            // 2. Получаем данные из старой базы
            try
            {
                var oldQuestions = await oldDb.TestQuestionDataModels.AsNoTracking().ToListAsync();

                // Передаем список вопросов и имя специальности в основной метод обработки
                await ProcessMigrationAsync(oldQuestions, specialityName);
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
        private async Task ProcessMigrationAsync(
    List<OldQuestion> oldQuestions,
    string? specialityName)
        {
            if (oldQuestions == null)
            {
                throw new ArgumentNullException(nameof(oldQuestions));
            }

            if (!oldQuestions.Any())
            {
                return;
            }

            specialityName = specialityName?.Trim();

            await using var transaction =
                await _newDb.Database.BeginTransactionAsync();

            try
            {
                // =========================================================
                // ЭТАП 0. СПЕЦИАЛЬНОСТЬ
                // =========================================================

                Speciality? targetSpeciality = null;

                if (!string.IsNullOrWhiteSpace(specialityName))
                {
                    targetSpeciality = await _newDb.Specialities
                        .Include(s => s.Themes)
                        .FirstOrDefaultAsync(s =>
                            s.Name.ToLower() == specialityName.ToLower());

                    // Создаем специальность если ее нет
                    if (targetSpeciality == null)
                    {
                        targetSpeciality = new Speciality
                        {
                            Name = specialityName
                        };

                        _newDb.Specialities.Add(targetSpeciality);

                        await _newDb.SaveChangesAsync();
                    }
                }

                // =========================================================
                // ЭТАП 1. ТЕМЫ
                // =========================================================

                var incomingThemeNames = oldQuestions
                    .Select(q => q.NameTheme?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!incomingThemeNames.Any())
                {
                    throw new InvalidOperationException(
                        "В старой базе не найдено ни одной темы.");
                }

                // Загружаем существующие темы
                var existingThemes = await _newDb.Themes
                    .Include(t => t.Specialities)
                    .Where(t => incomingThemeNames.Contains(t.Name))
                    .ToListAsync();

                var themeMap = existingThemes.ToDictionary(
                    t => t.Name,
                    t => t,
                    StringComparer.OrdinalIgnoreCase);

                // Создаем отсутствующие темы
                foreach (var themeName in incomingThemeNames)
                {
                    if (themeMap.ContainsKey(themeName))
                    {
                        continue;
                    }

                    var newTheme = new Theme
                    {
                        Name = themeName
                    };

                    _newDb.Themes.Add(newTheme);

                    themeMap.Add(themeName, newTheme);
                }

                // Сохраняем новые темы чтобы получить ID
                await _newDb.SaveChangesAsync();

                // =========================================================
                // ЭТАП 2. ПРИВЯЗКА ТЕМ К СПЕЦИАЛЬНОСТИ
                // =========================================================

                if (targetSpeciality != null)
                {
                    foreach (var theme in themeMap.Values)
                    {
                        theme.Specialities ??= new List<Speciality>();

                        bool alreadyLinked = theme.Specialities
                            .Any(s => s.SpecialityId ==
                                      targetSpeciality.SpecialityId);

                        if (!alreadyLinked)
                        {
                            theme.Specialities.Add(targetSpeciality);
                        }
                    }

                    await _newDb.SaveChangesAsync();
                }

                // =========================================================
                // ЭТАП 3. ПОДГОТОВКА ПРОВЕРКИ ДУБЛЕЙ ВОПРОСОВ
                // =========================================================

                static string GenerateQuestionKey(
                    string? question,
                    int themeId,
                    string? correct,
                    string? incorrect1,
                    string? incorrect2,
                    string? incorrect3)
                {
                    return string.Join("|",
                        question?.Trim().ToLower() ?? string.Empty,
                        themeId,
                        correct?.Trim().ToLower() ?? string.Empty,
                        incorrect1?.Trim().ToLower() ?? string.Empty,
                        incorrect2?.Trim().ToLower() ?? string.Empty,
                        incorrect3?.Trim().ToLower() ?? string.Empty);
                }

                var themeIds = themeMap.Values
                    .Select(t => t.ThemeId)
                    .Distinct()
                    .ToList();

                var existingQuestionKeys = await _newDb.TestQuestions
                    .AsNoTracking()
                    .Where(q => themeIds.Contains(q.ThemeId))
                    .Select(q => new
                    {
                        q.NameQuestion,
                        q.ThemeId,
                        q.NameAnswerCorrect1,
                        q.NameAnswerIncorrect1,
                        q.NameAnswerIncorrect2,
                        q.NameAnswerIncorrect3
                    })
                    .ToListAsync();

                var duplicateHashSet = existingQuestionKeys
                    .Select(q => GenerateQuestionKey(
                        q.NameQuestion,
                        q.ThemeId,
                        q.NameAnswerCorrect1,
                        q.NameAnswerIncorrect1,
                        q.NameAnswerIncorrect2,
                        q.NameAnswerIncorrect3))
                    .ToHashSet();

                // =========================================================
                // ЭТАП 4. СОЗДАНИЕ ВОПРОСОВ
                // =========================================================

                var questionsToInsert = new List<TestQuestion>();

                foreach (var oldQuestion in oldQuestions)
                {
                    try
                    {
                        string? themeName = oldQuestion.NameTheme?.Trim();

                        if (string.IsNullOrWhiteSpace(themeName))
                        {
                            continue;
                        }

                        if (!themeMap.TryGetValue(themeName, out var theme))
                        {
                            continue;
                        }

                        // Пропускаем полностью пустые вопросы
                        if (string.IsNullOrWhiteSpace(oldQuestion.NameQuestion))
                        {
                            continue;
                        }

                        string duplicateKey = GenerateQuestionKey(
                            oldQuestion.NameQuestion,
                            theme.ThemeId,
                            oldQuestion.NameAnswerCorrect1,
                            oldQuestion.NameAnswerIncorrect1,
                            oldQuestion.NameAnswerIncorrect2,
                            oldQuestion.NameAnswerIncorrect3);

                        // Проверка дублей
                        if (duplicateHashSet.Contains(duplicateKey))
                        {
                            continue;
                        }

                        var question = new TestQuestion
                        {
                            ThemeId = theme.ThemeId,

                            NameQuestion =
                                oldQuestion.NameQuestion?.Trim(),

                            NameTheme =
                                oldQuestion.NameTheme?.Trim(),

                            NameArticle =
                                oldQuestion.NameArticle?.Trim(),

                            ImageQuestion =
                                oldQuestion.ImageQuestion,

                            NameAnswerCorrect1 =
                                oldQuestion.NameAnswerCorrect1?.Trim(),

                            NameAnswerIncorrect1 =
                                oldQuestion.NameAnswerIncorrect1?.Trim(),

                            NameAnswerIncorrect2 =
                                oldQuestion.NameAnswerIncorrect2?.Trim(),

                            NameAnswerIncorrect3 =
                                oldQuestion.NameAnswerIncorrect3?.Trim()
                        };

                        questionsToInsert.Add(question);

                        duplicateHashSet.Add(duplicateKey);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Ошибка обработки вопроса: {ex.Message}");
                    }
                }

                // =========================================================
                // ЭТАП 5. СОХРАНЕНИЕ ВОПРОСОВ
                // =========================================================

                if (questionsToInsert.Any())
                {
                    _newDb.TestQuestions.AddRange(questionsToInsert);

                    await _newDb.SaveChangesAsync();
                }

                // =========================================================
                // COMMIT
                // =========================================================

                await transaction.CommitAsync();

                Console.WriteLine(
                    $"Миграция завершена успешно. " +
                    $"Импортировано тем: {themeMap.Count}, " +
                    $"вопросов: {questionsToInsert.Count}");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

                Console.WriteLine(
                    $"Ошибка БД при миграции: {dbEx.Message}");

                Console.WriteLine(
                    $"InnerException: {dbEx.InnerException?.Message}");

                throw new Exception(
                    "Ошибка сохранения данных в базе.");
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync();

                Console.WriteLine("Операция миграции отменена.");

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine(
                    $"Критическая ошибка миграции: {ex}");

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
            var existingThemes = await _newDb.Themes
                .Where(t => incomingThemeNames.Contains(t.Name))
                .ToListAsync();

            var existingThemeNamesMap = existingThemes.ToDictionary(t => t.Name, t => t);

            // Определяем, каких тем еще нет в базе
            var themesToInsert = incomingThemeNames
                .Where(name => !existingThemeNamesMap.ContainsKey(name))
                .Select(name => new Theme { Name = name })
                .ToList();

            if (themesToInsert.Any())
            {
                _newDb.Themes.AddRange(themesToInsert);
                await _newDb.SaveChangesAsync(); // Сохраняем, чтобы получить ID для новых тем

                // Добавляем новые созданные темы в общий словарь маппинга
                foreach (var theme in themesToInsert)
                {
                    existingThemeNamesMap.Add(theme.Name, theme);
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
            var existingQuestionsKeys = await _newDb.TestQuestions
                .Where(q => themeIds.Contains(q.ThemeId))
                .Select(q => new { q.NameQuestion, q.ThemeId, q.NameAnswerCorrect1, q.NameAnswerIncorrect1, q.NameAnswerIncorrect2, q.NameAnswerIncorrect3 })
                .AsNoTracking()
                .ToListAsync();

            var duplicateCheckSet = existingQuestionsKeys
                .Select(q => GenerateQuestionKey(q.NameQuestion, q.ThemeId, q.NameAnswerCorrect1, q.NameAnswerIncorrect1, q.NameAnswerIncorrect2, q.NameAnswerIncorrect3))
                .ToHashSet();

            var questionsToInsert = new List<TestQuestion>();

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
                    var newQuestion = new TestQuestion
                    {
                        ThemeId = theme.ThemeId,
                        NameQuestion = old.NameQuestion,
                        NameTheme = old.NameTheme,
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
                    _newDb.TestQuestions.AddRange(questionsToInsert);
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
