using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VladimirsTool.Models;
using VladimirsTool.ViewModels;
using Man = VladimirsTool.Models.Man;

namespace VladimirsTool.Views
{
    public partial class CoincidenceResultWindow : Window
    {
        private Dictionary<string, int> columnNumber = new Dictionary<string, int>();
        private IEnumerable<Man> men;

        public int CoincidedCount
        {
            set
            {
                ((CoincidenceViewModel)DataContext).CoincidedCount = value;
            }
        } 
        public DataHandleType HandleType
        {
            set
            {
                ((CoincidenceViewModel)DataContext).HandleType = value;
            }
        }

        public CoincidenceResultWindow(IEnumerable<Man> men)
        {
            InitializeComponent();
            this.men = men;
            ((CoincidenceViewModel)DataContext).OnMergeClick += MergeLines;
            DefineDictionary();
            DefineData();
        }

        private void MergeLines()
        {
            try
            {
                var data = GetMerged2DData(men);
                var viewModel = (CoincidenceViewModel)DataContext;
                viewModel.SetHeaders(data[0]);
                data.RemoveAt(0);
                viewModel.SetDataTable(data);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Ошибка при объединении");
            }
        }

        public void DefineData(bool fillColumns = true)
        {
            var data = GetObservable2DData(men);

            if (fillColumns)
            {
                menGrid.Columns.Clear();
                for(int i = 0; i < columnNumber.Count; i++)
                {
                    var col = new DataGridTextColumn();
                    col.Header = data[0][i];
                    col.Binding = new Binding(string.Format("[{0}]", i));
                    this.menGrid.Columns.Add(col);
                }
            }
            var viewModel = (CoincidenceViewModel)DataContext;
            viewModel.SetHeaders(data[0]);
            data.RemoveAt(0);
            viewModel.SetDataTable(data);
        }

        public ObservableCollection<ObservableCollection<string>> GetObservable2DData(IEnumerable<Man> men)
        {
            ObservableCollection<ObservableCollection<string>> dataTable = new ObservableCollection<ObservableCollection<string>>();

            dataTable.Add(new ObservableCollection<string>( new string[columnNumber.Count] ));

            int rowCounter = 1;
            foreach (var man in men)
            {
                dataTable.Add(new ObservableCollection<string>( new string[columnNumber.Count] ));
                foreach (var pair in man.GetKeyValues())
                {
                    int colNum = columnNumber[pair.Key];
                    if (string.IsNullOrEmpty(dataTable[0][colNum])) dataTable[0][colNum] = pair.Key;
                    dataTable[rowCounter][colNum] = pair.Value.ToString();
                }
                ++rowCounter;
            }
            return dataTable;
        }

        public ObservableCollection<ObservableCollection<string>> GetMerged2DData(IEnumerable<Man> men)
        {
            ObservableCollection<ObservableCollection<string>> dataTable = new ObservableCollection<ObservableCollection<string>>();

            dataTable.Add(new ObservableCollection<string>(new string[columnNumber.Count]));
            KeyHeaderStore store = KeyHeaderStore.GetInstance();
            int rowCounter = 0;
            Man prevMan = null;
            foreach (var man in men)
            {
                if (man.Equals(prevMan))
                {
                    foreach (var pair in man.GetKeyValues())
                    {
                        int colNum = columnNumber[pair.Key];
                        if (string.IsNullOrEmpty(dataTable[0][colNum])) dataTable[0][colNum] = pair.Key;
                        var dataValue = dataTable[rowCounter][colNum];
                        if (string.IsNullOrEmpty(dataValue))
                            dataTable[rowCounter][colNum] = pair.Value.ToString();
                        else if (dataValue.ToUpper() == pair.Value.ToString().ToUpper()) 
                            continue;
                        else if (!store.Contains(pair.Key))
                            dataTable[rowCounter][colNum] += $"\n{pair.Value}";
                    }
                }
                else
                {
                    prevMan = man;
                    ++rowCounter;
                    dataTable.Add(new ObservableCollection<string>(new string[columnNumber.Count]));
                    foreach (var pair in man.GetKeyValues())
                    {
                        int colNum = columnNumber[pair.Key];
                        if (string.IsNullOrEmpty(dataTable[0][colNum])) dataTable[0][colNum] = pair.Key;
                        dataTable[rowCounter][colNum] = pair.Value.ToString();
                    }
                }
            }
            return dataTable;
        }

        public void DefineDictionaryWithColumns(IEnumerable<string> columns)
        {
            columnNumber.Clear();
            foreach (var column in columns)
                columnNumber.Add(column, columnNumber.Count);
        }

        public void DefineDictionary(bool keysFirst = false)
        {
            columnNumber.Clear();
            int counter = columnNumber.Count;
            foreach (var man in men)
            {
                var headers = man.Headers.ToArray();
                for (int i = 0; i < headers.Length; i++)
                {
                    if (!columnNumber.ContainsKey(headers[i]))
                    {
                        columnNumber.Add(headers[i], counter++);
                    }
                }
            }
        }

        private void menGrid_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            columnNumber[e.Column.Header.ToString()] = e.Column.DisplayIndex;
        }

        private void menGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            DefineData();
        }
    }
}
