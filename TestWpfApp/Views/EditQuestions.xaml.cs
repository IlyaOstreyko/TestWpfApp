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
        public EditQuestions(EditQuestionsViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
        }

        public void Initialize(string title, bool editMode, int specialityId)
        {
            if (DataContext is EditQuestionsViewModel vm)
            {
                vm.Initialize(title, editMode, specialityId);
            }
        }
    }
}
