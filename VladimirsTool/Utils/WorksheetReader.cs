using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VladimirsTool.Models;
using Man = VladimirsTool.Models.Man;

namespace VladimirsTool.Utils
{
    public class WorksheetReader
    {
        private string[] _headerNames;
    
        public IEnumerable<string> Headers => _headerNames;

        public IEnumerable<Man> Parse(Worksheet sheet)
        {
            int rowCount = sheet.UsedRange.Rows.Count, colCount = sheet.UsedRange.Columns.Count;

            Range firstRow = sheet.UsedRange.Rows[1];
            Array myHeadvalues = (Array)firstRow.Cells.Value;
            _headerNames = new string[myHeadvalues.Length];

            int counter = 0;
            foreach (var cell in myHeadvalues)
            {
                _headerNames[counter++] = cell?.ToString().Trim().ToUpper();
            }

            //LINQ removes null cells. It causes bugs and wrong cell counting 
            //var _headerNames2 = myHeadvalues.OfType<object>().Select(p => p?.ToString()).ToArray();
            
            List<Man> men = new List<Man>();

            for (int n = 2; n <= rowCount; n++)
            {
                Man man = new Man();
                Range currentRow = sheet.UsedRange.Rows[n];
                for(int i = 0; i < _headerNames.Length; i++)
                {
                    if (_headerNames[i] == null) continue;
                    man.AddData(_headerNames[i], new CellValue(currentRow.Cells[i+1].Value));
                }
                string manString = man.ToString();
                if (string.IsNullOrEmpty(manString) || string.IsNullOrWhiteSpace(manString)) continue;
                man.CalculateHashCode();
                men.Add(man);
            }
            return men;
        }
    }
}
