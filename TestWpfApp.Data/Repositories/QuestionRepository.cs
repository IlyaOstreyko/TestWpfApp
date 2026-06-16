using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestWpfApp.Data.Context;
using TestWpfApp.Data.DataModels;
using TestWpfApp.Data.Interfaces;

namespace TestWpfApp.Data.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ApplicationContext _context;
        //private readonly IMapper _mapper;

        public QuestionRepository(ApplicationContext context)
        {
            //string currentDirectory = Directory.GetCurrentDirectory();
            _context = context;
            //var config = new MapperConfiguration(cfg => cfg.CreateMap<TestQuestion, TestQuestionDataModel>().ReverseMap()
            //.ForMember("FullImageQuestion", opt => opt.MapFrom(c => currentDirectory + "\\" + c.ImageQuestion)));
            ////var config = new MapperConfiguration(cfg => cfg.CreateMap<TestQuestion, TestQuestionDataModel>().ReverseMap());
            //_mapper = new Mapper(config);
        }

        public void Create(TestQuestion item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            try
            {
                _context.TestQuestions.Add(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении вопроса: {ex.Message}");
                throw;
            }
        }
        //public void Create(TestQuestion itemQuestion)
        //{
        //    TestQuestionDataModel testQuestionDM = _mapper.Map<TestQuestionDataModel>(itemQuestion);
        //    _applicationContext.TestQuestionDataModels.Add(testQuestionDM);
        //    _applicationContext.SaveChanges();
        //    itemQuestion.QuestionId = (int)testQuestionDM.QuestionId;
        //    //int returnValue = (int)this.ad.InsertCommand.ExecuteScalar();
        //}
        public int Create(List<TestQuestion> questions)
        {
            if (questions == null) throw new ArgumentNullException(nameof(questions));
            int countQuestions = 0;
            try
            {
                _context.TestQuestions.AddRange(questions);
                countQuestions = questions.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении списка вопросов: {ex.Message}");
                throw;
            }
            return countQuestions;
        }

        public void Delete(int id)
        {
            try
            {
                var entity = _context.TestQuestions.Find(id);
                if (entity != null)
                    _context.TestQuestions.Remove(entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении вопроса: {ex.Message}");
                throw;
            }
        }
        public TestQuestion? GetTracked(int id)
        {
            return _context.TestQuestions
                .FirstOrDefault(x => x.QuestionId == id);
        }
        public TestQuestion Get(int id)
        {
            TestQuestion? testQuestion = new TestQuestion();
            try
            {
                //TestQuestion? questionDM =  _context.TestQuestions.AsNoTracking().FirstOrDefault(x => x.QuestionId == id);
                TestQuestion? questionDM = _context.TestQuestions.FirstOrDefault(x => x.QuestionId == id);
                if (questionDM != null)
                {
                    testQuestion = questionDM;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении вопроса: {ex.Message}");
                throw;
            }
            return testQuestion;
        }

        public List<TestQuestion> GetAll()
        {
            List<TestQuestion> testQuestions = new List<TestQuestion>();
            try
            {
                List<TestQuestion> questionsDM =  _context.TestQuestions.AsNoTracking().ToList();
                if (questionsDM is null)
                {
                    return testQuestions;
                }
                else { testQuestions = questionsDM; }                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении всех вопросов: {ex.Message}");
                throw;
            }
            return testQuestions;
        }

        public List<TestQuestion> GetQuestionsInTheme(string nameTheme)
        {
            List<TestQuestion> testQuestions = new List<TestQuestion>();
            try
            {
                IQueryable<TestQuestion> query = _context.TestQuestions.AsNoTracking();
                query = query.Where(u => u.NameTheme == nameTheme);
                testQuestions = query.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении вопросов по теме: {ex.Message}");
                throw;
            }
            return testQuestions;
        }

        public List<TestQuestion> GetRndQuestionsInTheme(string nameTheme, int number)
        {
            List<TestQuestion> testQuestions = new List<TestQuestion>();
            try
            {
                IQueryable<TestQuestion> query = _context.TestQuestions.AsNoTracking();
                query = query.Where(u => u.NameTheme == nameTheme).OrderBy(_ => EF.Functions.Random()).Take(number);
                testQuestions = query.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении случайных вопросов по теме: {ex.Message}");
                throw;
            }
            return testQuestions;
        }
        public bool CheckQuestionsOnNameQuestion(string nameQuestion)
        {
            if (string.IsNullOrWhiteSpace(nameQuestion)) return false;
            try
            {
                return _context.TestQuestions
                .AsNoTracking()
                    .Any(q => q.NameQuestion == nameQuestion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке вопроса по имени: {ex.Message}");
                throw;
            }
        }
        public bool CheckQuestions(TestQuestion question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            try
            {
                return _context.TestQuestions.AsNoTracking().Any(u =>
            u.NameQuestion == question.NameQuestion &&
            u.NameTheme == question.NameTheme &&
            u.ImageQuestion == question.ImageQuestion &&
            u.NameAnswerCorrect1 == question.NameAnswerCorrect1 &&
            u.NameAnswerIncorrect1 == question.NameAnswerIncorrect1 &&
            u.NameAnswerIncorrect2 == question.NameAnswerIncorrect2 &&
            u.NameAnswerIncorrect3 == question.NameAnswerIncorrect3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке вопроса по всем параметрам: {ex.Message}");
                throw;
            }
        }

        public int? GetIdQuestions(TestQuestion question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            try
            {
                var questionInBase = _context.TestQuestions.AsNoTracking().FirstOrDefault(u =>
                u.NameQuestion == question.NameQuestion &&
                u.NameTheme == question.NameTheme &&
                u.NameAnswerCorrect1 == question.NameAnswerCorrect1 &&
                u.NameAnswerIncorrect1 == question.NameAnswerIncorrect1 &&
                u.NameAnswerIncorrect2 == question.NameAnswerIncorrect2 &&
                u.NameAnswerIncorrect3 == question.NameAnswerIncorrect3);
                if (questionInBase is null)
                {
                    return null;
                }
                return questionInBase.QuestionId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске вопроса по всем параметрам: {ex.Message}");
                throw;
            }
        }

        public TestQuestion GetQuestionsOnNameQuestion(string nameQuestion)
        {
            if (string.IsNullOrWhiteSpace(nameQuestion)) return new TestQuestion();            
            try
            {
                var questionDM = _context.TestQuestions.AsNoTracking().FirstOrDefault(u => u.NameQuestion == nameQuestion);
                if (questionDM is null)
                {
                    return new TestQuestion();
                }
                return questionDM;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске вопроса по названию вопроса: {ex.Message}");
                throw;
            }
        }

        public List<string> GetThemesInSpeciality(string nameSpeciality)
        {
            if (string.IsNullOrWhiteSpace(nameSpeciality)) return new List<string>();
            try
            {
                var themes = _context.TestQuestions.AsNoTracking().Where(u => u.NameSpeciality == nameSpeciality).Select(n => n.NameTheme).Distinct().ToList();
                if (themes is null)
                {
                    return new List<string>();
                }
                return themes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске темы по низванию темы: {ex.Message}");
                throw;
            }
        }

        public List<string> GetThemes()
        {
            var themes = new List<string>();
            try
            {
                themes = _context.TestQuestions.AsNoTracking().Select(n => n.NameTheme).Distinct().ToList();
                if (themes is null)
                {
                    return new List<string>();
                }
                return themes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения всех тем: {ex.Message}");
                throw;
            }
        }

        public void Update(TestQuestion question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            try
            {
                _context.TestQuestions.Update(question);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении вопроса: {ex.Message}");
                throw;
            }
        }

        public int GetNumberQuestionInTheme(string nameTheme)
        {
            try
            {
                return _context.TestQuestions
                    .Count(q => q.NameTheme == nameTheme);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при подсчёте вопросов: {ex.Message}");
                throw;
            }
        }
    }
}
