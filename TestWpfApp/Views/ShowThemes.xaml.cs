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
    /// Interaction logic for ShowQuestions.xaml
    /// </summary>
    public partial class ShowThemes: Window
    {
        public ShowThemes()
        {
            InitializeComponent();
            var db = App.HostContainer!.Services.GetRequiredService<IQuestionRepository>();
            var dialog = App.HostContainer.Services.GetRequiredService<IDialogService>();
            var window = App.HostContainer.Services.GetRequiredService<IWindowService>();
            var mapper = App.HostContainer.Services.GetRequiredService<IMapper>();

            // Pass the services into the ViewModel alongside the runtime testQuestion parameter
            DataContext = new ShowThemesViewModel(db, dialog, window, mapper);
        }
        private void PortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // xaml.cs code
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void TextBox_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
    }
}
