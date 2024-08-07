using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VladimirsTool.Models;
using VladimirsTool.Utils;
using VladimirsTool.Views;
using Excel = Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;

namespace VladimirsTool.ViewModels
{
    public enum DataHandleType
    {
        None,
        Coincided, //Совпадающие значения
        Unique //Уникальные значения
    }

    public class MainViewModel : BaseVM
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
        private Dictionary<string, int> _totalHeaders = new Dictionary<string, int>();
        private ObservableCollection<KeySettings> _memorySettings;
        public Dictionary<string, int> TotalHeaders => _totalHeaders;
        public Dictionary<WorksheetItem, List<Man>> MenInSheets => _menInSheets;
        public ObservableCollection<WorksheetItem> SheetKeys => _sheetKeys;
        public IEnumerable<WorksheetItem> SelectedWorksheets => _sheetKeys.Where(p => p.IsSelected);
        public ICommand ChooseFiles
        {
            get => new ClickCommand((obj) =>
            {
                DefaultDialogService dialogService = new DefaultDialogService();
                if (dialogService.OpenMultipleFilesDialog("All Files |*.*| Excel Files | *.xls; *.xlsx; *.xlsm| CSV| *.csv"))
                {
                    //Get the path of specified file
                    foreach (var path in dialogService.FilePaths)
                    {
                        var ext = Path.GetExtension(path).ToLower();
                        try
                        {
                            if (ext == ".xlsx" || ext == ".xlsm")
                                ReadExcelSheet(path);
                            else if (ext == ".xls")
                                ReadOldExcelSheet(path);
                            else if (ext == ".csv")
                                ReadCSV(path);
                            else
                                MessageBox.Show($"Формат файлов {ext} не поддерживается программой");
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString(), $"Ошибка чтения файла {path}");
                        }
                    }
                    _memorySettings = null;
                    RefreshKeys();
                }
            });
        }

        public ICommand DeleteFiles
        {
            get => new ClickCommand((obj) =>
            {
                foreach(var sheet in SheetKeys.ToArray())
                {
                    if (sheet.IsSelected)
                    {
                        MenInSheets.Remove(sheet);
                        SheetKeys.Remove(sheet);
                    }
                }
                OnPropertyChanged(nameof(SheetKeys));
                OnPropertyChanged(nameof(MenInSheets));
                _memorySettings = null;
                RefreshTotalHeaders();
                RefreshKeys();
            });
        }

        public ICommand ChooseKeys
        {
            get => new ClickCommand((obj) =>
            {
                if (SheetKeys.Count == 0)
                {
                    MessageBox.Show("Нет файлов для выбора ключей");
                    return;
                }
                KeySettingsWindow window = new KeySettingsWindow();
                var vm = (KeyViewModel)window.DataContext;

                if (_memorySettings != null)
                    vm.Headers = _memorySettings;
                else
                    vm.Headers = new ObservableCollection<KeySettings>(TotalHeaders.Select(h => new KeySettings(h.Key)).ToArray());
               
                bool? res = window.ShowDialog();
                _memorySettings = vm.Headers;
                RefreshKeys();
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

        private bool HandleData(DataHandleType type)
        {
            if (SheetKeys.Count == 0 || SelectedWorksheets.Count() == 0) return false;

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
                        return false;
                    }

                    break;
                case DataHandleType.Unique:
                    includedMen = GetUniqueMen();
                    if (includedMen.Count == 0)
                    { 
                        MessageBox.Show("Уникальных значений в выбранных файлах не найдено");
                        return false;
                    }
                    break;
            }

            if (includedMen == null) return false;
            includedMen.Sort();
            window = new CoincidenceResultWindow(includedMen);
            window.CoincidedCount = coincidedCount;
            window.HandleType = type;
            window.Show();
            //window.ShowDialog();
            //window = null;
            //GC.Collect();

            return true;
        }

        private void RefreshKeys()
        {
            KeyHeaderStore keyStore = KeyHeaderStore.GetInstance();

            if(_memorySettings != null)
                keyStore.SetKeys(_memorySettings.Where(s => s.IsSelected));
            foreach (var man in _menInSheets)
                man.Value.ForEach(m => m.CalculateHashCode());
        }

        private void RefreshTotalHeaders()
        {
            TotalHeaders.Clear();
            foreach(var men in _menInSheets.ToArray())
            {
                if(men.Value.Count > 0)
                    AddTotalHeaders(men.Value[0].Headers);
            }
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
            XLWorkbook workbook = new XLWorkbook(path);
            IXLWorksheet worksheet = workbook.Worksheet(1);

            var men = wsReader.Parse(worksheet);

            if (men.Count() == 0) return;
            WorksheetItem item = new WorksheetItem(Path.GetFileName(path), path);
            if (MenInSheets.ContainsKey(item))
            {
                MessageBox.Show($"Файл \"{item.Name}\" уже добавлен");
            }
            else
            {
                MenInSheets.Add(item, men.ToList());
                SheetKeys.Add(item);
            }

            workbook.Dispose();

            AddTotalHeaders(wsReader.Headers);
        }

        private void ReadOldExcelSheet(string path)
        {
            OldWorksheetReader wsReader = new OldWorksheetReader();

            Excel.Application excel = new Excel.Application();
            Workbook wb = excel.Workbooks.Open(path, ReadOnly: true);
            Worksheet ws = wb.Worksheets[1];

            var men = wsReader.Parse(ws);
            if (men.Count() == 0) return;
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

            AddTotalHeaders(wsReader.Headers);
        }

        private void ReadCSV(string path)
        {
            var csvReader = new CSVReader();

            var men = csvReader.Parse(path);
            if (men.Count() == 0) return;
            WorksheetItem item = new WorksheetItem(Path.GetFileName(path), path);
            if (MenInSheets.ContainsKey(item))
            {
                MessageBox.Show($"Файл \"{item.Name}\" уже добавлен");
            }
            else
            {
                MenInSheets.Add(item, men.ToList());
                SheetKeys.Add(item);
            }

            AddTotalHeaders(csvReader.Headers);
        }

        private void AddTotalHeaders(IEnumerable<string> headers)
        {
            foreach(var header in headers)
            {
                if (header == null) continue;
                if (!TotalHeaders.ContainsKey(header))
                {
                    TotalHeaders.Add(header, TotalHeaders.Count + 1);
                }
            }
        }
    }
}
