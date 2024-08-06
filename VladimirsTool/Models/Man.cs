using System;
using System.Collections.Generic;
using System.Linq;

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
        private string _firstName, _lastName, _surname;
        public string FirstName { get => _firstName; set => _firstName = value?.ToUpper(); }
        public string LastName { get => _lastName; set => _lastName = value?.ToUpper(); }
        public string Surname { get => _surname; set => _surname = value?.ToUpper(); }
        public DateTime BirthDate { get; set;}
        private Dictionary<string, CellValue> _manData = new Dictionary<string, CellValue>();
        private int _preHashCode = 0;

        public int DataCount => _manData.Count;
        public CellValue[] GetValues() => _manData.Values.ToArray();
        public IEnumerable<string> GetHeaders => _manData.Keys.ToList();
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
                    if (!store.Contains(data.Key)) continue;
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
            if (_preHashCode != 0) return _preHashCode;
            return HashCode.Combine(FirstName, LastName, Surname, BirthDate);
        }

        public override bool Equals(object obj)
        {
            if (obj is Man man)
            {
                if(_preHashCode != 0)
                {
                    KeyHeaderStore store = KeyHeaderStore.GetInstance();
                    foreach(var data in man.GetKeyValues())
                    {
                        if (!store.Contains(data.Key)) continue;
                        if (!_manData.ContainsKey(data.Key) || !data.Value.Equals(_manData[data.Key])) return false;
                    }
                    return true;
                }
                return this.LastName == man.LastName &&
                        this.FirstName == man.FirstName &&
                        this.Surname == man.Surname &&
                        this.BirthDate == man.BirthDate;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", LastName, FirstName, Surname, BirthDate.ToString("dd.MM.yyyy"));
        }

        public int CompareTo(object obj)
        {
            if(obj is Man man)
            {
                int compareVal = 0;
                if (_preHashCode != 0)
                {
                    KeyHeaderStore store = KeyHeaderStore.GetInstance();
                    foreach (var data in man.GetKeyValues())
                    {
                        if (!store.Contains(data.Key)) continue;
                        if (!_manData.ContainsKey(data.Key)) compareVal -= 1;
                        compareVal += data.Value.ToString().ToUpper().CompareTo(_manData[data.Key].ToString().ToUpper());
                    }
                    return compareVal;
                }
                compareVal += LastName == null ? -1 : LastName.CompareTo(man.LastName);
                compareVal += FirstName == null ? -1 : FirstName.CompareTo(man.LastName);
                compareVal += Surname == null ? -1 : Surname.CompareTo(man.LastName);
                compareVal += BirthDate.CompareTo(man.BirthDate);
                return compareVal;
            }
            return -1;
        }
    }
}
