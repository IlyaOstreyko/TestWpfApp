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
using TestWpfApp.ViewModels;

namespace TestWpfApp.Views
{
    /// <summary>
    /// Interaction logic for AdminGroupsDataWindow.xaml
    /// </summary>
    public partial class AdminGroupsDataWindow : Window
    {
        public AdminGroupsDataWindow(AdminGroupsDataViewModel viewModel)
        {
            InitializeComponent();
            // Устанавливаем пришедшую VM в DataContext
            DataContext = viewModel;
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
