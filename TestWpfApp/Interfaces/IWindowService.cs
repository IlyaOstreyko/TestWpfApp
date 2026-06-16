using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWpfApp.Data.DataModels;
using TestWpfApp.Models;

namespace TestWpfApp.Interfaces
{
    public interface IWindowService
    {
        // Базовые окна
        void ShowAddQuestion();
        void ShowEditQuestion(AddQuestionParameters parameters);
        //void ShowEditQuestions(bool isEdit);
        void ShowThemes();
        string? ShowNewTheme();
        void CloseApplication();
        // Результаты теста
        void ShowResults(
            List<TestQuestionVM> testQuestions,
            UserInfo userInfo,
            List<Result> results,
            bool isTest,
            bool timeOver);
        // Пример
        void ShowSample();
        // Просмотр/показ вопросов
        //void ShowQuestion(TestQuestionVM testQuestion);
        void ShowQuestion(List<TestQuestionVM> testQuestions, UserInfo userInfo, List<Result> results);
        void ShowQuestion(List<TestQuestionVM> testQuestions, bool isTest, List<Result> results);
        // Окно пользователя
        void ShowUser(List<TestQuestionVM> questions, List<Result> results, string spec);
    }
}
