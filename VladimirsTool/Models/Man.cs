using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VladimirsTool.Models
{
    public class Man : IComparable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Surname { get; set; }
        public DateTime BirthDate { get; set;}
        public string BirthDateString => BirthDate.ToString("dd.MM.yyyy");
        private Dictionary<string, object> _manData = new Dictionary<string, object>();

        public int DataCount => _manData.Count;
        public object[] GetValues() => _manData.Values.ToArray();
        public IEnumerable<string> GetHeaders => _manData.Keys.ToList();
        public KeyValuePair<string, object>[] GetKeyValues() => _manData.ToArray();

        public void AddData(string header, object data)
        {
            if (string.IsNullOrEmpty(header.Trim())) throw new Exception("Empty header");
            try
            {
                _manData.Add(header, data);
            }catch(Exception e)
            {
                throw e;
            }
        }

        public object GetData(string header)
        {
            object val;
            if (!_manData.TryGetValue(header, out val))
                return null;
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
            throw new ArgumentException("Некорректное значение параметра");
        }
    }
}
