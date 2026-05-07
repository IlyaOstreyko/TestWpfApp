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
using TestWpfApp.Models;
using TestWpfApp.ViewModels;

namespace TestWpfApp.Views
{
    /// <summary>
    /// Interaction logic for ShowQuestion.xaml
    /// </summary>
    public partial class ShowQuestion : Window
    {
        public TestQuestion TestQuestion { get; set; }
        public List<TestQuestion> TestQuestions { get; set; }
        public ShowQuestion(TestQuestion testQuestion)
        {
            //InitializeComponent();
            TestQuestion = testQuestion;
            //DataContext = TestQuestion;

            InitializeComponent();
            DataContext = new ShowQuestionViewModel(TestQuestion);
        }
        public ShowQuestion(List<TestQuestion> testQuestions, UserInfo userInfo, List<Result> results)
        {
            //InitializeComponent();
            //DataContext = TestQuestion;
            bool isTest = false;
            InitializeComponent();
            DataContext = new ShowQuestionViewModel(testQuestions, userInfo, results, isTest);
        }
        public ShowQuestion(List<TestQuestion> testQuestions, bool isTest, List<Result> results)
        {
            //InitializeComponent();
            //DataContext = TestQuestion;
            UserInfo userInfo = new UserInfo();
            InitializeComponent();
            DataContext = new ShowQuestionViewModel(testQuestions, userInfo, results, isTest);
        }

        private void StackPanel_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
    }
}
