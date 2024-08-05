using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VladimirsTool.Utils
{
    public interface IDialogService
    {
        void ShowMessage(string message);   // показ сообщения
        string FilePath { get; set; }   // путь к выбранному файлу
        string[] FilePaths { get; set; } //пути к выбранным файлам
        bool OpenFileDialog(string filter);  // открытие файла
        bool OpenMultipleFilesDialog(string filter);  // открытие файлов
        bool SaveFileDialog();
    }
}
