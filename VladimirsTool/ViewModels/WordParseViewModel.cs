using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VladimirsTool.Models;
using VladimirsTool.Utils;

namespace VladimirsTool.ViewModels
{
    public class WordParseViewModel : BaseVM
    {
        private string _bodyText;
        private string _highlightedText;
        private ObservableCollection<Splitter> _splitters = new ObservableCollection<Splitter>();
        private ObservableCollection<Header> _headers = new ObservableCollection<Header>();
        private List<string[]> _data = new List<string[]>();
        private bool _isAdd = false;

        public bool IsAdd => _isAdd;

        public delegate void CloseHandler();
        public event CloseHandler OnAddToListData;

        private bool _isChanging = false;
        private bool _isReadOnly = true;
        public IEnumerable<IEnumerable<string>> GetData() => _data;
        public bool IsChanging
        {
            get => _isChanging;
            set
            {
                _isChanging = value;
                OnPropertyChanged();
            }
        }
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                OnPropertyChanged();
            }
        }
        public string BodyText
        {
            get => _bodyText;
            set
            {
                _bodyText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HighlightedText));
            }
        }

        public string HighlightedText
        {
            get => VisualText();
            set
            {
                _highlightedText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Splitter> Splitters
        {
            get => _splitters;
            set
            {
                _splitters = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Header> Headers
        {
            get => _headers;
            set
            {
                _headers = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddSplitter
        {
            get => new ClickCommand((obj) =>
            {
                Splitters.Add(new Splitter());
            });
        }
        public ICommand RemoveSplitter
        {
            get => new ClickCommand((obj) =>
            {
                Splitters.RemoveAt(Splitters.Count - 1);
            });
        }

        public ICommand TestButton
        {
            get => new ClickCommand((obj) =>
            {
                OnPropertyChanged(nameof(HighlightedText));
            });
        }

        public ICommand ChangeButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                IsReadOnly = IsChanging;
                IsChanging = !IsChanging;
            });
        }

        public ICommand ExportInExcel
        {
            get => new ClickCommand((obj) =>
            {
                WorksheetWriter ws = new WorksheetWriter();

                ws.SaveExcelFile("", Headers.Select(h => h.Value.Trim().ToUpper()), _data);
            });
        }

        public ICommand AddDataToList
        {
            get => new ClickCommand((obj) =>
            {
                Dictionary<string, int> headPos = new Dictionary<string, int>();
                List<Tuple<int, string>> posToHeader = new List<Tuple<int, string>>();
                string[] headers = Headers.Select(h => h.Value.Trim().ToUpper()).ToArray();
                for(int i = 0; i < headers.Length; i++)
                {
                    if (string.IsNullOrEmpty(headers[i])) continue;
                    if (!headPos.ContainsKey(headers[i]))
                    {
                        headPos.Add(headers[i], i);
                    }
                    else
                    {
                        posToHeader.Add(new Tuple<int, string>(i, headers[i]));
                        headers[i] = string.Empty;
                    }
                }
                
                if(headPos.Count == 0)
                {
                    MessageBox.Show("Заполните необходимые заголовки", "Ошибка");
                    return;
                }

                for(int i = 0; i < _data.Count; i++)
                {
                    foreach(var pos in posToHeader)
                    {
                        int idx = headPos[pos.Item2];
                        string str = _data[i][idx];
                        str += _data[i][pos.Item1];
                        _data[i][idx] = str;
                    }
                }
                _data.Insert(0, headers);
                _isAdd = true;
                OnAddToListData?.Invoke();
            });
        }

        private string VisualText()
        {
            StringBuilder xamlBuilder = new StringBuilder();
            if (_bodyText != null) 
            {
                var splitters = Splitters.Select(s => s.Value).ToArray();
                var lines = _bodyText.Split('\n');
                int maxHeaders = 0;
                _data.Clear();
                xamlBuilder.Append("<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue;
                    xamlBuilder.Append("<Paragraph>");
                    var splittedLine = line.Split(splitters, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();
                    _data.Add(splittedLine);
                    maxHeaders = splittedLine.Length > maxHeaders ? splittedLine.Length : maxHeaders;
                    for(int i = 0; i < splittedLine.Length; i++)
                    {
                        xamlBuilder.Append($"<Run FontWeight=\"Bold\" FontSize=\"12\" Foreground=\"Orange\">{i+1}:[</Run>");
                        xamlBuilder.Append($"<Run>{splittedLine[i]}</Run>");
                        xamlBuilder.Append("<Run FontWeight=\"Bold\" FontSize=\"12\" Foreground=\"Orange\">]⠀</Run>");
                    }
                    xamlBuilder.Append("</Paragraph>");
                }
                xamlBuilder.Append("</FlowDocument>");
                if (Headers.Count != maxHeaders)
                {
                    Headers.Clear();
                    for (int i = 0; i < maxHeaders; i++)
                        Headers.Add(new Header(string.Empty));
                }
            }
            return _highlightedText = xamlBuilder.ToString();
        }
    }
}
