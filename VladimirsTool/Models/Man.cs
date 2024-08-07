using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VladimirsTool.Models
{
    public struct CellValue
    {
        public object value;
        public bool isDate;
        public string dateFormat;

        public CellValue(object value)
        {
            this.value = value;
            isDate = value is DateTime;
            this.dateFormat = "dd.MM.yyyy";
        }

        public override string ToString()
        {
            if (value == null) return string.Empty;
            return isDate ? ((DateTime)value).ToString(dateFormat) : value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (value == null) return false;
            if (isDate && obj is DateTime date)
                return ((DateTime)value).Equals(date);
            return value.ToString().ToUpper() == obj.ToString().ToUpper();
        }

        public override int GetHashCode()
        {
            return isDate ? ((DateTime)value).GetHashCode() : value.ToString().ToUpper().GetHashCode();
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

        public bool AddData(string header, object data)
        {
            if (string.IsNullOrEmpty(header.Trim())) return false;
            _manData.Add(header, new CellValue(data));
            return true;
        }

        public void CalculateHashCode()
        {
            KeyHeaderStore store = KeyHeaderStore.GetInstance();
            HashCode hash = new HashCode();
            lock (_manData)
            {
                foreach (var data in _manData)
                {
                    if (store.HasKeys && !store.Contains(data.Key)) continue;
                    hash.Add(data.Key);
                    hash.Add(data.Value.ToString().Trim().ToUpper());
                }
            }
            _preHashCode = hash.ToHashCode();
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
