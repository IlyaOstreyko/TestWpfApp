using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWpfApp.Interfaces
{
    public interface IDialogService
    {
        void ShowMessage(string message);   // показ сообщения
        string FilePath { get; set; }   // путь к выбранному файлу
        bool OpenFileDialog();  // открытие файла
        bool OpenFileDialog(string title, string filter);
        bool SaveFileDialog();  // сохранение файла
        void ShowError(string message, string title);
        bool ShowConfirmation(string message, string title);
        string? ShowInputDialog(string message, string title = "Ввод данных");
    }
}
