using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWpfApp.Models;

namespace TestWpfApp.Interfaces
{
    public interface IWindowService
    {
        // Базовые окна
        void ShowAddQuestion();
        void ShowEditQuestions(bool isEdit);
        void ShowThemes();
        string? ShowNewTheme();
        void CloseApplication();
        // Результаты теста
        void ShowResults(
            List<TestQuestion> testQuestions,
            UserInfo userInfo,
            List<Result> results,
            bool isTest,
            bool timeOver);
        // Пример
        void ShowSample();
        // Просмотр/показ вопросов
        void ShowQuestion(TestQuestion testQuestion);
        void ShowQuestion(List<TestQuestion> testQuestions, UserInfo userInfo, List<Result> results);
        void ShowQuestion(List<TestQuestion> testQuestions, bool isTest, List<Result> results);
        // Окно пользователя
        void ShowUser(List<TestQuestion> questions, List<Result> results);
    }
}
