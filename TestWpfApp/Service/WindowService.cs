using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TestWpfApp.Interfaces;
using TestWpfApp.Models;
using TestWpfApp.ViewModels;
using TestWpfApp.Views;

namespace TestWpfApp.Service
{
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public void ShowAddQuestion()
        {
            var vm = ActivatorUtilities.CreateInstance<AddQuestionViewModel>(
                _serviceProvider,
                null);

            var window = new AddQuestion(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            window.ShowDialog();
        }
        public void ShowEditQuestion(AddQuestionParameters parameters)
        {
            var vm =
                ActivatorUtilities.CreateInstance<AddQuestionViewModel>(
                    _serviceProvider,
                    parameters);

            var window = new AddQuestion(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation =
                    WindowStartupLocation.CenterOwner
            };

            window.ShowDialog();
        }

        public void ShowThemes()
        {
            new ShowThemes
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            }.ShowDialog();
        }

        public void CloseApplication()
        {
            Application.Current.Shutdown();
        }
        public void ShowResults(
            List<TestQuestionVM> testQuestions,
            UserInfo userInfo,
            List<Result> results,
            bool isTest,
            bool timeOver)
        {
            new Resaults(testQuestions, userInfo, results, isTest, timeOver)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            }.ShowDialog();
        }
        public void ShowSample()
        {
            new Sample
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            }.ShowDialog();
        }
        //public void ShowQuestion(TestQuestionVM testQuestion)
        //{
        //    new ShowQuestion(testQuestion)
        //    {
        //        Owner = Application.Current.MainWindow,
        //        WindowStartupLocation = WindowStartupLocation.CenterOwner
        //    }.ShowDialog();
        //}

        public void ShowQuestion(List<TestQuestionVM> testQuestions, UserInfo userInfo, List<Result> results)
        {
            new ShowQuestion(testQuestions, userInfo, results)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            }.ShowDialog();
        }

        public void ShowQuestion(List<TestQuestionVM> testQuestions, bool isTest, List<Result> results)
        {
            new ShowQuestion(testQuestions, isTest, results)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            }.ShowDialog();
        }

        public void ShowUser(List<TestQuestionVM> questions, List<Result> results, string spec)
        {
            new User(questions, results, spec)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            }.ShowDialog();
        }
        public string? ShowNewTheme()
        {
            var window = new NewTheme
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            bool? dialogResult = window.ShowDialog();

            if (dialogResult == true)
            {
                return window.TextNewTheme;
            }

            return null;
        }
    }
}
