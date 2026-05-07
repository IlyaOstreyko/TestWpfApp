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
using TestWpfApp.ViewModels;

namespace TestWpfApp.Views
{
    /// <summary>
    /// Interaction logic for EditQuestions.xaml
    /// </summary>
    public partial class EditQuestions : Window
    {
        public EditQuestions(string title, bool editTrue)
        {
            InitializeComponent();

            // Извлекаем необходимые сервисы из DI-контейнера
            var db = App.HostContainer!.Services.GetRequiredService<IQuestionRepository>();
            var dialog = App.HostContainer.Services.GetRequiredService<IDialogService>();
            var window = App.HostContainer.Services.GetRequiredService<IWindowService>();
            var mapper = App.HostContainer.Services.GetRequiredService<IMapper>();

            // Передаем сервисы и параметры в ViewModel
            DataContext = new EditQuestionsViewModel(title, editTrue, db, dialog, window, mapper);
        }
    }
}
