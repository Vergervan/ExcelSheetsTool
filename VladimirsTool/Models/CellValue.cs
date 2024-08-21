using System;

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
}
