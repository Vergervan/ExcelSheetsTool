using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VladimirsTool.Models
{
    public struct CellValue
    {
        private string rawValue;
        public DateTime date;
        public bool isDate;
        public string dateFormat;

        public string RawValue => rawValue;

        public CellValue(string rawValue)
        {
            this.rawValue = rawValue;
            isDate = false;
            date = new DateTime();
            dateFormat = null;
        }
        public CellValue(DateTime date, string rawValue, string dateFormat = null)
        {
            this.date = date;
            isDate = true;
            //this.rawValue = dateFormat == null ? date.ToString() : date.ToString(dateFormat);
            this.rawValue = rawValue;
            this.dateFormat = dateFormat;
        }

        public override string ToString()
        {
            if (isDate && dateFormat != null) return date.ToString(dateFormat);
            return rawValue;
        }

        public override bool Equals(object obj)
        {
            //if (rawValue == null && obj == null) return true;
            //if (!isDate && string.IsNullOrEmpty(rawValue)) return false;
            //if (isDate && obj is CellValue cell)
            //    return ToString() == cell.ToString();
            //return rawValue.ToUpper() == obj.ToString().ToUpper();
            return ToString().ToUpper() == obj.ToString().ToUpper();
        }

        public override int GetHashCode()
        {
            return isDate ? date.GetHashCode() : rawValue.ToUpper().GetHashCode();
        }
    }

    public class Man : IComparable
    {
        private Dictionary<string, CellValue> _manData = new Dictionary<string, CellValue>();
        private int _preHashCode = 0;

        public int DataCount => _manData.Count;
        public CellValue[] GetValues() => _manData.Values.ToArray();
        public IEnumerable<string> Headers => _manData.Keys.ToList();
        public KeyValuePair<string, CellValue>[] GetKeyValues() => _manData.ToArray();

        public bool AddData(string header, CellValue data)
        {
            if (string.IsNullOrEmpty(header.Trim())) return false;
            _manData.Add(header, data);
            return true;
        }

        public void CalculateHashCode()
        {
            KeyHeaderStore store = KeyHeaderStore.GetInstance();
            HashCode hash = new HashCode();
            bool keysMatch = false;
            CultureInfo ruRU = new CultureInfo("ru-RU");
            IDictionary<string, KeySettings> dateKeys = store.GetDateKeys();
            bool keysHasValue = false;
            lock (_manData)
            {
                foreach (var data in _manData.ToArray())
                {
                    var cellValue = data.Value;
                    var settings = store.GetSettings(data.Key);
                    if (store.HasKeys && settings == null) continue;
                    if (string.IsNullOrEmpty(cellValue.ToString())) continue;
                    keysHasValue = true;
                    //if (store.HasKeys && !store.Contains(data.Key)) continue;
                    keysMatch = true;
                    if (dateKeys.ContainsKey(data.Key))
                    {
                        var setting = dateKeys[data.Key];
                        if (cellValue.isDate)
                        {
                            cellValue.dateFormat = setting.OutDateFormat;
                            _manData[data.Key] = cellValue;
                        }
                        else
                        {
                            foreach(var probFormat in setting.InputFormats)
                            {
                                DateTime res;
                                if (DateTime.TryParseExact(cellValue.ToString(), probFormat, ruRU, DateTimeStyles.AllowWhiteSpaces, out res))
                                {
                                    cellValue.isDate = true;
                                    cellValue.date = res;
                                    cellValue.dateFormat = setting.OutDateFormat;
                                    _manData[data.Key] = cellValue;
                                    break;
                                }
                            }
                        }
                    }
                    else if (cellValue.isDate)
                    {
                        cellValue.dateFormat = null;
                        _manData[data.Key] = cellValue;
                    }
                    hash.Add(data.Key);
                    hash.Add(cellValue.ToString().Trim().ToUpper());
                }
            }
            _preHashCode = hash.ToHashCode();
            if (!keysMatch) ClearHashCode(); //If no keys found in this entity, then there's no hashcode
        }

        public void ClearHashCode() => _preHashCode = 0;

        public CellValue GetData(string header)
        {
            CellValue val;
            _manData.TryGetValue(header, out val);
            return val;
        }
        
        public override int GetHashCode()
        {
            return _preHashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Man man)
            {
                KeyHeaderStore store = KeyHeaderStore.GetInstance();
                foreach(var data in man.GetKeyValues())
                {
                    if (!store.Contains(data.Key)) continue;
                    if (!_manData.ContainsKey(data.Key) || !data.Value.Equals(_manData[data.Key])) return false;
                    //if(!data.Value.Equals(_manData[data.Key])) return false;
                }
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            KeyHeaderStore store = KeyHeaderStore.GetInstance();
            return string.Join(" ", _manData.Where(m => !store.HasKeys || store.Contains(m.Key)).Select(m => m.Value.ToString()));
        }

        public int CompareTo(object obj)
        {
            if(obj is Man man)
            {
                int compareVal = 0;
                KeyHeaderStore store = KeyHeaderStore.GetInstance();
                foreach (var data in man.GetKeyValues())
                {
                    if (!store.Contains(data.Key)) continue;
                    if (!_manData.ContainsKey(data.Key)) compareVal -= 1;
                    compareVal += data.Value.ToString().ToUpper().CompareTo(_manData[data.Key].ToString().ToUpper());
                }
                return compareVal;
            }
            return -1;
        }
    }
}
