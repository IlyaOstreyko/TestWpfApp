
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shapes;
using TestWpfApp.Models;
using TestWpfApp.Views;
using Xceed.Wpf.AvalonDock.Themes;

namespace TestWpfApp.ViewModels
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private UserInfo userInfo;
        private int allNumberQuestions;
        private List<TestQuestion> TestQuestions { get; set; }
        private List<Result> Results;
        
        public UserInfo UserInfo
        {
            get => userInfo;
            set
            {
                userInfo = value;

                RaisePropertyChanged(nameof(UserInfo));
            }
        }
        public int AllNumberQuestions
        {
            get => allNumberQuestions;
            set
            {
                allNumberQuestions = value;

                RaisePropertyChanged(nameof(AllNumberQuestions));
            }
        }

        public UserViewModel(List<TestQuestion> questions, List<Result> results)
        {

            
            TestQuestions = questions;
            Results = results;
            
            AllNumberQuestions = questions.Count;
            UserInfo = new UserInfo();
            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                string filename = currentDir + @"\settingsAdmin.txt";
                List<string> lines = File.ReadAllLines(filename).ToList();
                UserInfo.Chairman = lines[0];
                UserInfo.PositionChairman = lines[1];
                UserInfo.CommissionMember1 = lines[2];
                UserInfo.PositionCommissionMember1 = lines[3];
                UserInfo.Time = Int32.Parse(lines[4]);
            }            
            catch { }
            UserInfo.Date = DateTime.Now.Date;
        }

        RelayCommand start;
        public RelayCommand Start
        {

            get
            {
                return start ??
                    (start = new RelayCommand((o) =>
                    {
                        if (UserInfo.User != null && 
                        UserInfo.Chairman != null && 
                        UserInfo.PositionChairman != null && 
                        UserInfo.CommissionMember1 != null &&
                        UserInfo.PositionCommissionMember1 != null &&
                        UserInfo.PositionUser != null)
                        {
                            string settingsAdminForSave = UserInfo.Chairman +"\r\n";
                            settingsAdminForSave += UserInfo.PositionChairman + "\r\n";
                            settingsAdminForSave += UserInfo.CommissionMember1 + "\r\n";
                            settingsAdminForSave += UserInfo.PositionCommissionMember1 + "\r\n";
                            settingsAdminForSave += UserInfo.Time + "\r\n";
                            string currentDir = Directory.GetCurrentDirectory();
                            string filename = currentDir + @"\settingsAdmin.txt";
                            using (StreamWriter writer = new StreamWriter(filename, false))
                            {
                                writer.WriteLine(settingsAdminForSave);
                            }
                            ShowQuestion showQuestions = new ShowQuestion(TestQuestions, UserInfo, Results);
                            showQuestions.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Заполните все данные.", "Ошибка заполнения формы", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
