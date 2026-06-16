
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TestWpfApp.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using TestWpfApp.Data.DataModels;

namespace TestWpfApp.ViewModels
{
    public class ResaultsViewModel : INotifyPropertyChanged
    {
        public ICommand SaveCommand { get; }
        public ICommand SaveFileCommand { get; }
        
        public ICommand CloseCommand { get; }
        public bool VisibilityButtonResults { get; private set; }
        public bool TimeOver;
        public SaveFileDialog saveFileDialog;
        private UserInfo UserInfo;
        private List<Result> Results;
        public List<TestQuestionVM> CorrectTestQuestions { get; set; }
        public List<TestQuestionVM> IncorrectTestQuestions { get; set; }

        public ResaultsViewModel(List<TestQuestionVM> questions, UserInfo userInfo, List<Result> results, bool isTest, bool timeOver)
        {
            VisibilityButtonResults = !isTest;
            TimeOver = timeOver;
            Results = results;
            UserInfo = userInfo;
            CorrectTestQuestions = questions.Where(n => n.NameAnswer == null).ToList();
            IncorrectTestQuestions = questions.Where(n => n.NameAnswer != null).ToList();
            SaveCommand = new RelayCommand(Save);
            CloseCommand = new RelayCommand(Close);
            SaveFileCommand = new RelayCommand(SaveFile);
        }
        private void Save(object obj)
        {
            if (!TimeOver)
            {
                MessageBox.Show(
                    "У вас закончилось время для выполнения экзамена",
                    "Ошибка сохранения файла",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            // Сохранение результата как Word-файл
            saveFileDialog = new SaveFileDialog()
            {
                Title = "Сохранить отчет экзамена",
                Filter = "Word document (*.docx)|*.docx",
                DefaultExt = ".docx"
            };
            string currentDir = Directory.GetCurrentDirectory();
            string templatePath = Path.Combine(currentDir, "DOC.docx");
            if (saveFileDialog.ShowDialog() == true)
            {
                try { File.Copy(templatePath, saveFileDialog.FileName, true); }
                catch 
                { 
                    MessageBox.Show("Не удалось сохранить файл." + "\r\n" + "Возможно шаблон отсутствует или занят другим процессом", "Ошибка сохранения файла", 
                        MessageBoxButton.OK, MessageBoxImage.Error); 
                    return; 
                }                
            }
            try
            {
                using (WordprocessingDocument wordDoc =
                       WordprocessingDocument.Open(saveFileDialog.FileName, true))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;

                    var table = body.Elements<Table>().FirstOrDefault();
                    if (table == null)
                        throw new Exception("Таблица не найдена в документе.");

                    int sumAll = 0;
                    int sum = 0;
                    int sumMistake = 0;

                    var rows = table.Elements<TableRow>().ToList();

                    for (int i = 0; i < Results.Count; i++)
                    {
                        TableRow row = new TableRow();

                        row.Append(
                            CreateCell(Results[i].Theme),
                            CreateCell(Results[i].AllNumberQustions.ToString()),
                            CreateCell(Results[i].NumberQustions.ToString()),
                            CreateCell(Results[i].NumberMistake.ToString())
                        );

                        table.Append(row);

                        sum += Results[i].NumberQustions;
                        sumAll += Results[i].AllNumberQustions;
                        sumMistake += Results[i].NumberMistake;
                    }

                    // Итого
                    TableRow totalRow = new TableRow();
                    totalRow.Append(
                        CreateCell("Итого"),
                        CreateCell(sumAll.ToString()),
                        CreateCell(sum.ToString()),
                        CreateCell(sumMistake.ToString())
                    );
                    table.Append(totalRow);

                    // Оценка
                    string grade = sumMistake > UserInfo.NumberMistake
                        ? "неудовлетворительно"
                        : "удовлетворительно";

                    TableRow gradeRow = new TableRow();
                    gradeRow.Append(
                        CreateCell("Оценка теста"),
                        CreateCell(grade)
                    );

                    table.Append(gradeRow);

                    // ===== Замена bookmark-полей =====
                    ReplaceAllText(body, "DataDoc", UserInfo.Date.ToString("dd/MM/yyyy"));
                    ReplaceAllText(body, "UserDoc", UserInfo.User);
                    ReplaceAllText(body, "User2Doc", UserInfo.User);
                    ReplaceAllText(body, "PositionUserDoc", UserInfo.PositionUser);
                    ReplaceAllText(body, "ChairmanDoc", UserInfo.Chairman);
                    ReplaceAllText(body, "PositionChairmanDoc", UserInfo.PositionChairman);
                    ReplaceAllText(body, "CommissionMember1Doc", UserInfo.CommissionMember1);
                    ReplaceAllText(body, "PositionCommissionMember1Doc", UserInfo.PositionCommissionMember1);
                    ReplaceAllText(body, "MistakeDoc", UserInfo.NumberMistake.ToString());

                    wordDoc.MainDocumentPart.Document.Save();
                }
            }
            catch (Exception ex)
            {
                File.Delete(saveFileDialog.FileName);
                MessageBox.Show(
    $"Ошибка генерации документа{ex.Message}",
    "Ошибка",
    MessageBoxButton.OK,
    MessageBoxImage.Error);
            }
        }
        private TableCell CreateCell(string text)
        {
            return new TableCell(
                new Paragraph(
                    new Run(
                        new Text(text ?? string.Empty)
                        {
                            Space = SpaceProcessingModeValues.Preserve
                        }
                    )
                )
            );
        }

        private void ReplaceAllText(Body body, string placeholder, string value)
        {
            foreach (var text in body.Descendants<Text>())
            {
                if (text.Text != null && text.Text == placeholder)
                {
                    text.Text = value;
                }
            }
        }
        private void SaveFile(object obj)
        {
            string textForSave = "";
            if (IncorrectTestQuestions.Count != 0)
            {
                textForSave = "  ВОПРОСЫ ОТВЕЧЕННЫЕ НЕВЕРНО:" + "\r\n";
                foreach (TestQuestionVM incorrect in IncorrectTestQuestions)
                {
                    textForSave += "ВОПРОС:" + "\r\n" + incorrect.NameQuestion + "\r\n";
                    textForSave += "   1. :" + incorrect.NameAnswerCorrect1 + "\r\n";
                    textForSave += "   2. :" + incorrect.NameAnswerIncorrect1 + "\r\n";
                    textForSave += "   3. :" + incorrect.NameAnswerIncorrect2 + "\r\n";
                    textForSave += "   4. :" + incorrect.NameAnswerIncorrect3 + "\r\n";
                    textForSave += "ПРАВИЛЬНЫЙ ОТВЕТ:" + "\r\n" + incorrect.NameAnswerCorrect1 + "\r\n";
                    textForSave += "ВАШ ОТВЕТ:" + "\r\n" + incorrect.NameAnswer + "\r\n" + "\r\n";
                }
            }

            if (CorrectTestQuestions.Count != 0)
            {
                textForSave += "   ПРАВИЛЬНО ОТВЕЧЕННЫЕ ВОПРОСЫ:" + "\r\n";
                foreach (TestQuestionVM correct in CorrectTestQuestions)
                {
                    textForSave += "ВОПРОС:" + "\r\n" + correct.NameQuestion + "\r\n";
                    textForSave += "   1. :" + correct.NameAnswerCorrect1 + "\r\n";
                    textForSave += "   2. :" + correct.NameAnswerIncorrect1 + "\r\n";
                    textForSave += "   3. :" + correct.NameAnswerIncorrect2 + "\r\n";
                    textForSave += "   4. :" + correct.NameAnswerIncorrect3 + "\r\n";
                    textForSave += "ПРАВИЛЬНЫЙ ОТВЕТ:" + "\r\n" + correct.NameAnswerCorrect1 + "\r\n" + "\r\n";
                }
            }

            if (textForSave == "")
            {
                return;
            }
            saveFileDialog = new SaveFileDialog()
            {
                Title = "Сохранить результаты теста",
                Filter = "Text file (*.txt)|*.txt",
                DefaultExt = ".txt"
            };
            try
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filename = saveFileDialog.FileName;
                    using (StreamWriter writer = new StreamWriter(filename, false))
                    {
                        writer.WriteLine(textForSave);
                    }
                }
            }
            catch 
            { 
                MessageBox.Show("Не удалось сохранить файл." + "\r\n" + "Возможно файл открыт или занят другим процессом", "Ошибка сохранения файла", MessageBoxButton.OK, MessageBoxImage.Error); 
            }

        }

        private void Close(object obj)
        {
            System.Windows.Window showThemesWindow = null;
            System.Windows.Window mainWindow = null;
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                var type = window.GetType();
                var name = type.Name;
                if (name == "ShowThemes")
                {
                    showThemesWindow = window;

                }
                if (name == "MainWindow")
                {
                    mainWindow = window;

                }
                if (name != "MainWindow" && name != "ShowThemes")
                {
                    window.Close();
                }
            }
            if (mainWindow != null)
            {
                //mainWindow.Topmost = false;
            }
            if (showThemesWindow != null)
            {
                showThemesWindow.Topmost = true;
                showThemesWindow.Topmost = false;
                showThemesWindow.Activate();
                showThemesWindow.Focus();
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
//private void SaveOld(object obj)
//{
//    if (!TimeOver)
//    {
//        MessageBox.Show("У вас закончилось время для выполнения экзамена", "Ошибка сохранения файла", MessageBoxButton.OK, MessageBoxImage.Error);
//        return;
//    }

//    Application1 app = new Application1();
//    string currentDir = Directory.GetCurrentDirectory();
//    try
//    {
//        Document doc = app.Documents.Open(currentDir + @"\DOC.doc");
//        //var dateAndTime = DateTime.Now;
//        //var date = dateAndTime.Date;

//        int tablecount = doc.Tables.Count;
//        Table table = doc.Tables[1];
//        int sumAll = 0;
//        int sum = 0;
//        int sumMistake = 0;
//        for (int i = 0; i < Results.Count; i++)
//        {
//            table.Rows.Add();
//            table.Cell(i + 2, 1).Range.Text = Results[i].Theme;
//            table.Cell(i + 2, 2).Range.Text = Results[i].AllNumberQustions.ToString();
//            table.Cell(i + 2, 3).Range.Text = Results[i].NumberQustions.ToString();
//            table.Cell(i + 2, 4).Range.Text = Results[i].NumberMistake.ToString();
//            sum += Results[i].NumberQustions;
//            sumAll += Results[i].AllNumberQustions;
//            sumMistake += Results[i].NumberMistake;
//        }
//        table.Rows.Add();
//        table.Cell(Results.Count + 2, 1).Range.Text = "Итого";
//        table.Cell(Results.Count + 2, 2).Range.Text = sumAll.ToString();
//        table.Cell(Results.Count + 2, 3).Range.Text = sum.ToString();
//        table.Cell(Results.Count + 2, 4).Range.Text = sumMistake.ToString();
//        table.Rows.Add();
//        table.Cell(Results.Count + 3, 1).Range.Text = "Оценка теста";
//        if (sumMistake > UserInfo.NumberMistake)
//        {
//            table.Cell(Results.Count + 3, 2).Range.Text = "неудовлетворительно";
//        }
//        else
//        {
//            table.Cell(Results.Count + 3, 2).Range.Text = "удовлетворительно";
//        }
//        table.Rows[Results.Count + 3].Cells[2].Merge(table.Rows[Results.Count + 3].Cells[4]);

//        if (doc.Bookmarks.Exists("DataDoc"))
//        {
//            object oBookMark = "DataDoc";

//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.Date.ToString("dd/MM/yyyy");
//        }


//        if (doc.Bookmarks.Exists("UserDoc"))
//        {
//            object oBookMark = "UserDoc";
//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.User;
//        }
//        if (doc.Bookmarks.Exists("User2Doc"))
//        {
//            object oBookMark = "User2Doc";
//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.User;
//        }
//        if (doc.Bookmarks.Exists("PositionUserDoc"))
//        {
//            object oBookMark = "PositionUserDoc";
//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.PositionUser;
//        }
//        if (doc.Bookmarks.Exists("ChairmanDoc"))
//        {
//            object oBookMark = "ChairmanDoc";
//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.Chairman;
//        }
//        if (doc.Bookmarks.Exists("PositionChairmanDoc"))
//        {
//            object oBookMark = "PositionChairmanDoc";
//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.PositionChairman;
//        }
//        if (doc.Bookmarks.Exists("CommissionMember1Doc"))
//        {
//            object oBookMark = "CommissionMember1Doc";
//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.CommissionMember1;
//        }
//        if (doc.Bookmarks.Exists("PositionCommissionMember1Doc"))
//        {
//            object oBookMark = "PositionCommissionMember1Doc";
//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.PositionCommissionMember1;
//        }
//        if (doc.Bookmarks.Exists("MistakeDoc"))
//        {
//            object oBookMark = "MistakeDoc";
//            doc.Bookmarks.get_Item(ref oBookMark).Range.Text = UserInfo.NumberMistake.ToString();
//        }

//        saveFileDialog = new SaveFileDialog()
//        {
//            Title = "Сохранить отчет экзамена",
//            Filter = "Text file (*.pdf)|*.pdf",
//            DefaultExt = ".pdf"
//        };

//        string filename = "";
//        if (saveFileDialog.ShowDialog() == true)
//        {
//            filename = saveFileDialog.FileName;
//            doc.ExportAsFixedFormat(filename, WdExportFormat.wdExportFormatPDF);
//        }

//        //string nameFilePDF = UserInfo.User + ".pdf";

//        doc.Close(WdSaveOptions.wdDoNotSaveChanges);
//        app.Quit();
//    }
//    catch { MessageBox.Show("Не удалось сохранить файл." + "\r\n" + "Возможно шаблон отсутствует или занят другим процессом", "Ошибка сохранения файла", MessageBoxButton.OK, MessageBoxImage.Error); }
//}