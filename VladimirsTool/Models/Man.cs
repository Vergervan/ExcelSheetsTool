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
            return value.ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return isDate ? ((DateTime)value).GetHashCode() : value.ToString().GetHashCode();
        }
    }
    public class Man : IComparable
    {
        private string _firstName, _lastName, _surname;
        public string FirstName { get => _firstName; set => _firstName = value?.ToUpper(); }
        public string LastName { get => _lastName; set => _lastName = value?.ToUpper(); }
        public string Surname { get => _surname; set => _surname = value?.ToUpper(); }
        public DateTime BirthDate { get; set;}
        public string BirthDateString => BirthDate.ToString("dd.MM.yyyy");
        private Dictionary<string, CellValue> _manData = new Dictionary<string, CellValue>();

        public int DataCount => _manData.Count;
        public CellValue[] GetValues() => _manData.Values.ToArray();
        public IEnumerable<string> GetHeaders => _manData.Keys.ToList();
        public KeyValuePair<string, CellValue>[] GetKeyValues() => _manData.ToArray();

        public void AddData(string header, object data)
        {
            if (string.IsNullOrEmpty(header.Trim())) throw new Exception("Empty header");
            try
            {
                _manData.Add(header, new CellValue(data));
            }catch(Exception e)
            {
                throw e;
            }
        }

        public CellValue GetData(string header)
        {
            CellValue val;
            _manData.TryGetValue(header, out val);
            return val;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(FirstName, LastName, Surname, BirthDate);
        }

        public override bool Equals(object obj)
        {
            if (obj is Man man)
            {
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
                return LastName.CompareTo(man.LastName) + 
                        FirstName.CompareTo(man.FirstName) + 
                        Surname.CompareTo(man.Surname) + 
                        BirthDate.CompareTo(man.BirthDate);
            }
            return -1;
        }
    }
}
