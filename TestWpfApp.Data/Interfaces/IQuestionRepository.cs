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
        List<TestQuestionDataModel> GetAll();
        List<TestQuestionDataModel> GetQuestionsInTheme(string nameTheme);
        List<TestQuestionDataModel> GetRndQuestionsInTheme(string nameTheme, int number);
        List<string> GetThemes();
        List<string> GetThemesInSpeciality(string nameSpeciality);
        int GetNumberQuestionInTheme(string nameTheme);
        bool CheckQuestionsOnNameQuestion(string nameQuestion);
        bool CheckQuestions(TestQuestionDataModel question);
        int? GetIdQuestions(TestQuestionDataModel question);
        TestQuestionDataModel GetQuestionsOnNameQuestion(string nameQuestion);
        TestQuestionDataModel Get(int id);
        void Create(TestQuestionDataModel item);
        int Create(List<TestQuestionDataModel> items);
        void Update(TestQuestionDataModel item);
        void Delete(int id);
    }
}
