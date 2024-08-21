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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Excel = Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;

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
        private List<KeySettings> _memorySettings;
        private bool _isOpeningFiles;
        private int _openedFilesCount;
        private int _filesToOpenCount;
        public bool IsOpeningFiles
        {
            get => _isOpeningFiles;
            set
            {
                _isOpeningFiles = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanUseButton));
            }
        }
        public int OpenedFilesCount
        {
            get => _openedFilesCount;
            set
            {
                _openedFilesCount = value;
                OnPropertyChanged();
            }
        }
        public int FilesToOpenCount
        {
            get => _filesToOpenCount;
            set
            {
                _filesToOpenCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OpeningProgressVisibility));
            }
        }
        public bool CanUseButton => !IsOpeningFiles;
        public Visibility OpeningProgressVisibility => _isOpeningFiles ? Visibility.Visible : Visibility.Hidden;
        public Dictionary<string, int> TotalHeaders => _totalHeaders;
        public Dictionary<WorksheetItem, List<Man>> MenInSheets => _menInSheets;
        public ObservableCollection<WorksheetItem> SheetKeys => _sheetKeys;
        public IEnumerable<WorksheetItem> SelectedWorksheets => _sheetKeys.Where(p => p.IsSelected);
        public ICommand ChooseFiles
        {
            get => new ClickCommand(async (obj) =>
            {
                Task task = Task.Run(() =>
                {
                    IsOpeningFiles = true;
                    DefaultDialogService dialogService = new DefaultDialogService();
                    if (dialogService.OpenMultipleFilesDialog("All Files |*.*| Excel Files | *.xls; *.xlsx; *.xlsm| CSV| *.csv| Word| *.docx"))
                    {
                        FilesToOpenCount = dialogService.FilePaths.Length;
                        KeyHeaderStore store = KeyHeaderStore.GetInstance();
                        store.ClearKeys();
                        _memorySettings = null;
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
                                else if (ext == ".docx")
                                    ReadWord(path);
                                else
                                    MessageBox.Show($"Формат файлов {ext} не поддерживается программой", "Ошибка");
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString(), $"Ошибка чтения файла {path}");
                            }
                            ++OpenedFilesCount;
                        }
                        Task.Run(async () =>
                        {
                            await Task.Delay(400);
                            IsOpeningFiles = false;
                            OpenedFilesCount = 0;
                            FilesToOpenCount = 0;
                        });
                        RefreshKeys();
                    }
                    else
                    {
                        IsOpeningFiles = false;
                    }
                });
                await task;
            });
        }

        public ICommand DeleteFiles
        {
            get => new ClickCommand((obj) =>
            {
                if(SheetKeys.Count == 0)
                {
                    MessageBox.Show("Нет файлов на удаление", "Ошибка");
                    return;
                }
                var sheetList = SheetKeys.Where(s => s.IsSelected).ToArray();
                if(sheetList.Length == 0)
                {
                    MessageBox.Show("Не выделены файлы на удаление", "Ошибка");
                    return;
                }
                foreach(var sheet in sheetList)
                {
                    MenInSheets.Remove(sheet);
                    SheetKeys.Remove(sheet);
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
                    MessageBox.Show("Нет файлов для выбора ключей", "Ошибка");
                    return;
                }
                KeySettingsWindow window = new KeySettingsWindow();
                var vm = (KeyViewModel)window.DataContext;

                if (_memorySettings != null)
                {
                    vm.Headers = new ObservableCollection<KeySettings>(_memorySettings.OrderByDescending(k => k.IsSelected).ThenByDescending(k => k.Header));
                }
                else
                    vm.Headers = new ObservableCollection<KeySettings>(TotalHeaders.Select(h => new KeySettings(h.Key)).ToArray());
               
                bool? res = window.ShowDialog();
                _memorySettings = vm.Headers.ToList();
                RefreshKeys();
            });
        }

        public ICommand ReplaceCharacters
        {
            get => new ClickCommand((obj) =>
            {
                var sel = SelectedWorksheets.ToList();
                if (sel.Count == 0)
                {
                    MessageBox.Show("Не выбраны файлы для замены символов", "Ошибка");
                    return;
                }
                ReplaceCharactersWindow window = new ReplaceCharactersWindow();
                var vm = (ReplaceCharactersViewModel)window.DataContext;

                
                bool? res = window.ShowDialog();
                if (window.IsApplied)
                {
                    var replaceValues = vm.Values.ToList();

                    foreach(var sheet in sel)
                    {
                        var men = MenInSheets[sheet];
                        foreach(var man in men.ToArray())
                        {
                            bool changed = false;
                            Man newMan = vm.LeftOrigin ? man.Clone() : man;
                            string manString = man.ToString();
                            foreach (var val in man.GetKeyValues())
                            {
                                if (val.Value.isDate) continue;
                                foreach (var repVal in replaceValues)
                                {
                                    if (string.IsNullOrEmpty(repVal.OldValue) || manString.IndexOf(repVal.OldValue) == -1) continue;
                                    CellValue newData = val.Value;
                                    newData = new CellValue(val.Value.RawValue.Replace(repVal.OldValue, repVal.NewValue));
                                    newMan.ChangeData(val.Key, newData);
                                    changed = true;
                                }
                            }
                            if (changed && vm.LeftOrigin)
                                MenInSheets[sheet].Add(newMan);
                        }
                    }

                    RefreshKeys();
                }
            });
        }

        public ICommand FileItemDoubleClick
        {
            get => new ClickCommand((obj) =>
            {
                WorksheetItem item = obj as WorksheetItem;
                CoincidenceResultWindow window = new CoincidenceResultWindow(MenInSheets[item]);
                window.CoincidedCount = MenInSheets[item].Count;
                window.HandleType = DataHandleType.None;
                window.Show();
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
                        MessageBox.Show("Совпадений по выбранным файлам не найдено", "Уведомление");
                        return false;
                    }

                    break;
                case DataHandleType.Unique:
                    includedMen = GetUniqueMen();
                    if (includedMen.Count == 0)
                    { 
                        MessageBox.Show("Уникальных значений в выбранных файлах не найдено", "Уведомление");
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
                    if (man.GetHashCode() == 0) continue;
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
                    if (man.GetHashCode() == 0) continue;
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

        private void ReadWord(string path)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                WordParseWindow window = new WordParseWindow();
                WorksheetItem item = new WorksheetItem(Path.GetFileName(path), path);

                window.Title = $"Vladimir's Tool — Просмотр документа \"{item.Name}\"";
                var vm = (WordParseViewModel)window.DataContext;
                vm.OnAddToListData += () => window.Close();
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(path, false))
                {
                    Body body = wordDocument.MainDocumentPart.Document.Body;
                    StringBuilder contents = new StringBuilder();

                    var reg = new Regex(@"^[\s\p{L}\d\•\-\►]");

                    foreach (Paragraph co in
                                wordDocument.MainDocumentPart.Document.Body.Descendants<Paragraph>().Where(somethingElse =>
                                reg.IsMatch(somethingElse.InnerText)))
                    {
                        if (co.ParagraphProperties != null || co.ParagraphProperties?.NumberingProperties != null)
                        {
                            contents.Append($"{co.InnerText}\n");
                        }
                    }
                    wordDocument.Close();
                    vm.BodyText = contents.ToString();
                }
                bool? res = window.ShowDialog();

                if (vm.IsAdd)
                {
                    if (MenInSheets.ContainsKey(item))
                    {
                        MessageBox.Show($"Файл с именем \"{item.Name}\" уже добавлен", "Ошибка");
                        return;
                    }
                    var data = vm.GetData();
                    MenInSheets.Add(item, GetMenFromData(data).ToList());
                    App.Current.Dispatcher?.Invoke(() => SheetKeys.Add(item));
                    AddTotalHeaders(data.ElementAt(0));
                }
            });
        }

        private void ReadExcelSheet(string path)
        {
            WorksheetReader wsReader = new WorksheetReader();
            XLWorkbook workbook = new XLWorkbook(path);
            IXLWorksheet worksheet = workbook.Worksheet(1);

            WorksheetItem item = new WorksheetItem(Path.GetFileName(path), path);
            if (MenInSheets.ContainsKey(item))
            {
                MessageBox.Show($"Файл \"{item.Name}\" уже добавлен");
                workbook.Dispose();
                return;
            }

            var men = wsReader.Parse(worksheet);
            if (men.Count() == 0) return;

            MenInSheets.Add(item, men.ToList());
            App.Current.Dispatcher?.Invoke(() => SheetKeys.Add(item));
            workbook.Dispose();
            AddTotalHeaders(wsReader.Headers);
        }

        private void ReadOldExcelSheet(string path)
        {
            OldWorksheetReader wsReader = new OldWorksheetReader();

            Excel.Application excel = new Excel.Application();
            Excel.Workbook wb = excel.Workbooks.Open(path, ReadOnly: true);
            Excel.Worksheet ws = wb.Worksheets[1];

            WorksheetItem item = new WorksheetItem(wb.Name, path);
            if (MenInSheets.ContainsKey(item))
            {
                MessageBox.Show($"Файл \"{item.Name}\" уже добавлен");
                wb.Close();
                return;
            }

            var men = wsReader.Parse(ws);
            if (men.Count() == 0) return;

            MenInSheets.Add(item, men.ToList());
            App.Current.Dispatcher?.Invoke(() => SheetKeys.Add(item));
            wb.Close();

            AddTotalHeaders(wsReader.Headers);
        }

        private void ReadCSV(string path)
        {
            var csvReader = new CSVReader();

            WorksheetItem item = new WorksheetItem(Path.GetFileName(path), path);
            if (MenInSheets.ContainsKey(item))
            {
                MessageBox.Show($"Файл \"{item.Name}\" уже добавлен");
                return;
            }

            var men = csvReader.Parse(path);
            if (men.Count() == 0) return;

            MenInSheets.Add(item, men.ToList());
            App.Current.Dispatcher?.Invoke(() => SheetKeys.Add(item));
            AddTotalHeaders(csvReader.Headers);
        }

        private IEnumerable<Man> GetMenFromData(IEnumerable<IEnumerable<string>> data)
        {
            List<Man> men = new List<Man>();
            string[] headers = data.First().ToArray();
            for(int i = 1; i < data.Count(); i++)
            {
                Man man = new Man();
                int rowLength = data.ElementAt(i).Count();
                for (int j = 0; j < headers.Length; j++)
                {
                    if (headers[j] == null) continue;
                    if (rowLength < headers.Length && j >= rowLength)
                    {
                        man.AddData(headers[j], new CellValue(string.Empty));
                    }
                    else
                    {
                        man.AddData(headers[j], new CellValue(data.ElementAt(i).ElementAt(j)));
                    }
                }
                string manString = man.ToString();
                //if (string.IsNullOrEmpty(manString) || string.IsNullOrWhiteSpace(manString)) continue;
                man.CalculateHashCode();
                men.Add(man);
            }
            return men;
        }

        private void AddTotalHeaders(IEnumerable<string> headers)
        {
            foreach(var header in headers)
            {
                if (string.IsNullOrEmpty(header) || string.IsNullOrWhiteSpace(header)) continue;
                if (!TotalHeaders.ContainsKey(header))
                {
                    TotalHeaders.Add(header, TotalHeaders.Count + 1);
                }
            }
        }
    }
}
