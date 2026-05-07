using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.Windows;
using TestWpfApp.Models;
using TestWpfApp.Data.Interfaces;
using AutoMapper;
using TestWpfApp.Data.DataModels;

namespace TestWpfApp.Service
{
    public class QuestionFileImporter
    {
        private readonly IQuestionRepository db;
        public OpenFileDialog openFileDialog;
        public FileInfo FileInf;
        public event Action<int> ProgressChanged; // 🔔 Событие для уведомления о прогрессе
        public QuestionFileImporter(IQuestionRepository repository)
        {
            db = repository;
        }

        /// <summary>
        /// Основной метод, который читает вопросы из файла и добавляет их в БД.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу (.txt, .wop)</param>
        public void AddQuestionsFromFile(IMapper _mapper)
        {
            openFileDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "(*.TXT, *.WOP)|*.txt; *.wop|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    FileInf = new FileInfo(openFileDialog.FileName);
                }
                List<TestQuestion> testQuestions = new List<TestQuestion>();
                testQuestions = ReadQuestionsFromFile(FileInf.FullName);

                if (testQuestions.Count == 0)
                {
                    MessageBox.Show("Произошла ошибка ошибка чтения файла. Вопросы не найдены.", "Ошибка чтения файла", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int x = 0;

                int addedCount = 0;
                int total = testQuestions.Count;
                foreach (TestQuestion testQuestion in testQuestions)
                {
                    var question = _mapper.Map<TestQuestionDataModel>(testQuestion);
                    if (!db.CheckQuestions(question))
                    {
                        db.Create(question);
                        x++;
                        addedCount++;
                    }

                    else
                    {
                        MessageBox.Show("Данный вопрос уже существует в базе:" + "\r\n" + testQuestion.NameQuestion, "Ошибка заполнения базы", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    int progressPercent = (int)((x + 1) / (double)total * 100);
                    ProgressChanged?.Invoke(progressPercent);
                }

                MessageBox.Show("Вопросы успешно добавлены." + "\r\n" + "Вопросов добавленно в базу: " + x + "\r\n" + "Из найденных в файле: " + testQuestions.Count, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Считывает и разбирает текстовый файл в список вопросов.
        /// </summary>
        private List<TestQuestion> ReadQuestionsFromFile(string path)
        {
            List<TestQuestion> testQuestions = new List<TestQuestion>();
            List<string> themes = new List<string>();
            string w = "";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string allText = File.ReadAllText(path, Encoding.GetEncoding("windows-1251"));
            allText = allText.Replace("        ", "");
            allText = allText.Replace("       ", "");
            allText = allText.Replace("      ", "");
            allText = allText.Replace("     ", "");
            allText = allText.Replace("    ", "");
            allText = allText.Replace("   ", "");
            allText = allText.Replace("  ", "");
            allText = allText.Replace("\r\n\r\n\r\n\r\n\r\n", "\r\n\r\n");
            allText = allText.Replace("\r\n\r\n\r\n\r\n", "\r\n\r\n");
            allText = allText.Replace("\r\n\r\n\r\n", "\r\n\r\n");

            List<string> paragraphsWithSpace = allText.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> paragraphs = new List<string>();
            foreach (string x in paragraphsWithSpace)
            {
                if (!String.IsNullOrWhiteSpace(x))
                {
                    paragraphs.Add(x);
                }
            }

            bool startQuestion = false;

            for (int i = 0; i < paragraphs.Count; i++)
            {
                string currentParagraph = paragraphs[i].Trim();

                switch (currentParagraph[0])
                {
                    case 'W':
                        w = currentParagraph.Substring(2).Trim();
                        break;
                    case 'Л':
                        string currenеThem = currentParagraph.Trim();
                        themes.Add(currenеThem);
                        break;
                    case 'N':
                        startQuestion = true;
                        if (themes.Count == 0)
                        {
                            string input = "";
                            while (input == "")
                            {
                                input = Interaction.InputBox("Введите тему вопросов", "Не найдена тема для вопросов", "введите тему для текущего набора вопросов");
                            }
                            themes.Add("   " + input);
                        }

                        TestQuestion testQuestion = new TestQuestion();
                        try
                        {
                            testQuestion = StringOnQuestion(currentParagraph, path);
                        }
                        catch
                        {
                            MessageBox.Show("Не удалось преобразовать вопрос:" + "\r\n" + currentParagraph, "Ошибка добавления вопроса", MessageBoxButton.OK, MessageBoxImage.Error);
                            //Console.WriteLine("Вопрос не удалось преобразовать.");
                            break;
                        }

                        if (testQuestion.NameTheme == "")
                        {
                            try
                            {
                                testQuestion.NameTheme = themes.Where(x => x.Substring(0, 3) == "ЛЛЛ").FirstOrDefault();

                            }
                            catch
                            {
                                MessageBox.Show("Не удалось найти тему." + "\r\n" + "Выбрана тема по-умолчанию:" + "\r\n" + themes[0], "Ошибка поиска темы", MessageBoxButton.OK, MessageBoxImage.Information);
                                testQuestion.NameTheme = themes[0].Substring(3);
                            }

                        }
                        else
                        {
                            try
                            {
                                testQuestion.NameTheme = themes.Where(x => x.Substring(0, testQuestion.NameTheme.Length) == testQuestion.NameTheme).FirstOrDefault().Substring(testQuestion.NameTheme.Length + 1);
                            }
                            catch
                            {
                                MessageBox.Show("Не удалось найти тему:" + "\r\n" + testQuestion.NameTheme + "\r\n" + "выбрана тема по-умолчанию:" + "\r\n" + themes[0], "Ошибка поиска темы", MessageBoxButton.OK, MessageBoxImage.Information);
                                testQuestion.NameTheme = themes[0].Substring(3);
                            }
                        }
                        testQuestions.Add(testQuestion);
                        break;
                    default:
                        {
                            if (!startQuestion)
                            {
                                string currenеThem1 = "ЛЛЛ " + currentParagraph.Trim();
                                themes.Add(currenеThem1);
                                break;
                            }
                            else
                            {
                                MessageBox.Show("Не удалось считать вопрос:" + "\r\n" + currentParagraph, "Ошибка считывания вопроса", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            break;
                        }
                }
            }
            return testQuestions;
        }

        /// <summary>
        /// Преобразует блок текста (один вопрос) в объект TestQuestion.
        /// </summary>
        TestQuestion StringOnQuestion(string currentParagraph, string path)
        {
            TestQuestion testQuestion = new TestQuestion();
            int finishNoAndThemeAndArticle = currentParagraph.IndexOf("\r\n");
            string NoAndThemeAndArticle = currentParagraph.Substring(0, finishNoAndThemeAndArticle).Trim();
            string theme = "";
            string article = "";

            int startThemeAndArticle = NoAndThemeAndArticle.IndexOf('Л');

            if (startThemeAndArticle > -1)
            {
                string themeAndArticle = NoAndThemeAndArticle.Substring(startThemeAndArticle, NoAndThemeAndArticle.Length - 5).Trim(); ;
                int finishTheme = themeAndArticle.IndexOf(' ');
                if (finishTheme > -1)
                {
                    theme = themeAndArticle.Substring(0, finishTheme).Trim(); ;
                    article = themeAndArticle.Substring(finishTheme + 1).Trim(); ;
                }
                else
                {
                    theme = themeAndArticle;
                }
            }

            testQuestion.NameTheme = theme;
            testQuestion.NameArticle = article;

            int startImage = currentParagraph.IndexOf('{') + 1;

            int finishQuestion = currentParagraph.IndexOf("\r\n1.") - 1;

            if (startImage != 0)
            {
                int finishImage;
                finishImage = currentParagraph.IndexOf('}') - 1;
                string image = currentParagraph.Substring(startImage, finishImage - startImage + 1).Trim(); ;

                int startDirImage = 0;
                int lastDirImage = image.LastIndexOf(@"\"); ;
                string dirNameImage = image.Substring(startDirImage, lastDirImage);

                int startDir = 0;
                int lastDir = path.LastIndexOf(@"\") + 1; ;
                string dirName = path.Substring(startDir, lastDir) + image;

                string currentDir = Directory.GetCurrentDirectory();
                string targetPath = currentDir + @"\" + image;
                string newPath = currentDir + @"\" + dirNameImage;

                try
                {
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    //if (Directory.Exists(targetPath))
                    //{
                    //    targetPath += "1";
                    //    image += "1";
                    //}
                    File.Copy(dirName, targetPath, true);
                    testQuestion.ImageQuestion = image;
                }
                catch
                {
                    //MessageBox.Show("Картинка не найдена." + "\r\n" + image, "Ошибка копирования картинки", MessageBoxButton.OK, MessageBoxImage.Error);
                }


                testQuestion.NameQuestion = currentParagraph.Substring(finishImage + 2, finishQuestion - finishImage - 1).Trim();
            }
            else
            {
                int startQuestion = currentParagraph.IndexOf("\r\n") + 2;
                testQuestion.NameQuestion = currentParagraph.Substring(startQuestion, finishQuestion - startQuestion).Trim();
            }

            int startCorrect = currentParagraph.IndexOf("\r\n1.") + 4;
            int startInCorrect1 = currentParagraph.IndexOf("\r\n2.") + 4;
            int startInCorrect2 = currentParagraph.IndexOf("\r\n3.") + 4;
            int startInCorrect3 = currentParagraph.IndexOf("\r\n4.") + 4;

            testQuestion.NameAnswerCorrect1 = currentParagraph.Substring(startCorrect, startInCorrect1 - startCorrect - 2).Trim();
            testQuestion.NameAnswerIncorrect1 = currentParagraph.Substring(startInCorrect1, startInCorrect2 - startInCorrect1 - 2).Trim();
            testQuestion.NameAnswerIncorrect2 = currentParagraph.Substring(startInCorrect2, startInCorrect3 - startInCorrect2 - 2).Trim();
            testQuestion.NameAnswerIncorrect3 = currentParagraph.Substring(startInCorrect3).Trim();
            return testQuestion;
        }
    }
}
