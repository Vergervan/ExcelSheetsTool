using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            DefineDictionary();
            DefineData();
        }

        public void DefineData(bool fillColumns = true)
        {
            var data = GetObservable2DData(men);

            if (fillColumns)
            {
                for (int i = 0; i < columnNumber.Count; i++)
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
                    dataTable[rowCounter][columnNumber[pair.Key]] = pair.Value is DateTime date ? date.ToString("dd.MM.yyyy") : (pair.Value == null ? string.Empty : pair.Value.ToString());
                }
                ++rowCounter;
            }
            return dataTable;
        }

        public void DefineDictionaryWithColumns(IEnumerable<string> columns)
        {
            columnNumber.Clear();
            int counter = 0;
            foreach (var column in columns)
                columnNumber.Add(column, counter++);
        }

        public void DefineDictionary(bool useDefaultColumns = true)
        {
            columnNumber.Clear();
            if (useDefaultColumns)
                FillDefaultColumnNumbers();
            int counter = columnNumber.Count;
            foreach (var man in men)
            {
                var headers = man.GetHeaders.ToArray();
                for (int i = 0; i < headers.Length; i++)
                {
                    if (!columnNumber.ContainsKey(headers[i]))
                    {
                        columnNumber.Add(headers[i], counter++);
                    }
                }
            }
        }

        public void FillDefaultColumnNumbers()
        {
            columnNumber.Clear();
            columnNumber.Add("ФАМИЛИЯ", 0);
            columnNumber.Add("ИМЯ", 1);
            columnNumber.Add("ОТЧЕСТВО", 2);
            columnNumber.Add("ДАТА РОЖДЕНИЯ", 3);
        }

        private void menGrid_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            columnNumber[e.Column.Header.ToString()] = e.Column.DisplayIndex;
            DefineData(false);
        }
    }
}
