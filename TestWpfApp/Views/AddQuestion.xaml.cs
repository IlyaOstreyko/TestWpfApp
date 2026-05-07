using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TestWpfApp.Data.Interfaces;
using TestWpfApp.Interfaces;
using TestWpfApp.Models;
using TestWpfApp.Service;
using TestWpfApp.ViewModels;

namespace TestWpfApp.Views
{
    /// <summary>
    /// Interaction logic for AddQuestion.xaml
    /// </summary>
    public partial class AddQuestion : Window
    {
        public AddQuestion()
        {
            //InitializeComponent();
            //TestQuestion = testQuestion;
            //DataContext = TestQuestion;

            InitializeComponent();
            //DataContext = new AddQuestionViewModel();
            var db = App.HostContainer!.Services.GetRequiredService<IQuestionRepository>();
            var dialog = App.HostContainer.Services.GetRequiredService<IDialogService>();
            var window = App.HostContainer.Services.GetRequiredService<IWindowService>();
            var mapper = App.HostContainer.Services.GetRequiredService<IMapper>();
            var migrationOldTableService = App.HostContainer.Services.GetRequiredService<MigrationOldTableService>();
            

            // Pass the services into the ViewModel alongside the runtime testQuestion parameter
            DataContext = new AddQuestionViewModel(dialog, window, db, mapper, migrationOldTableService);
        }
        public AddQuestion(TestQuestion testQuestion)
        {
            //InitializeComponent();
            //TestQuestion = testQuestion;
            //DataContext = TestQuestion;

            InitializeComponent();
            //DataContext = new AddQuestionViewModel(testQuestion);
            var db = App.HostContainer!.Services.GetRequiredService<IQuestionRepository>();
            var dialog = App.HostContainer.Services.GetRequiredService<IDialogService>();
            var window = App.HostContainer.Services.GetRequiredService<IWindowService>();
            var mapper = App.HostContainer.Services.GetRequiredService<IMapper>();

            // Pass the services into the ViewModel alongside the runtime testQuestion parameter
            DataContext = new AddQuestionViewModel(testQuestion, dialog, window, db, mapper);
            //DataContext = App.HostContainer!.Services.GetRequiredService<AddQuestionViewModel>();
        }
    }
}
