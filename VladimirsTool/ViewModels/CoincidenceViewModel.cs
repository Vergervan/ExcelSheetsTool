﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using VladimirsTool.Utils;

namespace VladimirsTool.ViewModels
{
    public class CoincidenceViewModel : BaseVM
    {
        public delegate void MergeHandler();
        public delegate void HeadersHandler(IEnumerable<string> headers);
        public event HeadersHandler OnHeadersChanged;
        public event MergeHandler OnMergeClick;
        private ObservableCollection<string> _headers;
        private ObservableCollection<ObservableCollection<string>> _dataTable;
        private int _coincidedCount;
        private DataHandleType _handleType;
        public DataHandleType HandleType
        {
            get => _handleType;
            set
            {
                _handleType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
                OnPropertyChanged(nameof(IsCoincide));
            }
        }
        public Visibility IsCoincide
        {
            get => _handleType == DataHandleType.Coincided ? Visibility.Visible : Visibility.Collapsed;
        }
        public string WindowTitle
        {
            get
            {
                string title = "Vladimir's Tool — ";
                switch (_handleType)
                {
                    case DataHandleType.None:
                        return title + "Просмотр";
                    case DataHandleType.Coincided:
                        return title + "Совпадения";
                    case DataHandleType.Unique:
                        return title + "Уникальные значения";
                }
                return title + _handleType.ToString();
            }
        }
        public ObservableCollection<ObservableCollection<string>> DataTable => _dataTable;
        public ObservableCollection<string> Headers => _headers;

        public int RowCount => _dataTable == null ? 0 : _dataTable.Count;

        public int CoincidedCount
        {
            get => _coincidedCount;
            set
            {
                _coincidedCount = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExportInExcel
        {
            get => new ClickCommand((obj) =>
            {
                //MessageBox.Show(string.Join(" ", Headers) + "\n" + string.Join("\n", _dataTable.Select(r => string.Join("  ", r))));
                WorksheetWriter writer = new WorksheetWriter();
                writer.SaveExcelFile("", _headers, _dataTable);
            });
        }

        public ICommand ExportInWord
        {
            get => new ClickCommand((obj) =>
            {
                MessageBox.Show("В разработке");
                WorksheetWriter writer = new WorksheetWriter();
                writer.SaveWordFile(_headers, _dataTable);
            });
        }

        public ICommand MergeCoincidedLines
        {
            get => new ClickCommand((obj) =>
            {
                OnMergeClick?.Invoke();
            });
        }

        public ICommand ChooseHeaders
        {
            get => new ClickCommand((obj) =>
            {
                MessageBox.Show("В разработке");
            });
        }

        public void SetHeaders(ObservableCollection<string> headers)
        {
            _headers = headers;
            OnPropertyChanged(nameof(Headers));
        }
        public void SetDataTable(ObservableCollection<ObservableCollection<string>> data)
        {
            bool empty = _dataTable == null; //Save state before assignment. DO NOT TOUCH!!!
            //_dataTable = data;               //Assignment

            //Check if it was empty before assignment
            //if (empty)
            //{
            _dataTable = data;
            OnPropertyChanged(nameof(DataTable));
            OnPropertyChanged(nameof(RowCount));
            //}
        }
    }
}
