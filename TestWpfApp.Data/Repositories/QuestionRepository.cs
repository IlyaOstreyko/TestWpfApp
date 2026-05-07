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

        public void Create(TestQuestionDataModel item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            try
            {
                _context.TestQuestionDataModels.Add(item);
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
        public int Create(List<TestQuestionDataModel> questions)
        {
            if (questions == null) throw new ArgumentNullException(nameof(questions));
            int countQuestions = 0;
            try
            {
                _context.TestQuestionDataModels.AddRange(questions);
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
                var entity = _context.TestQuestionDataModels.Find(id);
                if (entity != null)
                    _context.TestQuestionDataModels.Remove(entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении вопроса: {ex.Message}");
                throw;
            }
        }

        public TestQuestionDataModel Get(int id)
        {
            TestQuestionDataModel? testQuestion = new TestQuestionDataModel();
            try
            {
                TestQuestionDataModel? questionDM =  _context.TestQuestionDataModels.AsNoTracking().FirstOrDefault(x => x.QuestionId == id);
                if (questionDM == null)
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

        public List<TestQuestionDataModel> GetAll()
        {
            List<TestQuestionDataModel> testQuestions = new List<TestQuestionDataModel>();
            try
            {
                List<TestQuestionDataModel> questionsDM =  _context.TestQuestionDataModels.AsNoTracking().ToList();
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

        public List<TestQuestionDataModel> GetQuestionsInTheme(string nameTheme)
        {
            List<TestQuestionDataModel> testQuestions = new List<TestQuestionDataModel>();
            try
            {
                IQueryable<TestQuestionDataModel> query = _context.TestQuestionDataModels.AsNoTracking();
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

        public List<TestQuestionDataModel> GetRndQuestionsInTheme(string nameTheme, int number)
        {
            List<TestQuestionDataModel> testQuestions = new List<TestQuestionDataModel>();
            try
            {
                IQueryable<TestQuestionDataModel> query = _context.TestQuestionDataModels.AsNoTracking();
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

        #region randomShafle
        private Random rng = new Random();
        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        #endregion
        public bool CheckQuestionsOnNameQuestion(string nameQuestion)
        {
            if (string.IsNullOrWhiteSpace(nameQuestion)) return false;
            try
            {
                return _context.TestQuestionDataModels
                .AsNoTracking()
                    .Any(q => q.NameQuestion == nameQuestion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке вопроса по имени: {ex.Message}");
                throw;
            }
        }
        public bool CheckQuestions(TestQuestionDataModel question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            try
            {
                return _context.TestQuestionDataModels.AsNoTracking().Any(u =>
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

        public int? GetIdQuestions(TestQuestionDataModel question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            try
            {
                var questionInBase = _context.TestQuestionDataModels.AsNoTracking().FirstOrDefault(u =>
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

        public TestQuestionDataModel GetQuestionsOnNameQuestion(string nameQuestion)
        {
            if (string.IsNullOrWhiteSpace(nameQuestion)) return new TestQuestionDataModel();            
            try
            {
                var questionDM = _context.TestQuestionDataModels.AsNoTracking().FirstOrDefault(u => u.NameQuestion == nameQuestion);
                if (questionDM is null)
                {
                    return new TestQuestionDataModel();
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
                var themes = _context.TestQuestionDataModels.AsNoTracking().Where(u => u.NameSpeciality == nameSpeciality).Select(n => n.NameTheme).Distinct().ToList();
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
                themes = _context.TestQuestionDataModels.AsNoTracking().Select(n => n.NameTheme).Distinct().ToList();
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

        public void Update(TestQuestionDataModel question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            try
            {
                _context.TestQuestionDataModels.Update(question);
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
                return _context.TestQuestionDataModels
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
