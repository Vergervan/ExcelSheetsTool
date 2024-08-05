using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VladimirsTool.Models;
using VladimirsTool.Utils;
using VladimirsTool.Views;
using Excel = Microsoft.Office.Interop.Excel;

namespace VladimirsTool.ViewModels
{
    public enum DataHandleType
    {
        None,
        Coincided, //Совпадающие значения
        Unique //Уникальные значения
    }

    public class MainViewModel : BaseVM, IDisposable
    {

        public struct OriginalManIterator
        {
            public Man man;
            public int counter;

            public OriginalManIterator(Man man)
            {
                this.man = man;
                counter = 1;
            }
        }

        private Dictionary<WorksheetItem, List<Man>> _menInSheets = new Dictionary<WorksheetItem, List<Man>>();
        private ObservableCollection<WorksheetItem> _sheetKeys = new ObservableCollection<WorksheetItem>();
        public Dictionary<WorksheetItem, List<Man>> MenInSheets => _menInSheets;
        public ObservableCollection<WorksheetItem> SheetKeys => _sheetKeys;
        public IEnumerable<WorksheetItem> SelectedWorksheets => _sheetKeys.Where(p => p.IsSelected);
        public ICommand ChooseFiles
        {
            get => new ClickCommand((obj) =>
            {
                DefaultDialogService dialogService = new DefaultDialogService();

                if (dialogService.OpenMultipleFilesDialog("Excel Files | *.xls; *.xlsx; *.xlsm"))
                {
                    //Get the path of specified file
                    foreach (var path in dialogService.FilePaths)
                    {
                        ReadExcelSheet(path);
                    }
                }
            });
        }

        public ICommand FileItemDoubleClick
        {
            get => new ClickCommand((obj) =>
            {
                WorksheetItem item = obj as WorksheetItem;
                MessageBox.Show(item.Name);
            });
        }

        public ICommand TestButton
        {
            get => new ClickCommand((obj) =>
            {
                MessageBox.Show(string.Join("\n", SelectedWorksheets.Select(p => new { Name = p.Name, Hash = p.GetHashCode() })));
            });
        }

        public ICommand FindCoincidence
        {
            get => new ClickCommand((obj) =>
            {
                HandleData(DataHandleType.Coincided);
            });
        }

        public ICommand FindUniqueValues
        {
            get => new ClickCommand((obj) =>
            {
                HandleData(DataHandleType.Unique);
            });
        }

        private void HandleData(DataHandleType type)
        {
            if (SelectedWorksheets.Count() == 0) return;

            CoincidenceResultWindow window;

            //MenInSheets[sel[0]].ForEach(m => counterDict.Add(m, new OriginalManIterator(m)));

            List<Man> includedMen = null;
            int coincidedCount = 0;

            switch (type)
            {
                case DataHandleType.Coincided:
                    includedMen = GetCoincidedMen(out coincidedCount);
                    if (includedMen.Count == 0)
                    {
                        MessageBox.Show("Совпадений по выбранным файлам не найдено");
                        return;
                    }

                    break;
                case DataHandleType.Unique:
                    includedMen = GetUniqueMen();
                    if (includedMen.Count == 0)
                    { 
                        MessageBox.Show("Уникальных значений в выбранных файлах не найдено");
                        return;
                    }
                    break;
            }

            if (includedMen == null) return;
            includedMen.Sort();
            window = new CoincidenceResultWindow(includedMen);
            window.CoincidedCount = coincidedCount;
            window.HandleType = type;
            window.Show();
            //window.ShowDialog();
            //window = null;
            //GC.Collect();
        }

        private List<Man> GetCoincidedMen(out int coincidedCount)
        {
            var sel = SelectedWorksheets.ToArray();
            coincidedCount = 0;
            Dictionary<Man, OriginalManIterator> counterDict = new Dictionary<Man, OriginalManIterator>();
            List<Man> includedMen = new List<Man>();
            for (int i = 0; i < sel.Length; i++)
            {
                var menList = MenInSheets[sel[i]];
                foreach (var man in menList)
                {
                    OriginalManIterator originInfo;

                    if (counterDict.TryGetValue(man, out originInfo))
                    {
                        if (originInfo.counter == 1)
                        {
                            includedMen.Add(originInfo.man);
                            ++originInfo.counter;
                            ++coincidedCount;
                            counterDict[man] = originInfo;
                        }
                        includedMen.Add(man);
                        continue;
                    }
                    counterDict.Add(man, new OriginalManIterator(man));
                }
            }
            return includedMen;
        }

        private List<Man> GetUniqueMen()
        {
            var sel = SelectedWorksheets.ToArray();
            Dictionary<Man, int> counterDict = new Dictionary<Man, int>();
            List<Man> includedMen = new List<Man>();

            for (int i = 0; i < sel.Length; i++)
            {
                var menList = MenInSheets[sel[i]];
                foreach (var man in menList)
                {
                    if (counterDict.ContainsKey(man))
                    {
                        counterDict[man]++;
                        continue;
                    }
                    counterDict.Add(man, 1);
                }
            }

            foreach(var man in counterDict.ToArray())
            {
                if (man.Value == 1)
                    includedMen.Add(man.Key);
            }

            return includedMen;
        }

        private void ReadExcelSheet(string path)
        {
            WorksheetReader wsReader = new WorksheetReader();
            Excel.Application excel = new Excel.Application();
            Workbook wb = excel.Workbooks.Open(path, ReadOnly: true);
            Worksheet ws = wb.Worksheets[1];

            var men = wsReader.ParseSheetsByNameAndBirth(ws);
            if (men == null) return;
            WorksheetItem item = new WorksheetItem(wb.Name, path);
            if (MenInSheets.ContainsKey(item))
            {
                MessageBox.Show($"Файл \"{item.Name}\" уже добавлен");
            }
            else
            {
                MenInSheets.Add(item, men.ToList());
                SheetKeys.Add(item);
            }
            wb.Close();
        }

        public void Dispose()
        {

        }
    }
}
