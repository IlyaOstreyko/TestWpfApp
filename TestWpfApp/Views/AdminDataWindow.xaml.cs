using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for AdminDataWindow.xaml
    /// </summary>
    public partial class AdminDataWindow : Window
    {
        public AdminDataWindow(AdminDataViewModel viewModel)
        {
            InitializeComponent();
            // Устанавливаем пришедшую VM в DataContext
            DataContext = viewModel;
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private static readonly Regex _numbersRegex = new Regex("[^0-9]+");

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Запрещаем всё кроме цифр
            e.Handled = _numbersRegex.IsMatch(e.Text);
        }

        private void QuestionCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            if (textBox.DataContext is not ThemeSelectionWrapper wrapper)
                return;

            // Пустое значение -> 0
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                wrapper.QuestionCount = 0;
                return;
            }

            // Если не число — сбрасываем
            if (!int.TryParse(textBox.Text, out int value))
            {
                wrapper.QuestionCount = 0;
                textBox.Text = "0";
                textBox.CaretIndex = textBox.Text.Length;
                return;
            }

            // Минимум 0
            if (value < 0)
            {
                value = 0;
            }

            // Максимум = TotalQuestionsCount
            if (value > wrapper.TotalQuestionsCount)
            {
                value = wrapper.TotalQuestionsCount;
            }

            // Обновляем только если значение изменилось
            if (wrapper.QuestionCount != value)
            {
                wrapper.QuestionCount = value;

                textBox.Text = value.ToString();
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
    }
}
