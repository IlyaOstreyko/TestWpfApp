using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWpfApp.Data.DataModels;

namespace TestWpfApp.Data.Interfaces
{
    public interface IQuestionRepository
    {
        List<TestQuestion> GetAll();
        List<TestQuestion> GetQuestionsInTheme(string nameTheme);
        TestQuestion? GetTracked(int id);
        List<TestQuestion> GetRndQuestionsInTheme(string nameTheme, int number);
        List<string> GetThemes();
        List<string> GetThemesInSpeciality(string nameSpeciality);
        int GetNumberQuestionInTheme(string nameTheme);
        bool CheckQuestionsOnNameQuestion(string nameQuestion);
        bool CheckQuestions(TestQuestion question);
        int? GetIdQuestions(TestQuestion question);
        TestQuestion GetQuestionsOnNameQuestion(string nameQuestion);
        TestQuestion Get(int id);
        void Create(TestQuestion item);
        int Create(List<TestQuestion> items);
        void Update(TestQuestion item);
        void Delete(int id);
    }
}
